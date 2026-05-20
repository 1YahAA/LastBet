using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardGameManager : MonoBehaviour
{
    [Header("Настройки игры")]
    public int winScoreThreshold = 35;
    public int totalRounds = 8;
    public int cardsToDeal = 6;

    [Header("Генерация клиентов")]
    public Sprite[] customerPortraits;

    [Header("UI")]
    public CustomerView customerView;
    public CardInfoPanel cardInfoPanel;

    public CardSlot[] rowSlots = new CardSlot[3];

    public Transform handPanel;
    public HandFanLayout handFanLayout;
    public GameObject cardViewPrefab;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI deckDebugText;

    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button continueButton;

    private CardDeck _deck;
    private RuntimeCustomer _currentCustomer;

    private int _totalScore;
    private int _currentRound;

    private int _riskRounds;
    private int _obedienceRounds;
    private int _analysisRounds;

    private readonly List<CardView> _handViews = new();

    private void Start()
    {
        _deck = GetComponent<CardDeck>();

        if (_deck == null)
        {
            Debug.LogError("[CardGameManager] CardDeck не найден", this);
            return;
        }

        if (handPanel == null || cardViewPrefab == null)
        {
            Debug.LogError("[CardGameManager] HandPanel или CardViewPrefab не назначены", this);
            return;
        }

        _deck.Initialize();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (cardInfoPanel != null)
            cardInfoPanel.Hide();

        _totalScore = 0;
        _currentRound = 0;

        _riskRounds = 0;
        _obedienceRounds = 0;
        _analysisRounds = 0;

        StartRound();
    }

    private void StartRound()
    {
        _currentRound++;

        if (roundText != null)
            roundText.text = $"Клиент {_currentRound} / {totalRounds}";

        foreach (CardSlot slot in rowSlots)
        {
            if (slot != null)
                slot.Clear();
        }

        ClearHand();

        _currentCustomer = GenerateCustomer();

        if (customerView != null)
            customerView.Show(_currentCustomer.PortraitSprite, _currentCustomer.RequestText);

        List<CardData> drawnCards = _deck.Draw(cardsToDeal);

        if (drawnCards.Count == 0)
        {
            EndGame(false, "Карты закончились");
            return;
        }

        foreach (CardData card in drawnCards)
            SpawnCardInHand(card);

        RefreshHandLayout();
        UpdateScoreUI();
    }

    private RuntimeCustomer GenerateCustomer()
    {
        var customer = new RuntimeCustomer
        {
            Name = "Гость",
            RequiredType = RandomRecipeType(),
            PreferredType = RandomRecipeType(),
            BonusForPreferred = 1,
            RuleType = RandomRule()
        };

        if (customerPortraits != null && customerPortraits.Length > 0)
            customer.PortraitSprite = customerPortraits[Random.Range(0, customerPortraits.Length)];

        customer.RequestText = BuildRequestText(customer);

        return customer;
    }

    private CocktailType RandomRecipeType()
    {
        int value = Random.Range(0, 3);

        return value switch
        {
            0 => CocktailType.Bitter,
            1 => CocktailType.Lemonchello,
            _ => CocktailType.Absinthe
        };
    }

    private CustomerRuleType RandomRule()
    {
        int value = Random.Range(0, 5);

        return value switch
        {
            0 => CustomerRuleType.None,
            1 => CustomerRuleType.NoDamagedCards,
            2 => CustomerRuleType.WantsRainbow,
            3 => CustomerRuleType.WantsTriplet,
            _ => CustomerRuleType.NoAdjacencyBonus
        };
    }

    private string BuildRequestText(RuntimeCustomer customer)
    {
        string required = TypeName(customer.RequiredType);
        string preferred = TypeName(customer.PreferredType);

        return customer.RuleType switch
        {
            CustomerRuleType.NoDamagedCards =>
                $"Хочу {required}. И никакой тухлятины.",

            CustomerRuleType.WantsRainbow =>
                $"Хочу {required}. Смешай три разных вкуса.",

            CustomerRuleType.WantsTriplet =>
                $"Хочу {required}. Сделай чистый вкус.",

            CustomerRuleType.NoAdjacencyBonus =>
                $"Хочу {required}. Без хитрых сочетаний.",

            _ =>
                $"Хочу {required}. Если добавишь {preferred}, заплачу больше."
        };
    }

    private void SpawnCardInHand(CardData card)
    {
        GameObject go = Instantiate(cardViewPrefab, handPanel);
        CardView view = go.GetComponent<CardView>();

        if (view == null)
        {
            Debug.LogError("[CardGameManager] В CardViewPrefab нет CardView", go);
            Destroy(go);
            return;
        }

        view.Init(card, this);
        _handViews.Add(view);
    }

    private void RefreshHandLayout()
    {
        if (handFanLayout != null)
            handFanLayout.Refresh();
    }

    private void ClearHand()
    {
        foreach (CardView view in _handViews)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        _handViews.Clear();
    }

    public void OnCardSelectedFromHand(CardView cardView)
    {
        if (cardView == null)
            return;

        foreach (CardSlot slot in rowSlots)
        {
            if (slot == null || slot.HasCard)
                continue;

            slot.PlaceCard(cardView);
            _handViews.Remove(cardView);
            RefreshHandLayout();

            if (RowFull())
                EvaluateRound();

            return;
        }
    }

    private bool RowFull()
    {
        foreach (CardSlot slot in rowSlots)
        {
            if (slot == null || !slot.HasCard)
                return false;
        }

        return true;
    }

    private void EvaluateRound()
    {
        CardData[] cards = new CardData[3];

        for (int i = 0; i < rowSlots.Length; i++)
            cards[i] = rowSlots[i].PlacedCard;

        RoundScoreResult result = CardGameScoring.CalculateRoundScore(cards, _currentCustomer);

        Debug.Log(
            $"[CocktailScore] " +
            $"Score={result.Score}, " +
            $"Base={result.BaseScore}, " +
            $"Bonus={result.BonusScore}, " +
            $"Failed={result.IsFailed}, " +
            $"Fatal={result.IsFatal}, " +
            $"Reason={result.Reason}, " +
            $"Risk={result.UsedRiskCard}, " +
            $"Damaged={result.UsedDamagedCard}"
        );

        for (int i = 0; i < cards.Length; i++)
        {
            Debug.Log(
                $"[CocktailCard {i}] " +
                $"{cards[i].name} | " +
                $"Type={cards[i].cocktailType} | " +
                $"Points={cards[i].points} | " +
                $"Left={cards[i].requiredLeft} | " +
                $"Right={cards[i].requiredRight} | " +
                $"AdjBonus={cards[i].adjacencyBonus} | " +
                $"Effect={cards[i].effectType} | " +
                $"Target={cards[i].effectTarget}"
            );
        }

        _deck.AddManyToDiscard(cards);

        TrackStrategy(result);

        if (result.IsFatal)
        {
            EndGame(false, result.Reason);
            return;
        }

        if (!result.IsFailed)
            _totalScore += result.Score;

        UpdateScoreUI();

        DOVirtual.DelayedCall(0.8f, () =>
        {
            if (_currentRound >= totalRounds)
                EndGame(_totalScore >= winScoreThreshold);
            else
                StartRound();
        });
    }

    private void TrackStrategy(RoundScoreResult result)
    {
        if (result == null)
            return;

        if (result.UsedRiskCard || result.UsedDamagedCard)
            _riskRounds++;

        if (!result.IsFailed && result.SatisfiedClient && !result.UsedRiskCard && !result.UsedDamagedCard)
            _obedienceRounds++;

        if (!result.IsFailed && result.BonusScore > result.BaseScore)
            _analysisRounds++;
    }

    private CocktailStrategy DetermineStrategy()
    {
        if (_riskRounds > _obedienceRounds && _riskRounds >= _analysisRounds)
            return CocktailStrategy.Revolt;

        if (_analysisRounds > _obedienceRounds)
            return CocktailStrategy.Analysis;

        return CocktailStrategy.Obedience;
    }

    private TokenType StrategyToToken(CocktailStrategy strategy)
    {
        return strategy switch
        {
            CocktailStrategy.Revolt => TokenType.Revolt,
            CocktailStrategy.Obedience => TokenType.Obedience,
            CocktailStrategy.Analysis => TokenType.Analysis,
            _ => TokenType.Analysis
        };
    }

    private void EndGame(bool won, string reason = "")
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        CocktailStrategy strategy = DetermineStrategy();

        if (resultText != null)
        {
            if (won)
            {
                resultText.text =
                    $"Клиент доволен!\n" +
                    $"Итого: {_totalScore} / {winScoreThreshold}\n" +
                    $"Стиль: {StrategyName(strategy)}";
            }
            else if (string.IsNullOrEmpty(reason))
            {
                resultText.text =
                    $"Недостаточно качества...\n" +
                    $"Итого: {_totalScore} / {winScoreThreshold}\n" +
                    $"Стиль: {StrategyName(strategy)}";
            }
            else
            {
                resultText.text =
                    $"Поражение!\n{reason}\n" +
                    $"Стиль: {StrategyName(strategy)}";
            }
        }

        if (resultPanel != null)
        {
            resultPanel.transform.localScale = Vector3.zero;
            resultPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => FinishGame(won, strategy));
        }
    }

    private void FinishGame(bool won, CocktailStrategy strategy)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[CardGameManager] GameManager.Instance не найден");
            return;
        }

        GameManager.Instance.FinishMiniGame(won, StrategyToToken(strategy));
    }

    public void ShowCardInfo(CardData card)
    {
        if (cardInfoPanel != null)
            cardInfoPanel.Show(card);
    }

    public void HideCardInfo()
    {
        if (cardInfoPanel != null)
            cardInfoPanel.Hide();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Очки: {_totalScore} / {winScoreThreshold}";

        if (deckDebugText != null && _deck != null)
        {
            deckDebugText.text =
                $"Колода: {_deck.DrawPileCount}\n" +
                $"Сброс: {_deck.DiscardPileCount}";
        }
    }

    private string TypeName(CocktailType type)
    {
        return type switch
        {
            CocktailType.Bitter => "Биттер",
            CocktailType.Lemonchello => "Лимончелло",
            CocktailType.Absinthe => "Абсент",
            CocktailType.Damaged => "Испорченная",
            _ => "любой вкус"
        };
    }

    private string StrategyName(CocktailStrategy strategy)
    {
        return strategy switch
        {
            CocktailStrategy.Revolt => "Бунт",
            CocktailStrategy.Obedience => "Послушание",
            CocktailStrategy.Analysis => "Анализ",
            _ => "Анализ"
        };
    }
}

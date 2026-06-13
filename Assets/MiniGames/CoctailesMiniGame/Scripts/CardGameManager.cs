using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardGameManager : MonoBehaviour
{
    [Header("Настройки игры")]
    [SerializeField] private int winScoreThreshold = 35;
    [SerializeField] private int totalRounds = 8;
    [SerializeField] private int cardsToDeal = 6;

    [Header("Генерация гостей")]
    [SerializeField] private Sprite[] customerPortraits;

    [Header("UI")]
    [SerializeField] private CustomerView customerView;
    [SerializeField] private CardSlot[] rowSlots = new CardSlot[3];
    [SerializeField] private Transform handPanel;
    [SerializeField] private GameObject cardViewPrefab;

    [Header("Финал мини-игры")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button continueButton;

    [Header("Паузы")]
    [SerializeField] private float reactionDelay = 1.15f;

    private CardDeck _deck;
    private RuntimeCustomer _currentCustomer;
    private int _totalScore;
    private int _currentRound;
    private bool _isFinished;
    private string _lastRequestKey;
    private int _lastPortraitIndex = -1;

    private readonly List<CardView> _handViews = new List<CardView>();

    private static readonly string[] BitterRequests =
    {
        "Сделай что-нибудь горькое. Сладким я сегодня не верю.",
        "Мне нужен Биттер. Такой, чтобы зал наконец заткнулся.",
        "Биттер. И не пытайся сделать его мягче.",
        "Налей горькое. В этом доме всё равно ничего честнее нет.",
        "Хочу Биттер. Пусть хотя бы напиток не притворяется добрым."
    };

    private static readonly string[] LemonchelloRequests =
    {
        "Лимончелло. Хочу вкус, будто вечер ещё можно спасти.",
        "Сделай Лимончелло. Что-нибудь светлое, пока свет не погас.",
        "Мне нужен Лимончелло. Без лишней драмы, если получится.",
        "Лимончелло. И пусть будет не таким кислым, как публика.",
        "Хочу Лимончелло. Слишком много дыма, нужно что-то яснее."
    };

    private static readonly string[] AbsintheRequests =
    {
        "Абсент. Сегодня хочется видеть чуть меньше правды.",
        "Сделай Абсент. Зеленее, тише и опаснее.",
        "Мне нужен Абсент. Только не спрашивай зачем.",
        "Абсент. Пусть хотя бы бокал скажет правду криво.",
        "Хочу Абсент. После полуночи всё равно никто не вспомнит вкус."
    };

    private static readonly string[] NoDamagedAdditions =
    {
        "И без испорченных ингредиентов. Я ещё различаю запах плесени.",
        "Только без тухлятины. У меня и так вечер испорчен.",
        "Если положишь что-то испорченное, я верну бокал обратно."
    };

    private static readonly string[] RainbowAdditions =
    {
        "Смешай три разных вкуса. Хочу увидеть хоть какое-то разнообразие.",
        "Пусть там будут разные оттенки. Одного вкуса мне мало.",
        "Собери из трёх разных основ. Сегодня мне нужна пестрота."
    };

    private static readonly string[] TripletAdditions =
    {
        "Сделай чистый вкус. Без метаний из стороны в сторону.",
        "Три одинаковые основы. Я устал от неожиданных решений.",
        "Хочу ровный состав. Один вкус, без фокусов."
    };

    private static readonly string[] NoAdjacencyAdditions =
    {
        "Без хитрых сочетаний. Просто собери нормально.",
        "Не играй в комбинации. Мне нужен понятный напиток.",
        "Без трюков с соседством. Я плачу не за загадки."
    };

    private static readonly string[] PreferredAdditions =
    {
        "Если добавишь {0}, будет лучше.",
        "С {0} я, возможно, даже улыбнусь.",
        "Добавь {0}, если хочешь сделать это прилично.",
        "{0} не обязателен, но я бы заметил."
    };

    private static readonly string[] GoodReactions =
    {
        "Неплохо. Значит, за стойкой ещё остались люди с руками.",
        "Вот это уже похоже на заказ, а не на наказание.",
        "Удивительно. Я почти доволен.",
        "Хорошо. Сегодня это редкость.",
        "Сойдёт. Даже лучше, чем я ожидал."
    };

    private static readonly string[] BadReactions =
    {
        "Нет. Ты вообще слышала, что я просил?",
        "Это не мой заказ. Это чьё-то сожаление в бокале.",
        "Плохо. Даже для этого места.",
        "Я просил напиток, а не повод уйти.",
        "Нет. Забери это, пока я не передумал платить."
    };

    private static readonly string[] FatalReactions =
    {
        "Это пить нельзя. Даже здесь есть предел.",
        "Ты хочешь меня отравить или просто вечер такой?",
        "Убери. Этот бокал уже проиграл до того, как я его взял."
    };

    private void Start()
    {
        _deck = GetComponent<CardDeck>();

        if (_deck == null)
        {
            Debug.LogError("[CardGameManager] CardDeck не найден", this);
            return;
        }

        if (handPanel == null)
        {
            Debug.LogError("[CardGameManager] HandPanel не назначен", this);
            return;
        }

        if (cardViewPrefab == null)
        {
            Debug.LogError("[CardGameManager] CardViewPrefab не назначен", this);
            return;
        }

        if (rowSlots == null || rowSlots.Length == 0)
        {
            Debug.LogError("[CardGameManager] RowSlots не назначены", this);
            return;
        }

        _deck.Initialize();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        _totalScore = 0;
        _currentRound = 0;
        _isFinished = false;

        StartRound();
    }

    private void StartRound()
    {
        if (_isFinished)
            return;

        _currentRound++;

        // Между гостями очищаем только стол: выбранные карты уже ушли в сброс.
        // Руку не пересоздаём полностью, а добираем до cardsToDeal.
        ClearSlots();
        RefillHandToTarget();

        List<CardData> availableHandCards = GetCurrentHandCards();

        if (availableHandCards.Count == 0)
        {
            EndGame(false, "Карты закончились.");
            return;
        }

        _currentCustomer = GenerateCustomer(availableHandCards);

        if (customerView != null)
            customerView.Show(_currentCustomer.PortraitSprite, _currentCustomer.RequestText);
    }

    private RuntimeCustomer GenerateCustomer(List<CardData> drawnCards)
    {
        RuntimeCustomer customer = new RuntimeCustomer
        {
            Name = "Гость",
            RequiredType = PickAvailableRecipeType(drawnCards),
            PreferredType = PickAvailableRecipeType(drawnCards),
            BonusForPreferred = 1,
            RuleType = PickPossibleRule(drawnCards)
        };

        customer.RequestText = BuildRequestText(customer, out string requestKey);
        customer.RequestKey = requestKey;

        if (customerPortraits != null && customerPortraits.Length > 0)
        {
            int index = Random.Range(0, customerPortraits.Length);

            if (customerPortraits.Length > 1 && index == _lastPortraitIndex)
                index = (index + 1) % customerPortraits.Length;

            _lastPortraitIndex = index;
            customer.PortraitSprite = customerPortraits[index];
        }

        return customer;
    }


    private CocktailType PickAvailableRecipeType(List<CardData> cards)
    {
        List<CocktailType> available = GetAvailableRecipeTypes(cards);

        if (available.Count == 0)
            return RandomRecipeType();

        return available[Random.Range(0, available.Count)];
    }

    private List<CocktailType> GetAvailableRecipeTypes(List<CardData> cards)
    {
        var available = new List<CocktailType>();

        if (HasRecipeType(cards, CocktailType.Bitter))
            available.Add(CocktailType.Bitter);

        if (HasRecipeType(cards, CocktailType.Lemonchello))
            available.Add(CocktailType.Lemonchello);

        if (HasRecipeType(cards, CocktailType.Absinthe))
            available.Add(CocktailType.Absinthe);

        return available;
    }

    private bool HasRecipeType(List<CardData> cards, CocktailType type)
    {
        if (cards == null)
            return false;

        foreach (CardData card in cards)
        {
            if (card == null)
                continue;

            if (card.cocktailType == type)
                return true;

            // Карта с эффектом AnyTypeForAdjacency визуально работает как универсальная добавка.
            // Для заказа её тоже считаем допустимой, чтобы раунд не становился невозможным.
            if (card.effectType == CardEffectType.AnyTypeForAdjacency && IsRecipeType(type))
                return true;
        }

        return false;
    }

    private CustomerRuleType PickPossibleRule(List<CardData> cards)
    {
        var possible = new List<CustomerRuleType>();

        possible.Add(CustomerRuleType.None);
        possible.Add(CustomerRuleType.NoAdjacencyBonus);

        if (CountNonDamaged(cards) >= 3)
            possible.Add(CustomerRuleType.NoDamagedCards);

        if (GetAvailableRecipeTypes(cards).Count >= 3)
            possible.Add(CustomerRuleType.WantsRainbow);

        if (HasAnyTriplet(cards))
            possible.Add(CustomerRuleType.WantsTriplet);

        return possible[Random.Range(0, possible.Count)];
    }

    private int CountNonDamaged(List<CardData> cards)
    {
        int count = 0;

        if (cards == null)
            return count;

        foreach (CardData card in cards)
        {
            if (card != null && card.cocktailType != CocktailType.Damaged)
                count++;
        }

        return count;
    }

    private bool HasAnyTriplet(List<CardData> cards)
    {
        return CountRecipeType(cards, CocktailType.Bitter) >= 3
            || CountRecipeType(cards, CocktailType.Lemonchello) >= 3
            || CountRecipeType(cards, CocktailType.Absinthe) >= 3;
    }

    private int CountRecipeType(List<CardData> cards, CocktailType type)
    {
        int count = 0;

        if (cards == null)
            return count;

        foreach (CardData card in cards)
        {
            if (card != null && card.cocktailType == type)
                count++;
        }

        return count;
    }

    private bool IsRecipeType(CocktailType type)
    {
        return type == CocktailType.Bitter
            || type == CocktailType.Lemonchello
            || type == CocktailType.Absinthe;
    }

    private CocktailType RandomRecipeType()
    {
        int value = Random.Range(0, 3);

        switch (value)
        {
            case 0: return CocktailType.Bitter;
            case 1: return CocktailType.Lemonchello;
            default: return CocktailType.Absinthe;
        }
    }

    private CustomerRuleType RandomRule()
    {
        int value = Random.Range(0, 5);

        switch (value)
        {
            case 0: return CustomerRuleType.None;
            case 1: return CustomerRuleType.NoDamagedCards;
            case 2: return CustomerRuleType.WantsRainbow;
            case 3: return CustomerRuleType.WantsTriplet;
            default: return CustomerRuleType.NoAdjacencyBonus;
        }
    }

    private string BuildRequestText(RuntimeCustomer customer, out string requestKey)
    {
        string required = TypeName(customer.RequiredType);
        string preferred = TypeName(customer.PreferredType);

        string baseLine = PickRequiredLine(customer.RequiredType);
        string addition = PickRuleLine(customer.RuleType, preferred);

        requestKey = customer.RequiredType + "_" + customer.RuleType + "_" + addition;

        if (requestKey == _lastRequestKey)
        {
            // Не меняем правило на случайное следующее: оно может стать невозможным для текущей руки.
            // Достаточно заново выбрать формулировку той же выполнимой просьбы.
            baseLine = PickRequiredLine(customer.RequiredType);
            addition = PickRuleLine(customer.RuleType, preferred);
            requestKey = customer.RequiredType + "_" + customer.RuleType + "_" + addition;
        }

        _lastRequestKey = requestKey;
        return baseLine + "\n" + addition;
    }

    private string PickRequiredLine(CocktailType requiredType)
    {
        switch (requiredType)
        {
            case CocktailType.Bitter: return Pick(BitterRequests);
            case CocktailType.Lemonchello: return Pick(LemonchelloRequests);
            case CocktailType.Absinthe: return Pick(AbsintheRequests);
            default: return "Сделай что-нибудь приличное.";
        }
    }

    private string PickRuleLine(CustomerRuleType ruleType, string preferred)
    {
        switch (ruleType)
        {
            case CustomerRuleType.NoDamagedCards:
                return Pick(NoDamagedAdditions);

            case CustomerRuleType.WantsRainbow:
                return Pick(RainbowAdditions);

            case CustomerRuleType.WantsTriplet:
                return Pick(TripletAdditions);

            case CustomerRuleType.NoAdjacencyBonus:
                return Pick(NoAdjacencyAdditions);

            default:
                return string.Format(Pick(PreferredAdditions), preferred);
        }
    }

    private CustomerRuleType NextRule(CustomerRuleType ruleType)
    {
        switch (ruleType)
        {
            case CustomerRuleType.None: return CustomerRuleType.NoDamagedCards;
            case CustomerRuleType.NoDamagedCards: return CustomerRuleType.WantsRainbow;
            case CustomerRuleType.WantsRainbow: return CustomerRuleType.WantsTriplet;
            case CustomerRuleType.WantsTriplet: return CustomerRuleType.NoAdjacencyBonus;
            default: return CustomerRuleType.None;
        }
    }

    private string Pick(string[] values)
    {
        if (values == null || values.Length == 0)
            return "";

        return values[Random.Range(0, values.Length)];
    }

    private void RefillHandToTarget()
    {
        int missingCards = cardsToDeal - _handViews.Count;

        if (missingCards <= 0)
            return;

        List<CardData> drawnCards = _deck.Draw(missingCards);

        if (drawnCards == null || drawnCards.Count == 0)
            return;

        foreach (CardData card in drawnCards)
            SpawnCardInHand(card);
    }

    private List<CardData> GetCurrentHandCards()
    {
        var cards = new List<CardData>();

        foreach (CardView view in _handViews)
        {
            if (view != null && view.Data != null)
                cards.Add(view.Data);
        }

        return cards;
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

    private void ClearSlots()
    {
        if (rowSlots == null)
            return;

        foreach (CardSlot slot in rowSlots)
        {
            if (slot != null)
                slot.Clear();
        }
    }

    private void ClearHand()
    {
        foreach (CardView view in _handViews)
        {
            if (view != null)
            {
                view.KillTweens();
                Destroy(view.gameObject);
            }
        }

        _handViews.Clear();
    }

    public void OnCardSelectedFromHand(CardView cardView)
    {
        if (_isFinished || cardView == null)
            return;

        if (rowSlots == null || rowSlots.Length == 0)
        {
            Debug.LogError("[CardGameManager] RowSlots не назначены", this);
            return;
        }

        foreach (CardSlot slot in rowSlots)
        {
            if (slot == null || slot.HasCard)
                continue;

            slot.PlaceCard(cardView);
            _handViews.Remove(cardView);

            if (RowFull())
                EvaluateRound();

            return;
        }
    }

    private bool RowFull()
    {
        if (rowSlots == null || rowSlots.Length == 0)
            return false;

        foreach (CardSlot slot in rowSlots)
        {
            if (slot == null || !slot.HasCard)
                return false;
        }

        return true;
    }

    private void EvaluateRound()
    {
        CardData[] cards = new CardData[rowSlots.Length];

        for (int i = 0; i < rowSlots.Length; i++)
            cards[i] = rowSlots[i].PlacedCard;

        RoundScoreResult result = CardGameScoring.CalculateRoundScore(cards, _currentCustomer);

        Debug.Log(
            $"[CocktailScore] Round={_currentRound}, Request={TypeName(_currentCustomer.RequiredType)}, " +
            $"Cards={DescribeCards(cards)}, HandLeft={_handViews.Count}, Score={result.Score}, " +
            $"Failed={result.IsFailed}, Fatal={result.IsFatal}, Reason={result.Reason}"
        );

        _deck.AddManyToDiscard(cards);

        if (!result.IsFailed)
            _totalScore += result.Score;

        if (customerView != null)
            customerView.ShowReaction(BuildReactionText(result));

        DOVirtual.DelayedCall(reactionDelay, () =>
        {
            if (_isFinished)
                return;

            if (result.IsFatal)
            {
                EndGame(false, result.Reason);
                return;
            }

            if (_currentRound >= totalRounds)
            {
                bool won = _totalScore >= winScoreThreshold;
                EndGame(won);
            }
            else
            {
                StartRound();
            }
        });
    }

    private string DescribeCards(CardData[] cards)
    {
        if (cards == null || cards.Length == 0)
            return "пусто";

        var parts = new List<string>();

        foreach (CardData card in cards)
        {
            if (card == null)
            {
                parts.Add("null");
                continue;
            }

            parts.Add($"{card.displayName}/{TypeName(card.cocktailType)}");
        }

        return string.Join(", ", parts);
    }

    private string BuildReactionText(RoundScoreResult result)
    {
        if (result == null)
            return "...";

        if (result.IsFatal)
            return Pick(FatalReactions) + "\n" + result.Reason;

        if (result.IsFailed)
            return Pick(BadReactions) + "\n" + result.Reason;

        if (result.Score >= 12)
            return Pick(GoodReactions);

        return "Сойдёт. Не праздник, но пить можно.";
    }

    private void EndGame(bool won, string reason = "")
    {
        _isFinished = true;
        ClearHand();

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            resultPanel.transform.DOKill();
            resultPanel.transform.localScale = Vector3.zero;
            resultPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        if (resultText != null)
        {
            resultText.text = won
                ? "Лео внимательно смотрит на собранные заказы.\n\n«Ты ещё можешь думать. Хорошо».\n\nКоктейль был из моей стойки. Но заказ был не мой."
                : "Лео хмурится, но всё равно кладёт ключ на стойку.\n\n«Полночь не будет ждать, пока ты придёшь в себя».";

            if (!string.IsNullOrEmpty(reason))
                resultText.text += "\n\n" + reason;
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => FinishGame(won));
        }
    }

    private void FinishGame(bool won)
    {
        GameManager gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("[CardGameManager] GameManager.Instance не найден", this);
            return;
        }

        gameManager.FinishBarMiniGame(won);
    }

    private string TypeName(CocktailType type)
    {
        switch (type)
        {
            case CocktailType.Bitter: return "Биттер";
            case CocktailType.Lemonchello: return "Лимончелло";
            case CocktailType.Absinthe: return "Абсент";
            case CocktailType.Damaged: return "испорченный ингредиент";
            default: return "любой вкус";
        }
    }
}

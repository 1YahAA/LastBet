using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackMarkGameManager : MonoBehaviour
{
    [Header("Настройки")]
    public float roundTime = 60f;
    public int startLives = 3;
    public int maxJokerActivations = 3;
    public float hintPreviewSeconds = 2f;

    [Header("Карты")]
    public BlackMarkCardView cardPrefab;
    public Transform gridParent;

    [Header("Card Visuals")]
    public Sprite baseSprite;
    public Sprite frameSprite;
    public Sprite backSprite;
    public Sprite jokerSprite;
    public List<BlackMarkClueSpriteSet> clueSprites = new();

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI targetText;
    public Image targetImage;
    public GameObject panicOverlay;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button continueButton;

    [Header("Интеграция")]
    public bool addStrategyTokenBeforeFinish = true;

    private readonly List<BlackMarkCardView> _cards = new();

    private BlackMarkState _state;
    private ClueType _correctClue;
    private BlackMarkCardView _firstOpen;
    private BlackMarkCardView _secondOpen;

    private float _timeLeft;
    private int _lives;
    private int _jokerActivations;
    private int _mistakes;
    private int _falsePairsFound;
    private bool _timerRunning;

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (!_timerRunning || _state != BlackMarkState.Gameplay)
            return;

        _timeLeft -= Time.deltaTime;

        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            UpdateUI();
            Lose("Время вышло");
            return;
        }

        UpdateTimerUI();
    }

    public void StartGame()
    {
        _state = BlackMarkState.ShowHint;

        _timeLeft = roundTime;
        _lives = startLives;
        _jokerActivations = 0;
        _mistakes = 0;
        _falsePairsFound = 0;

        _firstOpen = null;
        _secondOpen = null;
        _timerRunning = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (panicOverlay != null)
            panicOverlay.SetActive(false);

        GenerateRound();
        UpdateUI();

        StartCoroutine(ShowHintThenStart());
    }

    private IEnumerator ShowHintThenStart()
    {
        if (statusText != null)
            statusText.text = "Запомни улику.";

        RevealCorrectPairForPreview(true);

        yield return new WaitForSeconds(hintPreviewSeconds);

        RevealCorrectPairForPreview(false);

        if (statusText != null)
            statusText.text = "Найди Чёрную метку.";

        _state = BlackMarkState.Gameplay;
        _timerRunning = true;
    }

    private void GenerateRound()
    {
        ClearGrid();

        _correctClue = RandomClue();

        if (hintText != null)
            hintText.text = GenerateHint(_correctClue);

        if (targetText != null)
            targetText.text = $"Цель: {ClueName(_correctClue)}";

        if (targetImage != null)
        {
            targetImage.sprite = GetSprite(_correctClue);
            targetImage.enabled = targetImage.sprite != null;
            targetImage.preserveAspect = true;
        }

        var instances = new List<BlackMarkCardInstance>();

        AddPair(instances, _correctClue);

        var falseClues = new List<ClueType>
        {
            ClueType.BlackMark,
            ClueType.Mask,
            ClueType.Glass,
            ClueType.Key
        };

        falseClues.Remove(_correctClue);
        Shuffle(falseClues);

        for (int i = 0; i < 3; i++)
            AddPair(instances, falseClues[i]);

        instances.Add(new BlackMarkCardInstance(ClueType.Joker, true, jokerSprite));

        Shuffle(instances);

        foreach (var instance in instances)
        {
            BlackMarkCardView view = Instantiate(cardPrefab, gridParent);
            view.Init(instance, this, backSprite, baseSprite, frameSprite);
            _cards.Add(view);
        }
    }

    private void AddPair(List<BlackMarkCardInstance> list, ClueType clue)
    {
        Sprite sprite = GetSprite(clue);

        list.Add(new BlackMarkCardInstance(clue, false, sprite));
        list.Add(new BlackMarkCardInstance(clue, false, sprite));
    }

    private void ClearGrid()
    {
        foreach (var card in _cards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        _cards.Clear();
    }

    public void OnCardClicked(BlackMarkCardView card)
    {
        if (_state != BlackMarkState.Gameplay || card == null)
            return;

        if (_firstOpen != null && _secondOpen != null)
            return;

        card.Open();

        if (card.Data.IsJoker)
        {
            StartCoroutine(HandleJoker(card));
            return;
        }

        if (_firstOpen == null)
        {
            _firstOpen = card;
            return;
        }

        if (_secondOpen == null && card != _firstOpen)
        {
            _secondOpen = card;
            StartCoroutine(CheckOpenedPair());
        }
    }

    private IEnumerator CheckOpenedPair()
    {
        yield return new WaitForSeconds(0.35f);

        if (_firstOpen == null || _secondOpen == null)
            yield break;

        bool sameClue = _firstOpen.Data.ClueType == _secondOpen.Data.ClueType;

        if (!sameClue)
        {
            _mistakes++;
            LoseLife("Ошибка");

            _firstOpen.Shake();
            _secondOpen.Shake();

            yield return new WaitForSeconds(0.25f);

            _firstOpen.Close();
            _secondOpen.Close();

            ResetSelection();
            yield break;
        }

        if (_firstOpen.Data.ClueType == _correctClue)
        {
            _firstOpen.LockOpen();
            _secondOpen.LockOpen();
            Win();
            yield break;
        }

        _falsePairsFound++;

        if (statusText != null)
            statusText.text = "Ложный след.";

        _firstOpen.LockOpen();
        _secondOpen.LockOpen();

        ResetSelection();
    }

    private IEnumerator HandleJoker(BlackMarkCardView jokerCard)
    {
        _state = BlackMarkState.JokerEvent;
        _jokerActivations++;

        if (statusText != null)
            statusText.text = "Джокер вмешался.";

        LoseLife(null);

        yield return new WaitForSeconds(0.25f);

        JokerEffectType effect = RollJokerEffect();

        switch (effect)
        {
            case JokerEffectType.Fog:
                ApplyFog();
                break;

            case JokerEffectType.Swap:
                ApplySwap();
                break;

            case JokerEffectType.FalseLead:
                yield return StartCoroutine(ApplyFalseLead());
                break;

            case JokerEffectType.Panic:
                yield return StartCoroutine(ApplyPanic());
                break;
        }

        jokerCard.Close();
        ResetSelection();

        yield return new WaitForSeconds(0.4f);

        if (_lives <= 0)
        {
            Lose("Закончились жизни");
            yield break;
        }

        if (_jokerActivations >= maxJokerActivations)
        {
            Lose("Джокер сорвал расследование");
            yield break;
        }

        _state = BlackMarkState.Gameplay;
        UpdateUI();
    }

    private JokerEffectType RollJokerEffect()
    {
        float roll = Random.value;

        if (roll < 0.35f)
            return JokerEffectType.Fog;

        if (roll < 0.65f)
            return JokerEffectType.Swap;

        if (roll < 0.85f)
            return JokerEffectType.FalseLead;

        return JokerEffectType.Panic;
    }

    private void ApplyFog()
    {
        int closed = 0;

        foreach (var card in _cards)
        {
            if (card == null || card.Data.IsJoker || !card.IsOpen || card.IsLocked)
                continue;

            card.Close();
            closed++;

            if (closed >= 3)
                break;
        }

        if (statusText != null)
            statusText.text = "Туман скрыл часть улик.";
    }

    private void ApplySwap()
    {
        var candidates = new List<BlackMarkCardView>();

        foreach (var card in _cards)
        {
            if (card != null && !card.IsOpen && !card.IsLocked)
                candidates.Add(card);
        }

        if (candidates.Count < 2)
            return;

        int a = Random.Range(0, candidates.Count);
        int b = Random.Range(0, candidates.Count);

        while (b == a)
            b = Random.Range(0, candidates.Count);

        int indexA = candidates[a].transform.GetSiblingIndex();
        int indexB = candidates[b].transform.GetSiblingIndex();

        candidates[a].transform.SetSiblingIndex(indexB);
        candidates[b].transform.SetSiblingIndex(indexA);

        if (statusText != null)
            statusText.text = "Карты поменялись местами.";
    }

    private IEnumerator ApplyFalseLead()
    {
        if (statusText != null)
            statusText.text = "Кажется, это почти она...";

        var falseCards = new List<BlackMarkCardView>();

        foreach (var card in _cards)
        {
            if (card == null || card.Data.IsJoker || card.Data.ClueType == _correctClue)
                continue;

            falseCards.Add(card);
        }

        if (falseCards.Count > 0)
        {
            BlackMarkCardView card = falseCards[Random.Range(0, falseCards.Count)];
            card.Open();

            yield return new WaitForSeconds(0.6f);

            if (!card.IsLocked)
                card.Close();
        }
    }

    private IEnumerator ApplyPanic()
    {
        if (panicOverlay != null)
            panicOverlay.SetActive(true);

        if (statusText != null)
            statusText.text = "Паника.";

        yield return new WaitForSeconds(5f);

        if (panicOverlay != null)
            panicOverlay.SetActive(false);
    }

    private void LoseLife(string message)
    {
        _lives--;

        if (!string.IsNullOrEmpty(message) && statusText != null)
            statusText.text = message;

        UpdateUI();

        if (_lives <= 0)
            Lose("Закончились жизни");
    }

    private void Win()
    {
        _state = BlackMarkState.Win;
        _timerRunning = false;

        ShowResult(true, "Улика найдена");
    }

    private void Lose(string reason)
    {
        if (_state == BlackMarkState.Lose || _state == BlackMarkState.Win)
            return;

        _state = BlackMarkState.Lose;
        _timerRunning = false;

        ShowResult(false, reason);
    }

    private void ShowResult(bool won, string reason)
    {
        BlackMarkStrategy strategy = DetermineStrategy(won);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text =
                $"{(won ? "Победа" : "Поражение")}\n" +
                $"{reason}\n" +
                $"Стиль: {StrategyName(strategy)}";
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => FinishMiniGame(won, strategy));
        }
    }

    private BlackMarkStrategy DetermineStrategy(bool won)
    {
        if (won && _mistakes <= 1 && _jokerActivations == 0 && _timeLeft >= roundTime * 0.35f)
            return BlackMarkStrategy.Analysis;

        if (_jokerActivations >= 2 || _falsePairsFound >= 2)
            return BlackMarkStrategy.Revolt;

        return BlackMarkStrategy.Obedience;
    }

    private void FinishMiniGame(bool won, BlackMarkStrategy strategy)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[BlackMarkGameManager] GameManager.Instance не найден");
            return;
        }

        if (addStrategyTokenBeforeFinish && GameManager.Instance.gameState != null)
            GameManager.Instance.gameState.AddToken(StrategyToToken(strategy));

        GameManager.Instance.FinishMiniGame(won);
    }

    private TokenType StrategyToToken(BlackMarkStrategy strategy)
    {
        return strategy switch
        {
            BlackMarkStrategy.Revolt => TokenType.Revolt,
            BlackMarkStrategy.Obedience => TokenType.Obedience,
            BlackMarkStrategy.Analysis => TokenType.Analysis,
            _ => TokenType.Analysis
        };
    }

    private void ResetSelection()
    {
        _firstOpen = null;
        _secondOpen = null;
    }

    private void RevealCorrectPairForPreview(bool open)
    {
        foreach (var card in _cards)
        {
            if (card == null || card.Data == null)
                continue;

            if (card.Data.ClueType != _correctClue)
                continue;

            if (open)
                card.Open();
            else
                card.Close();
        }
    }

    private void UpdateUI()
    {
        UpdateTimerUI();

        if (livesText != null)
            livesText.text = $"Жизни: {_lives}";
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = $"Время: {Mathf.CeilToInt(_timeLeft)}";
    }

    private ClueType RandomClue()
    {
        return (ClueType)Random.Range(0, 4);
    }

    private Sprite GetSprite(ClueType clue)
    {
        if (clue == ClueType.Joker)
            return jokerSprite;

        foreach (var item in clueSprites)
        {
            if (item != null && item.clueType == clue)
                return item.sprite;
        }

        return null;
    }

    private string GenerateHint(ClueType clue)
    {
        return clue switch
        {
            ClueType.BlackMark => "Ищи настоящую Чёрную метку.",
            ClueType.Mask => "Я запомнила маску.",
            ClueType.Glass => "На столе остался бокал.",
            ClueType.Key => "Ключ был у того, кто знал проход.",
            _ => "Найди настоящую улику."
        };
    }

    private string ClueName(ClueType clue)
    {
        return clue switch
        {
            ClueType.BlackMark => "Чёрная метка",
            ClueType.Mask => "Маска",
            ClueType.Glass => "Бокал",
            ClueType.Key => "Ключ",
            ClueType.Joker => "Джокер",
            _ => "?"
        };
    }

    private string StrategyName(BlackMarkStrategy strategy)
    {
        return strategy switch
        {
            BlackMarkStrategy.Revolt => "Бунт",
            BlackMarkStrategy.Obedience => "Послушание",
            BlackMarkStrategy.Analysis => "Анализ",
            _ => "Анализ"
        };
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
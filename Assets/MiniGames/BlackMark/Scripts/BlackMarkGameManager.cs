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
    public float openedPairCheckDelay = 0.35f;
    public float wrongPairCloseDelay = 0.35f;
    public float panicDuration = 1.4f;

    [Header("Joker Feedback")]
    public float jokerMessageDelay = 0.8f;
    public int jokerMoveEveryTurns = 3;

    private int _playerTurns;

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
    public GameObject panicOverlay;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button continueButton;

    [Header("Интеграция")]
    public bool addStrategyTokenBeforeFinish = true;

    private readonly List<BlackMarkCardView> _cards = new();

    private BlackMarkState _state = BlackMarkState.Idle;
    private ClueType _correctClue;
    private BlackMarkCardView _firstOpen;
    private BlackMarkCardView _secondOpen;

    private float _timeLeft;
    private int _lives;
    private int _jokerActivations;
    private int _mistakes;
    private int _falsePairsFound;
    private int _wrongPairs;
    private bool _timerRunning;
    private bool _isResolving;

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

        _playerTurns = 0;

        _timeLeft = roundTime;
        _lives = startLives;
        _jokerActivations = 0;
        _mistakes = 0;
        _falsePairsFound = 0;
        _wrongPairs = 0;

        _firstOpen = null;
        _secondOpen = null;
        _timerRunning = false;
        _isResolving = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (panicOverlay != null)
            panicOverlay.SetActive(false);

        SetMainUiVisible(true);

        GenerateRound();
        UpdateUI();

        StartCoroutine(ShowHintThenStart());
    }

    private IEnumerator ShowHintThenStart()
    {
        if (statusText != null)
            statusText.text = "Изучи подсказку.";

        yield return new WaitForSeconds(hintPreviewSeconds);

        if (statusText != null)
            statusText.text = "Найди настоящую пару.";

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
        if (_state != BlackMarkState.Gameplay || card == null || _isResolving)
            return;

        if (card.Data == null)
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

        if (!card.Data.IsJoker)
        {
            _playerTurns++;

            if (_playerTurns % jokerMoveEveryTurns == 0)
                MoveJokerToRandomClosedPosition();
        }
    }

    private void MoveJokerToRandomClosedPosition()
    {
        BlackMarkCardView joker = null;
        var candidates = new List<BlackMarkCardView>();

        foreach (var card in _cards)
        {
            if (card == null || card.Data == null)
                continue;

            if (card.Data.IsJoker)
            {
                joker = card;
                continue;
            }

            if (!card.IsOpen && !card.IsLocked)
                candidates.Add(card);
        }

        if (joker == null || candidates.Count == 0)
            return;

        BlackMarkCardView target = candidates[Random.Range(0, candidates.Count)];

        int jokerIndex = joker.transform.GetSiblingIndex();
        int targetIndex = target.transform.GetSiblingIndex();

        joker.transform.SetSiblingIndex(targetIndex);
        target.transform.SetSiblingIndex(jokerIndex);

        joker.ForceCloseFromJoker();
    }
    
    private IEnumerator CheckOpenedPair()
    {
        _isResolving = true;
        _state = BlackMarkState.CheckWin;

        yield return new WaitForSeconds(openedPairCheckDelay);

        if (_firstOpen == null || _secondOpen == null)
        {
            _isResolving = false;
            _state = BlackMarkState.Gameplay;
            yield break;
        }

        bool sameClue = _firstOpen.Data.ClueType == _secondOpen.Data.ClueType;

        if (!sameClue)
        {
            _mistakes++;
            _wrongPairs++;

            if (statusText != null)
                statusText.text = "Улики не совпали.";

            _firstOpen.Shake();
            _secondOpen.Shake();

            LoseLife(null);

            yield return new WaitForSeconds(wrongPairCloseDelay);

            if (_firstOpen != null)
                _firstOpen.ForceCloseFromJoker();

            if (_secondOpen != null)
                _secondOpen.ForceCloseFromJoker();

            ResetSelection();

            if (_state != BlackMarkState.Lose)
                _state = BlackMarkState.Gameplay;

            _isResolving = false;
            yield break;
        }

        if (_firstOpen.Data.ClueType == _correctClue)
        {
            _firstOpen.LockOpen();
            _secondOpen.LockOpen();

            _isResolving = false;
            Win();
            yield break;
        }

        _falsePairsFound++;

        if (statusText != null)
            statusText.text = "Ложный след. Это не настоящая улика.";

        _firstOpen.LockOpen();
        _secondOpen.LockOpen();

        ResetSelection();

        if (_state != BlackMarkState.Lose)
            _state = BlackMarkState.Gameplay;

        _isResolving = false;
    }

    private IEnumerator HandleJoker(BlackMarkCardView jokerCard)
    {
        _isResolving = true;
        _state = BlackMarkState.JokerEvent;
        _jokerActivations++;

        if (statusText != null)
            statusText.text = "Джокер вмешался.";

        yield return new WaitForSeconds(0.25f);

        JokerEffectType effect = RollJokerEffect();

        switch (effect)
        {
            case JokerEffectType.Fog:
                yield return StartCoroutine(ApplyFog());
                break;

            case JokerEffectType.Swap:
                yield return StartCoroutine(ApplySwap());
                break;

            case JokerEffectType.FalseLead:
                yield return StartCoroutine(ApplyFalseLead());
                break;

            case JokerEffectType.Panic:
                yield return StartCoroutine(ApplyPanic());
                break;
        }

        if (jokerCard != null)
            jokerCard.ForceCloseFromJoker();

        ResetSelection();

        yield return new WaitForSeconds(0.25f);

        if (_jokerActivations >= maxJokerActivations)
        {
            Lose("Джокер сорвал расследование");
            _isResolving = false;
            yield break;
        }

        if (_lives <= 0)
        {
            Lose("Закончились жизни");
            _isResolving = false;
            yield break;
        }

        if (_state != BlackMarkState.Lose && _state != BlackMarkState.Win)
        {
            MoveJokerToRandomClosedPosition();
            _state = BlackMarkState.Gameplay;
            _isResolving = false;
            UpdateUI();
        }
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

    private IEnumerator ApplyFog()
    {
        if (statusText != null)
            statusText.text = "Джокер напустил туман.";

        if (panicOverlay != null)
            panicOverlay.SetActive(true);

        yield return new WaitForSeconds(0.7f);

        var candidates = new List<BlackMarkCardView>();

        foreach (var card in _cards)
        {
            if (card == null || card.Data == null || card.Data.IsJoker)
                continue;

            if (card.IsOpen && !card.IsLocked)
                candidates.Add(card);
        }

        Shuffle(candidates);

        int count = Mathf.Min(Random.Range(2, 4), candidates.Count);

        for (int i = 0; i < count; i++)
        {
            candidates[i].Shake();
            candidates[i].ForceCloseFromJoker();
            yield return new WaitForSeconds(0.15f);
        }

        if (panicOverlay != null)
            panicOverlay.SetActive(false);

        if (statusText != null)
            statusText.text = count > 0
                ? $"Туман скрыл улик: {count}."
                : "Туман скрыл поле, но открытых улик не было.";

        yield return new WaitForSeconds(0.8f);
    }

    private IEnumerator ApplySwap()
    {
        if (statusText != null)
            statusText.text = "Джокер меняет карты местами.";

        yield return new WaitForSeconds(jokerMessageDelay);

        var candidates = new List<BlackMarkCardView>();

        foreach (var card in _cards)
        {
            if (card != null && !card.IsOpen && !card.IsLocked)
                candidates.Add(card);
        }

        if (candidates.Count < 2)
        {
            if (statusText != null)
                statusText.text = "Подмена не удалась.";

            yield return new WaitForSeconds(jokerMessageDelay);
            yield break;
        }

        int a = Random.Range(0, candidates.Count);
        int b = Random.Range(0, candidates.Count);

        while (b == a)
            b = Random.Range(0, candidates.Count);

        candidates[a].Shake();
        candidates[b].Shake();

        yield return new WaitForSeconds(0.3f);

        int indexA = candidates[a].transform.GetSiblingIndex();
        int indexB = candidates[b].transform.GetSiblingIndex();

        candidates[a].transform.SetSiblingIndex(indexB);
        candidates[b].transform.SetSiblingIndex(indexA);

        if (statusText != null)
            statusText.text = "Две закрытые карты поменялись местами.";

        yield return new WaitForSeconds(jokerMessageDelay);
    }

    private IEnumerator ApplyFalseLead()
    {
        if (statusText != null)
            statusText.text = "Джокер показывает ложную улику.";

        yield return new WaitForSeconds(jokerMessageDelay);

        var falseCards = new List<BlackMarkCardView>();

        foreach (var card in _cards)
        {
            if (card == null || card.Data == null)
                continue;

            if (card.Data.IsJoker || card.Data.ClueType == _correctClue)
                continue;

            if (card.IsLocked)
                continue;

            falseCards.Add(card);
        }

        if (falseCards.Count == 0)
        {
            if (statusText != null)
                statusText.text = "Ложный след не найден.";

            yield return new WaitForSeconds(jokerMessageDelay);
            yield break;
        }

        ClueType falseClue = falseCards[Random.Range(0, falseCards.Count)].Data.ClueType;

        var pair = new List<BlackMarkCardView>();

        foreach (var card in falseCards)
        {
            if (card.Data.ClueType == falseClue)
                pair.Add(card);
        }

        foreach (var card in pair)
        {
            card.Open();
            card.HighlightFalseLead();
        }

        if (statusText != null)
            statusText.text = $"Ложная улика: {ClueName(falseClue)}.";

        yield return new WaitForSeconds(1.4f);

        foreach (var card in pair)
        {
            if (card != null && !card.IsLocked)
                card.ForceCloseFromJoker();
        }

        if (statusText != null)
            statusText.text = "Не доверяй подсказке Джокера.";

        yield return new WaitForSeconds(jokerMessageDelay);
    }

    private IEnumerator ApplyPanic()
    {
        if (statusText != null)
            statusText.text = "Паника. Подсказки скрыты.";

        if (panicOverlay != null)
            panicOverlay.SetActive(true);

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (livesText != null)
            livesText.gameObject.SetActive(false);

        if (hintText != null)
            hintText.gameObject.SetActive(false);

        if (targetText != null)
            targetText.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        if (timerText != null)
            timerText.gameObject.SetActive(true);

        if (livesText != null)
            livesText.gameObject.SetActive(true);

        if (hintText != null)
            hintText.gameObject.SetActive(true);

        if (targetText != null)
            targetText.gameObject.SetActive(true);

        if (panicOverlay != null)
            panicOverlay.SetActive(false);

        if (statusText != null)
            statusText.text = "Паника прошла.";

        yield return new WaitForSeconds(jokerMessageDelay);
    }

    private void LoseLife(string message)
    {
        _lives = Mathf.Max(0, _lives - 1);

        if (!string.IsNullOrEmpty(message) && statusText != null)
            statusText.text = message;

        UpdateUI();

        if (_lives <= 0)
            Lose("Закончились жизни");
    }

    private void Win()
    {
        if (_state == BlackMarkState.Lose)
            return;

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
                $"{reason}";
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => FinishMiniGame(won, strategy));
        }
    }

    private BlackMarkStrategy DetermineStrategy(bool won)
    {
        float timeRatio = roundTime <= 0f ? 0f : _timeLeft / roundTime;

        if (won && _mistakes <= 1 && _jokerActivations == 0 && timeRatio >= 0.3f)
            return BlackMarkStrategy.Analysis;

        if (_jokerActivations >= 2 || _falsePairsFound >= 2 || _mistakes >= 3)
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
                card.ForceCloseFromJoker();
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

    private void SetMainUiVisible(bool visible)
    {
        if (timerText != null)
            timerText.gameObject.SetActive(visible);

        if (livesText != null)
            livesText.gameObject.SetActive(visible);

        if (hintText != null)
            hintText.gameObject.SetActive(visible);

        if (targetText != null)
            targetText.gameObject.SetActive(visible);

        if (statusText != null)
            statusText.gameObject.SetActive(true);
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
            ClueType.BlackMark => "Среди подделок есть настоящая Чёрная метка.",
            ClueType.Mask => "Я запомнила маску в толпе.",
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
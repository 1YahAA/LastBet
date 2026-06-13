using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Главный координатор мини-игры «Последняя ставка».
// Обязанности:
// — запуск и остановка раунда;
// — передача событий карт в модель раунда (LastBetRoundModel);
// — обновление UI по состоянию модели;
// — запуск панели выбора подозреваемого;
// — передача результата в GameState
public class LastBetMiniGameManager : MonoBehaviour
{
    [Header("Карты")]
    [SerializeField] private LastBetCardView cardPrefab;
    [SerializeField] private Transform cardParent;
    [SerializeField] private int cardsOnTable = 6;
    [SerializeField] private List<LastBetCardData> deckTemplates = new List<LastBetCardData>();

    [Header("Спрайты карт")]
    [SerializeField] private Sprite cardBaseSprite;
    [SerializeField] private Sprite cardBackSprite;
    [SerializeField] private Sprite cardFrameSprite;
    [SerializeField] private Sprite jokerFullCardSprite;

    [Header("Панель улик")]
    [SerializeField] private LastBetEvidencePanel evidencePanel;
    [SerializeField] private Transform evidenceContentParent;
    [SerializeField] private LastBetClueSlotView clueSlotPrefab;
    [SerializeField] private LastBetTooltip evidenceTooltip;

    [Header("Кнопки")]
    [SerializeField] private Button openCardButton;
    [SerializeField] private Button takeInfoButton;
    [SerializeField] private Button continueButton;

    [Header("Тексты")]
    [SerializeField] private TMP_Text croupierLineText;
    [SerializeField] private TMP_Text infoLineText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text suspicionText;

    [Header("Панели")]
    [SerializeField] private LastBetRulesPanel rulesPanelController;
    [SerializeField] private LastBetSuspectPanel suspectPanelController;
    [SerializeField] private GameObject suspectPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultBodyText;

    [Header("Кнопки подозреваемых")]
    [SerializeField] private LastBetChoiceButton helgaChoice;
    [SerializeField] private LastBetChoiceButton victorChoice;
    [SerializeField] private LastBetChoiceButton marieChoice;

    [Header("Параметры раунда")]
    [SerializeField] private float roundTime = 150f;
    [SerializeField] private int minInformationToChoose = 3;
    [SerializeField] private int suspicionLimit = 5;

    [Header("Индикаторы подозрения")]
    [Tooltip("Красные огоньки/кружки подозрения. Можно не заполнять — менеджер попробует найти объект SuspicionCircles.")]
    [SerializeField] private Image[] suspicionIndicators;
    [SerializeField] private Sprite suspicionEmptySprite;
    [SerializeField] private Sprite suspicionFilledSprite;
    [SerializeField] private Color suspicionEmptyColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color suspicionFilledColor = Color.white;

    [Header("Туториал")]
    [SerializeField] private bool playIntroTutorial = true;

    [Tooltip("Перетащи объект UITutorialManager из сцены.")]
    [SerializeField] private MonoBehaviour githubTutorialManager;

    [Tooltip("Имя SequenceID туториала. Обычно LastBetIntro.")]
    [SerializeField] private string introTutorialSequenceName = "LastBetIntro";

    // Модель раунда — единственный источник правды о времени, подозрении и сведениях.
    // Менеджер не дублирует эти счётчики у себя.
    private readonly LastBetRoundModel _round = new LastBetRoundModel();

    private readonly List<LastBetCardView> _cardViews = new List<LastBetCardView>();

    private int _nextCardIndex;
    private bool _introTutorialStarted;

    // Результат передаётся в GameState после завершения мини-игры.
    private LastBetSuspect _selectedSuspect = LastBetSuspect.None;
    private LastBetStrategyToken _resultToken = LastBetStrategyToken.None;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        AutoBindSceneReferences();
        WireButtons();
        ConfigureSubPanels();
        HideRuntimePanels();
    }

    private void Start()
    {
        // Показываем панель правил если она есть в сцене.
        // После нажатия «Начать» запускается туториал, и только потом — раунд.
        // Если панели правил нет — сразу туториал или раунд.
        if (rulesPanelController != null && rulesPanelController.Exists)
            rulesPanelController.Show(OnRulesAccepted, minInformationToChoose, suspicionLimit);
        else
            OnRulesAccepted();
    }

    private void Update()
    {
        // Тик времени делегируем модели. Менеджер только читает результат.
        // Пока туториал не завершён — модель не активна, тик ничего не делает.
        if (!_round.Active || _round.ChoiceOpened)
            return;

        _round.Tick(Time.deltaTime);

        if (_round.TimeLeft <= 0f)
        {
            EndRoundByTime();
            return;
        }

        RefreshTopUi();
    }

    // ── Запуск ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Вызывается когда игрок закрыл панель правил.
    /// Запускает туториал. Таймер ещё не идёт.
    /// </summary>
    private void OnRulesAccepted()
    {
        if (rulesPanelController != null)
            rulesPanelController.Hide();

        // Туториал показывается до старта раунда.
        // StartRound вызывается только после его завершения.
        if (playIntroTutorial)
            StartGithubTutorialIfNeeded(onCompleted: StartRound);
        else
            StartRound();
    }

    /// <summary>
    /// Инициализирует раунд и запускает таймер.
    /// Вызывается только после туториала — таймер не идёт во время обучения.
    /// </summary>
    public void StartRound()
    {
        _nextCardIndex = 0;
        _selectedSuspect = LastBetSuspect.None;
        _resultToken = LastBetStrategyToken.None;

        // Модель сбрасывает все счётчики и запускает таймер.
        // До этого вызова время не идёт.
        _round.Start(roundTime);

        _cardViews.Clear();
        LastBetUiUtility.ClearChildren(cardParent);

        if (evidencePanel != null)
            evidencePanel.Clear();

        if (suspectPanelController != null)
            suspectPanelController.Hide();

        LastBetUiUtility.SetPanelVisible(resultPanel, false);

        List<LastBetCardData> deck = LastBetDeckService.BuildDeck(deckTemplates);
        BuildCardsOnTable(deck);

        SetCroupierLine("Карты уже на столе. Не ищите один прямой ответ — ищите след, которому Эвелин готова поверить.");
        SetInfoLine("Откройте карту. Когда версия начнёт складываться, остановитесь и сделайте вывод.");

        RefreshTopUi();
        RefreshButtonState();
    }

    // ── Карты ────────────────────────────────────────────────────────────────

    private void BuildCardsOnTable(List<LastBetCardData> deck)
    {
        if (cardPrefab == null || cardParent == null)
        {
            Debug.LogError("[LastBet] Card Prefab или Card Parent не назначены в Inspector.");
            return;
        }

        int count = Mathf.Min(cardsOnTable, deck.Count);
        for (int i = 0; i < count; i++)
        {
            LastBetCardView view = Instantiate(cardPrefab, cardParent);
            view.gameObject.SetActive(true);
            view.Setup(deck[i], cardBaseSprite, cardBackSprite, cardFrameSprite, jokerFullCardSprite);
            _cardViews.Add(view);
        }
    }

    private void OpenNextCard()
    {
        if (!_round.Active || _round.ChoiceOpened)
            return;

        if (_nextCardIndex >= _cardViews.Count)
        {
            SetInfoLine("Карты на столе закончились. Теперь нужно сделать вывод.");
            RefreshButtonState();
            return;
        }

        LastBetCardView cardView = _cardViews[_nextCardIndex];
        _nextCardIndex++;

        if (cardView == null || cardView.Opened)
            return;

        cardView.ShowOpened();
        ApplyCard(cardView.Data);

        RefreshTopUi();
        RefreshButtonState();
    }

    private void ApplyCard(LastBetCardData data)
    {
        if (data == null)
            return;

        // Модель обновляет счётчики и возвращает результат применения карты.
        LastBetCardApplyResult result = _round.ApplyCard(data, suspicionLimit);

        SetCroupierLine(string.IsNullOrWhiteSpace(data.croupierLine)
            ? "Карта молчит. Но молчание за этим столом редко бывает пустым."
            : data.croupierLine);

        if (result.IsJoker)
        {
            ApplyJokerEffect();
        }
        else
        {
            // Обычная карта добавляет улику в панель.
            if (result.AddedEvidence && evidencePanel != null)
                evidencePanel.AddEvidence(data);

            UpdateInfoLineAfterCard();
        }

        // Подозрение достигло лимита — раунд завершается принудительно.
        if (result.OpenChoiceBecauseSuspicionLimit)
            EndRoundBySuspicion();
    }

    private void ApplyJokerEffect()
    {
        // Джокер не добавляет улику, но затуманивает последнюю найденную.
        // Если улик ещё нет — даём отдельный текст, чтобы игрок понял что произошло.
        if (evidencePanel != null && evidencePanel.VisibleCount > 0)
        {
            evidencePanel.MarkLastEvidenceAsUnstable();
            SetInfoLine("Джокер вмешался. Последний найденный след теперь под сомнением — его могли подбросить намеренно.");
        }
        else
        {
            // Улик ещё нет — Джокер просто усиливает напряжение.
            SetInfoLine("Джокер появился раньше улик. Кто-то следит за Эвелин с самого начала.");
        }
    }

    private void UpdateInfoLineAfterCard()
    {
        // Все тексты читаем из модели — не из локальных переменных.
        if (_round.Suspicion >= suspicionLimit - 1)
        {
            SetInfoLine("Обстановка стала опасной. Ещё одна ошибка — и внимание переключится на Эвелин.");
            return;
        }

        if (_round.Information < minInformationToChoose)
        {
            SetInfoLine($"Сведений пока мало: {_round.Information}/{minInformationToChoose}. Откройте ещё карту.");
            return;
        }

        SetInfoLine("Версия уже может сложиться. Можно рискнуть ещё одной картой или сделать вывод сейчас.");
    }

    // ── Решение ──────────────────────────────────────────────────────────────

    private void TryStartDecision()
    {
        if (!_round.Active || _round.ChoiceOpened)
            return;

        if (!_round.HasEnoughInformation(minInformationToChoose))
        {
            SetInfoLine($"Эвелин пока не готова сделать вывод. Нужно хотя бы {minInformationToChoose} сведения.");
            return;
        }

        StartDecision();
    }

    private void StartDecision()
    {
        // Модель фиксирует что выбор открыт — таймер останавливается.
        _round.OpenChoice();

        SetCroupierLine("Теперь не ищите доказательство. Решите, как Эвелин прочитала этот след.");
        SetInfoLine("Выберите версию: Хэльга, Виктор или Мари.");

        RefreshButtonState();

        if (suspectPanelController != null)
        {
            suspectPanelController.Initialize(OnSuspectSelected);
            suspectPanelController.Show();
        }
        else
        {
            LastBetUiUtility.SetPanelVisible(suspectPanel, true);
        }
    }

    private void OnSuspectSelected(LastBetSuspect suspect)
    {
        _selectedSuspect = suspect;
        _round.SelectSuspect(suspect);

        // Токен определяется через ResultResolver — единая точка этой логики.
        _resultToken = LastBetResultResolver.ResolveToken(
            suspect,
            _round.Information,
            _round.Suspicion,
            suspicionLimit
        );

        suspectPanelController.SetSelected(suspect);
        ShowResultPanel();
    }

    private void ShowResultPanel()
    {
        LastBetUiUtility.SetPanelVisible(resultPanel, true);

        // Панель результата не пересказывает выбор игроку —
        // она даёт Эвелин внутреннюю реакцию на принятое решение.
        // Текст зависит от того, кого выбрали.
        if (resultTitleText != null)
            resultTitleText.text = BuildResultTitle();

        if (resultBodyText != null)
            resultBodyText.text = BuildResultBody();

        SetInfoLine("Решение принято. Продолжите сюжет.");
    }

    private string BuildResultTitle()
    {
        // Заголовок — внутреннее состояние Эвелин, не оценка выбора.
        return _selectedSuspect switch
        {
            LastBetSuspect.Helga  => "Эвелин сделала выбор",
            LastBetSuspect.Victor => "Эвелин сделала выбор",
            LastBetSuspect.Marie  => "Эвелин сделала выбор",
            _                     => "Партия завершена"
        };
    }

    private string BuildResultBody()
    {
        // Текст передаёт ощущение Эвелин — без оценки правильности.
        // Игрок не знает верен ли его выбор, только что Эвелин на это решилась.
        return _selectedSuspect switch
        {
            LastBetSuspect.Helga =>
                "Эвелин смотрит на карты. Следы ведут к Хэльге — или она хочет, чтобы так казалось.\n\n" +
                "Что-то не даёт покоя. Но выбор сделан.",

            LastBetSuspect.Victor =>
                "Виктор слишком очевиден. Эвелин знает это — и всё равно выбирает его след.\n\n" +
                "Иногда самый заметный след оказывается настоящим.",

            LastBetSuspect.Marie =>
                "Мари почти не оставляет следов. Именно поэтому Эвелин смотрит в её сторону.\n\n" +
                "Тихие люди в кабаре знают больше всех.",

            _ =>
                "Эвелин не успела собрать устойчивую версию. " +
                "Останется только ощущение чужого вмешательства."
        };
    }

    // ── Завершение по внешним условиям ───────────────────────────────────────

    private void EndRoundBySuspicion()
    {
        // Подозрение достигло предела — игрок не успел сделать вывод.
        // Токен Obedience: Эвелин спасовала под давлением.
        _round.OpenChoice();
        _resultToken = LastBetStrategyToken.Obedience;

        SetCroupierLine("За столом стало слишком тихо. Теперь смотрят уже не на карты, а на Эвелин.");
        SetInfoLine("Подозрение достигло предела. Версия осталась неполной.");

        LastBetUiUtility.SetPanelVisible(resultPanel, true);

        if (resultTitleText != null)
            resultTitleText.text = "Слишком много внимания";

        if (resultBodyText != null)
            resultBodyText.text =
                "Эвелин пыталась увидеть больше, но кабаре заметило её интерес. " +
                "Некоторые выводы придётся сделать уже после этой партии.";

        RefreshButtonState();
    }

    private void EndRoundByTime()
    {
        // Время истекло — токен Analysis: Эвелин наблюдала, но не успела решить.
        _round.OpenChoice();
        _resultToken = LastBetStrategyToken.Analysis;

        SetCroupierLine("Время партии вышло. Некоторые следы так и остались между строк.");
        SetInfoLine("Время закончилось. Продолжите с тем, что успели заметить.");

        LastBetUiUtility.SetPanelVisible(resultPanel, true);

        if (resultTitleText != null)
            resultTitleText.text = "Партия завершена";

        if (resultBodyText != null)
            resultBodyText.text =
                "Эвелин не получила полной картины. " +
                "Но даже неполные сведения иногда меняют то, как смотришь на людей вокруг.";

        RefreshButtonState();
    }

    // ── Передача результата в GameState ──────────────────────────────────────

    private void ContinueAfterResult()
    {
        ApplyResultToGameState();

        Debug.Log(
            "[LastBet] Мини-игра завершена. " +
            $"Токен={_resultToken}, Подозреваемый={_selectedSuspect}, " +
            $"Улики={string.Join(", ", _round.CollectedClues)}"
        );

        // Переход к следующей сцене по порядку из GameManager.sceneOrder.
        GameManager.Instance.LoadNextScene();
    }

    private void ApplyResultToGameState()
    {
        // GameManager.Instance.gameState — публичное поле, не свойство State.
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[LastBet] GameManager.Instance не найден. Результат не записан.");
            return;
        }

        GameState state = GameManager.Instance.gameState;
        if (state == null)
        {
            Debug.LogWarning("[LastBet] GameManager.gameState == null. Результат не записан.");
            return;
        }

        // Конвертируем наш токен в TokenType который понимает GameState.
        // LastBetStrategyToken и TokenType — разные enum, поэтому маппинг явный.
        TokenType tokenType = _resultToken switch
        {
            LastBetStrategyToken.Revolt     => TokenType.Revolt,
            LastBetStrategyToken.Obedience  => TokenType.Obedience,
            LastBetStrategyToken.Analysis   => TokenType.Analysis,
            // None — мини-игра завершилась без выбора (не должно происходить в норме).
            // Записываем Analysis как нейтральный исход.
            _                               => TokenType.Analysis
        };

        state.AddToken(tokenType);

        // LastBetSuspectChoice и LastBetCompleted в GameState не объявлены —
        // выбор подозреваемого пишем только в лог, чтобы не ломать компиляцию.
        // Если нужно сохранять выбор в GameState — добавь поля туда:
        //   public string lastBetSuspectChoice;
        //   public bool lastBetCompleted;
        // и раскомментируй строки ниже:
        // state.lastBetSuspectChoice = _selectedSuspect.ToString();
        // state.lastBetCompleted = true;

        Debug.Log($"[LastBet] Записан токен {tokenType} | Подозреваемый: {_selectedSuspect}");
    }

    // ── UI helpers ───────────────────────────────────────────────────────────

    private void RefreshTopUi()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(_round.TimeLeft).ToString();

        if (suspicionText != null)
            suspicionText.text = $"{_round.Suspicion}/{suspicionLimit}";

        RefreshSuspicionIndicators();
    }

    private void RefreshSuspicionIndicators()
    {
        EnsureSuspicionIndicatorsBound();

        if (suspicionIndicators == null || suspicionIndicators.Length == 0)
            return;

        for (int i = 0; i < suspicionIndicators.Length; i++)
        {
            Image indicator = suspicionIndicators[i];
            if (indicator == null)
                continue;

            bool filled = i < _round.Suspicion;
            indicator.enabled = true;

            if (suspicionEmptySprite != null || suspicionFilledSprite != null)
                indicator.sprite = filled ? suspicionFilledSprite : suspicionEmptySprite;

            indicator.color = filled ? suspicionFilledColor : suspicionEmptyColor;
        }
    }

    private void EnsureSuspicionIndicatorsBound()
    {
        if (suspicionIndicators != null && suspicionIndicators.Length > 0)
            return;

        GameObject root = LastBetSceneLookup.FindObjectIncludeInactive("SuspicionCircles");
        if (root == null)
            return;

        Image[] allImages = root.GetComponentsInChildren<Image>(true);
        List<Image> result = new List<Image>();

        foreach (Image image in allImages)
        {
            if (image == null)
                continue;

            // Берём только дочерние изображения, чтобы не захватить фон панели.
            if (image.transform == root.transform)
                continue;

            result.Add(image);
        }

        suspicionIndicators = result.ToArray();
    }

    private void RefreshButtonState()
    {
        if (openCardButton != null)
        {
            openCardButton.interactable =
                _round.Active &&
                !_round.ChoiceOpened &&
                _nextCardIndex < _cardViews.Count &&
                _round.Suspicion < suspicionLimit;
        }

        if (takeInfoButton != null)
        {
            takeInfoButton.interactable =
                _round.Active &&
                !_round.ChoiceOpened &&
                _round.HasEnoughInformation(minInformationToChoose);
        }
    }

    private void SetCroupierLine(string value)
    {
        if (croupierLineText != null)
            croupierLineText.text = value ?? string.Empty;
    }

    private void SetInfoLine(string value)
    {
        if (infoLineText != null)
            infoLineText.text = value ?? string.Empty;
    }

    // ── Инициализация ────────────────────────────────────────────────────────

    private void WireButtons()
    {
        if (openCardButton != null)
        {
            openCardButton.onClick.RemoveAllListeners();
            openCardButton.onClick.AddListener(OpenNextCard);
        }

        if (takeInfoButton != null)
        {
            takeInfoButton.onClick.RemoveAllListeners();
            takeInfoButton.onClick.AddListener(TryStartDecision);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueAfterResult);
        }
    }

    private void ConfigureSubPanels()
    {
        if (evidencePanel == null)
            evidencePanel = GetComponent<LastBetEvidencePanel>();
        if (evidencePanel == null)
            evidencePanel = gameObject.AddComponent<LastBetEvidencePanel>();

        evidencePanel.Configure(evidenceContentParent, clueSlotPrefab, evidenceTooltip);

        if (suspectPanelController == null)
            suspectPanelController = GetComponent<LastBetSuspectPanel>();
        if (suspectPanelController == null)
            suspectPanelController = gameObject.AddComponent<LastBetSuspectPanel>();

        suspectPanelController.Configure(suspectPanel, helgaChoice, victorChoice, marieChoice);
    }

    private void HideRuntimePanels()
    {
        LastBetUiUtility.SetPanelVisible(resultPanel, false);

        if (suspectPanelController != null)
            suspectPanelController.Hide();
        else
            LastBetUiUtility.SetPanelVisible(suspectPanel, false);
    }

    private void AutoBindSceneReferences()
    {
        if (githubTutorialManager == null)
            githubTutorialManager = FindGithubTutorialManager();

        EnsureSuspicionIndicatorsBound();

        if (openCardButton == null)
            openCardButton = LastBetSceneLookup.FindButton("OpenCardButton");
        if (takeInfoButton == null)
            takeInfoButton = LastBetSceneLookup.FindButton("TakeInfoButton");
        if (continueButton == null)
            continueButton = LastBetSceneLookup.FindButton("ContinueButton");

        if (rulesPanelController == null)
            rulesPanelController = FindAnyObjectByType<LastBetRulesPanel>(FindObjectsInactive.Include);

        if (cardParent == null)
        {
            GameObject go = LastBetSceneLookup.FindObjectIncludeInactive("CardParent");
            if (go != null) cardParent = go.transform;
        }

        if (evidenceContentParent == null)
        {
            // Ищем по уникальному имени — не "Content", чтобы не попасть на чужой объект.
            GameObject go = LastBetSceneLookup.FindObjectIncludeInactive("EvidenceContent");
            if (go != null) evidenceContentParent = go.transform;
        }

        if (suspectPanel == null)
            suspectPanel = LastBetSceneLookup.FindObjectIncludeInactive("SuspectPanel");
        if (resultPanel == null)
            resultPanel = LastBetSceneLookup.FindObjectIncludeInactive("ResultPanel");

        if (croupierLineText == null)
            croupierLineText = LastBetSceneLookup.FindText("CroupierLineText");
        if (infoLineText == null)
            infoLineText = LastBetSceneLookup.FindText("InfoLineText");
        if (timerText == null)
            timerText = LastBetSceneLookup.FindText("TimerText");
        if (suspicionText == null)
            suspicionText = LastBetSceneLookup.FindText("SuspicionText");
    }

    // ── Туториал ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Запускает туториал и вызывает onCompleted когда он завершится.
    /// Если туториал не найден — сразу вызывает onCompleted.
    /// Таймер раунда не стартует пока туториал не закончен.
    /// </summary>
    private void StartGithubTutorialIfNeeded(Action onCompleted)
    {
        if (_introTutorialStarted)
        {
            onCompleted?.Invoke();
            return;
        }

        if (githubTutorialManager == null)
            githubTutorialManager = FindGithubTutorialManager();

        if (githubTutorialManager == null)
        {
            Debug.LogWarning("[LastBet] UITutorialManager не найден. Туториал пропущен, раунд стартует.");
            onCompleted?.Invoke();
            return;
        }

        // Подписываемся на завершение туториала через callback.
        // Когда туториал закончится — вызовем onCompleted (то есть StartRound).
        bool started = TryStartGithubTutorial(githubTutorialManager, introTutorialSequenceName, onCompleted);

        if (started)
        {
            _introTutorialStarted = true;
            Debug.Log($"[LastBet] Туториал запущен: {introTutorialSequenceName}. Раунд ждёт завершения.");
        }
        else
        {
            Debug.LogWarning("[LastBet] Туториал не запустился. Раунд стартует без него.");
            onCompleted?.Invoke();
        }
    }

    private MonoBehaviour FindGithubTutorialManager()
    {
        foreach (MonoBehaviour b in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
        {
            if (b != null && b.GetType().Name == "UITutorialManager")
                return b;
        }
        return null;
    }

    private bool TryStartGithubTutorial(MonoBehaviour manager, string sequenceName, Action onCompleted)
    {
        if (manager == null || string.IsNullOrWhiteSpace(sequenceName))
            return false;

        Type type = manager.GetType();

        // Сначала пробуем подписаться на событие завершения туториала,
        // чтобы вызвать StartRound только после его окончания.
        TrySubscribeToTutorialEnd(manager, type, onCompleted);

        foreach (var method in type.GetMethods())
        {
            if (method.Name != "StartTutorial")
                continue;

            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                continue;

            Type paramType = parameters[0].ParameterType;
            object argument;

            if (paramType.IsEnum)
            {
                try { argument = Enum.Parse(paramType, sequenceName); }
                catch { continue; }
            }
            else if (paramType == typeof(string))
            {
                argument = sequenceName;
            }
            else
            {
                continue;
            }

            method.Invoke(manager, new[] { argument });
            return true;
        }

        return false;
    }

    /// <summary>
    /// Пытается подписаться на событие завершения туториала (OnTutorialEnd или подобное).
    /// Если такого события нет — onCompleted вызывается сразу как fallback.
    /// </summary>
    private void TrySubscribeToTutorialEnd(MonoBehaviour manager, Type type, Action onCompleted)
    {
        // Ищем событие по типичным именам из GitHub-пакетов туториалов.
        var endEvent = type.GetEvent("OnTutorialEnd")
                    ?? type.GetEvent("OnSequenceCompleted")
                    ?? type.GetEvent("OnCompleted");

        if (endEvent != null)
        {
            try
            {
                // Создаём делегат нужного типа и подписываемся.
                var handler = Delegate.CreateDelegate(endEvent.EventHandlerType, onCompleted.Target, onCompleted.Method, throwOnBindFailure: false);
                if (handler != null)
                {
                    endEvent.AddEventHandler(manager, handler);
                    return;
                }
            }
            catch
            {
                // Тип делегата не совпал — используем fallback ниже.
            }
        }

        // Если подписаться не удалось — запускаем раунд сразу.
        // Туториал покажется, но таймер не будет ждать его завершения.
        Debug.LogWarning("[LastBet] Не удалось подписаться на завершение туториала. " +
                         "Раунд стартует параллельно. Свяжи OnTutorialEnd вручную в Inspector.");
        onCompleted?.Invoke();
    }
}
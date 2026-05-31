using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Главный сценарный координатор мини-игры «Последняя ставка»
public class LastBetMiniGameManager : MonoBehaviour
{
    [Header("Rules")]
    [SerializeField] private int cardsOnTable = 6;
    [SerializeField] private int suspicionLimit = 5;
    [SerializeField] private int minInformationToChoose = 2;
    [SerializeField] private float roundTime = 180f;
    [SerializeField] private bool startOnAwake = false;
    [SerializeField] private bool showRulesBeforeStart = true;

    [Header("Cards")]
    [SerializeField] private LastBetCardView cardPrefab;
    [SerializeField] private Transform cardParent;
    [SerializeField] private Sprite cardBaseSprite;
    [SerializeField] private Sprite cardBackSprite;
    [SerializeField] private Sprite cardFrameSprite;
    [SerializeField] private Sprite jokerFullCardSprite;
    [SerializeField] private List<LastBetCardData> deckTemplates = new List<LastBetCardData>();

    [Header("Evidence Panel")]
    [SerializeField] private Transform evidenceTable;
    [SerializeField] private LastBetClueSlotView clueSlotPrefab;
    [SerializeField] private LastBetEvidencePanel evidencePanel;

    [Header("Buttons")]
    [SerializeField] private Button openCardButton;
    [SerializeField] private Button takeInfoButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private LastBetUIButtonSpriteState openCardButtonSpriteState;
    [SerializeField] private LastBetUIButtonSpriteState takeInfoButtonSpriteState;

    [Header("Suspects")]
    [SerializeField] private LastBetSuspectPanel suspectPanelController;
    [SerializeField] private GameObject suspectPanel;
    [SerializeField] private LastBetChoiceButton helgaChoice;
    [SerializeField] private LastBetChoiceButton victorChoice;
    [SerializeField] private LastBetChoiceButton marieChoice;

    [Header("Texts")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text suspicionText;
    [SerializeField] private TMP_Text croupierText;
    [SerializeField] private TMP_Text knownEvidenceText;
    [SerializeField] private TMP_Text resultMainText;
    [SerializeField] private TMP_Text resultInfoText;

    [Header("Intro Rules")]
    [SerializeField] private LastBetRulesPanel rulesPanelController;
    [SerializeField] private GameObject rulesPanel;
    [SerializeField] private Button rulesStartButton;
    [SerializeField] private TMP_Text rulesTitleText;
    [SerializeField] private TMP_Text rulesBodyText;

    [Header("Result")]
    [SerializeField] private GameObject resultPanel;

    [Header("Suspicion Circles")]
    [SerializeField] private Sprite suspicionEmptySprite;
    [SerializeField] private Sprite suspicionFilledSprite;

    [Header("Integration")]
    [SerializeField] private bool addStrategyTokenBeforeFinish = true;
    [SerializeField] private bool addStoryCluesToGameState = true;
    [SerializeField] private bool finishThroughGameManager = true;

    private readonly LastBetRoundModel _round = new LastBetRoundModel();
    private readonly List<LastBetCardView> _cardViews = new List<LastBetCardView>();
    private readonly List<Image> _suspicionCircles = new List<Image>();

    private int _nextCardIndex;

    private void Awake()
    {
        AutoBindSceneReferences();
        ConfigureSubControllers();
        WireButtons();
        CacheSuspicionCircles();
        PrepareInitialUi();
    }

    private void Update()
    {
        if (!_round.Active || _round.ChoiceOpened || roundTime <= 0f)
            return;

        _round.Tick(Time.deltaTime);

        if (_round.TimeLeft <= 0f)
            OpenChoicePanel("Время вышло. Теперь Эвелин должна выбрать версию по тем сведениям, которые уже есть.");

        RefreshTopUi();
    }

    private void PrepareInitialUi()
    {
        suspectPanelController.Hide();
        LastBetUiUtility.SetPanelVisible(resultPanel, false);

        if (showRulesBeforeStart && rulesPanelController.Exists)
            rulesPanelController.Show(minInformationToChoose, suspicionLimit, StartRound);
        else if (startOnAwake || !showRulesBeforeStart)
            StartRound();
    }

    public void StartRound()
    {
        _round.Start(roundTime);
        _nextCardIndex = 0;

        LastBetUiUtility.ClearChildren(cardParent);
        evidencePanel.Clear();
        _cardViews.Clear();

        rulesPanelController.Hide();
        suspectPanelController.Hide();
        LastBetUiUtility.SetPanelVisible(resultPanel, false);

        BuildCardsOnTable(LastBetDeckService.BuildDeck(deckTemplates));

        SetCroupierLine("Откройте карты. Улики появятся справа. Когда сведений будет достаточно, заберите сведения и выберите версию.");

        RefreshTopUi();
        RefreshButtonState();
    }

    private void BuildCardsOnTable(List<LastBetCardData> deck)
    {
        if (cardPrefab == null || cardParent == null)
        {
            Debug.LogError("[LastBet] Card Prefab or Card Parent is not assigned.");
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
            SetInfoLine("Все карты на столе открыты. Теперь нужно забрать сведения и выбрать версию.");
            RefreshButtonState();
            return;
        }

        LastBetCardView view = _cardViews[_nextCardIndex++];
        if (view == null || view.Data == null)
        {
            RefreshButtonState();
            return;
        }

        view.ShowOpened();
        ApplyCard(view.Data);
        RefreshTopUi();
        RefreshButtonState();
    }

    private void ApplyCard(LastBetCardData data)
    {
        LastBetCardApplyResult result = _round.ApplyCard(data, suspicionLimit);

        if (result.IsJoker)
        {
            SetInfoLine("Джокер вмешался в расклад. Он повышает подозрение, но не добавляет улику.");
            SetCroupierLine(string.IsNullOrWhiteSpace(data.croupierLine) ? "Джокер делает выводы слишком удобными." : data.croupierLine);
        }
        else
        {
            if (result.AddedEvidence)
                evidencePanel.AddEvidence(data);

            SetInfoLine($"Собрана улика: {data.title}. Сведения: {_round.Information}/{minInformationToChoose}. Подозрение: {_round.Suspicion}/{suspicionLimit}.");
            SetCroupierLine(data.croupierLine);
        }

        if (result.OpenChoiceBecauseSuspicionLimit)
            OpenChoicePanel("Подозрение достигло предела. Эвелин больше не может тянуть время.");
    }

    private void TryTakeInformation()
    {
        if (!_round.Active || _round.ChoiceOpened)
            return;

        if (!_round.HasEnoughInformation(minInformationToChoose))
        {
            SetInfoLine($"Сведений пока мало: {_round.Information}/{minInformationToChoose}. Откройте ещё карту или дождитесь вынужденного выбора.");
            SetCroupierLine("Одна деталь редко делает версию. Вторая уже начинает показывать маршрут.");
            return;
        }

        OpenChoicePanel("Сведения собраны. Теперь выберите версию Эвелин.");
    }

    private void OpenChoicePanel(string reason)
    {
        if (_round.ChoiceOpened)
            return;

        _round.OpenChoice();
        suspectPanelController.Show();
        SetInfoLine(reason);
        SetCroupierLine("Последняя ставка сделана. Это не ответ суда, а версия, с которой Эвелин пойдёт дальше.");
        RefreshButtonState();
    }

    private void SelectSuspect(LastBetSuspect suspect)
    {
        _round.SelectSuspect(suspect);
        suspectPanelController.SetSelected(suspect);
        ShowResult();
    }

    private void ShowResult()
    {
        bool useful = LastBetResultResolver.IsInformationUseful(_round.Information, minInformationToChoose, _round.Suspicion, suspicionLimit);
        LastBetStrategyToken token = LastBetResultResolver.ResolveToken(_round.SelectedSuspect, _round.Information, _round.Suspicion, suspicionLimit);

        LastBetUiUtility.SetPanelVisible(resultPanel, true);
        if (resultPanel != null)
            resultPanel.transform.SetAsLastSibling();

        if (resultMainText != null)
            resultMainText.text = LastBetResultResolver.BuildTitle(useful);

        if (resultInfoText != null)
            resultInfoText.text = LastBetResultResolver.BuildBody(_round.SelectedSuspect, token, useful, _round.CollectedClues);

        SetInfoLine("Раунд завершён. Версия сохранена как часть расследования.");
        SetCroupierLine("Карты сказали достаточно. Но не всё.");
        RefreshButtonState();
    }

    private void ContinueAfterResult()
    {
        bool useful = LastBetResultResolver.IsInformationUseful(_round.Information, minInformationToChoose, _round.Suspicion, suspicionLimit);
        LastBetStrategyToken token = LastBetResultResolver.ResolveToken(_round.SelectedSuspect, _round.Information, _round.Suspicion, suspicionLimit);

        GameObject manager = GameObject.Find("GameManager");
        if (finishThroughGameManager && manager != null)
        {
            if (addStrategyTokenBeforeFinish)
                manager.SendMessage("AddToken", token.ToString(), SendMessageOptions.DontRequireReceiver);

            if (addStoryCluesToGameState)
            {
                foreach (LastBetStoryClue clue in _round.CollectedClues)
                    manager.SendMessage("AddStoryClue", clue.ToString(), SendMessageOptions.DontRequireReceiver);
            }

            manager.SendMessage("FinishMiniGame", useful, SendMessageOptions.DontRequireReceiver);
            return;
        }

        Debug.Log($"[LastBet] Finished. useful={useful}, token={token}, clues={string.Join(", ", _round.CollectedClues)}");
    }

    private void RefreshTopUi()
    {
        if (timerText != null)
            timerText.text = roundTime > 0f ? Mathf.CeilToInt(_round.TimeLeft).ToString() : "--";

        if (suspicionText != null)
            suspicionText.text = $"{_round.Suspicion}/{suspicionLimit}";

        RefreshSuspicionCircles();
    }

    private void RefreshButtonState()
    {
        bool canOpen = _round.Active && !_round.ChoiceOpened && _nextCardIndex < _cardViews.Count;
        bool canTakeInfo = _round.Active && !_round.ChoiceOpened;
        bool warning = _round.Suspicion >= Mathf.Max(1, suspicionLimit - 1);
        bool hasEnoughInfo = _round.HasEnoughInformation(minInformationToChoose);

        if (openCardButton != null)
            openCardButton.interactable = canOpen;

        if (takeInfoButton != null)
        {
            takeInfoButton.gameObject.SetActive(!_round.ChoiceOpened);
            takeInfoButton.interactable = canTakeInfo;
        }

        if (openCardButtonSpriteState != null)
            openCardButtonSpriteState.SetInteractableVisual(canOpen);

        if (takeInfoButtonSpriteState != null)
        {
            takeInfoButtonSpriteState.SetInteractableVisual(canTakeInfo);
            takeInfoButtonSpriteState.SetWarning((warning || hasEnoughInfo) && canTakeInfo);
        }
    }

    private void WireButtons()
    {
        openCardButton = openCardButton != null ? openCardButton : LastBetSceneLookup.FindButton("OpenCardButton");
        takeInfoButton = takeInfoButton != null ? takeInfoButton : LastBetSceneLookup.FindButton("TakeInfoCardButton");
        continueButton = continueButton != null ? continueButton : LastBetSceneLookup.FindButton("ContinueButton");

        if (openCardButton != null)
        {
            openCardButton.onClick.RemoveAllListeners();
            openCardButton.onClick.AddListener(OpenNextCard);
        }

        if (takeInfoButton != null)
        {
            takeInfoButton.onClick.RemoveAllListeners();
            takeInfoButton.onClick.AddListener(TryTakeInformation);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueAfterResult);
        }
    }

    private void ConfigureSubControllers()
    {
        if (evidencePanel == null)
            evidencePanel = gameObject.AddComponent<LastBetEvidencePanel>();
        evidencePanel.Configure(evidenceTable, clueSlotPrefab);

        if (suspectPanelController == null)
            suspectPanelController = gameObject.AddComponent<LastBetSuspectPanel>();
        suspectPanelController.Configure(suspectPanel, helgaChoice, victorChoice, marieChoice);
        suspectPanelController.Initialize(SelectSuspect);

        if (rulesPanelController == null)
            rulesPanelController = gameObject.AddComponent<LastBetRulesPanel>();
        rulesPanelController.Configure(rulesPanel, rulesStartButton, rulesTitleText, rulesBodyText);
    }

    private void AutoBindSceneReferences()
    {
        if (cardParent == null) cardParent = LastBetSceneLookup.FindTransform("CardParent");
        if (evidenceTable == null) evidenceTable = LastBetSceneLookup.FindTransform("EvidenceTable");
        if (suspectPanel == null) suspectPanel = LastBetSceneLookup.FindObjectIncludeInactive("SuspectPanel");
        if (resultPanel == null) resultPanel = LastBetSceneLookup.FindObjectIncludeInactive("ResultPanel");
        if (rulesPanel == null) rulesPanel = LastBetSceneLookup.FindObjectIncludeInactive("RulesPanel");

        if (timerText == null) timerText = LastBetSceneLookup.FindText("TimerText");
        if (infoText == null) infoText = LastBetSceneLookup.FindText("InfoText");
        if (suspicionText == null) suspicionText = LastBetSceneLookup.FindText("SuspicionText");
        if (croupierText == null) croupierText = LastBetSceneLookup.FindText("CrupieText");
        if (knownEvidenceText == null) knownEvidenceText = LastBetSceneLookup.FindText("KnownEvidenceText");
        if (resultMainText == null) resultMainText = LastBetSceneLookup.FindText("ResultMainText");
        if (resultInfoText == null) resultInfoText = LastBetSceneLookup.FindText("ResultInfoText");
        if (rulesTitleText == null) rulesTitleText = LastBetSceneLookup.FindText("RulesTitleText");
        if (rulesBodyText == null) rulesBodyText = LastBetSceneLookup.FindText("RulesBodyText");

        if (openCardButtonSpriteState == null)
            openCardButtonSpriteState = LastBetSceneLookup.FindObjectIncludeInactive("OpenCardButton")?.GetComponent<LastBetUIButtonSpriteState>();
        if (takeInfoButtonSpriteState == null)
            takeInfoButtonSpriteState = LastBetSceneLookup.FindObjectIncludeInactive("TakeInfoCardButton")?.GetComponent<LastBetUIButtonSpriteState>();

        if (knownEvidenceText != null)
            knownEvidenceText.text = "Собранные улики";
    }

    private void CacheSuspicionCircles()
    {
        _suspicionCircles.Clear();
        Transform root = LastBetSceneLookup.FindTransform("SuspicionCircles");
        if (root == null)
            return;

        for (int i = 1; i <= 5; i++)
        {
            Image image = root.Find($"Circle_{i}")?.GetComponent<Image>();
            if (image != null)
                _suspicionCircles.Add(image);
        }
    }

    private void RefreshSuspicionCircles()
    {
        for (int i = 0; i < _suspicionCircles.Count; i++)
        {
            Image circle = _suspicionCircles[i];
            if (circle == null)
                continue;

            Sprite sprite = i < _round.Suspicion ? suspicionFilledSprite : suspicionEmptySprite;
            if (sprite != null)
                circle.sprite = sprite;
            circle.preserveAspect = true;
        }
    }

    private void SetInfoLine(string value)
    {
        if (infoText != null)
            infoText.text = value ?? string.Empty;
    }

    private void SetCroupierLine(string value)
    {
        if (croupierText != null)
            croupierText.text = string.IsNullOrWhiteSpace(value) ? "Крупье молчит." : value;
    }
}

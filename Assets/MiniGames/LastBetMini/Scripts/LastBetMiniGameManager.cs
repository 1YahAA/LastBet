using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LastBetMiniGameManager : MonoBehaviour
{
    [Header("Cards")]
    [SerializeField] private LastBetCardView cardPrefab;
    [SerializeField] private Transform cardParent;
    [SerializeField] private int cardsOnTable = 6;
    [SerializeField] private List<LastBetCardData> deckTemplates = new List<LastBetCardData>();
    [SerializeField] private LastBetCardTooltip cardTooltip;

    [Header("Card Sprites")]
    [SerializeField] private Sprite cardBaseSprite;
    [SerializeField] private Sprite cardBackSprite;
    [SerializeField] private Sprite jokerCardSprite;

    [Header("Buttons")]
    [SerializeField] private Button openCardButton;
    [SerializeField] private Button makeBetButton;
    [SerializeField] private Button continueButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text victorLineText;
    [SerializeField] private TMP_Text infoLineText;
    [SerializeField] private TMP_Text openedCardsText;
    [SerializeField] private TMP_Text pressureText;
    [SerializeField] private TMP_Text decisionTitleText;

    [Header("Panels")]
    [SerializeField] private GameObject decisionPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultBodyText;

    [Header("Outcome Buttons")]
    [SerializeField] private LastBetOutcomeButton freedomChoice;
    [SerializeField] private LastBetOutcomeButton cageChoice;
    [SerializeField] private LastBetOutcomeButton truthChoice;

    [Header("Round Parameters")]
    [SerializeField] private int minimumCardsToChoose = 3;
    [SerializeField] private int pressureLimit = 2;
    [SerializeField] private bool autoStartRound = true;
    [SerializeField] private bool returnToNextSceneOnContinue = true;

    private readonly List<LastBetCardView> _cardViews = new List<LastBetCardView>();

    private int _nextCardIndex;
    private int _openedCardsCount;
    private int _freedomScore;
    private int _cageScore;
    private int _truthScore;
    private int _pressureScore;

    private LastBetOutcome _selectedOutcome = LastBetOutcome.None;
    private bool _roundActive;
    private bool _resultShown;
    private bool _lastBetWon;

    private Button _freedomButton;
    private Button _cageButton;
    private Button _truthButton;

    private void Awake()
    {
        AutoBindSceneReferences();
        BindOutcomeButtons();
        WireButtons();
        SetStaticTexts();
        HideResultPanel();
    }

    private void Start()
    {
        if (autoStartRound)
            StartRound();
    }

    public void StartRound()
    {
        _nextCardIndex = 0;
        _openedCardsCount = 0;
        _freedomScore = 0;
        _cageScore = 0;
        _truthScore = 0;
        _pressureScore = 0;
        _selectedOutcome = LastBetOutcome.None;
        _roundActive = true;
        _resultShown = false;
        _lastBetWon = false;

        LastBetUiUtility.ClearChildren(cardParent);
        _cardViews.Clear();

        HideResultPanel();
        LastBetUiUtility.SetPanelVisible(decisionPanel, true);

        List<LastBetCardData> deck = LastBetDeckService.BuildDeck(deckTemplates);
        BuildCardsOnTable(deck);

        SetVictorLine("Перед вами — последняя ставка Эвелин. Пусть карты покажут, готова ли она к собственному решению.");
        SetInfoLine($"Откройте минимум {minimumCardsToChoose} карты, затем выберите: свобода, клетка или правда.");

        RefreshUi();
    }

    private void BuildCardsOnTable(List<LastBetCardData> deck)
    {
        if (cardPrefab == null || cardParent == null)
        {
            Debug.LogError("[LastBet] Card Prefab или Card Parent не назначены.");
            return;
        }

        int count = Mathf.Min(cardsOnTable, deck.Count);
        for (int i = 0; i < count; i++)
        {
            LastBetCardView view = Instantiate(cardPrefab, cardParent);
            view.gameObject.SetActive(true);
            view.Setup(deck[i], cardTooltip, cardBaseSprite, cardBackSprite, jokerCardSprite);
            _cardViews.Add(view);
        }
    }

    private void OpenNextCard()
    {
        if (!_roundActive || _resultShown)
            return;

        if (_nextCardIndex >= _cardViews.Count)
        {
            SetInfoLine("Карты закончились. Теперь нужно сделать ставку.");
            RefreshUi();
            return;
        }

        LastBetCardView cardView = _cardViews[_nextCardIndex];
        _nextCardIndex++;

        if (cardView == null || cardView.Opened)
            return;

        cardView.ShowOpened();
        ApplyCard(cardView.Data);
        RefreshUi();
    }

    private void ApplyCard(LastBetCardData data)
    {
        if (data == null)
            return;

        data.NormalizeValuesFromSymbol();

        _openedCardsCount++;
        _freedomScore += data.freedomValue;
        _cageScore += data.cageValue;
        _truthScore += data.truthValue;
        _pressureScore += data.pressureValue;

        SetVictorLine(string.IsNullOrWhiteSpace(data.victorLine)
            ? GetDefaultVictorLine(data.symbolType)
            : data.victorLine);

        if (data.IsJoker)
        {
            SetInfoLine("Джокер вмешался в расклад. Давление Виктора усиливается.");
            return;
        }

        if (_openedCardsCount < minimumCardsToChoose)
        {
            SetInfoLine($"Открыто карт: {_openedCardsCount}/{minimumCardsToChoose}. Эвелин ещё не готова сделать ставку.");
            return;
        }

        SetInfoLine("Теперь можно сделать ставку или открыть ещё одну карту.");
    }

    private string GetDefaultVictorLine(LastBetSymbolType symbol)
    {
        switch (symbol)
        {
            case LastBetSymbolType.Bird:
                return "Птица всегда мечтает о небе. Пока не вспоминает, кто её кормит.";

            case LastBetSymbolType.Cage:
            case LastBetSymbolType.Cocktail:
                return "Дом узнаёт своих. Даже если они делают вид, что хотят уйти.";

            case LastBetSymbolType.Eye:
            case LastBetSymbolType.Microphone:
                return "Осторожнее, дорогая. Не все истины стоит произносить при публике.";

            case LastBetSymbolType.Joker:
                return "Вот видишь? Даже судьба противится твоей дерзости.";

            default:
                return "Карта молчит. Но зал всё равно смотрит.";
        }
    }

    private void SelectOutcome(LastBetOutcome outcome)
    {
        if (!_roundActive || _resultShown)
            return;

        if (_openedCardsCount < minimumCardsToChoose)
        {
            SetInfoLine($"Сначала нужно открыть минимум {minimumCardsToChoose} карты.");
            return;
        }

        _selectedOutcome = outcome;
        UpdateOutcomeVisuals();
        RefreshUi();
    }

    private void MakeBet()
    {
        if (!_roundActive || _resultShown)
            return;

        if (_openedCardsCount < minimumCardsToChoose)
        {
            SetInfoLine($"Сначала нужно открыть минимум {minimumCardsToChoose} карты.");
            return;
        }

        if (_selectedOutcome == LastBetOutcome.None)
        {
            SetInfoLine("Выберите, к чему склоняется Эвелин: свобода, клетка или правда.");
            return;
        }

        bool matchesSpread = IsSelectedOutcomeTiedForStrongest(_selectedOutcome);
        bool pressureTooHigh = _pressureScore >= pressureLimit;

        _lastBetWon = matchesSpread && !pressureTooHigh;
        _roundActive = false;
        _resultShown = true;

        ShowResultPanel();
        RefreshUi();
    }

    private bool IsSelectedOutcomeTiedForStrongest(LastBetOutcome outcome)
    {
        int max = Mathf.Max(_freedomScore, Mathf.Max(_cageScore, _truthScore));

        switch (outcome)
        {
            case LastBetOutcome.Freedom:
                return _freedomScore == max;

            case LastBetOutcome.Cage:
                return _cageScore == max;

            case LastBetOutcome.Truth:
                return _truthScore == max;

            default:
                return false;
        }
    }

    private void ShowResultPanel()
    {
        LastBetUiUtility.SetPanelVisible(resultPanel, true);

        if (resultTitleText != null)
            resultTitleText.text = _lastBetWon ? "Ставка сделана" : "Ставка сорвалась";

        if (resultBodyText != null)
        {
            resultBodyText.text = _lastBetWon
                ? "Виктор улыбается, но на мгновение теряет контроль.\n\n«Впечатляет. Я почти забыл, какой упрямой ты можешь быть»."
                : "Свет прожектора становится резче. Шум зала давит сильнее.\n\n«Тише, Эвелин. Не всем дано выдержать свет прожектора».";
        }

        SetVictorLine(_lastBetWon
            ? "Впечатляет. Я почти забыл, какой упрямой ты можешь быть."
            : "Тише, Эвелин. Не всем дано выдержать свет прожектора.");

        SetInfoLine("Раунд завершён. Продолжите финальную сцену.");
    }

    private void ContinueAfterResult()
    {
        ApplyResultToGameState();

        Debug.Log(
            "[LastBet] Завершено. " +
            $"Выбор={_selectedOutcome}, Победа={_lastBetWon}, " +
            $"Freedom={_freedomScore}, Cage={_cageScore}, Truth={_truthScore}, Pressure={_pressureScore}"
        );

        if (!returnToNextSceneOnContinue)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.LoadNextScene();
        else
            Debug.LogWarning("[LastBet] GameManager.Instance не найден. Переход не выполнен.");
    }

    private void ApplyResultToGameState()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null)
        {
            Debug.LogWarning("[LastBet] GameState не найден. Результат не записан.");
            return;
        }

        GameState state = GameManager.Instance.gameState;

        switch (_selectedOutcome)
        {
            case LastBetOutcome.Freedom:
                state.AddToken(TokenType.Revolt, 2);
                break;

            case LastBetOutcome.Cage:
                state.AddToken(TokenType.Obedience, 2);
                break;

            case LastBetOutcome.Truth:
                state.AddToken(TokenType.Analysis, 2);
                break;
        }

        if (_lastBetWon)
            state.AddToken(TokenType.Analysis, 1);

        // В GameState нужно добавить поля:
        // public bool lastBetCompleted;
        // public bool lastBetWon;
        // public string lastBetChoice;
        // public int lastBetPressureScore;
        //
        // После добавления раскомментировать:
        // state.lastBetCompleted = true;
        // state.lastBetWon = _lastBetWon;
        // state.lastBetChoice = _selectedOutcome.ToString();
        // state.lastBetPressureScore = _pressureScore;
    }

    private void RefreshUi()
    {
        if (openedCardsText != null)
            openedCardsText.text = $"Карты: {_openedCardsCount}/{minimumCardsToChoose}";

        if (pressureText != null)
            pressureText.text = _pressureScore > 0 ? $"Давление: {_pressureScore}/{pressureLimit}" : string.Empty;

        if (openCardButton != null)
            openCardButton.interactable = _roundActive && !_resultShown && _nextCardIndex < _cardViews.Count;

        bool canChoose = _roundActive && !_resultShown && _openedCardsCount >= minimumCardsToChoose;

        SetButtonInteractable(_freedomButton, canChoose);
        SetButtonInteractable(_cageButton, canChoose);
        SetButtonInteractable(_truthButton, canChoose);

        if (makeBetButton != null)
            makeBetButton.interactable = canChoose && _selectedOutcome != LastBetOutcome.None;

        if (continueButton != null)
            continueButton.interactable = _resultShown;

        UpdateOutcomeVisuals();
    }

    private void UpdateOutcomeVisuals()
    {
        if (freedomChoice != null)
            freedomChoice.SetSelected(_selectedOutcome == LastBetOutcome.Freedom);

        if (cageChoice != null)
            cageChoice.SetSelected(_selectedOutcome == LastBetOutcome.Cage);

        if (truthChoice != null)
            truthChoice.SetSelected(_selectedOutcome == LastBetOutcome.Truth);
    }

    private void SetStaticTexts()
    {
        if (speakerNameText != null)
            speakerNameText.text = "Виктор";

        if (decisionTitleText != null)
            decisionTitleText.text = "Что выбирает Эвелин?";

        SetButtonText(openCardButton, "Вскрыть карту");
        SetButtonText(makeBetButton, "Сделать ставку");
        SetButtonText(_freedomButton, "Свобода");
        SetButtonText(_cageButton, "Клетка");
        SetButtonText(_truthButton, "Правда");
    }

    private void BindOutcomeButtons()
    {
        _freedomButton = freedomChoice != null ? freedomChoice.GetComponent<Button>() : null;
        _cageButton = cageChoice != null ? cageChoice.GetComponent<Button>() : null;
        _truthButton = truthChoice != null ? truthChoice.GetComponent<Button>() : null;

        if (freedomChoice != null) freedomChoice.BindDefaults();
        if (cageChoice != null) cageChoice.BindDefaults();
        if (truthChoice != null) truthChoice.BindDefaults();
    }

    private void WireButtons()
    {
        if (openCardButton != null)
        {
            openCardButton.onClick.RemoveAllListeners();
            openCardButton.onClick.AddListener(OpenNextCard);
        }

        if (makeBetButton != null)
        {
            makeBetButton.onClick.RemoveAllListeners();
            makeBetButton.onClick.AddListener(MakeBet);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueAfterResult);
        }

        if (_freedomButton != null)
        {
            _freedomButton.onClick.RemoveAllListeners();
            _freedomButton.onClick.AddListener(() => SelectOutcome(LastBetOutcome.Freedom));
        }

        if (_cageButton != null)
        {
            _cageButton.onClick.RemoveAllListeners();
            _cageButton.onClick.AddListener(() => SelectOutcome(LastBetOutcome.Cage));
        }

        if (_truthButton != null)
        {
            _truthButton.onClick.RemoveAllListeners();
            _truthButton.onClick.AddListener(() => SelectOutcome(LastBetOutcome.Truth));
        }
    }

    private void HideResultPanel()
    {
        LastBetUiUtility.SetPanelVisible(resultPanel, false);
    }

    private void AutoBindSceneReferences()
    {
        if (openCardButton == null)
            openCardButton = LastBetSceneLookup.FindButton("OpenCardButton");

        if (makeBetButton == null)
            makeBetButton = LastBetSceneLookup.FindButton("MakeBetButton");

        if (continueButton == null)
            continueButton = LastBetSceneLookup.FindButton("ContinueButton");

        if (cardParent == null)
        {
            GameObject go = LastBetSceneLookup.FindObjectIncludeInactive("CardParent");
            if (go != null) cardParent = go.transform;
        }

        if (decisionPanel == null)
            decisionPanel = LastBetSceneLookup.FindObjectIncludeInactive("DecisionPanel");

        if (resultPanel == null)
            resultPanel = LastBetSceneLookup.FindObjectIncludeInactive("ResultPanel");

        if (cardTooltip == null)
        {
            GameObject go = LastBetSceneLookup.FindObjectIncludeInactive("CardTooltip");
            if (go != null)
                cardTooltip = go.GetComponent<LastBetCardTooltip>();
        }

        if (speakerNameText == null)
            speakerNameText = LastBetSceneLookup.FindText("SpeakerNameText");

        if (victorLineText == null)
            victorLineText = LastBetSceneLookup.FindText("VictorLineText");

        if (infoLineText == null)
            infoLineText = LastBetSceneLookup.FindText("InfoLineText");

        if (openedCardsText == null)
            openedCardsText = LastBetSceneLookup.FindText("OpenedCardsText");

        if (pressureText == null)
            pressureText = LastBetSceneLookup.FindText("PressureText");

        if (decisionTitleText == null)
            decisionTitleText = LastBetSceneLookup.FindText("DecisionTitleText");
    }

    private void SetVictorLine(string value)
    {
        if (victorLineText != null)
            victorLineText.text = value ?? string.Empty;
    }

    private void SetInfoLine(string value)
    {
        if (infoLineText != null)
            infoLineText.text = value ?? string.Empty;
    }

    private static void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    private static void SetButtonText(Button button, string value)
    {
        if (button == null)
            return;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
            text.text = value;
    }
}

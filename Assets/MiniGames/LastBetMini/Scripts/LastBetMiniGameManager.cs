using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LastBetMiniGameManager : MonoBehaviour
{
    [Header("Rules")]
    [SerializeField] private int cardsOnTable = 6;
    [SerializeField] private int suspicionLimit = 5;
    [SerializeField] private int minInformationToChoose = 2;
    [SerializeField] private float roundTime = 75f;
    [SerializeField] private bool startOnAwake = true;

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

    [Header("Buttons")]
    [SerializeField] private Button openCardButton;
    [SerializeField] private Button takeInfoButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private LastBetUIButtonSpriteState openCardButtonSpriteState;
    [SerializeField] private LastBetUIButtonSpriteState takeInfoButtonSpriteState;

    [Header("Suspects")]
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

    [Header("Result")]
    [SerializeField] private GameObject resultPanel;

    [Header("Suspicion Circles")]
    [SerializeField] private Sprite suspicionEmptySprite;
    [SerializeField] private Sprite suspicionFilledSprite;

    [Header("Integration")]
    [SerializeField] private bool addStrategyTokenBeforeFinish = true;
    [SerializeField] private bool addStoryCluesToGameState = true;
    [SerializeField] private bool finishThroughGameManager = true;

    private readonly List<LastBetCardData> _deck = new List<LastBetCardData>();
    private readonly List<LastBetCardView> _cardViews = new List<LastBetCardView>();
    private readonly List<LastBetStoryClue> _collectedClues = new List<LastBetStoryClue>();
    private readonly List<Image> _suspicionCircles = new List<Image>();

    private int _nextCardIndex;
    private int _information;
    private int _suspicion;
    private float _timeLeft;
    private bool _roundActive;
    private bool _choiceOpened;
    private LastBetSuspect _selectedSuspect = LastBetSuspect.None;

    private void Awake()
    {
        AutoBindSceneReferences();
        WireButtons();
        BindChoiceButtons();
        CacheSuspicionCircles();

        if (startOnAwake)
            StartRound();
    }

    private void Update()
    {
        if (!_roundActive || _choiceOpened || roundTime <= 0f)
            return;

        _timeLeft -= Time.deltaTime;
        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            OpenChoicePanel();
        }

        RefreshTopUi();
    }

    public void StartRound()
    {
        _information = 0;
        _suspicion = 0;
        _nextCardIndex = 0;
        _selectedSuspect = LastBetSuspect.None;
        _choiceOpened = false;
        _roundActive = true;
        _timeLeft = roundTime;

        ClearChildren(cardParent);
        ClearChildren(evidenceTable);
        _cardViews.Clear();
        _collectedClues.Clear();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (suspectPanel != null)
            suspectPanel.SetActive(false);

        BuildDeck();
        BuildCardsOnTable();

        SetCroupierLine("Откройте карту. Сведения появятся в панели справа.");
        SetInfoLine("Откройте карту, чтобы получить первую улику.");

        RefreshTopUi();
        RefreshButtonState();
        RefreshChoiceSelection();
    }

    private void BuildDeck()
    {
        _deck.Clear();

        if (deckTemplates != null && deckTemplates.Count > 0)
        {
            _deck.AddRange(deckTemplates.Where(card => card != null));
        }

        if (_deck.Count == 0)
            _deck.AddRange(CreateFallbackDeck());

        Shuffle(_deck);
    }

    private void BuildCardsOnTable()
    {
        if (cardPrefab == null || cardParent == null)
        {
            Debug.LogError("[LastBet] Card Prefab or Card Parent is not assigned.");
            return;
        }

        int count = Mathf.Min(cardsOnTable, _deck.Count);

        for (int i = 0; i < count; i++)
        {
            LastBetCardView view = Instantiate(cardPrefab, cardParent);
            view.gameObject.SetActive(true);
            view.Setup(_deck[i], cardBaseSprite, cardBackSprite, cardFrameSprite, jokerFullCardSprite);
            _cardViews.Add(view);
        }
    }

    private void OpenNextCard()
    {
        if (!_roundActive || _choiceOpened)
            return;

        if (_nextCardIndex >= _cardViews.Count)
        {
            SetInfoLine("Все карты на столе открыты. Можно забрать сведения.");
            RefreshButtonState();
            return;
        }

        LastBetCardView view = _cardViews[_nextCardIndex];
        _nextCardIndex++;

        if (view == null || view.Data == null)
        {
            RefreshButtonState();
            return;
        }

        LastBetCardData data = view.Data;
        view.ShowOpened();

        ApplyCard(data);
        RefreshTopUi();
        RefreshButtonState();
    }

    private void ApplyCard(LastBetCardData data)
    {
        if (data == null)
            return;

        _information += Mathf.Max(0, data.informationValue);
        _suspicion += Mathf.Max(0, data.suspicionValue);
        _suspicion = Mathf.Clamp(_suspicion, 0, suspicionLimit);

        if (data.IsJoker)
        {
            SetInfoLine("Джокер вмешался в расклад. Эта карта не добавлена к уликам.");
            SetCroupierLine(string.IsNullOrWhiteSpace(data.croupierLine)
                ? "Джокер любит появляться там, где выводы становятся слишком удобными."
                : data.croupierLine);
            return;
        }

        if (data.AddsEvidence)
        {
            AddEvidenceSlot(data);
            if (!_collectedClues.Contains(data.storyClue))
                _collectedClues.Add(data.storyClue);
        }

        SetInfoLine($"Собрана улика: {data.title}");
        SetCroupierLine(data.croupierLine);
    }

    private void AddEvidenceSlot(LastBetCardData data)
    {
        if (data == null)
            return;

        Transform parent = GetEvidenceContentParent();
        if (parent == null)
        {
            Debug.LogWarning("[LastBet] Evidence Table is not assigned, clue slot was not created.");
            return;
        }

        if (clueSlotPrefab == null)
        {
            Debug.LogWarning("[LastBet] Clue Slot Prefab is not assigned, clue slot was not created.");
            return;
        }

        LastBetClueSlotView slot = Instantiate(clueSlotPrefab, parent);
        slot.gameObject.SetActive(true);

        string evidenceTitle = MakeReadableTitle(data.title);
        slot.Setup(data.clueSprite, evidenceTitle, data.evidencePanelDescription);

        LayoutRebuilder.ForceRebuildLayoutImmediate(parent as RectTransform);
    }

    private Transform GetEvidenceContentParent()
    {
        if (evidenceTable != null)
            return evidenceTable;

        GameObject byName = GameObject.Find("EvidenceTable");
        if (byName != null)
            return byName.transform;

        GameObject content = GameObject.Find("Content");
        if (content != null)
            return content.transform;

        return null;
    }

    private void OpenChoicePanel()
    {
        if (_choiceOpened)
            return;

        _choiceOpened = true;
        _roundActive = false;

        if (suspectPanel != null)
            suspectPanel.SetActive(true);

        SetInfoLine("Выберите версию Эвелин.");
        SetCroupierLine("Последняя ставка сделана. Теперь останется только версия.");

        RefreshButtonState();
        RefreshChoiceSelection();
    }

    private void SelectSuspect(LastBetSuspect suspect)
    {
        _selectedSuspect = suspect;
        RefreshChoiceSelection();
        ShowResult();
    }

    private void ShowResult()
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        LastBetStrategyToken token = ResolveStrategyToken();
        bool useful = ResolveUsefulOutcome();

        if (resultMainText != null)
            resultMainText.text = useful ? "СВЕДЕНИЯ СОХРАНЕНЫ" : "СЛЕДЫ ПРОТИВОРЕЧИВЫ";

        if (resultInfoText != null)
            resultInfoText.text = BuildResultText(token, useful);

        SetInfoLine("Раунд завершён.");
        SetCroupierLine("Карты сказали достаточно. Но не всё.");

        RefreshButtonState();
    }

    private bool ResolveUsefulOutcome()
    {
        return _information >= minInformationToChoose && _suspicion < suspicionLimit;
    }

    private LastBetStrategyToken ResolveStrategyToken()
    {
        if (_selectedSuspect == LastBetSuspect.Helga)
            return LastBetStrategyToken.Revolt;

        if (_selectedSuspect == LastBetSuspect.Victor)
            return LastBetStrategyToken.Obedience;

        if (_information >= 4 && _suspicion <= 3)
            return LastBetStrategyToken.Analysis;

        return LastBetStrategyToken.Analysis;
    }

    private string BuildResultText(LastBetStrategyToken token, bool useful)
    {
        if (useful)
        {
            return "Эвелин сохранила часть сведений. Несколько деталей складываются в общий маршрут, но ни одна карта не даёт полной уверенности.";
        }

        return "Раунд оставил слишком много шума. Часть сведений можно сохранить, но доверять всей версии опасно.";
    }

    private void ContinueAfterResult()
    {
        LastBetStrategyToken token = ResolveStrategyToken();
        bool useful = ResolveUsefulOutcome();

        // Здесь оставлены безопасные попытки интеграции. Если в проекте нет таких методов,
        // код просто продолжит работу без ошибки компиляции, потому что используется SendMessage.
        if (finishThroughGameManager && GameManagerExists())
        {
            GameObject manager = GameObject.Find("GameManager");
            if (manager != null)
            {
                if (addStrategyTokenBeforeFinish)
                    manager.SendMessage("AddToken", token.ToString(), SendMessageOptions.DontRequireReceiver);

                if (addStoryCluesToGameState)
                {
                    foreach (LastBetStoryClue clue in _collectedClues)
                        manager.SendMessage("AddStoryClue", clue.ToString(), SendMessageOptions.DontRequireReceiver);
                }

                manager.SendMessage("FinishMiniGame", useful, SendMessageOptions.DontRequireReceiver);
                return;
            }
        }

        Debug.Log($"[LastBet] Finished. useful={useful}, token={token}, clues={string.Join(", ", _collectedClues)}");
    }

    private bool GameManagerExists()
    {
        return GameObject.Find("GameManager") != null;
    }

    private void RefreshTopUi()
    {
        if (timerText != null)
            timerText.text = roundTime > 0f ? Mathf.CeilToInt(_timeLeft).ToString() : "--";

        if (suspicionText != null)
            suspicionText.text = $"{_suspicion}/{suspicionLimit}";

        RefreshSuspicionCircles();
    }

    private void RefreshButtonState()
    {
        bool canOpen = _roundActive && !_choiceOpened && _nextCardIndex < _cardViews.Count;
        bool canTakeInfo = !_choiceOpened && _information >= minInformationToChoose;
        bool warning = _suspicion >= Mathf.Max(1, suspicionLimit - 1);

        if (openCardButton != null)
            openCardButton.interactable = canOpen;

        if (takeInfoButton != null)
        {
            takeInfoButton.gameObject.SetActive(!_choiceOpened);
            takeInfoButton.interactable = canTakeInfo;
        }

        if (openCardButtonSpriteState != null)
            openCardButtonSpriteState.SetInteractableVisual(canOpen);

        if (takeInfoButtonSpriteState != null)
        {
            takeInfoButtonSpriteState.SetInteractableVisual(canTakeInfo);
            takeInfoButtonSpriteState.SetWarning(warning && canTakeInfo);
        }
    }

    private void RefreshChoiceSelection()
    {
        if (helgaChoice != null) helgaChoice.SetSelected(_selectedSuspect == LastBetSuspect.Helga);
        if (victorChoice != null) victorChoice.SetSelected(_selectedSuspect == LastBetSuspect.Victor);
        if (marieChoice != null) marieChoice.SetSelected(_selectedSuspect == LastBetSuspect.Marie);
    }

    private void RefreshSuspicionCircles()
    {
        if (_suspicionCircles.Count == 0)
            CacheSuspicionCircles();

        for (int i = 0; i < _suspicionCircles.Count; i++)
        {
            Image circle = _suspicionCircles[i];
            if (circle == null)
                continue;

            Sprite sprite = i < _suspicion ? suspicionFilledSprite : suspicionEmptySprite;
            if (sprite != null)
                circle.sprite = sprite;

            circle.preserveAspect = true;
        }
    }

    private void WireButtons()
    {
        AutoAddButton(ref openCardButton, "OpenCardButton");
        AutoAddButton(ref takeInfoButton, "TakeInfoCardButton");
        AutoAddButton(ref continueButton, "ContinueButton");

        if (openCardButton != null)
        {
            openCardButton.onClick.RemoveAllListeners();
            openCardButton.onClick.AddListener(OpenNextCard);
        }

        if (takeInfoButton != null)
        {
            takeInfoButton.onClick.RemoveAllListeners();
            takeInfoButton.onClick.AddListener(OpenChoicePanel);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueAfterResult);
        }
    }

    private void BindChoiceButtons()
    {
        AutoBindChoice(ref helgaChoice, "SuspectedHelga", LastBetSuspect.Helga);
        AutoBindChoice(ref victorChoice, "SuspectedVictor", LastBetSuspect.Victor);
        AutoBindChoice(ref marieChoice, "SuspectedMari", LastBetSuspect.Marie);
    }

    private void AutoBindChoice(ref LastBetChoiceButton choice, string objectName, LastBetSuspect suspect)
    {
        GameObject go = choice != null ? choice.gameObject : GameObject.Find(objectName);
        if (go == null)
            return;

        if (choice == null)
            choice = go.GetComponent<LastBetChoiceButton>();

        if (choice == null)
            choice = go.AddComponent<LastBetChoiceButton>();

        choice.BindDefaults();
        choice.SetSelected(false);

        Button button = go.GetComponent<Button>();
        if (button == null)
            button = go.AddComponent<Button>();

        button.transition = Selectable.Transition.None;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SelectSuspect(suspect));
    }

    private void AutoAddButton(ref Button button, string objectName)
    {
        GameObject go = button != null ? button.gameObject : GameObject.Find(objectName);
        if (go == null)
            return;

        if (button == null)
            button = go.GetComponent<Button>();

        if (button == null)
            button = go.AddComponent<Button>();
    }

    private void AutoBindSceneReferences()
    {
        if (cardParent == null) cardParent = FindTransform("CardParent");
        if (evidenceTable == null) evidenceTable = FindTransform("EvidenceTable");

        if (suspectPanel == null) suspectPanel = FindObject("SuspectPanel");
        if (resultPanel == null) resultPanel = FindObject("ResultPanel");

        if (timerText == null) timerText = FindText("TimerText");
        if (infoText == null) infoText = FindText("InfoText");
        if (suspicionText == null) suspicionText = FindText("SuspicionText");
        if (croupierText == null) croupierText = FindText("CrupieText");
        if (knownEvidenceText == null) knownEvidenceText = FindText("KnownEvidenceText");
        if (resultMainText == null) resultMainText = FindText("ResultMainText");
        if (resultInfoText == null) resultInfoText = FindText("ResultInfoText");

        if (openCardButtonSpriteState == null)
        {
            GameObject go = FindObject("OpenCardButton");
            if (go != null) openCardButtonSpriteState = go.GetComponent<LastBetUIButtonSpriteState>();
        }

        if (takeInfoButtonSpriteState == null)
        {
            GameObject go = FindObject("TakeInfoCardButton");
            if (go != null) takeInfoButtonSpriteState = go.GetComponent<LastBetUIButtonSpriteState>();
        }

        if (knownEvidenceText != null)
            knownEvidenceText.text = "Собранные улики";
    }

    private void CacheSuspicionCircles()
    {
        _suspicionCircles.Clear();

        Transform root = FindTransform("SuspicionCircles");
        if (root == null)
            return;

        for (int i = 1; i <= 5; i++)
        {
            Transform child = root.Find($"Circle_{i}");
            if (child == null)
                continue;

            Image image = child.GetComponent<Image>();
            if (image != null)
                _suspicionCircles.Add(image);
        }
    }

    private static GameObject FindObject(string name)
    {
        GameObject direct = GameObject.Find(name);
        return direct;
    }

    private static Transform FindTransform(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go.transform : null;
    }

    private static TMP_Text FindText(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go.GetComponent<TMP_Text>() : null;
    }

    private void SetInfoLine(string value)
    {
        if (infoText != null)
            infoText.text = value ?? string.Empty;
    }

    private void SetCroupierLine(string value)
    {
        if (croupierText != null)
            croupierText.text = string.IsNullOrWhiteSpace(value)
                ? "Крупье молчит."
                : value;
    }

    private static void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    private static void Shuffle<T>(IList<T> list)
    {
        if (list == null)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static string MakeReadableTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim().ToLowerInvariant() switch
        {
            "зажигалка" => "Зажигалка",
            "vip-билет" => "VIP-билет",
            "записка" => "Записка",
            "маска" => "Маска",
            "завязка" => "Завязка маски",
            "ставки" => "Порядок ставок",
            "пометка" => "Служебная пометка",
            "коридор" => "Закрытый коридор",
            _ => value
        };
    }

    private static IEnumerable<LastBetCardData> CreateFallbackDeck()
    {
        return new[]
        {
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.LighterAtServiceDoor,
                informationValue = 1,
                suspicionValue = 0,
                title = "ЗАЖИГАЛКА",
                cardDescription = "Чужая вещь у служебного входа.",
                evidencePanelDescription = "Тяжёлая металлическая зажигалка с потёртой гравировкой. Её нашли возле двери, которой пользуется персонал кабаре.",
                croupierLine = "Интересная находка. Но вещь у двери ещё не говорит, кто её оставил."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.VipBillWithoutNumber,
                informationValue = 1,
                suspicionValue = 0,
                title = "VIP-БИЛЕТ",
                cardDescription = "След закрытой ложи.",
                evidencePanelDescription = "Билет из закрытой ложи второго этажа. На обороте осталась едва заметная пометка карандашом.",
                croupierLine = "Закрытая ложа многое скрывает. Иногда слишком многое."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.RewrittenLetter,
                informationValue = 1,
                suspicionValue = 0,
                title = "ЗАПИСКА",
                cardDescription = "Фраза без подписи.",
                evidencePanelDescription = "«После последней ставки дверь будет открыта». Записка сложена слишком аккуратно, будто её подготовили заранее.",
                croupierLine = "Бумага терпит любые слова. Подпись обычно говорит больше."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.FalseTrail,
                storyClue = LastBetStoryClue.MaskWithPowder,
                informationValue = 1,
                suspicionValue = 1,
                title = "МАСКА",
                cardDescription = "Снята слишком поспешно.",
                evidencePanelDescription = "Чёрная сценическая маска с надорванной лентой. На внутренней стороне остался след грима.",
                croupierLine = "Маска слишком заметна. Иногда такие следы оставляют специально."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.Shield,
                storyClue = LastBetStoryClue.HelgaWarning,
                informationValue = 1,
                suspicionValue = 0,
                title = "ЗАВЯЗКА",
                cardDescription = "След ткани и грима.",
                evidencePanelDescription = "Тонкая лента пропитана запахом духов и театрального грима. Похожую ткань используют за сценой.",
                croupierLine = "За сценой всё пахнет гримом. Но не каждый след ведёт к врагу."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.Doubt,
                storyClue = LastBetStoryClue.BettingOrderChanged,
                informationValue = 1,
                suspicionValue = 1,
                title = "СТАВКИ",
                cardDescription = "Последовательность изменена.",
                evidencePanelDescription = "Кто-то исправил порядок карточных ставок прямо перед началом игры. Исправления сделаны уверенной рукой.",
                croupierLine = "В ставках нет случайностей. Есть только те, кто умеет ждать."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.ForgedEvidence,
                informationValue = 1,
                suspicionValue = 0,
                title = "ПОМЕТКА",
                cardDescription = "Знак персонала.",
                evidencePanelDescription = "Небольшой знак на полях документа совпадает с внутренними отметками персонала кабаре.",
                croupierLine = "Персонал всегда знает больше гостей. Но не всегда говорит правду."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.SecretVipCorridor,
                informationValue = 1,
                suspicionValue = 0,
                title = "КОРИДОР",
                cardDescription = "Маршрут за кулисы.",
                evidencePanelDescription = "На схеме отмечен служебный проход, ведущий мимо сцены и VIP-комнат. Обычные гости о нём не знают.",
                croupierLine = "Двери для гостей и двери для своих редко ведут в одно место."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.Joker,
                storyClue = LastBetStoryClue.JokerManipulatedTable,
                informationValue = 0,
                suspicionValue = 1,
                title = "ДЖОКЕР",
                cardDescription = "Карта вмешательства.",
                evidencePanelDescription = string.Empty,
                croupierLine = "Джокер любит появляться там, где выводы становятся слишком удобными."
            }
        };
    }
}
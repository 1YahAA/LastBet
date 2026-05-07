// Управляет раундами, клиентами, подсчётом очков и передаёт результат
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CardGameManager : MonoBehaviour
{
    [Header("Настройки игры")]
    [Tooltip("Сумма очков за всю игру (3 раунда), необходимая для победы")]
    public int winScoreThreshold = 8;

    [Tooltip("Количество клиентов (раундов) за одну игру")]
    public int totalRounds = 3;

    [Tooltip("Карт раздаётся в руку за раунд (из них выбирают 3)")]
    public int cardsToDeal = 4;

    [Header("UI: Клиент")]
    [Tooltip("Image — иконка обязательного цвета (Required). Дочерний объект CustomerPanel")]
    public Image customerRequiredIcon;

    [Tooltip("Image — иконка предпочтительного цвета (Preferred). Дочерний объект CustomerPanel")]
    public Image customerPreferredIcon;

    [Tooltip("Текст над иконками клиента, например «Клиент хочет:»")]
    public TextMeshProUGUI customerLabel;

    [Header("UI: Слоты ряда")]
    [Tooltip("3 слота по порядку слева направо — Slot_0, Slot_1, Slot_2")]
    public CardSlot[] rowSlots = new CardSlot[3];

    [Header("UI: Рука игрока")]
    [Tooltip("Transform с HorizontalLayoutGroup — сюда спавнятся карты руки")]
    public Transform handPanel;

    [Tooltip("Префаб карты (Card.prefab) — нужен для Instantiate")]
    public GameObject cardViewPrefab;

    [Header("UI: Счёт и раунд")]
    [Tooltip("Текст «Очки: N»")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Текст «Клиент N/3»")]
    public TextMeshProUGUI roundText;

    [Header("UI: Панель результата")]
    [Tooltip("GameObject ResultPanel — скрыт в начале, показывается по итогу игры")]
    public GameObject resultPanel;

    [Tooltip("Текст результата внутри ResultPanel")]
    public TextMeshProUGUI resultText;

    [Tooltip("Кнопка «Продолжить» внутри ResultPanel")]
    public Button continueButton;

    [Header("Иконки цветов")]
    [Tooltip("Спрайты иконок в порядке enum: [0]=Red [1]=Yellow [2]=Blue [3]=Black\n" +
             "Используются для отображения Required/Preferred у клиента")]
    public Sprite[] colorIcons = new Sprite[4];

    // ПРИВАТНОЕ СОСТОЯНИЕ
    private CardDeck _deck;
    private int _totalScore;
    private int _currentRound;
    private CardColor _requiredColor;
    private CardColor _preferredColor;
    private readonly List<CardView> _handViews = new();

    // ЖИЗНЕННЫЙ ЦИКЛ
    void Start()
    {
        _deck = GetComponent<CardDeck>();
        _deck.Shuffle();

        resultPanel.SetActive(false);
        _totalScore = 0;
        _currentRound = 0;

        StartRound();
    }

    // РАУНД
    void StartRound()
    {
        _currentRound++;
        roundText.text = $"Клиент {_currentRound} / {totalRounds}";

        // Очистить слоты от карт предыдущего раунда
        foreach (var slot in rowSlots)
            slot.Clear();

        // Раздать карты в руку
        ClearHand();
        var drawnCards = _deck.Draw(cardsToDeal);
        foreach (var data in drawnCards)
            SpawnCardInHand(data);

        // Сгенерировать нового клиента
        GenerateCustomer(drawnCards);
        UpdateScoreUI();
    }

    // Клиент
    void GenerateCustomer(List<CardData> dealtCards)
    {
        // Required берём из цветов, которые точно есть в руке → игрок всегда может выполнить
        var available = new List<CardColor>();
        foreach (var d in dealtCards)
            if (d.color != CardColor.None && !available.Contains(d.color))
                available.Add(d.color);

        _requiredColor  = available[Random.Range(0, available.Count)];
        _preferredColor = available[Random.Range(0, available.Count)];

        // Обновить иконки клиента
        customerRequiredIcon.sprite  = ColorToIcon(_requiredColor);
        customerPreferredIcon.sprite = ColorToIcon(_preferredColor);

        if (customerLabel != null)
            customerLabel.text = _requiredColor == _preferredColor
                ? $"Требует: {ColorName(_requiredColor)} (особо любит!)"
                : $"Требует: {ColorName(_requiredColor)}  |  Любит: {ColorName(_preferredColor)}";
    }

    Sprite ColorToIcon(CardColor c)
    {
        int idx = (int)c - 1; // Red=1→0, Yellow=2→1, Blue=3→2, Black=4→3
        return (idx >= 0 && idx < colorIcons.Length) ? colorIcons[idx] : null;
    }

    static string ColorName(CardColor c) => c switch
    {
        CardColor.Red    => "Spirits",
        CardColor.Yellow => "Citrus",
        CardColor.Blue   => "Bitter",
        CardColor.Black  => "Spoiled",
        _ => "?"
    };

    // Рука игрока
    void SpawnCardInHand(CardData data)
    {
        var go = Instantiate(cardViewPrefab, handPanel);
        var cv = go.GetComponent<CardView>();
        cv.Init(data, this);
        _handViews.Add(cv);

        // Анимация появления
        go.transform.localScale = Vector3.zero;
        go.transform.DOScale(1f, 0.22f).SetDelay(_handViews.Count * 0.05f).SetEase(Ease.OutBack);
    }

    void ClearHand()
    {
        foreach (var cv in _handViews)
            if (cv != null) Destroy(cv.gameObject);
        _handViews.Clear();
    }

    // СОБЫТИЕ: игрок кликнул карту в руке
    public void OnCardSelectedFromHand(CardView cardView)
    {
        // Найти первый свободный слот
        bool placed = false;
        foreach (var slot in rowSlots)
        {
            if (!slot.HasCard)
            {
                slot.PlaceCard(cardView);
                _handViews.Remove(cardView);
                placed = true;
                break;
            }
        }

        if (!placed) return; // все слоты заняты — игнорируем клик

        // Если ряд заполнен — считать очки
        if (RowFull()) EvaluateRound();
    }

    bool RowFull()
    {
        foreach (var slot in rowSlots)
            if (!slot.HasCard) return false;
        return true;
    }

    // ПОДСЧЁТ ОЧКОВ ЗА РАУНД
    void EvaluateRound()
    {
        // Собрать карты из слотов
        var cards = new CardData[3];
        for (int i = 0; i < 3; i++)
            cards[i] = rowSlots[i].PlacedCard;

        // CardGameScoring.CalculateRoundScore возвращает -1 при отсутствии required
        int roundScore = CardGameScoring.CalculateRoundScore(cards, _requiredColor, _preferredColor);

        if (roundScore < 0)
        {
            // Нет required цвета — мгновенное поражение
            EndGame(won: false, reason: $"Нет нужного ингредиента ({ColorName(_requiredColor)})!");
            return;
        }

        _totalScore += roundScore;
        UpdateScoreUI();

        // Визуальная пауза перед следующим раундом
        DOVirtual.DelayedCall(0.8f, () =>
        {
            if (_currentRound >= totalRounds)
                EndGame(won: _totalScore >= winScoreThreshold);
            else
                StartRound();
        });
    }

    // КОНЕЦ ИГРЫ
    void EndGame(bool won, string reason = "")
    {
        resultPanel.SetActive(true);

        if (won)
            resultText.text = $"Клиент доволен!\nИтого: {_totalScore} очков";
        else if (string.IsNullOrEmpty(reason))
            resultText.text = $"Недостаточно качества...\nИтого: {_totalScore} / {winScoreThreshold}";
        else
            resultText.text = $"Поражение!\n{reason}";

        // Анимация появления панели
        resultPanel.transform.localScale = Vector3.zero;
        resultPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

        // Привязать кнопку
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            if (GameManager.Instance != null)
                GameManager.Instance.FinishMiniGame(won);
            else
                Debug.LogWarning("CardGameManager: GameManager.Instance не найден! " +
                                 "Убедись что сцена _Persistent загружена.");
        });
    }
    
    void UpdateScoreUI()
    {
        scoreText.text = $"Очки: {_totalScore}  (нужно {winScoreThreshold})";
    }
}

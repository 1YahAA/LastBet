using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class JokerMemoryGameManager : MonoBehaviour
{
    [Header("Префабы и контейнер")]
    [SerializeField] private JokerMemoryCard cardPrefab;
    [SerializeField] private Transform cardHolder;

    [Header("Спрайты")]
    [Tooltip("Рубашка карты")]
    [SerializeField] private Sprite cardBack;

    [Tooltip("4 спрайта обычных карт. Каждый спрайт будет продублирован и станет парой.")]
    [SerializeField] private Sprite[] pairSprites = new Sprite[4];

    [Tooltip("Отдельная карта Джокера. Не образует пару.")]
    [SerializeField] private Sprite jokerSprite;

    [Header("UI: только жизни и время")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Панель результата")]
    [SerializeField] private CanvasGroup resultPanel;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI resultDescriptionText;
    [SerializeField] private Button continueButton;

    [Header("Джокер")]
    [SerializeField] private CanvasGroup jokerVoiceOverlay;
    [SerializeField] private TextMeshProUGUI jokerVoiceText;
    [SerializeField] private float jokerOverlayDuration = 1.2f;

    [Header("Настройки")]
    [SerializeField] private float maxTime = 45f;
    [SerializeField] private int startLives = 3;
    [SerializeField] private float mismatchCloseDelay = 0.45f;
    [SerializeField] private bool autoStart = true;

    [Header("Интеграция")]
    [SerializeField] private JokerMemoryGameStateAdapter gameStateAdapter;

    private readonly List<JokerMemoryCard> spawnedCards = new();
    private readonly List<JokerMemoryCardData> deck = new();

    private JokerMemoryCard firstCard;
    private JokerMemoryCard secondCard;

    private int lives;
    private int matchedPairs;
    private float timer;
    private bool isGameActive;
    private bool isBusy;
    private bool resultSent;

    private const int TotalPairs = 4;
    private const int JokerPairId = -1;

    public Sprite CardBackSprite => cardBack;

    private void Awake()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueAfterResult);
    }

    private void Start()
    {
        HideResultPanelImmediate();
        HideJokerOverlayImmediate();

        if (autoStart)
            StartGame();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueAfterResult);
    }

    private void Update()
    {
        if (!isGameActive)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = 0f;
            UpdateTimerText();
            Lose();
            return;
        }

        UpdateTimerText();
    }

    public void StartGame()
    {
        StopAllCoroutines();

        lives = startLives;
        matchedPairs = 0;
        timer = maxTime;

        firstCard = null;
        secondCard = null;

        isBusy = false;
        resultSent = false;
        isGameActive = true;

        HideResultPanelImmediate();
        HideJokerOverlayImmediate();

        RebuildDeck();
        SpawnCards();

        UpdateLivesText();
        UpdateTimerText();
    }

    public void TryOpenCard(JokerMemoryCard card)
    {
        if (!isGameActive || isBusy || card == null || card.IsOpen || card.IsMatched)
            return;

        card.Open();

        if (card.IsJoker)
        {
            StartCoroutine(HandleJoker(card));
            return;
        }

        if (firstCard == null)
        {
            firstCard = card;
            return;
        }

        if (secondCard == null && firstCard != card)
        {
            secondCard = card;
            StartCoroutine(CheckPair());
        }
    }

    private IEnumerator CheckPair()
    {
        isBusy = true;

        yield return new WaitForSeconds(0.15f);

        bool isPair =
            firstCard != null &&
            secondCard != null &&
            firstCard.PairId == secondCard.PairId;

        if (isPair)
        {
            firstCard.MarkMatched();
            secondCard.MarkMatched();

            matchedPairs++;

            firstCard = null;
            secondCard = null;
            isBusy = false;

            if (matchedPairs >= TotalPairs)
                Win();
        }
        else
        {
            yield return new WaitForSeconds(mismatchCloseDelay);

            if (firstCard != null) firstCard.Close();
            if (secondCard != null) secondCard.Close();

            firstCard = null;
            secondCard = null;
            isBusy = false;
        }
    }

    private IEnumerator HandleJoker(JokerMemoryCard joker)
    {
        isBusy = true;

        lives--;
        UpdateLivesText();

        if (jokerVoiceText != null)
            jokerVoiceText.text = "Ты устала. Тебе нельзя доверять собственным мыслям.";

        yield return FadeCanvasGroup(jokerVoiceOverlay, 0f, 1f, 0.2f);
        yield return new WaitForSeconds(jokerOverlayDuration);
        yield return FadeCanvasGroup(jokerVoiceOverlay, 1f, 0f, 0.25f);

        firstCard = null;
        secondCard = null;

        if (lives <= 0)
        {
            Lose();
            yield break;
        }

        CloseAllCards();
        ShuffleCurrentCards();

        isBusy = false;
    }

    private void Win()
    {
        if (!isGameActive)
            return;

        isGameActive = false;
        SendResult(true);

        ShowResult(
            "Память собрана",
            "Полночь — не номер. Это ловушка.\nЭвелин понимает, что Виктор заранее подготовил финальный выход."
        );
    }

    private void Lose()
    {
        if (!isGameActive && resultSent)
            return;

        isGameActive = false;
        SendResult(false);

        ShowResult(
            "Память искажена",
            "Всё смешалось. Но Эвелин знает одно: в полночь нельзя молчать."
        );
    }

    private void SendResult(bool won)
    {
        if (resultSent)
            return;

        resultSent = true;

        if (gameStateAdapter != null)
        {
            gameStateAdapter.ApplyJokerMemoryResult(won);
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishJokerMiniGame(won);
            return;
        }

        Debug.LogWarning("[JokerMemory] Нет JokerMemoryGameStateAdapter и GameManager.Instance. Результат не передан.");
    }

    private void ContinueAfterResult()
    {
        if (gameStateAdapter != null)
        {
            gameStateAdapter.ReturnToMainScene();
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.ReturnFromMiniGame();
    }

    private void RebuildDeck()
    {
        deck.Clear();

        if (pairSprites == null || pairSprites.Length < TotalPairs)
        {
            Debug.LogError($"[JokerMemory] Нужно назначить {TotalPairs} спрайта в pairSprites.", this);
            return;
        }

        for (int pairId = 0; pairId < TotalPairs; pairId++)
        {
            Sprite pairSprite = pairSprites[pairId];

            if (pairSprite == null)
            {
                Debug.LogError($"[JokerMemory] pairSprites[{pairId}] не назначен.", this);
                continue;
            }

            deck.Add(new JokerMemoryCardData($"pair_{pairId}_a", pairId, pairSprite, false));
            deck.Add(new JokerMemoryCardData($"pair_{pairId}_b", pairId, pairSprite, false));
        }

        if (jokerSprite == null)
        {
            Debug.LogError("[JokerMemory] jokerSprite не назначен.", this);
        }
        else
        {
            deck.Add(new JokerMemoryCardData("joker", JokerPairId, jokerSprite, true));
        }

        Shuffle(deck);
    }

    private void SpawnCards()
    {
        ClearCards();

        for (int i = 0; i < deck.Count; i++)
        {
            JokerMemoryCard card = Instantiate(cardPrefab, cardHolder);
            card.Init(this, deck[i]);
            spawnedCards.Add(card);
        }
    }

    private void CloseAllCards()
    {
        foreach (JokerMemoryCard card in spawnedCards)
        {
            if (card == null)
                continue;

            if (!card.IsMatched)
                card.Close();
        }

        firstCard = null;
        secondCard = null;
    }

    private void ShuffleCurrentCards()
    {
        List<JokerMemoryCard> cardsToShuffle = new List<JokerMemoryCard>();

        foreach (JokerMemoryCard card in spawnedCards)
        {
            if (card == null)
                continue;

            if (!card.IsMatched)
                cardsToShuffle.Add(card);
        }

        for (int i = 0; i < cardsToShuffle.Count; i++)
        {
            int randomIndex = Random.Range(i, cardsToShuffle.Count);

            int siblingA = cardsToShuffle[i].transform.GetSiblingIndex();
            int siblingB = cardsToShuffle[randomIndex].transform.GetSiblingIndex();

            cardsToShuffle[i].transform.SetSiblingIndex(siblingB);
            cardsToShuffle[randomIndex].transform.SetSiblingIndex(siblingA);

            JokerMemoryCard temp = cardsToShuffle[i];
            cardsToShuffle[i] = cardsToShuffle[randomIndex];
            cardsToShuffle[randomIndex] = temp;
        }
    }

    private void ClearCards()
    {
        foreach (JokerMemoryCard card in spawnedCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        spawnedCards.Clear();

        if (cardHolder == null)
            return;

        for (int i = cardHolder.childCount - 1; i >= 0; i--)
            Destroy(cardHolder.GetChild(i).gameObject);
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private void UpdateLivesText()
    {
        if (livesText != null)
            livesText.text = $"{lives}";
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
            timerText.text = $"{Mathf.CeilToInt(timer)} СЕК";
    }

    private void ShowResult(string title, string description)
    {
        if (resultTitleText != null)
            resultTitleText.text = title;

        if (resultDescriptionText != null)
            resultDescriptionText.text = description;

        StartCoroutine(FadeCanvasGroup(resultPanel, 0f, 1f, 0.25f, true));
    }

    private void HideResultPanelImmediate()
    {
        if (resultPanel == null)
            return;

        resultPanel.alpha = 0f;
        resultPanel.interactable = false;
        resultPanel.blocksRaycasts = false;
        resultPanel.gameObject.SetActive(false);
    }

    private void HideJokerOverlayImmediate()
    {
        if (jokerVoiceOverlay == null)
            return;

        jokerVoiceOverlay.alpha = 0f;
        jokerVoiceOverlay.interactable = false;
        jokerVoiceOverlay.blocksRaycasts = false;
        jokerVoiceOverlay.gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration, bool keepActive = false)
    {
        if (group == null)
            yield break;

        group.gameObject.SetActive(true);
        group.alpha = from;
        group.blocksRaycasts = true;
        group.interactable = true;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        group.alpha = to;

        bool visible = to > 0.001f;
        group.blocksRaycasts = visible;
        group.interactable = visible;

        if (!visible && !keepActive)
            group.gameObject.SetActive(false);
    }
}

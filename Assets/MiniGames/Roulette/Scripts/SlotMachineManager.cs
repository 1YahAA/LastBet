using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// ======================================================
// SlotMachineManager.cs — исправленная версия
// Путь: Assets/MiniGames/Roulette/Scripts/SlotMachineManager.cs
// ======================================================

public class SlotMachineManager : MonoBehaviour
{
    [Header("━━ Барабаны ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("Три компонента ReelController: Reel_Left, Reel_Center, Reel_Right")]
    public ReelController[] reels = new ReelController[3];

    [Header("━━ Символы (5 штук) ━━━━━━━━━━━━━━━━━━━━━━━")]
    public Sprite[] symbols = new Sprite[5];

    [Header("━━ Кнопки ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("Кнопка 'Крутить'")]
    public Button spinButton;

    [Tooltip("Кнопка 'Сломать автомат' — внутри ChoicePanel")]
    public Button breakButton;

    [Tooltip("Кнопка 'Крутить ещё' — внутри ChoicePanel")]
    public Button continueButton;

    [Header("━━ UI панели ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("Стартовая панель")]
    public CanvasGroup startPanel;

    [Tooltip("Панель результата (ключ получен)")]
    public CanvasGroup resultPanel;

    [Tooltip("Панель выбора Сломать/Продолжить")]
    public CanvasGroup choicePanel;

    [Header("━━ Тексты ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI startText;

    [Header("━━ Звуки ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    public AudioClip spinSound;
    public AudioClip jackpotSound;
    public AudioClip breakSound;
    public AudioClip doorSound;

    [Header("━━ Настройки ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("Задержка между остановками барабанов")]
    public float reelStopDelay = 0.5f;

    [Tooltip("Задержка перед показом результата")]
    public float resultDelay = 0.8f;

    // ── Состояние ─────────────────────────────────────

    private AudioSource _audio;
    private bool  _isSpinning       = false;
    private int   _spinCount        = 0;
    private bool  _chosenToContinue = false;
    private int[] _result           = new int[3];

    const string TXT_START     = "Старый автомат. Говорят, он сломан.\nГоворят многое.";
    const string TXT_WIN_EARLY = "Три одинаковых.\nЧто-то щёлкнуло внутри. Ключ?\n\n*звук открывающейся двери*";
    const string TXT_WIN_BREAK = "Рычаг сломался.\nИз щели выпал маленький ключ.\n\n*звук открывающейся двери*";
    const string TXT_WIN_LATE  = "Наконец-то. Ключ упал на пол.\n\n*звук открывающейся двери*";

    // ── Инициализация ─────────────────────────────────

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        foreach (var reel in reels)
            reel.Initialize(symbols);

        if (startText != null) startText.text = TXT_START;

        ShowPanel(startPanel, instant: true);
        HidePanel(resultPanel, instant: true);
        HidePanel(choicePanel, instant: true);

        // SpinButton выключена — включится после нажатия "Играть"
        SetActive(spinButton?.gameObject, false);
    }

    // ── Кнопки ────────────────────────────────────────

    /// Кнопка "Играть" на стартовой панели
    /// Подключить: Button_Start → OnClick → OnStartClicked()
    public void OnStartClicked()
    {
        HidePanel(startPanel, instant: false, onDone: () =>
        {
            SetActive(spinButton?.gameObject, true);
            if (spinButton != null) spinButton.interactable = true;
        });
    }

    /// Кнопка "Крутить"
    /// Подключить: SpinButton → OnClick → OnSpinClicked()
    public void OnSpinClicked()
    {
        if (_isSpinning) return;
        StartCoroutine(SpinRoutine());
    }

    /// Кнопка "Сломать автомат" (внутри ChoicePanel)
    /// Подключить: BreakButton → OnClick → OnBreakClicked()
    public void OnBreakClicked()
    {
        HidePanel(choicePanel, instant: false);
        PlaySound(breakSound);
        GameManager.Instance.gameState.AddToken(TokenType.Revolt);
        Debug.Log("[SlotMachine] Сломала → +1 Revolt");
        ShowKeyResult(TXT_WIN_BREAK);
    }

    /// Кнопка "Крутить ещё" (внутри ChoicePanel)
    /// Подключить: ContinueButton → OnClick → OnContinueClicked()
    public void OnContinueClicked()
    {
        if (!_chosenToContinue)
        {
            _chosenToContinue = true;
            GameManager.Instance.gameState.AddToken(TokenType.Obedience);
            Debug.Log("[SlotMachine] Выбрала продолжить → +1 Obedience");
        }

        // Скрываем выбор, возвращаем кнопку Крутить
        HidePanel(choicePanel, instant: false, onDone: () =>
        {
            SetActive(spinButton?.gameObject, true);
            if (spinButton != null) spinButton.interactable = true;
        });
    }

    /// Кнопка "Продолжить" на финальной панели
    /// Подключить: Button_Continue → OnClick → OnResultContinueClicked()
    public void OnResultContinueClicked()
    {
        GameManager.Instance.LoadNextScene();
    }

    // ── Логика вращения ───────────────────────────────

    private IEnumerator SpinRoutine()
    {
        _isSpinning = true;
        if (spinButton != null) spinButton.interactable = false;

        _spinCount++;
        Debug.Log($"[SlotMachine] Кручение #{_spinCount}");

        bool isForcedJackpot = (_spinCount >= 6);

        if (isForcedJackpot)
        {
            int sym = Random.Range(0, symbols.Length);
            _result[0] = _result[1] = _result[2] = sym;
            Debug.Log($"[SlotMachine] Принудительный джекпот: символ {sym}");
        }
        else
        {
            for (int i = 0; i < 3; i++)
                _result[i] = Random.Range(0, symbols.Length);
        }

        PlaySound(spinSound);

        foreach (var reel in reels)
            reel.StartSpin();

        // Обычная остановка для всех кручений включая 6-е
        // На 6-м просто результат предопределён — визуально неотличимо
        for (int i = 0; i < reels.Length; i++)
        {
            yield return new WaitForSeconds(reelStopDelay);
            reels[i].StopSpin(_result[i]);
        }
        yield return new WaitUntil(() => !reels[reels.Length - 1].IsSpinning);

        yield return new WaitForSeconds(resultDelay);

        _isSpinning = false;

        bool isJackpot = (_result[0] == _result[1] && _result[1] == _result[2]);

        if (isJackpot)
        {
            OnJackpot();
        }
        else
        {
            // После 3-го кручения — показать выбор. До — вернуть кнопку Крутить.
            if (_spinCount >= 3)
            {
                ShowChoicePanel();
            }
            else
            {
                if (spinButton != null) spinButton.interactable = true;
            }
        }
    }

    private void OnJackpot()
    {
        PlaySound(jackpotSound);
        foreach (var reel in reels)
            reel.PlayJackpotEffect();

        if (!_chosenToContinue)
        {
            GameManager.Instance.gameState.AddToken(TokenType.Revolt);
            Debug.Log("[SlotMachine] Джекпот (ранний) → +1 Revolt");
        }
        else
        {
            Debug.Log("[SlotMachine] Джекпот (после продолжения) → токен не добавляем");
        }

        string msg = _spinCount >= 6 ? TXT_WIN_LATE : TXT_WIN_EARLY;
        ShowKeyResult(msg);
    }

    private void ShowChoicePanel()
    {
        Debug.Log("[SlotMachine] ShowChoicePanel");
        // Скрываем SpinButton, показываем ChoicePanel
        SetActive(spinButton?.gameObject, false);
        ShowPanel(choicePanel, instant: false);
    }

    private void ShowKeyResult(string text)
    {
        PlaySound(doorSound);
        if (resultText != null) resultText.text = text;
        ShowPanel(resultPanel, instant: false);
    }

    // ── Утилиты ───────────────────────────────────────

    private void PlaySound(AudioClip clip)
    {
        if (_audio != null && clip != null)
            _audio.PlayOneShot(clip);
    }

    private void SetActive(GameObject obj, bool state)
    {
        if (obj != null) obj.SetActive(state);
    }

    private void ShowPanel(CanvasGroup g, bool instant, System.Action onDone = null)
    {
        if (g == null) { onDone?.Invoke(); return; }
        g.gameObject.SetActive(true);
        if (instant)
        {
            g.alpha = 1f; g.interactable = true; g.blocksRaycasts = true;
            onDone?.Invoke();
        }
        else
        {
            g.alpha = 0f; g.interactable = false; g.blocksRaycasts = false;
            g.DOFade(1f, 0.4f).SetUpdate(true).OnComplete(() =>
            {
                g.interactable = true; g.blocksRaycasts = true;
                onDone?.Invoke();
            });
        }
    }

    private void HidePanel(CanvasGroup g, bool instant, System.Action onDone = null)
    {
        if (g == null) { onDone?.Invoke(); return; }
        if (instant)
        {
            g.alpha = 0f; g.interactable = false; g.blocksRaycasts = false;
            g.gameObject.SetActive(false); onDone?.Invoke();
        }
        else
        {
            g.interactable = false; g.blocksRaycasts = false;
            g.DOFade(0f, 0.35f).SetUpdate(true).OnComplete(() =>
            {
                g.gameObject.SetActive(false); onDone?.Invoke();
            });
        }
    }
}
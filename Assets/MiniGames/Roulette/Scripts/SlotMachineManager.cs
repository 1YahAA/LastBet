using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SlotMachineManager : MonoBehaviour
{
    [Header("━━ Барабаны ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    public ReelController[] reels = new ReelController[3];

    [Header("━━ Символы ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    public Sprite[] symbols = new Sprite[5];

    [Header("━━ Кнопки ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    public Button spinButton;
    public Button breakButton;
    public Button continueSpinButton;

    [Header("━━ Панель выбора ━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    public CanvasGroup choicePanel;

    [Header("━━ Диалоги ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    public DialogueTrigger dialogueTrigger;

    [Header("━━ Звуки ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    public AudioClip spinSound;
    public AudioClip jackpotSound;
    public AudioClip breakSound;
    public AudioClip doorSound;

    [Header("━━ Настройки ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    public float reelStopDelay = 0.5f;
    public float resultDelay = 0.8f;

    private AudioSource _audio;
    private bool _isSpinning = false;
    private int _spinCount = 0;
    private bool _chosenToContinue = false;
    private int[] _result = new int[3];

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        foreach (var reel in reels)
            reel.Initialize(symbols);

        HidePanel(choicePanel, instant: true);

        if (spinButton != null) spinButton.interactable = true;
    }

 
    public void OnSpinClicked()
    {
        if (_isSpinning) return;
        StartCoroutine(SpinRoutine());
    }

    public void OnBreakClicked()
    {
        HidePanel(choicePanel, instant: false);
        PlaySound(breakSound);
        GameManager.Instance.gameState.AddToken(TokenType.Revolt);
        Debug.Log("[SlotMachine] Сломала → +1 Revolt");
        StartCoroutine(PlayWinDialogueAndReturn("SlotMachine_WinBreak"));
    }

    public void OnContinueSpinClicked()
    {
        if (!_chosenToContinue)
        {
            _chosenToContinue = true;
            GameManager.Instance.gameState.AddToken(TokenType.Obedience);
            Debug.Log("[SlotMachine] Продолжила → +1 Obedience");
        }

        HidePanel(choicePanel, instant: false, onDone: () =>
        {
            if (spinButton != null) spinButton.interactable = true;
        });
    }

    private IEnumerator SpinRoutine()
    {
        _isSpinning = true;
        if (spinButton != null) spinButton.interactable = false;

        _spinCount++;

        if (_spinCount >= 6)
        {
            int sym = Random.Range(0, symbols.Length);
            _result[0] = _result[1] = _result[2] = sym;
        }
        else
        {
            for (int i = 0; i < 3; i++)
                _result[i] = Random.Range(0, symbols.Length);
        }

        PlaySound(spinSound);

        foreach (var reel in reels)
            reel.StartSpin();

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
            OnJackpot();
        else if (_spinCount >= 3)
            ShowChoicePanel();
        else if (spinButton != null)
            spinButton.interactable = true;
    }

    private void OnJackpot()
    {
        PlaySound(jackpotSound);

        if (!_chosenToContinue)
        {
            GameManager.Instance.gameState.AddToken(TokenType.Revolt);
            Debug.Log("[SlotMachine] Джекпот → +1 Revolt");
        }

        StartCoroutine(JackpotFlash());

        string node = _spinCount >= 6 ? "SlotMachine_WinLate" : "SlotMachine_WinEarly";
        StartCoroutine(PlayWinDialogueAndReturn(node));
    }

    private void ShowChoicePanel()
    {
        if (spinButton != null) spinButton.gameObject.SetActive(false);
        ShowPanel(choicePanel, instant: false);
    }

    private IEnumerator PlayWinDialogueAndReturn(string nodeName)
    {
        PlaySound(doorSound);
        yield return new WaitForSeconds(0.5f);

        if (dialogueTrigger != null)
        {
            dialogueTrigger.StartDialogueNode(nodeName);
            yield return new WaitForSeconds(0.2f);
            while (GameManager.Instance != null && GameManager.Instance.IsInDialogue)
                yield return null;
        }

        GameManager.Instance.ReturnFromMiniGame();
    }

    private IEnumerator JackpotFlash()
    {
        for (int i = 0; i < 6; i++)
        {
            foreach (var reel in reels)
                foreach (var img in reel.symbolImages)
                    if (img != null) img.color = new Color(1f, 0.85f, 0.2f);

            yield return new WaitForSeconds(0.15f);

            foreach (var reel in reels)
                foreach (var img in reel.symbolImages)
                    if (img != null) img.color = Color.white;

            yield return new WaitForSeconds(0.15f);
        }
    }


    private void PlaySound(AudioClip clip)
    {
        if (_audio != null && clip != null) _audio.PlayOneShot(clip);
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
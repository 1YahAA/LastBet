using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CabaretDirector : MonoBehaviour
{
    [Header("Кулисы")]
    public Animator curtainsAnimator;
    public float curtainOpenDuration = 1.5f;

    [Header("Фоны")]
    public GameObject bgAudienceScene;
    public GameObject bgAudience;

    [Header("Эвелин")]
    public GameObject evelynObject;

    [Header("Моргание")]
    public Image blinkOverlay;
    public float fadeCloseDuration = 0.3f;
    public float pauseAtBlack = 0.2f;
    public float fadeOpenDuration = 1.0f;

    [Header("Зрители")]
    public AudienceButton[] audienceButtons;
    public int thoughtsToTrigger = 3;

    [Header("Виктор и Лео")]
    public GameObject bgViktorFocus;
    public GameObject bgLeoFocus;
    public Button buttonViktor;
    public Button buttonLeo;

    [Header("UI")]
    public Button continueButton;

    [Header("Музыка")]
    public AudioSource musicSource;

    [Header("Диалоги")]
    public DialogueTrigger dialogueTrigger;

    private int _thoughtsRead = 0;
    private bool _analysisGiven = false;
    private bool _viktorViewed = false;
    private bool _leoViewed = false;

    void Start()
    {
        InitialState();
        StartCoroutine(RunScene());
    }

    void InitialState()
    {
        if (bgAudienceScene != null) bgAudienceScene.SetActive(false);
        if (bgAudience != null) bgAudience.SetActive(false);
        if (bgViktorFocus != null) bgViktorFocus.SetActive(false);
        if (bgLeoFocus != null) bgLeoFocus.SetActive(false);

        SetAudienceActive(false);

        if (buttonViktor != null) buttonViktor.gameObject.SetActive(false);
        if (buttonLeo != null) buttonLeo.gameObject.SetActive(false);

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (blinkOverlay != null)
        {
            blinkOverlay.color = new Color(0, 0, 0, 0);
            blinkOverlay.raycastTarget = false;
        }

        if (musicSource != null) { musicSource.loop = true; musicSource.Play(); }
    }

    IEnumerator RunScene()
    {
        yield return new WaitForSeconds(1f);

        if (bgAudienceScene != null) bgAudienceScene.SetActive(true);
        if (evelynObject != null) evelynObject.SetActive(true);

        if (curtainsAnimator != null && curtainsAnimator.runtimeAnimatorController != null)
            curtainsAnimator.SetTrigger("Open");

        yield return new WaitForSeconds(curtainOpenDuration);

        yield return new WaitForSeconds(0.3f);
        dialogueTrigger.StartDialogueNode("Cabaret_Song");

        yield return new WaitForSeconds(0.2f);
        while (GameManager.Instance != null && GameManager.Instance.IsInDialogue)
            yield return null;

        yield return StartCoroutine(BlinkTransition());

        EnableAudiencePhase();
    }

    IEnumerator BlinkTransition()
    {
        if (blinkOverlay == null) { SwapBackgrounds(); yield break; }

        blinkOverlay.raycastTarget = true;
        yield return StartCoroutine(FadeOverlay(0f, 1f, fadeCloseDuration));

        SwapBackgrounds();
        yield return new WaitForSeconds(pauseAtBlack);

        yield return StartCoroutine(FadeOverlay(1f, 0f, fadeOpenDuration));
        blinkOverlay.raycastTarget = false;
    }

    void SwapBackgrounds()
    {
        if (bgAudienceScene != null) bgAudienceScene.SetActive(false);
        if (evelynObject != null) evelynObject.SetActive(false);
        if (curtainsAnimator != null) curtainsAnimator.gameObject.SetActive(false);
        if (bgAudience != null) bgAudience.SetActive(true);
    }

    IEnumerator FadeOverlay(float from, float to, float duration)
    {
        float elapsed = 0f;
        blinkOverlay.color = new Color(0, 0, 0, from);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blinkOverlay.color = new Color(0, 0, 0,
                Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        blinkOverlay.color = new Color(0, 0, 0, to);
    }

    void EnableAudiencePhase()
    {
        SetupAudienceButtons();
        SetAudienceActive(true);

        if (buttonViktor != null)
        {
            buttonViktor.gameObject.SetActive(true);
            buttonViktor.onClick.RemoveAllListeners();
            buttonViktor.onClick.AddListener(OnViktorClicked);
        }
        if (buttonLeo != null)
        {
            buttonLeo.gameObject.SetActive(true);
            buttonLeo.onClick.RemoveAllListeners();
            buttonLeo.onClick.AddListener(OnLeoClicked);
        }
    }

    void SetupAudienceButtons()
    {
        if (audienceButtons == null) return;
        foreach (var ab in audienceButtons)
        {
            if (ab == null) continue;
            ab.gameObject.SetActive(true);
            ab.GetComponent<Button>().onClick.AddListener(() => OnAudienceThoughtShown());
        }
    }

    void OnAudienceThoughtShown()
    {
        if (_analysisGiven) return;
        _thoughtsRead++;
        if (_thoughtsRead >= thoughtsToTrigger)
        {
            _analysisGiven = true;
            StartCoroutine(TriggerAnalysisEffect());
        }
    }

    IEnumerator TriggerAnalysisEffect()
    {
        HideAllBubbles();
        yield return new WaitForSeconds(0.5f);
        dialogueTrigger.StartDialogueNode("Cabaret_Audience");
        yield return new WaitForSeconds(0.2f);
        while (GameManager.Instance != null && GameManager.Instance.IsInDialogue)
            yield return null;
        CheckAllViewed();
    }

    void HideAllBubbles()
    {
        if (audienceButtons == null) return;
        foreach (var ab in audienceButtons)
            ab?.ForceHide();
    }

    void SetAudienceActive(bool active)
    {
        if (audienceButtons == null) return;
        foreach (var ab in audienceButtons)
            if (ab != null) ab.gameObject.SetActive(active);
    }

    void OnViktorClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInDialogue) return;
        if (_viktorViewed) return;
        HideAllBubbles();
        StartCoroutine(PlayWithBG("Cabaret_Viktor", bgViktorFocus, OnViktorDone));
    }

    void OnLeoClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInDialogue) return;
        if (_leoViewed) return;
        HideAllBubbles();
        StartCoroutine(PlayWithBG("Cabaret_Leo", bgLeoFocus, OnLeoDone));
    }

    IEnumerator PlayWithBG(string node, GameObject charBG, System.Action onDone)
    {
        if (bgAudience != null) bgAudience.SetActive(false);
        if (charBG != null) charBG.SetActive(true);

        dialogueTrigger.StartDialogueNode(node);
        yield return new WaitForSeconds(0.2f);
        while (GameManager.Instance != null && GameManager.Instance.IsInDialogue)
            yield return null;

        if (charBG != null) charBG.SetActive(false);
        if (bgAudience != null) bgAudience.SetActive(true);

        onDone?.Invoke();
    }

    void OnViktorDone() { _viktorViewed = true; CheckAllViewed(); }
    void OnLeoDone() { _leoViewed = true; CheckAllViewed(); }

    void CheckAllViewed()
    {
        if (!_viktorViewed || !_leoViewed) return;
        if (continueButton != null)
            continueButton.gameObject.SetActive(true);
    }

    public void OnContinueClicked()
    {
        if (musicSource != null) musicSource.Stop();
        GameManager.Instance.LoadNextScene();
    }
}
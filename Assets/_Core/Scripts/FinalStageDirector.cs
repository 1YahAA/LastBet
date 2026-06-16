using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class FinalStageDirector : MonoBehaviour
{
    [Header("Фоны")]
    public GameObject bgStage;
    public GameObject bgAudience;

    [Header("Кулисы")]
    public GameObject curtainsObject;
    public Animator curtainsAnimator;
    public float curtainOpenDuration = 1.5f;

    [Header("Зал — мысли гостей")]
    public AudienceButton[] audienceButtons;
    private int _audienceThoughtsRead = 0;

    [Header("Зал — голоса при поражении")]
    public GameObject defeatVoicesPanel;

    [Header("Финальные кадры")]
    public GameObject bgEndingAwareness1;
    public GameObject bgEndingAwareness2;
    public GameObject bgEndingSubmission;
    public GameObject bgEndingFreedom;
    public GameObject bgEndingAwareness;

    [Header("Эффекты")]
    public Image blackOverlay;
    public AudioSource audienceSound;
    public AudioSource clockSound;
    public AudioSource applauseSound;
    public AudioSource doorOpenSound;

    [Header("Диалоги")]
    public DialogueTrigger dialogueTrigger;
    public DialogueRunner dialogueRunner;

    private static bool _returnedFromMiniGame = false;
    private static bool _miniGameWon = false;
    private bool _weakReplyShown = false;
    private string _pendingEnding = "";

    void Start()
    {
        HideAll();

        bool returned = _returnedFromMiniGame;
        _returnedFromMiniGame = false;

        if (returned)
            StartCoroutine(AfterMiniGame(_miniGameWon));
        else
            StartCoroutine(RunScene());
    }

    void HideAll()
    {
        SetActive(bgStage, false);
        SetActive(bgAudience, false);
        SetActive(defeatVoicesPanel, false);
        SetActive(bgEndingAwareness1, false);
        SetActive(bgEndingAwareness2, false);
        SetActive(bgEndingSubmission, false);
        SetActive(bgEndingFreedom, false);
        SetActive(bgEndingAwareness, false);
        SetActive(curtainsObject, false);

        foreach (var ab in audienceButtons)
            if (ab != null) ab.gameObject.SetActive(false);

        if (blackOverlay != null)
        {
            blackOverlay.color = new Color(0, 0, 0, 1);
            blackOverlay.raycastTarget = false;
        }
    }

    IEnumerator RunScene()
    {
        if (audienceSound != null) audienceSound.Play();
        if (clockSound != null) clockSound.Play();

        yield return new WaitForSeconds(2f);

        SetActive(bgStage, true);
        SetActive(curtainsObject, true);

        if (curtainsAnimator != null && curtainsAnimator.runtimeAnimatorController != null)
            curtainsAnimator.SetTrigger("Open");

        yield return FadeOverlay(1f, 0f, 1.0f);
        yield return new WaitForSeconds(curtainOpenDuration);

        dialogueTrigger.StartDialogueNode("FinalStage_VictorIntro");
        yield return WaitDialogue();
    }

    IEnumerator AfterMiniGame(bool won)
    {
        SetActive(bgStage, true);
        yield return FadeOverlay(1f, 0f, 0.6f);

        if (won)
        {
            GameManager.Instance.gameState.ApplyFinalBetResult(true);
            dialogueTrigger.StartDialogueNode("FinalStage_Win");
            yield return WaitDialogue();
        }
        else
        {
            GameManager.Instance.gameState.ApplyFinalBetResult(false);

            yield return FadeOverlay(0f, 1f, 0.3f);
            SetActive(bgStage, false);
            SetActive(bgAudience, true);
            SetActive(defeatVoicesPanel, true);
            yield return FadeOverlay(1f, 0f, 0.4f);

            yield return new WaitForSeconds(2.5f);

            yield return FadeOverlay(0f, 1f, 0.3f);
            SetActive(bgAudience, false);
            SetActive(defeatVoicesPanel, false);
            SetActive(bgStage, true);
            yield return FadeOverlay(1f, 0f, 0.4f);

            dialogueTrigger.StartDialogueNode("FinalStage_Lose");
            yield return WaitDialogue();
        }

        _pendingEnding = "";
        SetupYarnVariables();
        dialogueTrigger.StartDialogueNode("FinalStage_FinalChoice");
        yield return WaitDialogue();
        yield return StartCoroutine(PlayPendingEnding());
    }

    
    public void SetPendingEnding(string ending)
    {
        _pendingEnding = ending;
    }

    IEnumerator PlayPendingEnding()
    {
        switch (_pendingEnding)
        {
            case "submission": yield return StartCoroutine(PlayEndingSubmission()); break;
            case "freedom": yield return StartCoroutine(PlayEndingFreedom()); break;
            case "awareness": yield return StartCoroutine(PlayEndingAwareness()); break;
            default:
                Debug.LogWarning("[FinalStage] Концовка не задана.");
                break;
        }
    }


    void SetupYarnVariables()
    {
        if (dialogueRunner == null) return;
        var storage = dialogueRunner.GetComponent<InMemoryVariableStorage>();
        if (storage == null) return;

        var gs = GameManager.Instance.gameState;

        bool showStay = gs.obedience >= 5
            || gs.obedience > gs.revolt + gs.analysis
            || (!gs.finalBetWon && gs.obedience >= 4);

        bool showLeave = gs.revolt >= 5
            || gs.revolt > gs.obedience + gs.analysis
            || (gs.revolt >= 4 && gs.finalBetWon);

        bool showTruth = gs.analysis >= 7
            || gs.jokerWon
            || (gs.cocktailInspected && gs.midnightPlanKnown && gs.analysis >= 5)
            || (gs.finalBetWon && gs.analysis >= 5);

        bool showCocktail = (gs.cocktailDrunk || gs.cocktailInspected || gs.analysis >= 3)
            && !_weakReplyShown;

        if (!showStay && !showLeave && !showTruth) showStay = true;

        storage.SetValue("$show_stay", showStay);
        storage.SetValue("$show_leave", showLeave);
        storage.SetValue("$show_truth", showTruth);
        storage.SetValue("$show_cocktail", showCocktail);
    }

    public void OnWeakReplyDone()
    {
        _weakReplyShown = true;
        StartCoroutine(RestartFinalChoice());
    }

    IEnumerator RestartFinalChoice()
    {
        yield return null;
        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);
        _pendingEnding = "";
        SetupYarnVariables();
        dialogueTrigger.StartDialogueNode("FinalStage_FinalChoice");
        yield return WaitDialogue();
        yield return StartCoroutine(PlayPendingEnding());
    }

    public IEnumerator ShowAudienceForFirstChoice()
    {
        yield return FadeOverlay(0f, 1f, 0.3f);
        SetActive(bgStage, false);
        SetActive(bgAudience, true);
        yield return FadeOverlay(1f, 0f, 0.4f);

        _audienceThoughtsRead = 0;
        foreach (var ab in audienceButtons)
        {
            if (ab == null) continue;
            ab.gameObject.SetActive(true);
            ab.GetComponent<Button>()?.onClick.AddListener(OnAudienceThoughtRead);
        }

        int total = audienceButtons != null ? audienceButtons.Length : 3;
        yield return new WaitUntil(() => _audienceThoughtsRead >= total);

        foreach (var ab in audienceButtons)
            if (ab != null) { ab.ForceHide(); ab.gameObject.SetActive(false); }

        yield return FadeOverlay(0f, 1f, 0.3f);
        SetActive(bgAudience, false);
        SetActive(bgStage, true);
        yield return FadeOverlay(1f, 0f, 0.4f);
    }

    void OnAudienceThoughtRead() => _audienceThoughtsRead++;


    IEnumerator PlayEndingSubmission()
    {
        if (applauseSound != null) applauseSound.Play();

        dialogueTrigger.StartDialogueNode("Ending_Submission");
        yield return WaitDialogue();

        yield return FadeOverlay(0f, 1f, 0.8f);
        SetActive(bgStage, false);
        SetActive(bgEndingSubmission, true);
        yield return FadeOverlay(1f, 0f, 0.8f);
    }

    IEnumerator PlayEndingFreedom()
    {
        dialogueTrigger.StartDialogueNode("Ending_Freedom");
        yield return WaitDialogue();

        yield return FadeOverlay(0f, 1f, 0.8f);
        if (doorOpenSound != null) doorOpenSound.Play();
        SetActive(bgStage, false);
        SetActive(bgEndingFreedom, true);
        yield return FadeOverlay(1f, 0f, 0.8f);
    }

    IEnumerator PlayEndingAwareness()
    {
        dialogueTrigger.StartDialogueNode("Ending_Truth_Part1");
        yield return WaitDialogue();

        yield return FadeOverlay(0f, 1f, 0.4f);
        SetActive(bgStage, false);
        SetActive(bgEndingAwareness1, true);
        yield return FadeOverlay(1f, 0f, 0.5f);

        dialogueTrigger.StartDialogueNode("Ending_Truth_Part2");
        yield return WaitDialogue();

        yield return FadeOverlay(0f, 1f, 0.4f);
        SetActive(bgEndingAwareness1, false);
        SetActive(bgEndingAwareness2, true);
        yield return FadeOverlay(1f, 0f, 0.5f);

        dialogueTrigger.StartDialogueNode("Ending_Truth_Part3");
        yield return WaitDialogue();

        yield return FadeOverlay(0f, 1f, 0.4f);
        SetActive(bgEndingAwareness2, false);
        SetActive(bgStage, true);
        yield return FadeOverlay(1f, 0f, 0.5f);

        dialogueTrigger.StartDialogueNode("Ending_Truth_Part4");
        yield return WaitDialogue();

        yield return FadeOverlay(0f, 1f, 0.8f);
        SetActive(bgStage, false);
        SetActive(bgEndingAwareness, true);
        yield return FadeOverlay(1f, 0f, 0.8f);
    }

    public static void SetMiniGameResult(bool won)
    {
        _returnedFromMiniGame = true;
        _miniGameWon = won;
    }

    IEnumerator WaitDialogue()
    {
        yield return new WaitForSeconds(0.2f);
        while (GameManager.Instance != null && GameManager.Instance.IsInDialogue)
            yield return null;
    }

    IEnumerator FadeOverlay(float from, float to, float duration)
    {
        if (blackOverlay == null) yield break;
        float elapsed = 0f;
        blackOverlay.color = new Color(0, 0, 0, from);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blackOverlay.color = new Color(0, 0, 0,
                Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        blackOverlay.color = new Color(0, 0, 0, to);
    }

    void SetActive(GameObject obj, bool active) { if (obj != null) obj.SetActive(active); }
}
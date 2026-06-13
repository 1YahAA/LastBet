using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BarDirector : MonoBehaviour
{
    [Header("Фаза 1: Бар")]
    public GameObject bgBar;
    public GameObject leo;

    [Header("Фаза 2: После мини-игры")]
    public GameObject bgBarCounter;
    public GameObject leoAfter;
    public GameObject bgAfterGame;
    public GameObject key;
    public GameObject keyClick;
    public Button     buttonKey;

    [Header("Эффекты")]
    public Image blackOverlay;

    [Header("Диалоги")]
    public DialogueTrigger dialogueTrigger;

    private static bool _returnedFromMiniGame = false;
    private static bool _miniGameWon          = false;

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
        SetActive(bgBar,         false);
        SetActive(leo,           false);
        SetActive(bgBarCounter,  false);
        SetActive(leoAfter,      false);
        SetActive(bgAfterGame,   false);
        SetActive(key,           false);
        SetActive(keyClick,      false);
        SetBtn(buttonKey,        false);

        if (blackOverlay != null)
        {
            blackOverlay.color = new Color(0, 0, 0, 0);
            blackOverlay.raycastTarget = false;
        }
    }

    IEnumerator RunScene()
    {
        SetActive(bgBar, true);
        SetActive(leo,   true);

        yield return new WaitForSeconds(0.5f);
        dialogueTrigger.StartDialogueNode("Bar_Intro");
        yield return WaitDialogue();

        dialogueTrigger.StartDialogueNode("Bar_Choice");
        yield return WaitDialogue();

        dialogueTrigger.StartDialogueNode("Bar_BeforeGame");
        yield return WaitDialogue();
    }

    IEnumerator AfterMiniGame(bool won)
    {
        yield return FadeOverlay(0f, 1f, 0.4f);

        SetActive(bgBar,        false);
        SetActive(leo,          false);
        SetActive(bgBarCounter, true);
        SetActive(leoAfter,     true);
        SetActive(bgAfterGame,  true);

        yield return FadeOverlay(1f, 0f, 0.6f);
        if (blackOverlay != null) blackOverlay.raycastTarget = false;

        string node = won ? "Bar_WinResult" : "Bar_LoseResult";
        dialogueTrigger.StartDialogueNode(node);
        yield return WaitDialogue();

        SetActive(key, true);
        SetBtn(buttonKey, true);
        buttonKey.onClick.AddListener(OnKeyClicked);
    }

    void OnKeyClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInDialogue) return;

        SetActive(key,      false);
        SetActive(keyClick, true);

        StartCoroutine(AfterKeyTaken());
    }

    IEnumerator AfterKeyTaken()
    {
        yield return new WaitForSeconds(0.5f);

        dialogueTrigger.StartDialogueNode("Bar_FinalChoice");
        yield return WaitDialogue();

        yield return FadeOverlay(0f, 1f, 0.5f);
        GameManager.Instance.LoadNextScene();
    }

    public static void SetMiniGameResult(bool won)
    {
        _returnedFromMiniGame = true;
        _miniGameWon          = won;
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
        blackOverlay.raycastTarget = true;
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

    void SetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }

    void SetBtn(Button btn, bool active)
    {
        if (btn != null) btn.gameObject.SetActive(active);
    }
}

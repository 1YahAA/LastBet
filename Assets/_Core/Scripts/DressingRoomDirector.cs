using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DressingRoomDirector : MonoBehaviour
{
    [Header("=== ФАЗА 1: ЗЕРКАЛО ===")]
    public GameObject bgMirror;
    public Animator mirrorAnimator;
    public float approachDuration = 1.0f;
    public Button buttonTable;

    [Header("=== ФАЗА 2: СТОЛ ===")]
    public GameObject bgTable;
    public Button noteFullscreenButton;
    public Button buttonNote;
    public Button buttonGlass;

    [Header("=== ФАЗА 3: ПОСЛЕ ВЫПИТОГО ===")]
    public GameObject bgMirrorNoNote;
    public GameObject evelynBlood;

    [Header("=== ФАЗА 4: ДВЕРЬ И АВТОМАТ ===")]
    public GameObject bgDoor;
    public Button buttonDoor;
    public Button buttonMachine;

    [Header("=== ЭФФЕКТЫ ===")]
    public Image blackOverlay;
    public AudioSource fallSound;

    [Header("=== ДИАЛОГИ ===")]
    public DialogueTrigger dialogueTrigger;

    private bool _noteOpen = false;
    private bool _cocktailUsed = false;
    private bool _doorAttempted = false;

    private static bool _miniGameJustFinished = false;

    void Start()
    {
        HideAll();

        bool returnedFromMini = _miniGameJustFinished;
        _miniGameJustFinished = false;

        if (returnedFromMini)
        {
            Debug.Log("[Dressing] Вернулись из мини-игры → дверь");
            StartDoorPhase(machineUsed: true);
        }
        else
        {
            Debug.Log("[Dressing] Обычный старт → зеркало");
            StartCoroutine(RunScene());
        }
    }

    void HideAll()
    {
        SetActive(bgMirror, false);
        SetActive(bgTable, false);
        SetActive(bgMirrorNoNote, false);
        SetActive(evelynBlood, false);
        SetActive(bgDoor, false);

        SetBtn(buttonTable, false);
        SetBtn(buttonNote, false);
        SetBtn(buttonGlass, false);
        SetBtn(noteFullscreenButton, false);
        SetBtn(buttonDoor, false);
        SetBtn(buttonMachine, false);

        if (blackOverlay != null)
        {
            blackOverlay.color = new Color(0, 0, 0, 0);
            blackOverlay.raycastTarget = false;
        }
    }


    IEnumerator RunScene()
    {
        SetActive(bgMirror, true);

        yield return new WaitForSeconds(0.5f);
        dialogueTrigger.StartDialogueNode("Dressing_Intro");
        yield return WaitDialogue();

        SetBtn(buttonTable, true);
        buttonTable.onClick.AddListener(OnTableClicked);
    }

    void OnTableClicked()
    {
        SetBtn(buttonTable, false);
        StartCoroutine(ApproachAndShowTable());
    }

    IEnumerator ApproachAndShowTable()
    {
        if (mirrorAnimator != null && mirrorAnimator.runtimeAnimatorController != null)
            mirrorAnimator.SetTrigger("Approach");

        yield return new WaitForSeconds(approachDuration);

        SetActive(bgMirror, false);
        SetActive(bgTable, true);

        SetBtn(buttonNote, true);
        SetBtn(buttonGlass, true);

        buttonNote.onClick.AddListener(OnNoteClicked);
        buttonGlass.onClick.AddListener(OnGlassClicked);
    }

    void OnNoteClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInDialogue) return;
        if (_noteOpen) return;

        _noteOpen = true;
        SetBtn(noteFullscreenButton, true);
        noteFullscreenButton.onClick.RemoveAllListeners();
        noteFullscreenButton.onClick.AddListener(OnNoteClose);
    }

    void OnNoteClose()
    {
        _noteOpen = false;
        SetBtn(noteFullscreenButton, false);
        dialogueTrigger.StartDialogueNode("Dressing_Note");
    }

    void OnGlassClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInDialogue) return;
        if (_cocktailUsed) return;
        _cocktailUsed = true;

        SetBtn(buttonGlass, false);
        StartCoroutine(CocktailFlow());
    }

    IEnumerator CocktailFlow()
    {
        dialogueTrigger.StartDialogueNode("Dressing_Cocktail");
        yield return WaitDialogue();

        bool drunk = GameManager.Instance != null &&
                     GameManager.Instance.gameState.cocktailDrunk;

        if (drunk)
            yield return StartCoroutine(DrunkEffect());
        else
            StartCoroutine(FadeToDoor());
    }

    IEnumerator DrunkEffect()
    {
        yield return new WaitForSeconds(0.4f);

        yield return FadeOverlay(0f, 1f, 0.4f);

        if (fallSound != null)
            fallSound.Play();
        else
            Debug.LogWarning("[Dressing] fallSound не назначен!");

        yield return new WaitForSeconds(0.8f);

        SetActive(bgTable, false);
        SetActive(bgMirrorNoNote, false);
        SetBtn(buttonNote, false);
        SetBtn(buttonGlass, false);
        SetActive(bgMirror, true);
        SetActive(evelynBlood, true);

        yield return FadeOverlay(1f, 0f, 1.2f);
        if (blackOverlay != null) blackOverlay.raycastTarget = false;

        dialogueTrigger.StartDialogueNode("Dressing_AfterDrunk");
        yield return WaitDialogue();

        yield return FadeOverlay(0f, 1f, 0.4f);
        SetActive(bgMirror, false);
        SetActive(evelynBlood, false);
        StartDoorPhase(machineUsed: false);
        yield return FadeOverlay(1f, 0f, 0.6f);
        if (blackOverlay != null) blackOverlay.raycastTarget = false;
    }


    IEnumerator FadeToDoor()
    {
        yield return FadeOverlay(0f, 1f, 0.4f);

        SetActive(bgTable, false);
        SetActive(bgMirror, false);
        SetBtn(buttonNote, false);
        SetBtn(buttonGlass, false);

        StartDoorPhase(machineUsed: false);

        yield return FadeOverlay(1f, 0f, 0.6f);
        if (blackOverlay != null) blackOverlay.raycastTarget = false;
    }

    void StartDoorPhase(bool machineUsed)
    {
        SetActive(bgDoor, true);

        SetBtn(buttonDoor, true);
        buttonDoor.onClick.RemoveAllListeners();
        buttonDoor.onClick.AddListener(OnDoorClicked);

        SetBtn(buttonMachine, !machineUsed);
        if (!machineUsed)
        {
            buttonMachine.onClick.RemoveAllListeners();
            buttonMachine.onClick.AddListener(OnMachineClicked);
        }
    }

    void OnDoorClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInDialogue) return;

        bool machineUsed = buttonMachine == null ||
                           !buttonMachine.gameObject.activeSelf;

        if (machineUsed)
            StartCoroutine(PlayAndLeave("Dressing_Door_Unlocked"));
        else if (!_doorAttempted)
        {
            _doorAttempted = true;
            dialogueTrigger.StartDialogueNode("Dressing_Door_Locked");
        }
        else
            dialogueTrigger.StartDialogueNode("Dressing_Door_Remind");
    }

    void OnMachineClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInDialogue) return;
        _miniGameJustFinished = true;
        dialogueTrigger.StartDialogueNode("Dressing_Machine_Intro");
    }

    IEnumerator PlayAndLeave(string node)
    {
        dialogueTrigger.StartDialogueNode(node);
        yield return WaitDialogue();
        GameManager.Instance.LoadNextScene();
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

    public void EnableDoor() { }
}
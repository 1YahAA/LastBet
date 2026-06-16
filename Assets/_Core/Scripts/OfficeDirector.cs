using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OfficeDirector : MonoBehaviour
{
    [Header("Кадр 4.1 — Кабинет")]
    public GameObject bgOffice;

    [Header("Кадр 4.1 — Интерактивные предметы")]
    public Button buttonPosters;
    public Button buttonCage;
    public Button buttonRecord;
    public Button buttonCards;

    [Header("Кадр 4.2 — Дверь и Виктор")]
    public GameObject bgDoor;
    public GameObject bgDoorShadow;
    public GameObject bgDoorShadowBig;
    public GameObject bgViktorEnter;

    [Header("Эффекты")]
    public Image blackOverlay;
    public AudioSource doorSound;
    public AudioSource heartbeatSound;
    public AudioSource clockSound;

    [Header("Диалоги")]
    public DialogueTrigger dialogueTrigger;

    private bool _postersClicked = false;
    private bool _cageClicked = false;
    private bool _recordClicked = false;
    private bool _cardsClicked = false;

    private static bool _returnedFromMiniGame = false;
    private static bool _miniGameWon = false;

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
        SetActive(bgDoor, false);
        SetActive(bgDoorShadow, false);
        SetActive(bgDoorShadowBig, false);
        SetActive(bgViktorEnter, false);

        SetBtn(buttonPosters, false);
        SetBtn(buttonCage, false);
        SetBtn(buttonRecord, false);
        SetBtn(buttonCards, false);

        if (blackOverlay != null)
        {
            blackOverlay.color = new Color(0, 0, 0, 0);
            blackOverlay.raycastTarget = false;
        }
    }

    // ── КАДР 4.1 ─────────────────────────────────────────────────────────────

    IEnumerator RunScene()
    {
        yield return new WaitForSeconds(0.5f);
        dialogueTrigger.StartDialogueNode("Office_Intro");
        yield return WaitDialogue();
        EnableItems();
    }

    void EnableItems()
    {
        SetBtn(buttonPosters, true);
        SetBtn(buttonCage, true);
        SetBtn(buttonRecord, true);
        SetBtn(buttonCards, true);

        buttonPosters.onClick.AddListener(OnPostersClicked);
        buttonCage.onClick.AddListener(OnCageClicked);
        buttonRecord.onClick.AddListener(OnRecordClicked);
        buttonCards.onClick.AddListener(OnCardsClicked);
    }

    void OnPostersClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInDialogue) return;
        if (_postersClicked) return;
        _postersClicked = true;
        dialogueTrigger.StartDialogueNode("Office_Posters");
    }

    void OnCageClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInDialogue) return;
        if (_cageClicked) return;
        _cageClicked = true;
        dialogueTrigger.StartDialogueNode("Office_Cage");
    }

    void OnRecordClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInDialogue) return;
        if (_recordClicked) return;
        _recordClicked = true;
        dialogueTrigger.StartDialogueNode("Office_Record");
    }

    void OnCardsClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsInDialogue) return;
        if (_cardsClicked) return;
        _cardsClicked = true;

        SetBtn(buttonPosters, false);
        SetBtn(buttonCage, false);
        SetBtn(buttonRecord, false);
        SetBtn(buttonCards, false);

        StartCoroutine(LaunchJoker());
    }

    // Запуск мини-игры ДЖОКЕР (сцена 4, карты на столе)
    IEnumerator LaunchJoker()
    {
        dialogueTrigger.StartDialogueNode("Office_Cards");
        yield return WaitDialogue();

        yield return FadeOverlay(0f, 1f, 0.5f);
        GameManager.Instance.LoadMiniGame("JokerMiniGame", MiniGameType.Roulette);
    }

    // ── ПОСЛЕ МИНИ-ИГРЫ ДЖОКЕР ───────────────────────────────────────────────

    IEnumerator AfterMiniGame(bool won)
    {
        yield return new WaitForSeconds(0.3f);

        string node = won ? "Office_AfterGame_Win" : "Office_AfterGame_Lose";
        dialogueTrigger.StartDialogueNode(node);
        yield return WaitDialogue();

        yield return StartCoroutine(VictorEnters());
    }

    // ── КАДР 4.2 — ВИКТОР ВХОДИТ ─────────────────────────────────────────────

    IEnumerator VictorEnters()
    {
        if (doorSound != null) doorSound.Play();

        yield return FadeOverlay(0f, 1f, 0.4f);
        SetActive(bgOffice, false);
        SetActive(bgDoor, true);
        yield return FadeOverlay(1f, 0f, 0.5f);

        yield return new WaitForSeconds(0.5f);

        yield return FadeOverlay(0f, 1f, 0.3f);
        SetActive(bgDoor, false);
        SetActive(bgDoorShadow, true);
        yield return FadeOverlay(1f, 0f, 0.4f);

        yield return new WaitForSeconds(0.4f);

        yield return FadeOverlay(0f, 1f, 0.3f);
        SetActive(bgDoorShadow, false);
        SetActive(bgDoorShadowBig, true);
        yield return FadeOverlay(1f, 0f, 0.4f);

        yield return new WaitForSeconds(0.3f);

        if (heartbeatSound != null) heartbeatSound.Play();

        yield return FadeOverlay(0f, 1f, 0.3f);
        SetActive(bgDoorShadowBig, false);
        SetActive(bgViktorEnter, true);
        yield return FadeOverlay(1f, 0f, 0.5f);

        dialogueTrigger.StartDialogueNode("Office_Viktor");
        yield return WaitDialogue();

        dialogueTrigger.StartDialogueNode("Office_FinalChoice");
        yield return WaitDialogue();

        if (clockSound != null) clockSound.Play();
        yield return new WaitForSeconds(1.5f);

        yield return FadeOverlay(0f, 1f, 0.6f);
        GameManager.Instance.LoadNextScene();
    }

    public static void SetMiniGameResult(bool won)
    {
        _returnedFromMiniGame = true;
        _miniGameWon = won;
    }

    // ── УТИЛИТЫ ───────────────────────────────────────────────────────────────

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
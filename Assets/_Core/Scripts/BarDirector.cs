using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BarDirector : MonoBehaviour
{
    [Header("Кадр 3.1/3.2 — Общий вид бара")]
    public GameObject bgBar;
    public GameObject leo;

    [Header("Кадр 3.3 — Крупный план стойки")]
    public GameObject bgBarCounter;

    [Header("Кадр 3.3 — Лео с ключом (ключ вшит)")]
    public GameObject bgAfterGame;

    [Header("Эффекты")]
    public Image blackOverlay;

    [Header("Диалоги")]
    public DialogueTrigger dialogueTrigger;

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
        SetActive(bgBar, false);
        SetActive(leo, false);
        SetActive(bgBarCounter, false);
        SetActive(bgAfterGame, false);

        if (blackOverlay != null)
        {
            blackOverlay.color = new Color(0, 0, 0, 0);
            blackOverlay.raycastTarget = false;
        }
    }

    // ── КАДР 3.1 / 3.2 ───────────────────────────────────────────────────────

    IEnumerator RunScene()
    {
        SetActive(bgBar, true);
        SetActive(leo, true);

        yield return new WaitForSeconds(0.5f);

        // Диалог + выбор + <<launch_cocktail_game>> — всё в одном Yarn узле
        dialogueTrigger.StartDialogueNode("Bar_Scene");
        yield return WaitDialogue();
    }

    // ── КАДР 3.3 — после мини-игры ───────────────────────────────────────────

    IEnumerator AfterMiniGame(bool won)
    {
        // Шаг 1: показываем BG_Bar (общий вид) на секунду
        SetActive(bgBar, true);
        SetActive(leo, true);

        yield return new WaitForSeconds(1.0f);

        // Шаг 2: плавно переходим на крупный план стойки
        yield return FadeOverlay(0f, 1f, 0.4f);
        SetActive(bgBar, false);
        SetActive(leo, false);
        SetActive(bgBarCounter, true);
        yield return FadeOverlay(1f, 0f, 0.5f);

        yield return new WaitForSeconds(0.8f);

        // Шаг 3: плавно переходим на Лео с ключом
        yield return FadeOverlay(0f, 1f, 0.4f);
        SetActive(bgBarCounter, false);
        SetActive(bgAfterGame, true);
        yield return FadeOverlay(1f, 0f, 0.5f);

        if (blackOverlay != null) blackOverlay.raycastTarget = false;

        // Диалог в зависимости от результата мини-игры
        string node = won ? "Bar_AfterGame_Win" : "Bar_AfterGame_Lose";
        dialogueTrigger.StartDialogueNode(node);
        yield return WaitDialogue();

        // Переход к сцене 4
        yield return FadeOverlay(0f, 1f, 0.5f);
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
}
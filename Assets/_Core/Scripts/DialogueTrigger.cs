using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Yarn Spinner")]
    public DialogueRunner dialogueRunner;

    [Header("UI")]
    [Tooltip("ContinueButton — Raycast выключен когда диалог не идёт")]
    public Button continueButton;

    [Tooltip("CanvasGroup на Line Presenter — управляем альфой вручную")]
    public CanvasGroup linePresenterCanvasGroup;

    void Start()
    {
        if (dialogueRunner == null)
        {
            return;
        }

        dialogueRunner.onDialogueComplete.AddListener(OnDialogueFinished);

        StartCoroutine(HideOnStart());
    }

    private IEnumerator HideOnStart()
    {
        yield return null;
        SetDialogueVisible(false);
    }

    void OnDestroy()
    {
        if (dialogueRunner != null)
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueFinished);
    }


    public void StartDialogueNode(string nodeName)
    {
        if (dialogueRunner == null) return;

        if (dialogueRunner.IsDialogueRunning)
        {
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnDialogueStart();

        SetDialogueVisible(true);

        dialogueRunner.StartDialogue(nodeName);
    }

    private void OnDialogueFinished()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnDialogueEnd();

        SetDialogueVisible(false);

    }


    private void SetDialogueVisible(bool visible)
    {
        if (linePresenterCanvasGroup != null)
        {
            linePresenterCanvasGroup.alpha = visible ? 1f : 0f;
            linePresenterCanvasGroup.interactable = visible;
            linePresenterCanvasGroup.blocksRaycasts = visible;
        }

        if (continueButton != null)
        {
            continueButton.interactable = visible;
            var img = continueButton.GetComponent<Image>();
            if (img != null) img.raycastTarget = visible;
        }
    }


    [YarnCommand("add_token")]
    public static void YarnAddToken(string tokenName)
    {
        if (System.Enum.TryParse<TokenType>(tokenName, out TokenType tokenType))
            GameManager.Instance.gameState.AddToken(tokenType);
        else
            Debug.LogError($"[DialogueTrigger] Неизвестный жетон: '{tokenName}'");
    }

    [YarnCommand("drink_cocktail")]
    public static void YarnDrinkCocktail()
    {
        GameManager.Instance.gameState.DrinkCocktail();
    }

    [YarnCommand("refuse_cocktail")]
    public static void YarnRefuseCocktail()
    {
        GameManager.Instance.gameState.RefuseCocktail();
    }

    [YarnCommand("load_next_scene")]
    public static void YarnLoadNextScene()
    {
        GameManager.Instance.LoadNextScene();
    }

    [YarnCommand("enable_object")]
    public static void YarnEnableObject(string objectName)
    {
        var obj = GameObject.Find(objectName);
        if (obj == null) { Debug.LogError($"[DialogueTrigger] '{objectName}' не найден"); return; }
        var interactable = obj.GetComponent<InteractableObject>();
        if (interactable != null) interactable.Enable(true);
        else Debug.LogWarning($"[DialogueTrigger] На '{objectName}' нет InteractableObject");
    }

    [YarnCommand("launch_roulette")]
    public static void YarnLaunchRoulette()
    {
        GameManager.Instance.LoadMiniGame("Roulette", MiniGameType.Roulette);
    }

    [YarnCommand("enable_door")]
    public static void YarnEnableDoor()
    {
        var director = Object.FindAnyObjectByType<DressingRoomDirector>();
        if (director != null) director.EnableDoor();
    }
}
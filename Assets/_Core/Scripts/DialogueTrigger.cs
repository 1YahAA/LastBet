using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Yarn Spinner")]
    public DialogueRunner dialogueRunner;

    [Header("UI")]
    public Button continueButton;
    public CanvasGroup linePresenterCanvasGroup;

    void Start()
    {
        if (dialogueRunner == null) return;
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
        if (dialogueRunner.IsDialogueRunning) return;
        if (GameManager.Instance != null) GameManager.Instance.OnDialogueStart();
        SetDialogueVisible(true);
        dialogueRunner.StartDialogue(nodeName);
    }

    private void OnDialogueFinished()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnDialogueEnd();
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
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;
        if (System.Enum.TryParse<TokenType>(tokenName, out TokenType t))
            GameManager.Instance.gameState.AddToken(t);
        else Debug.LogError($"[DialogueTrigger] Неизвестный жетон: '{tokenName}'");
    }

    [YarnCommand("drink_cocktail")]
    public static void YarnDrinkCocktail()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;
        GameManager.Instance.gameState.DrinkCocktail();
    }

    [YarnCommand("refuse_cocktail")]
    public static void YarnRefuseCocktail()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;
        GameManager.Instance.gameState.RefuseCocktail();
    }

    [YarnCommand("inspect_cocktail")]
    public static void YarnInspectCocktail()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;
        GameManager.Instance.gameState.InspectCocktail();
    }

    [YarnCommand("load_next_scene")]
    public static void YarnLoadNextScene()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.LoadNextScene();
    }

    [YarnCommand("launch_roulette")]
    public static void YarnLaunchRoulette()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.LoadMiniGame("Roulette", MiniGameType.Roulette);
    }

    [YarnCommand("launch_cocktail_game")]
    public static void YarnLaunchCocktailGame()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.LoadMiniGame("CoctailesMiniGame", MiniGameType.CardGame);
    }

    [YarnCommand("launch_last_bet")]
    public static void YarnLaunchLastBet()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.LoadMiniGame("LastBetMiniGame", MiniGameType.Roulette);
    }

    [YarnCommand("show_audience_stage5")]
    public static IEnumerator YarnShowAudienceStage5()
    {
        var director = Object.FindAnyObjectByType<FinalStageDirector>();
        if (director != null)
            yield return director.ShowAudienceForFirstChoice();
    }

    [YarnCommand("set_pending_ending")]
    public static void YarnSetPendingEnding(string ending)
    {
        var director = Object.FindAnyObjectByType<FinalStageDirector>();
        if (director != null) director.SetPendingEnding(ending);
    }

    [YarnCommand("weak_reply_done")]
    public static void YarnWeakReplyDone()
    {
        var director = Object.FindAnyObjectByType<FinalStageDirector>();
        if (director != null) director.OnWeakReplyDone();
    }

    [YarnCommand("enable_object")]
    public static void YarnEnableObject(string objectName)
    {
        var obj = GameObject.Find(objectName);
        if (obj == null) { Debug.LogError($"[DialogueTrigger] '{objectName}' не найден"); return; }
        var interactable = obj.GetComponent<InteractableObject>();
        if (interactable != null) interactable.Enable(true);
    }

    [YarnCommand("enable_door")]
    public static void YarnEnableDoor()
    {
        var director = Object.FindAnyObjectByType<DressingRoomDirector>();
        if (director != null) director.EnableDoor();
    }
}
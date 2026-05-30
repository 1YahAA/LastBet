using UnityEngine;
using Yarn.Unity;

// DialogueTrigger.cs
//
// ЧТО ДЕЛАЕТ:
//   1. Запускает нужный узел диалога Yarn Spinner по имени.
//   2. Сообщает GameManager о начале/конце диалога.
//   3. Содержит статические Yarn команды — Yarn вызывает их
//      без поиска объекта в сцене (работает надёжнее).
//
// ПРИКРЕПЛЯТЬ К: объект Dialogue System в каждой игровой сцене.
// В INSPECTOR: Dialogue Runner → перетащить компонент DialogueRunner.
//
// КОМАНДЫ ДЛЯ .YARN ФАЙЛОВ (без указания объекта):
//   <<add_token Revolt>>         → +1 Бунт
//   <<add_token Obedience>>      → +1 Послушание
//   <<add_token Analysis>>       → +1 Анализ
//   <<drink_cocktail>>           → выпить коктейль
//   <<refuse_cocktail>>          → отказаться от коктейля
//   <<load_next_scene>>          → следующая сцена
//   <<enable_object ИмяОбъекта>> → включить InteractableObject
//   <<launch_roulette>>          → запустить мини-игру автомат

public class DialogueTrigger : MonoBehaviour
{
    [Header("Yarn Spinner")]
    [Tooltip("Компонент DialogueRunner на этом же объекте.")]
    public DialogueRunner dialogueRunner;

    void Start()
    {
        if (dialogueRunner == null)
        {
            Debug.LogError("[DialogueTrigger] dialogueRunner не назначен в Inspector!");
            return;
        }
        dialogueRunner.onDialogueComplete.AddListener(OnDialogueFinished);
    }

    void OnDestroy()
    {
        if (dialogueRunner != null)
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueFinished);
    }

    // ── Запуск диалога ────────────────────────────────

    public void StartDialogueNode(string nodeName)
    {
        if (dialogueRunner == null) return;
        if (dialogueRunner.IsDialogueRunning)
        {
            Debug.LogWarning($"[DialogueTrigger] Диалог уже идёт, '{nodeName}' проигнорирован");
            return;
        }
        GameManager.Instance.OnDialogueStart();
        dialogueRunner.StartDialogue(nodeName);
        Debug.Log($"[DialogueTrigger] Запущен диалог: {nodeName}");
    }

    private void OnDialogueFinished()
    {
        GameManager.Instance.OnDialogueEnd();
        Debug.Log("[DialogueTrigger] Диалог завершён");
    }

    // ── Yarn команды (static — Yarn не ищет объект в сцене) ──

    [YarnCommand("add_token")]
    public static void YarnAddToken(string tokenName)
    {
        if (System.Enum.TryParse<TokenType>(tokenName, out TokenType tokenType))
            GameManager.Instance.gameState.AddToken(tokenType);
        else
            Debug.LogError($"[DialogueTrigger] Неизвестный жетон: '{tokenName}'. Используй: Revolt, Obedience, Analysis");
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
        if (obj == null) { Debug.LogError($"[DialogueTrigger] Объект '{objectName}' не найден"); return; }
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
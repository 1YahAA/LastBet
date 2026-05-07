using UnityEngine;

public class DressingRoomDirector : MonoBehaviour
{
    [Header("Кликабельные объекты сцены")]
    [Tooltip("Interactable_Note — записка от Виктора")]
    public InteractableObject noteInteractable;

    [Tooltip("Interactable_Cocktail — бокал. Выключен при старте.")]
    public InteractableObject cocktailInteractable;

    [Tooltip("Interactable_Door — дверь в бар")]
    public InteractableObject doorInteractable;

    [Tooltip("Interactable_Machine — автомат. Выключен при старте.")]
    public InteractableObject machineInteractable;

    [Header("Система диалогов")]
    [Tooltip("DialogueTrigger с объекта Dialogue System")]
    public DialogueTrigger dialogueTrigger;

    private bool _noteRead      = false;
    private bool _doorAttempted = false;
    private bool _cocktailUsed  = false;

    void Start()
    {
        if (cocktailInteractable != null) cocktailInteractable.Enable(false);
        if (machineInteractable  != null) machineInteractable.Enable(false);
        if (doorInteractable     != null) doorInteractable.Enable(false);
    }

    // Клик на записку
    public void OnNoteClicked()
    {
        dialogueTrigger.StartDialogueNode("Dressing_Note");

        if (!_noteRead)
        {
            _noteRead = true;
            if (cocktailInteractable != null)
                cocktailInteractable.Enable(true);
            Debug.Log("[Dressing] Записка прочитана → бокал доступен");
        }
    }

    // Клик на бокал — только один раз
    public void OnCocktailClicked()
    {
        if (_cocktailUsed) return;
        _cocktailUsed = true;

        // Выключаем сразу чтобы нельзя было нажать повторно
        if (cocktailInteractable != null)
            cocktailInteractable.Enable(false);

        dialogueTrigger.StartDialogueNode("Dressing_Cocktail");
    }

    // Клик на дверь
    public void OnDoorClicked()
    {
        if (!_doorAttempted)
        {
            _doorAttempted = true;
            if (machineInteractable != null)
                machineInteractable.Enable(true);
            Debug.Log("[Dressing] Дверь заперта → автомат включён");
            dialogueTrigger.StartDialogueNode("Dressing_Door_Locked");
        }
        else
        {
            dialogueTrigger.StartDialogueNode("Dressing_Door_Remind");
        }
    }

    // Клик на автомат
    public void OnMachineClicked()
    {
        dialogueTrigger.StartDialogueNode("Dressing_Machine_Intro");
    }

    public void EnableDoor()
    {
        if (doorInteractable != null)
            doorInteractable.Enable(true);
        Debug.Log("[Dressing] Дверь разблокирована после бокала");
    }
}
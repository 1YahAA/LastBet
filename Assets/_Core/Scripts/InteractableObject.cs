using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// Вешается на любой объект в сцене на который можно кликнуть

public class InteractableObject : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Настройки")]
    [Tooltip("Текст подсказки при наведении мыши.\nПример: 'Прочитать записку'")]
    public string hintText = "Взаимодействовать";

    [Tooltip("Включено ли взаимодействие прямо сейчас.\nПример: дверь начинает со значением false — заперта.")]
    public bool isEnabled = true;

    [Header("Действие при клике")]
    [Tooltip("Что вызвать при клике.\nНажать + → перетащить DressingRoomDirector → выбрать метод.\nПример: DressingRoomDirector → OnNoteClicked()")]
    public UnityEvent onInteract;

    // Клик
    public void OnPointerClick(PointerEventData eventData)
    {
        // Не реагируем если объект выключен
        if (!isEnabled) return;

        // Не реагируем если GameManager недоступен 
        if (GameManager.Instance == null) return;

        // Не реагируем если игра на паузе
        if (GameManager.Instance.IsPaused) return;

        // Не реагируем если прямо сейчас идёт диалог
        if (GameManager.Instance.IsInDialogue) return;

        Debug.Log($"[Interact] Клик: {gameObject.name}");
        onInteract?.Invoke();
    }

    // Наведение (пока просто лог)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isEnabled) return;
        Debug.Log($"[Hint] {hintText}");
        // TODO: HintUI.Instance.Show(hintText)
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // TODO: HintUI.Instance.Hide()
    }

    // Включить или выключить взаимодействие из кода
    public void Enable(bool value)
    {
        isEnabled = value;
        Debug.Log($"[Interact] {gameObject.name} → {(value ? "включён" : "выключен")}");
    }
}
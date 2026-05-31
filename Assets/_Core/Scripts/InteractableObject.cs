using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEnabled) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsPaused) return;
        if (GameManager.Instance.IsInDialogue) return;
        onInteract?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isEnabled) return;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    public void Enable(bool value)
    {
        isEnabled = value;
    }
}
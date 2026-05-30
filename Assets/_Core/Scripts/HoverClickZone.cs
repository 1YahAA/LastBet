using UnityEngine;
using UnityEngine.EventSystems;

public class HoverClickZone : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public System.Action onHoverEnter;
    public System.Action onHoverExit;
    public System.Action onClick;

    public void OnPointerEnter(PointerEventData e) => onHoverEnter?.Invoke();
    public void OnPointerExit(PointerEventData e) => onHoverExit?.Invoke();
    public void OnPointerClick(PointerEventData e) => onClick?.Invoke();
}
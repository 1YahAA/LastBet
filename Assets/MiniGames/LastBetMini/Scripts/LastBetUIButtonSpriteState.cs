using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LastBetUIButtonSpriteState : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Sprite disabledSprite;
    [SerializeField] private Sprite warningSprite;

    private bool _hovered;
    private bool _pressed;
    private bool _warning;
    private bool _interactable = true;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        Refresh();
    }

    public void SetInteractableVisual(bool interactable)
    {
        _interactable = interactable;
        _pressed = false;
        Refresh();
    }

    public void SetWarning(bool warning)
    {
        _warning = warning;
        Refresh();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
        Refresh();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        _pressed = false;
        Refresh();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        Refresh();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        Refresh();
    }

    private void Refresh()
    {
        if (targetImage == null)
            return;

        Sprite sprite = idleSprite;

        if (!_interactable && disabledSprite != null)
            sprite = disabledSprite;
        else if (_warning && warningSprite != null)
            sprite = warningSprite;
        else if (_pressed && pressedSprite != null)
            sprite = pressedSprite;
        else if (_hovered && hoverSprite != null)
            sprite = hoverSprite;

        if (sprite != null)
            targetImage.sprite = sprite;
    }
}
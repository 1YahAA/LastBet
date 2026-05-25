using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LastBetClueSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;

    [Header("Tooltip Anchor")]
    [SerializeField] private RectTransform tooltipAnchor;

    private string _title;
    private string _description;
    private LastBetTooltip _tooltip;

    public void Setup(Sprite icon, string title, string description, LastBetTooltip tooltip)
    {
        _title = title ?? string.Empty;
        _description = description ?? string.Empty;
        _tooltip = tooltip;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }

        if (titleText != null)
            titleText.text = _title;

        if (tooltipAnchor == null && iconImage != null)
            tooltipAnchor = iconImage.rectTransform;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_tooltip == null)
            return;

        RectTransform anchor = tooltipAnchor != null
            ? tooltipAnchor
            : transform as RectTransform;

        _tooltip.Show(_title, _description, anchor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_tooltip != null)
            _tooltip.Hide();
    }
}
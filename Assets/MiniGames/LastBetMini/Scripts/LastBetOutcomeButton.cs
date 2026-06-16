using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public sealed class LastBetOutcomeButton : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite idleBackground;
    [SerializeField] private Sprite selectedBackground;

    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.78f, 0.35f, 1f);

    private RectTransform _rectTransform;
    private LayoutElement _layoutElement;
    private Vector2 _initialSize;
    private bool _bound;

    private void Awake()
    {
        BindDefaults();
    }

    public void BindDefaults()
    {
        if (_bound)
            return;

        _rectTransform = GetComponent<RectTransform>();
        _initialSize = _rectTransform.rect.size;

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (backgroundImage != null)
        {
            if (idleBackground == null)
                idleBackground = backgroundImage.sprite;

            idleColor = backgroundImage.color;
            backgroundImage.preserveAspect = false;
        }

        _layoutElement = GetComponent<LayoutElement>();
        if (_layoutElement == null)
            _layoutElement = gameObject.AddComponent<LayoutElement>();

        // Prevent Horizontal/Vertical/Grid Layout Group from resizing the button
        // when the selected sprite has a different source texture size.
        if (_initialSize.x > 0f)
            _layoutElement.preferredWidth = _initialSize.x;
        if (_initialSize.y > 0f)
            _layoutElement.preferredHeight = _initialSize.y;

        _bound = true;
    }

    public void SetSelected(bool selected)
    {
        BindDefaults();

        if (backgroundImage == null)
            return;

        Sprite targetSprite = selected ? selectedBackground : idleBackground;
        if (targetSprite != null)
            backgroundImage.sprite = targetSprite;

        backgroundImage.color = selected ? selectedColor : idleColor;

        // Keep the visual size stable even after sprite swap.
        if (_initialSize.x > 0f)
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _initialSize.x);
        if (_initialSize.y > 0f)
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _initialSize.y);
    }
}

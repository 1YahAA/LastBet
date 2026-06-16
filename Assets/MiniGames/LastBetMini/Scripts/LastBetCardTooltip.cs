using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LastBetCardTooltip : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Position")]
    [SerializeField] private Vector2 offset = new Vector2(24f, 24f);

    private RectTransform _canvasRect;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (root == null)
            root = GetComponent<RectTransform>();

        _canvas = GetComponentInParent<Canvas>();
        if (_canvas != null)
            _canvasRect = _canvas.GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Tooltip object must stay active. We hide it through CanvasGroup,
        // otherwise Show() can activate it and Awake() can immediately hide it again.
        Hide();
    }

    public void Show(LastBetCardData data, RectTransform target)
    {
        if (data == null || target == null)
            return;

        if (titleText != null)
            titleText.text = data.title;

        if (descriptionText != null)
            descriptionText.text = data.cardDescription;

        gameObject.SetActive(true);
        PlaceNear(target);

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    public void Hide()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    private void PlaceNear(RectTransform target)
    {
        if (root == null || _canvasRect == null)
            return;

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Camera eventCamera = null;
        if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            eventCamera = _canvas.worldCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[2]);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPoint,
            eventCamera,
            out Vector2 localPoint);

        root.anchoredPosition = localPoint + offset;
    }
}

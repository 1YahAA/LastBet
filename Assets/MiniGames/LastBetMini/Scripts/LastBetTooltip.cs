using TMPro;
using UnityEngine;

public class LastBetTooltip : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Position")]
    [SerializeField] private Vector2 offset = new Vector2(0f, 8f);

    private Canvas _canvas;
    private RectTransform _canvasRect;

    private void Awake()
    {
        if (root == null)
            root = GetComponent<RectTransform>();

        _canvas = GetComponentInParent<Canvas>();
        if (_canvas != null)
            _canvasRect = _canvas.GetComponent<RectTransform>();

        Hide();
    }

    public void Show(string title, string body, RectTransform anchor)
    {
        if (root == null || anchor == null || _canvasRect == null)
            return;

        if (titleText != null)
            titleText.text = title ?? string.Empty;

        if (bodyText != null)
            bodyText.text = body ?? string.Empty;

        root.gameObject.SetActive(true);
        root.SetAsLastSibling();

        // Нижний правый угол тултипа должен встать в заданную позицию.
        root.pivot = new Vector2(1f, 0f);

        Vector3 worldCorner = GetTopRightWorldCorner(anchor);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
            worldCorner
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPoint,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
            out Vector2 localPoint
        );

        root.anchoredPosition = localPoint + offset;
    }

    private static Vector3 GetTopRightWorldCorner(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        // 0 = bottom-left, 1 = top-left, 2 = top-right, 3 = bottom-right
        return corners[2];
    }

    public void Hide()
    {
        if (root != null)
            root.gameObject.SetActive(false);
    }

    private static Vector3 GetTopLeftWorldCorner(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        // 0 = bottom-left, 1 = top-left, 2 = top-right, 3 = bottom-right
        return corners[1];
    }
}
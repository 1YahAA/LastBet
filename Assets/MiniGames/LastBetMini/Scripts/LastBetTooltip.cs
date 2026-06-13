using TMPro;
using UnityEngine;

// Общий tooltip для панели улик
public sealed class LastBetTooltip : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private RectTransform root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Position")]
    [SerializeField] private Vector2 offset = new Vector2(0f, 8f);
    [SerializeField] private bool keepInsideCanvas = true;

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
        if (root == null || anchor == null)
            return;

        if (_canvas == null)
            _canvas = GetComponentInParent<Canvas>();

        if (_canvasRect == null && _canvas != null)
            _canvasRect = _canvas.GetComponent<RectTransform>();

        if (_canvasRect == null)
            return;

        if (titleText != null)
            titleText.text = string.IsNullOrWhiteSpace(title) ? "Улика" : title;

        if (bodyText != null)
            bodyText.text = string.IsNullOrWhiteSpace(body) ? "Описание улики не найдено." : body;

        root.gameObject.SetActive(true);
        root.SetAsLastSibling();

        // Нижний правый угол tooltip будет поставлен в позицию верхнего правого угла иконки.
        root.pivot = new Vector2(1f, 0f);

        Vector3 worldCorner = GetTopRightWorldCorner(anchor);
        Camera camera = GetCanvasCamera();

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldCorner);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPoint, camera, out Vector2 localPoint);

        root.anchoredPosition = localPoint + offset;

        if (keepInsideCanvas)
            ClampInsideCanvas();
    }

    public void Hide()
    {
        if (root != null)
            root.gameObject.SetActive(false);
    }

    private void ClampInsideCanvas()
    {
        Canvas.ForceUpdateCanvases();

        Vector2 position = root.anchoredPosition;
        Vector2 size = root.rect.size;
        Rect canvasRect = _canvasRect.rect;

        // root.pivot = (1, 0), значит anchoredPosition является нижним правым углом tooltip.
        float left = position.x - size.x;
        float right = position.x;
        float bottom = position.y;
        float top = position.y + size.y;

        if (left < canvasRect.xMin)
            position.x += canvasRect.xMin - left;
        if (right > canvasRect.xMax)
            position.x -= right - canvasRect.xMax;
        if (bottom < canvasRect.yMin)
            position.y += canvasRect.yMin - bottom;
        if (top > canvasRect.yMax)
            position.y -= top - canvasRect.yMax;

        root.anchoredPosition = position;
    }

    private Camera GetCanvasCamera()
    {
        if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return _canvas.worldCamera;
    }

    private static Vector3 GetTopRightWorldCorner(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return corners[2];
    }
}

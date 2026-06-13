using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Визуальное представление карты на столе
public class LastBetCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Images")]
    [SerializeField] private Image baseImage;
    [SerializeField] private Image backImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image clueImage;
    [SerializeField] private Image jokerFullCardImage;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Info Block")]
    [SerializeField] private GameObject infoBlock;

    [Header("Hover Inspect")]
    [SerializeField] private bool inspectOnHover = true;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float hoverScale = 1.18f;
    [SerializeField] private float hoverMoveY = 0f;
    [SerializeField] private float hoverDuration = 0.12f;
    [SerializeField] private int hoverSortingOrder = 50;

    private LastBetCardData _data;
    private bool _opened;

    private Vector3 _initialVisualScale;
    private Vector3 _initialVisualLocalPosition;
    private bool _initialStateRecorded;

    private Canvas _sortingCanvas;
    private bool _createdSortingCanvas;
    private Coroutine _hoverRoutine;

    public LastBetCardData Data => _data;
    public bool Opened => _opened;

    private Transform VisualTarget => visualRoot != null ? visualRoot : transform;

    private void Awake()
    {
        if (visualRoot == null)
        {
            Transform found = transform.Find("VisualRoot");
            if (found != null)
                visualRoot = found;
        }

        RecordVisualInitialState();

        _sortingCanvas = GetComponent<Canvas>();
        if (_sortingCanvas == null)
        {
            _sortingCanvas = gameObject.AddComponent<Canvas>();
            _createdSortingCanvas = true;
        }

        _sortingCanvas.overrideSorting = false;
        _sortingCanvas.sortingOrder = 0;
    }

    public void Setup(
        LastBetCardData data,
        Sprite cardBaseSprite,
        Sprite cardBackSprite,
        Sprite cardFrameSprite,
        Sprite jokerFullCardSprite)
    {
        _data = data;
        _opened = false;

        RecordVisualInitialState();

        if (baseImage != null) baseImage.sprite = cardBaseSprite;
        if (backImage != null) backImage.sprite = cardBackSprite;
        if (frameImage != null) frameImage.sprite = cardFrameSprite;

        if (clueImage != null) clueImage.sprite = data != null ? data.clueSprite : null;
        if (jokerFullCardImage != null) jokerFullCardImage.sprite = jokerFullCardSprite;

        ShowClosed();
    }

    public void ShowClosed()
    {
        _opened = false;
        StopHoverAnimation();
        RestoreVisualImmediate();
        SetHoverSorting(false);

        SetActive(baseImage, false);
        SetActive(frameImage, false);
        SetActive(clueImage, false);
        SetActive(jokerFullCardImage, false);
        SetActive(backImage, true);

        SetInfoBlock(false);
        SetText(titleText, string.Empty);
        SetText(descriptionText, string.Empty);
    }

    public void ShowOpened()
    {
        _opened = true;

        bool isJoker = _data != null && _data.IsJoker;

        SetActive(backImage, false);
        SetActive(baseImage, !isJoker);
        SetActive(frameImage, !isJoker);
        SetActive(clueImage, !isJoker);
        SetActive(jokerFullCardImage, isJoker);

        SetInfoBlock(!isJoker);
        SetText(titleText, _data != null ? _data.title : string.Empty);
        SetText(descriptionText, _data != null ? _data.cardDescription : string.Empty);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!inspectOnHover || !_opened)
            return;

        SetHoverSorting(true);

        Vector3 targetScale = _initialVisualScale * hoverScale;
        Vector3 targetPosition = _initialVisualLocalPosition + new Vector3(0f, hoverMoveY, 0f);

        AnimateTo(targetScale, targetPosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!inspectOnHover || !_opened)
            return;

        AnimateTo(_initialVisualScale, _initialVisualLocalPosition, disableSortingAfter: true);
    }

    private void RecordVisualInitialState()
    {
        Transform target = VisualTarget;
        _initialVisualScale = target.localScale;
        _initialVisualLocalPosition = target.localPosition;
        _initialStateRecorded = true;
    }

    private void AnimateTo(Vector3 targetScale, Vector3 targetPosition, bool disableSortingAfter = false)
    {
        StopHoverAnimation();
        _hoverRoutine = StartCoroutine(AnimateVisual(targetScale, targetPosition, disableSortingAfter));
    }

    private IEnumerator AnimateVisual(Vector3 targetScale, Vector3 targetPosition, bool disableSortingAfter)
    {
        Transform target = VisualTarget;
        Vector3 startScale = target.localScale;
        Vector3 startPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < hoverDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = hoverDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / hoverDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            target.localScale = Vector3.Lerp(startScale, targetScale, t);
            target.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        target.localScale = targetScale;
        target.localPosition = targetPosition;

        if (disableSortingAfter)
            SetHoverSorting(false);

        _hoverRoutine = null;
    }

    private void StopHoverAnimation()
    {
        if (_hoverRoutine == null)
            return;

        StopCoroutine(_hoverRoutine);
        _hoverRoutine = null;
    }

    private void RestoreVisualImmediate()
    {
        if (!_initialStateRecorded)
            RecordVisualInitialState();

        Transform target = VisualTarget;
        target.localScale = _initialVisualScale;
        target.localPosition = _initialVisualLocalPosition;
    }

    private void SetHoverSorting(bool enabled)
    {
        if (_sortingCanvas == null)
            return;

        _sortingCanvas.overrideSorting = enabled;
        _sortingCanvas.sortingOrder = enabled ? hoverSortingOrder : 0;
    }

    private void SetInfoBlock(bool active)
    {
        if (infoBlock != null)
            infoBlock.SetActive(active);
    }

    private static void SetActive(Graphic graphic, bool active)
    {
        if (graphic != null)
            graphic.gameObject.SetActive(active);
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value ?? string.Empty;
    }
}

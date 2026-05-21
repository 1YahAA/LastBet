using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class BlackMarkCardView : MonoBehaviour, IPointerClickHandler
{
    public BlackMarkCardInstance Data { get; private set; }
    public bool IsOpen { get; private set; }
    public bool IsLocked { get; private set; }

    [Header("Card Layers")]
    public Image baseImage;
    public Image iconImage;
    public Image backImage;
    public Image frameImage;
    public Image jokerImage;

    private BlackMarkGameManager _manager;
    private Sprite _backSprite;
    private Sprite _baseSprite;
    private Sprite _frameSprite;

    public void Init(
        BlackMarkCardInstance data,
        BlackMarkGameManager manager,
        Sprite backSprite,
        Sprite baseSprite,
        Sprite frameSprite)
    {
        Data = data;
        _manager = manager;

        _backSprite = backSprite;
        _baseSprite = baseSprite;
        _frameSprite = frameSprite;

        IsOpen = false;
        IsLocked = false;

        ShowBackInstant();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_manager == null || IsOpen || IsLocked)
            return;

        _manager.OnCardClicked(this);
    }

    public void Open()
    {
        if (Data == null)
            return;

        IsOpen = true;
        transform.DOKill();

        transform.DOScaleX(0f, 0.08f).OnComplete(() =>
        {
            ShowFrontInstant();
            transform.DOScaleX(1f, 0.08f);
        });
    }

    public void Close()
    {
        IsOpen = false;
        transform.DOKill();

        transform.DOScaleX(0f, 0.08f).OnComplete(() =>
        {
            ShowBackInstant();
            transform.DOScaleX(1f, 0.08f);
        });
    }

    public void LockOpen()
    {
        IsLocked = true;
        IsOpen = true;

        ShowFrontInstant();

        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();

        group.alpha = 0.55f;
    }

    public void Shake()
    {
        transform.DOKill();
        transform.DOShakePosition(0.25f, 8f, 12);
    }

    public void ShowBackInstant()
    {
        if (backImage != null)
        {
            backImage.sprite = _backSprite;
            backImage.enabled = true;
        }

        if (baseImage != null)
            baseImage.enabled = false;

        if (iconImage != null)
            iconImage.enabled = false;

        if (jokerImage != null)
            jokerImage.enabled = false;

        if (frameImage != null)
            frameImage.enabled = false;

        IsOpen = false;
    }

    private void ShowFrontInstant()
    {
        if (backImage != null)
            backImage.enabled = false;

        if (baseImage != null)
        {
            baseImage.sprite = _baseSprite;
            baseImage.enabled = _baseSprite != null;
        }

        bool isJoker = Data != null && Data.IsJoker;

        if (iconImage != null)
        {
            iconImage.sprite = isJoker ? null : Data.FrontSprite;
            iconImage.enabled = !isJoker && Data.FrontSprite != null;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
        }

        if (jokerImage != null)
        {
            jokerImage.sprite = isJoker ? Data.FrontSprite : null;
            jokerImage.enabled = isJoker && Data.FrontSprite != null;
            jokerImage.color = Color.white;
            jokerImage.preserveAspect = true;
        }

        if (frameImage != null)
        {
            frameImage.sprite = _frameSprite;
            frameImage.enabled = _frameSprite != null;
        }
    }
}
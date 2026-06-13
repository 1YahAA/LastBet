using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public CardData Data { get; private set; }

    private CardGameManager _manager;
    private Image _image;
    private RectTransform _rectTransform;
    private bool _interactable = true;
    private Vector2 _hoverBasePosition;

    public void Init(CardData data, CardGameManager manager)
    {
        Data = data;
        _manager = manager;

        _image = GetComponent<Image>();
        _rectTransform = transform as RectTransform;

        if (_image == null)
        {
            Debug.LogError("[CardView] Image не найден", this);
            return;
        }

        if (data == null)
        {
            Debug.LogError("[CardView] CardData == null", this);
            return;
        }

        if (data.cardSprite == null)
        {
            Debug.LogWarning($"[CardView] У карты {data.name} не назначен Card Sprite", this);
            return;
        }

        _image.sprite = data.cardSprite;
        _image.color = Color.white;
        _image.preserveAspect = true;
        _image.raycastTarget = true;

        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public void SetInteractable(bool value)
    {
        _interactable = value;

        if (!value)
            KillTweens();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_interactable)
            return;

        KillTweens();

        if (_manager != null)
            _manager.OnCardSelectedFromHand(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_interactable || _rectTransform == null)
            return;

        _hoverBasePosition = _rectTransform.anchoredPosition;

        KillTweens();
        _rectTransform.DOAnchorPos(_hoverBasePosition + new Vector2(0f, 35f), 0.12f).SetTarget(_rectTransform);
        transform.DOScale(1.06f, 0.12f).SetTarget(transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_interactable || _rectTransform == null)
            return;

        KillTweens();
        _rectTransform.DOAnchorPos(_hoverBasePosition, 0.12f).SetTarget(_rectTransform);
        transform.DOScale(1f, 0.12f).SetTarget(transform);
    }

    public void KillTweens()
    {
        if (_rectTransform != null)
            _rectTransform.DOKill();

        transform.DOKill();
    }

    private void OnDestroy()
    {
        KillTweens();
    }
}

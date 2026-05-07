using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class CardSlot : MonoBehaviour
{
    [Tooltip("Image-рамка слота. Подсвечивается когда слот свободен")]
    public Image slotFrame;

    [Tooltip("Цвет рамки когда слот пустой")]
    public Color emptyColor = new Color(1f, 1f, 1f, 0.3f);

    [Tooltip("Цвет рамки когда слот занят")]
    public Color filledColor = new Color(1f, 1f, 1f, 0f);

    public bool HasCard => _placedView != null;
    public CardData PlacedCard => _placedView != null ? _placedView.Data : null;

    private CardView _placedView;

    void Awake()
    {
        if (slotFrame != null)
            slotFrame.color = emptyColor;
    }

    // Поместить карту в слот. Карта перемещается анимацией к центру слота
    public void PlaceCard(CardView cardView)
    {
        _placedView = cardView;

        // Карта становится дочерней к слоту
        cardView.transform.SetParent(transform, worldPositionStays: true);

        // Анимация перелёта к центру слота
        cardView.transform.DOLocalMove(Vector3.zero, 0.18f).SetEase(Ease.OutQuad);
        cardView.transform.DOScale(0.88f, 0.15f);

        // Заблокировать клики на карте
        cardView.SetInteractable(false);

        // Скрыть подсветку рамки
        if (slotFrame != null)
            slotFrame.DOColor(filledColor, 0.15f);
    }

    // Очистить слот: уничтожить карту и вернуть рамку
    public void Clear()
    {
        if (_placedView != null)
        {
            Destroy(_placedView.gameObject);
            _placedView = null;
        }

        if (slotFrame != null)
            slotFrame.color = emptyColor;
    }
}

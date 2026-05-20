using UnityEngine;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour
{
    public Image slotFrame;
    public Vector2 placedCardSize = new Vector2(150f, 220f);

    public CardData PlacedCard { get; private set; }
    
    public bool HasCard => PlacedCard != null;

    private CardView _currentView;

    public void PlaceCard(CardView cardView)
    {
        if (cardView == null)
            return;

        _currentView = cardView;

        PlacedCard = cardView.Data;

        RectTransform rt = cardView.transform as RectTransform;

        if (rt == null)
            return;

        rt.SetParent(transform, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);

        rt.anchorMax = new Vector2(0.5f, 0.5f);

        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.anchoredPosition = Vector2.zero;

        rt.localRotation = Quaternion.identity;
        
        rt.localScale = Vector3.one;
        rt.sizeDelta = placedCardSize;

        cardView.SetInteractable(false);
    }

    public void Clear()
    {
        if (_currentView != null)
        {
            Destroy(_currentView.gameObject);
        }

        _currentView = null;

        PlacedCard = null;
    }
}
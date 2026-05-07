// Отображает данные карты и обрабатывает клик/hover
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class CardView : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Дочерние элементы (перетащить из иерархии префаба)")]
    [Tooltip("Image для арта карты")]
    public Image cardArtImage;

    [Tooltip("TextMeshPro — числовое значение карты (1, 2 или 3)")]
    public TextMeshProUGUI valueText;

    [Tooltip("Image-иконка бонуса соседства СЛЕВА (скрывается если нет бонуса)")]
    public Image leftAdjIcon;

    [Tooltip("Image-иконка бонуса соседства СПРАВА (скрывается если нет бонуса)")]
    public Image rightAdjIcon;

    [Header("Цвета фона карты по CardColor")]
    [Tooltip("Фон для Red-карты")]
    public Color redBg    = new Color(0.78f, 0.20f, 0.20f);

    [Tooltip("Фон для Yellow-карты")]
    public Color yellowBg = new Color(0.88f, 0.75f, 0.10f);

    [Tooltip("Фон для Blue-карты")]
    public Color blueBg   = new Color(0.18f, 0.40f, 0.85f);

    [Tooltip("Фон для Black-карты (испорченная)")]
    public Color blackBg  = new Color(0.18f, 0.18f, 0.18f);

    [Header("Иконки цветов для adjacenc")]
    [Tooltip("Спрайты маленьких иконок цветов: [0]=Red [1]=Yellow [2]=Blue [3]=Black")]
    public Sprite[] adjColorIcons = new Sprite[4];

    public CardData Data { get; private set; }

    private CardGameManager _manager;
    private Image _bg;
    private bool _interactable = true;

    public void Init(CardData data, CardGameManager manager)
    {
        Data = data;
        _manager = manager;
        _bg = GetComponent<Image>();

        // Цвет фона
        _bg.color = data.color switch
        {
            CardColor.Red => redBg,
            CardColor.Yellow => yellowBg,
            CardColor.Blue => blueBg,
            CardColor.Black => blackBg,
            _ => Color.white
        };

        // Арт (Настя добавит спрайты в CardData)
        if (cardArtImage != null)
            cardArtImage.sprite = data.cardSprite;

        // Значение карты
        if (valueText != null)
        {
            // Чёрные карты показываем со знаком минус
            valueText.text = data.color == CardColor.Black
                ? $"-{data.value}"
                : data.value.ToString();
        }

        // Иконки adjacency: показать только если есть условие
        SetAdjIcon(leftAdjIcon, data.leftAdjacencyColor);
        SetAdjIcon(rightAdjIcon, data.rightAdjacencyColor);
    }

    void SetAdjIcon(Image icon, CardColor adjColor)
    {
        if (icon == null) return;
        if (adjColor == CardColor.None)
        {
            icon.gameObject.SetActive(false);
            return;
        }
        icon.gameObject.SetActive(true);
        int idx = (int)adjColor - 1;
        if (idx >= 0 && idx < adjColorIcons.Length)
            icon.sprite = adjColorIcons[idx];
    }

    // Блокировать/разблокировать клики
    public void SetInteractable(bool value)
    {
        _interactable = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_interactable) return;

        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.12f, 0.18f, 6, 0.5f);

        _manager.OnCardSelectedFromHand(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_interactable) return;
        transform.DOKill();
        transform.DOScale(1.08f, 0.12f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_interactable) return;
        transform.DOKill();
        transform.DOScale(1f, 0.12f).SetEase(Ease.OutQuad);
    }
}

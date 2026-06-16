using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class JokerMemoryCard : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image cardImage;

    private JokerMemoryGameManager gameManager;
    private JokerMemoryCardData data;
    private bool isOpen;
    private bool isMatched;

    public int PairId => data.PairId;
    public bool IsJoker => data.IsJoker;
    public bool IsOpen => isOpen;
    public bool IsMatched => isMatched;

    private void Awake()
    {
        if (cardImage == null)
            cardImage = GetComponent<Image>();
    }

    public void Init(JokerMemoryGameManager manager, JokerMemoryCardData cardData)
    {
        gameManager = manager;
        data = cardData;
        isOpen = false;
        isMatched = false;
        ShowBack();
        gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameManager == null)
            return;

        gameManager.TryOpenCard(this);
    }

    public void Open()
    {
        if (isMatched)
            return;

        isOpen = true;
        if (cardImage != null)
            cardImage.sprite = data.FaceSprite;
    }

    public void Close()
    {
        if (isMatched)
            return;

        isOpen = false;
        ShowBack();
    }

    public void MarkMatched()
    {
        isMatched = true;
        isOpen = true;

        if (cardImage != null)
            cardImage.sprite = data.FaceSprite;
    }

    public void ResetCard()
    {
        isOpen = false;
        isMatched = false;
        ShowBack();
    }

    private void ShowBack()
    {
        if (cardImage != null && gameManager != null)
            cardImage.sprite = gameManager.CardBackSprite;
    }
}

public readonly struct JokerMemoryCardData
{
    public readonly string Id;
    public readonly int PairId;
    public readonly Sprite FaceSprite;
    public readonly bool IsJoker;

    public JokerMemoryCardData(string id, int pairId, Sprite faceSprite, bool isJoker)
    {
        Id = id;
        PairId = pairId;
        FaceSprite = faceSprite;
        IsJoker = isJoker;
    }
}

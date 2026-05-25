using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LastBetCardView : MonoBehaviour
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

    [SerializeField] private GameObject infoBlock;

    private LastBetCardData _data;
    private bool _opened;

    public LastBetCardData Data => _data;
    public bool Opened => _opened;

    public void Setup(
        LastBetCardData data,
        Sprite cardBaseSprite,
        Sprite cardBackSprite,
        Sprite cardFrameSprite,
        Sprite jokerFullCardSprite)
    {
        _data = data;
        _opened = false;

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

        SetActive(baseImage, false);
        SetActive(frameImage, false);
        SetActive(clueImage, false);
        SetActive(jokerFullCardImage, false);
        SetActive(backImage, true);

        if (infoBlock != null)
            infoBlock.SetActive(false);

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

        if (infoBlock != null)
            infoBlock.SetActive(!isJoker);

        SetText(titleText, _data != null ? _data.title : string.Empty);
        SetText(descriptionText, _data != null ? _data.cardDescription : string.Empty);
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
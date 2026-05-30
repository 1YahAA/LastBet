using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LastBetClueSlotView : MonoBehaviour
{
    [SerializeField] private Image clueImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    public void Setup(Sprite sprite, string title, string description)
    {
        if (clueImage != null)
        {
            clueImage.sprite = sprite;
            clueImage.preserveAspect = true;
            clueImage.gameObject.SetActive(sprite != null);
        }

        if (titleText != null)
            titleText.text = title ?? string.Empty;

        if (descriptionText != null)
            descriptionText.text = description ?? string.Empty;

        gameObject.SetActive(true);
    }
}
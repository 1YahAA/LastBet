using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerView : MonoBehaviour
{
    [Header("Портрет / силуэт гостя")]
    public Image portraitImage;

    [Header("Реплика гостя")]
    public TextMeshProUGUI bubbleText;

    public void Show(Sprite portrait, string text)
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
            portraitImage.preserveAspect = true;
        }

        SetText(text);
    }

    public void ShowReaction(string text)
    {
        SetText(text);
    }

    public void Clear()
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }

        SetText("");
    }

    private void SetText(string text)
    {
        if (bubbleText != null)
            bubbleText.text = text;
    }
}

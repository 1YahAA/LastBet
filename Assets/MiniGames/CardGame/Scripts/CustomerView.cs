using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerView : MonoBehaviour
{
    public Image portraitImage;

    public TextMeshProUGUI bubbleText;

    public void Show(Sprite portrait, string text)
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
            portraitImage.preserveAspect = true;
        }

        if (bubbleText != null)
        {
            bubbleText.text = text;
        }
    }

    public void Clear()
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }

        if (bubbleText != null)
        {
            bubbleText.text = "";
        }    
    }
}
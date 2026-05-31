using UnityEngine;
using UnityEngine.UI;

public class LastBetChoiceButton : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite idleBackground;
    [SerializeField] private Sprite activeBackground;

    [Header("Fallback Colors")]
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color activeColor = new Color(1f, 0.78f, 0.35f, 1f);

    public void BindDefaults()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (backgroundImage != null && idleBackground == null)
            idleColor = backgroundImage.color;
    }

    public void SetSelected(bool selected)
    {
        BindDefaults();

        if (backgroundImage == null)
            return;

        Sprite target = selected ? activeBackground : idleBackground;

        if (target != null)
            backgroundImage.sprite = target;

        backgroundImage.color = selected ? activeColor : idleColor;
    }
}
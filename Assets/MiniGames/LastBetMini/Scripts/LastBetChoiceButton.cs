using UnityEngine;
using UnityEngine.UI;

public class LastBetChoiceButton : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite idleBackground;
    [SerializeField] private Sprite activeBackground;

    public void BindDefaults()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
    }

    public void SetSelected(bool selected)
    {
        BindDefaults();

        if (backgroundImage == null)
            return;

        Sprite target = selected ? activeBackground : idleBackground;
        if (target != null)
            backgroundImage.sprite = target;
    }
}
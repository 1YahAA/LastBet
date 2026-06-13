using UnityEngine;
using UnityEngine.UI;

// Визуальное состояние кнопки выбора подозреваемого
public class LastBetChoiceButton : MonoBehaviour
{
    [Header("Фон кнопки")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite idleBackground;
    [SerializeField] private Sprite activeBackground;

    [Header("Цвета (fallback, если спрайты не назначены)")]
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color activeColor = new Color(1f, 0.78f, 0.35f, 1f);

    // Привязывает фоновый Image если он не назначен в Inspector
    public void BindDefaults()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        // Если idle-спрайт не назначен — запоминаем текущий цвет объекта
        if (backgroundImage != null && idleBackground == null)
            idleColor = backgroundImage.color;
    }

    // Переключает визуальное состояние кнопки
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

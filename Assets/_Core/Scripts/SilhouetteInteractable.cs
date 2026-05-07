using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SilhouetteInteractable : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Персонаж")]
    [Tooltip("Уникальный ID: Viktor / Leo / Helga / Mari")]
    public string characterId;

    [Header("Визуал")]
    [Tooltip("Image самого силуэта")]
    public Image silhouetteImage;

    [Tooltip("Image белой обводки — дочерний объект чуть больше силуэта.\nВыключен по умолчанию.")]
    public Image outlineImage;

    [Header("Связи")]
    [Tooltip("CabaretDirector в сцене")]
    public CabaretDirector director;

    public bool IsViewed { get; private set; } = false;

    void Start()
    {
        if (outlineImage != null)
            outlineImage.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (outlineImage != null)
            outlineImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (outlineImage != null)
            outlineImage.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (director != null)
            director.OnSilhouetteClicked(this);
    }

    // Вызывается из CabaretDirector после просмотра
    public void MarkAsViewed()
    {
        IsViewed = true;
        // Слегка осветляем силует — визуальная обратная связь что уже смотрели
        if (silhouetteImage != null)
            silhouetteImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
    }
}
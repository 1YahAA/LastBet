using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Компактная запись улики в хранилище: иконка + название
public sealed class LastBetClueSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image clueImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button removeButton;

    [Header("Tooltip")]
    [SerializeField] private RectTransform tooltipAnchor;

    [Header("Visual State")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject unstableMark;

    private string _title;
    private string _description;
    private LastBetTooltip _tooltip;
    private Action<LastBetClueSlotView> _onRemove;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        AutoBind();
    }

    public void Setup(Sprite sprite, string title, string description, LastBetTooltip tooltip, Action<LastBetClueSlotView> onRemove = null)
    {
        AutoBind();

        _title = string.IsNullOrWhiteSpace(title) ? "Улика" : title.Trim();
        _description = string.IsNullOrWhiteSpace(description) ? "Описание улики не задано." : description.Trim();
        _tooltip = tooltip;
        _onRemove = onRemove;

        if (clueImage != null)
        {
            clueImage.sprite = sprite;
            clueImage.preserveAspect = true;
            clueImage.enabled = sprite != null;
            clueImage.gameObject.SetActive(true);
        }

        if (titleText != null)
            titleText.text = _title;

        if (tooltipAnchor == null && clueImage != null)
            tooltipAnchor = clueImage.rectTransform;

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(RemoveFromPanel);
            removeButton.gameObject.SetActive(_onRemove != null);
        }

        SetUnstable(false);
        gameObject.SetActive(true);
    }

    public void SetUnstable(bool unstable)
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = unstable ? 0.45f : 1f;
        canvasGroup.interactable = !unstable;
        canvasGroup.blocksRaycasts = !unstable;

        if (unstableMark != null)
            unstableMark.SetActive(unstable);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_tooltip == null)
            return;

        RectTransform anchor = tooltipAnchor != null ? tooltipAnchor : transform as RectTransform;
        _tooltip.Show(_title, _description, anchor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_tooltip != null)
            _tooltip.Hide();
    }

    private void RemoveFromPanel()
    {
        _tooltip?.Hide();
        _onRemove?.Invoke(this);
    }

    private void AutoBind()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (clueImage == null)
        {
            Transform byName = transform.Find("ClueIcon")
                ?? transform.Find("ClueImage")
                ?? transform.Find("Icon")
                ?? transform.Find("Image");

            if (byName != null)
                clueImage = byName.GetComponent<Image>();

            if (clueImage == null)
                clueImage = GetComponentInChildren<Image>(true);
        }

        if (titleText == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                string n = text.gameObject.name.ToLowerInvariant();
                if (n.Contains("title") || n.Contains("name") || n.Contains("cluename"))
                {
                    titleText = text;
                    break;
                }
            }

            if (titleText == null && texts.Length > 0)
                titleText = texts[0];
        }

        if (tooltipAnchor == null && clueImage != null)
            tooltipAnchor = clueImage.rectTransform;

        if (removeButton == null)
        {
            Transform remove = transform.Find("RemoveButton")
                ?? transform.Find("DeleteButton")
                ?? transform.Find("CloseButton");

            if (remove != null)
                removeButton = remove.GetComponent<Button>();
        }

        if (unstableMark == null)
        {
            Transform mark = transform.Find("UnstableMark")
                ?? transform.Find("JokerMark")
                ?? transform.Find("FogMark");

            if (mark != null)
                unstableMark = mark.gameObject;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[System.Serializable]
public class CharacterData
{
    [Tooltip("ID персонажа — должен совпадать с SilhouetteInteractable.characterId")]
    public string characterId;

    [Tooltip("Имя персонажа для отображения")]
    public string displayName;

    [Tooltip("Спрайт персонажа (цветной, не силует)\nПока нет арта — оставь пустым")]
    public Sprite characterSprite;

    [Tooltip("Мысли персонажа об Эвелин во время выступления")]
    [TextArea(3, 8)]
    public string thoughts;
}

public class CharacterPanel : MonoBehaviour
{
    [Header("UI компоненты")]
    [Tooltip("CanvasGroup на этом объекте — для fade анимации")]
    public CanvasGroup canvasGroup;

    [Tooltip("Image для спрайта персонажа (левая часть окна)")]
    public Image characterImage;

    [Tooltip("TMP имя персонажа")]
    public TextMeshProUGUI nameText;

    [Tooltip("TMP мысли персонажа")]
    public TextMeshProUGUI thoughtsText;

    [Tooltip("Кнопка закрытия (крестик)")]
    public Button closeButton;

    [Header("Данные персонажей")]
    [Tooltip("Данные 4 персонажей: Viktor, Leo, Helga, Mari")]
    public CharacterData[] characters;

    private System.Action _onClose;

    void Start()
    {
        gameObject.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    public void Show(string characterId, System.Action onClose = null)
    {
        _onClose = onClose;

        CharacterData data = System.Array.Find(characters, c => c.characterId == characterId);
        if (data == null)
        {
            Debug.LogError($"[CharacterPanel] Персонаж '{characterId}' не найден в массиве characters[]");
            return;
        }

        if (nameText != null) nameText.text = data.displayName;
        if (thoughtsText != null) thoughtsText.text = data.thoughts;

        if (characterImage != null)
        {
            if (data.characterSprite != null)
            {
                characterImage.sprite = data.characterSprite;
                characterImage.color = Color.white;
                characterImage.enabled = true;
            }
            else
            {
                characterImage.enabled = true;
                characterImage.color = new Color(0.3f, 0.3f, 0.3f);
            }
        }

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, 0.3f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }

        Debug.Log($"[CharacterPanel] Открыт: {data.displayName}");
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, 0.25f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    _onClose?.Invoke();
                });
        }
        else
        {
            gameObject.SetActive(false);
            _onClose?.Invoke();
        }
    }
}
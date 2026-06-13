using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public sealed class JackpotTutorialOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup root;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Button closeButton;
    [SerializeField] private float fadeDuration = 0.25f;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }
    }

    public void Show()
    {
        if (tutorialText != null)
        {
            tutorialText.text =
                "Автомат не показывает правильный выбор. " +
                "Каждый прокрут усиливает риск. " +
                "Остановиться можно в любой момент, но последствия проявятся позже.";
        }

        if (root == null)
            return;

        root.gameObject.SetActive(true);
        root.blocksRaycasts = true;
        root.interactable = true;
        root.alpha = 0f;
        root.DOFade(1f, fadeDuration);
    }

    public void Hide()
    {
        if (root == null)
            return;

        root.DOKill();
        root.blocksRaycasts = false;
        root.interactable = false;
        root.DOFade(0f, fadeDuration).OnComplete(() => root.gameObject.SetActive(false));
    }

    private void OnDestroy()
    {
        if (root != null)
            root.DOKill();
    }
}

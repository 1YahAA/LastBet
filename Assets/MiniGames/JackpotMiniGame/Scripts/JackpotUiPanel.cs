using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class JackpotUiPanel : MonoBehaviour
{
    [Header("Кнопки")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button continueButton;

    [Header("Joker Card")]
    [SerializeField] private RectTransform jokerRewardCard;
    [SerializeField] private CanvasGroup jokerCardCanvasGroup;
    [SerializeField] private RectTransform jokerCardSpawnPoint;
    [SerializeField] private RectTransform jokerCardTargetPoint;

    [Header("Анимация")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float jokerMoveDuration = 0.7f;

    public Button SpinButton => spinButton;
    public Button ContinueButton => continueButton;

    private Image _jokerImage;
    private Sequence _jokerSequence;

    private void Awake()
    {
        CacheJokerComponents();
    }

    public void Initialize()
    {
        CacheJokerComponents();

        SetSpinEnabled(true);
        SetContinueVisible(false);
        HideJokerCardInstant();
    }

    public void OnSpinStarted()
    {
        SetSpinEnabled(false);
        SetContinueVisible(false);
    }

    public void OnSpinEnded(bool canSpinAgain)
    {
        SetSpinEnabled(canSpinAgain);
        SetContinueVisible(false);
    }

    public void SetSpinEnabled(bool enabled)
    {
        if (spinButton == null)
            return;

        spinButton.interactable = enabled;
        spinButton.gameObject.SetActive(enabled);
    }

    public void SetContinueVisible(bool visible)
    {
        if (continueButton == null)
            return;

        continueButton.gameObject.SetActive(visible);
        continueButton.interactable = visible;
    }

    public void ShowFinalState()
    {
        SetSpinEnabled(false);
        SetContinueVisible(true);
    }

    public void AnimateJokerCard(Action onComplete)
    {
        CacheJokerComponents();

        if (jokerRewardCard == null)
        {
            Debug.LogWarning("[JackpotUiPanel] jokerRewardCard не назначен.", this);
            onComplete?.Invoke();
            return;
        }

        _jokerSequence?.Kill();
        jokerRewardCard.DOKill();
        jokerCardCanvasGroup?.DOKill();

        jokerRewardCard.gameObject.SetActive(true);
        jokerRewardCard.SetAsLastSibling();
        jokerRewardCard.localScale = Vector3.one * 0.75f;

        if (jokerCardSpawnPoint != null)
            jokerRewardCard.position = jokerCardSpawnPoint.position;

        ForceJokerImageAlpha(1f);

        if (jokerCardCanvasGroup != null)
        {
            jokerCardCanvasGroup.alpha = 0f;
            jokerCardCanvasGroup.interactable = false;
            jokerCardCanvasGroup.blocksRaycasts = false;
        }

        Vector3 targetPosition = jokerCardTargetPoint != null
            ? jokerCardTargetPoint.position
            : jokerRewardCard.position;

        _jokerSequence = DOTween.Sequence();

        if (jokerCardCanvasGroup != null)
            _jokerSequence.Append(jokerCardCanvasGroup.DOFade(1f, fadeDuration));
        else
            _jokerSequence.AppendInterval(fadeDuration);

        _jokerSequence.Join(jokerRewardCard.DOScale(1f, fadeDuration));
        _jokerSequence.Append(jokerRewardCard.DOMove(targetPosition, jokerMoveDuration).SetEase(Ease.OutCubic));
        _jokerSequence.OnComplete(() => onComplete?.Invoke());
    }

    private void HideJokerCardInstant()
    {
        _jokerSequence?.Kill();

        if (jokerRewardCard != null)
        {
            jokerRewardCard.DOKill();
            jokerRewardCard.localScale = Vector3.one;

            if (jokerCardSpawnPoint != null)
                jokerRewardCard.position = jokerCardSpawnPoint.position;

            jokerRewardCard.gameObject.SetActive(false);
        }

        ForceJokerImageAlpha(1f);

        if (jokerCardCanvasGroup != null)
        {
            jokerCardCanvasGroup.DOKill();
            jokerCardCanvasGroup.alpha = 0f;
            jokerCardCanvasGroup.interactable = false;
            jokerCardCanvasGroup.blocksRaycasts = false;
        }
    }

    private void CacheJokerComponents()
    {
        if (jokerRewardCard != null && _jokerImage == null)
            _jokerImage = jokerRewardCard.GetComponent<Image>();

        if (jokerRewardCard != null && jokerCardCanvasGroup == null)
            jokerCardCanvasGroup = jokerRewardCard.GetComponent<CanvasGroup>();

        if (jokerRewardCard != null && jokerCardCanvasGroup == null)
            jokerCardCanvasGroup = jokerRewardCard.gameObject.AddComponent<CanvasGroup>();
    }

    private void ForceJokerImageAlpha(float alpha)
    {
        if (_jokerImage == null)
            return;

        Color color = _jokerImage.color;
        color.a = alpha;
        _jokerImage.color = color;
        _jokerImage.enabled = _jokerImage.sprite != null;
        _jokerImage.preserveAspect = true;
    }

    private void OnDestroy()
    {
        _jokerSequence?.Kill();

        if (jokerRewardCard != null)
            jokerRewardCard.DOKill();

        if (jokerCardCanvasGroup != null)
            jokerCardCanvasGroup.DOKill();
    }
}

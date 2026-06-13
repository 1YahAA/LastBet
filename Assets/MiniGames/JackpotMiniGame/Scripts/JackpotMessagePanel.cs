using TMPro;
using UnityEngine;
using DG.Tweening;

public sealed class JackpotMessagePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.2f;

    public void ShowIntro()
    {
        Show("Старый автомат ждёт ставки. Можно уйти сразу. Можно рискнуть.");
    }

    public void ShowSpinStarted()
    {
        Show("Барабаны начинают движение. Металл внутри звучит слишком живо.");
    }

    public void ShowSpinResult(JackpotSpinResult result, JackpotRiskModel risk)
    {
        if (result == null)
        {
            Show("Автомат молчит.");
            return;
        }

        if (result.HasHairpin)
        {
            Show("Среди символов мелькает тонкая золотая заколка. Слишком знакомая деталь для случайного автомата.");
            return;
        }

        if (result.HasDebt)
        {
            Show("На стекле проступает долговая метка. Выигрыш становится уже не главным.");
            return;
        }

        if (risk != null && (int)risk.CurrentRiskLevel >= (int)JackpotRiskLevel.High)
        {
            Show("Автомат будто ждёт, что Эвелин сделает ещё один шаг.");
            return;
        }

        if (result.RewardDelta > 0)
        {
            Show("Монеты падают мягко. Именно так автомат заставляет поверить ему ещё раз.");
            return;
        }

        Show("Пустой ход. Но тишина почему-то давит сильнее проигрыша.");
    }

    public void ShowDecision(JackpotRiskModel risk)
    {
        if (risk != null && (int)risk.CurrentRiskLevel >= (int)JackpotRiskLevel.High)
        {
            Show("Можно остановиться. Можно проверить, насколько далеко автомат позволит зайти.");
            return;
        }

        Show("Забрать результат или сделать ещё один прокрут?");
    }

    public void ShowForcedEnd()
    {
        Show("Автомат больше не спрашивает. Он уже решил, что игра закончена.");
    }

    public void Show(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeDuration);
        }
    }

    private void OnDestroy()
    {
        if (canvasGroup != null)
            canvasGroup.DOKill();
    }
}

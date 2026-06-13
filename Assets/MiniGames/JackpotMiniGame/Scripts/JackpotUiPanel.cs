using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public sealed class JackpotUiPanel : MonoBehaviour
{
    [Header("Кнопки")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button continueButton;

    [Header("Тексты")]
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text riskText;
    [SerializeField] private TMP_Text spinText;
    [SerializeField] private TMP_Text debtText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text resultDebtText;

    [Header("Панели")]
    [SerializeField] private GameObject resultPanel;

    [Header("Risk Indicator / лампочки")]
    [SerializeField] private Graphic riskGlowLow;
    [SerializeField] private Graphic riskGlowMedium;
    [SerializeField] private Graphic riskGlowHigh;
    [SerializeField] private float activeLampAlpha = 1f;
    [SerializeField] private float inactiveLampAlpha = 0.18f;

    [Header("Machine Effects")]
    [SerializeField] private Graphic machineNoiseOverlay;
    [SerializeField] private Graphic debtFlashOverlay;
    [SerializeField] private Graphic machineEyeLight;
    [SerializeField] private Graphic reelShadowOverlay;

    [Header("Рычаг")]
    [SerializeField] private GameObject leverIdle;
    [SerializeField] private GameObject leverPull;

    [Header("Анимация")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float flashDuration = 0.18f;
    [SerializeField] private float spinReelShadowAlpha = 0.55f;
    [SerializeField] private float idleReelShadowAlpha = 0.25f;

    public Button SpinButton => spinButton;
    public Button StopButton => stopButton;
    public Button ContinueButton => continueButton;

    public void Initialize()
    {
        SetResultPanel(false);
        SetSpinEnabled(true);
        SetStopEnabled(false);
        SetContinueEnabled(false);
        SetLeverPulled(false);
        SetRiskVisuals(JackpotRiskLevel.Low);
        SetGraphicAlpha(debtFlashOverlay, 0f);
        SetGraphicAlpha(machineEyeLight, 0f);
        SetGraphicAlpha(reelShadowOverlay, idleReelShadowAlpha);
    }

    public void SetSpinEnabled(bool value)
    {
        if (spinButton != null)
            spinButton.interactable = value;
    }

    public void SetStopEnabled(bool value)
    {
        if (stopButton != null)
            stopButton.interactable = value;
    }

    public void SetContinueEnabled(bool value)
    {
        if (continueButton != null)
            continueButton.interactable = value;
    }

    public void OnSpinStarted()
    {
        SetLeverPulled(true);
        FadeGraphic(reelShadowOverlay, spinReelShadowAlpha, fadeDuration);
    }

    public void OnSpinEnded()
    {
        SetLeverPulled(false);
        FadeGraphic(reelShadowOverlay, idleReelShadowAlpha, fadeDuration);
    }

    public void UpdateState(JackpotRiskModel risk)
    {
        if (risk == null)
            return;

        if (rewardText != null)
            rewardText.text = $"Выигрыш: {risk.Reward}";

        if (riskText != null)
            riskText.text = $"Риск: {RiskName(risk.CurrentRiskLevel)}";

        if (spinText != null)
            spinText.text = $"Прокрутки: {risk.SpinCount} / {risk.MaxSpins}";

        if (debtText != null)
            debtText.text = risk.Debt.ToString();

        SetRiskVisuals(risk.CurrentRiskLevel);
    }

    public void ShowDebtFlash()
    {
        if (debtFlashOverlay == null)
            return;

        debtFlashOverlay.DOKill();
        SetGraphicAlpha(debtFlashOverlay, 0f);
        debtFlashOverlay.DOFade(1f, flashDuration)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() => SetGraphicAlpha(debtFlashOverlay, 0f));
    }

    public void ShowHairpinInterference()
    {
        if (machineEyeLight != null)
        {
            machineEyeLight.DOKill();
            machineEyeLight.DOFade(0.8f, fadeDuration)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => SetGraphicAlpha(machineEyeLight, 0f));
        }

        if (machineNoiseOverlay != null)
        {
            machineNoiseOverlay.DOKill();
            float target = Mathf.Max(machineNoiseOverlay.color.a, 0.45f);
            machineNoiseOverlay.DOFade(target, fadeDuration);
        }
    }

    public void ShowResult(JackpotFinalResult result)
    {
        SetResultPanel(true);
        SetSpinEnabled(false);
        SetStopEnabled(false);
        SetContinueEnabled(true);
        SetLeverPulled(false);

        if (result == null)
            return;

        if (resultText != null)
            resultText.text = BuildResultText(result);

        if (resultDebtText != null)
            resultDebtText.text = result.Debt.ToString();
    }

    private string BuildResultText(JackpotFinalResult result)
    {
        return result.Outcome switch
        {
            JackpotOutcome.ControlledExit => "Вы остановились вовремя.",
            JackpotOutcome.RiskyDefiance => "Вы остановились, но автомат уже успел оставить след.",
            JackpotOutcome.TrappedByDebt => "Некоторые долги остаются даже после тишины.",
            JackpotOutcome.ForcedStop => "Автомат оборвал игру сам.",
            _ => "Автомат замолкает."
        };
    }

    private void SetResultPanel(bool value)
    {
        if (resultPanel != null)
            resultPanel.SetActive(value);
    }

    private void SetLeverPulled(bool pulled)
    {
        if (leverIdle != null)
            leverIdle.SetActive(!pulled);

        if (leverPull != null)
            leverPull.SetActive(pulled);
    }

    private void SetRiskVisuals(JackpotRiskLevel level)
    {
        SetLamp(riskGlowLow, level == JackpotRiskLevel.Low);
        SetLamp(riskGlowMedium, level == JackpotRiskLevel.Medium);
        SetLamp(riskGlowHigh, level == JackpotRiskLevel.High || level == JackpotRiskLevel.Critical);

        if (machineNoiseOverlay != null)
        {
            float noise = level switch
            {
                JackpotRiskLevel.Low => 0.05f,
                JackpotRiskLevel.Medium => 0.18f,
                JackpotRiskLevel.High => 0.35f,
                JackpotRiskLevel.Critical => 0.55f,
                _ => 0f
            };

            FadeGraphic(machineNoiseOverlay, noise, fadeDuration);
        }

        if (machineEyeLight != null && (level == JackpotRiskLevel.High || level == JackpotRiskLevel.Critical))
        {
            FadeGraphic(machineEyeLight, level == JackpotRiskLevel.Critical ? 0.55f : 0.32f, fadeDuration);
        }
        else if (machineEyeLight != null)
        {
            FadeGraphic(machineEyeLight, 0f, fadeDuration);
        }
    }

    private void SetLamp(Graphic graphic, bool active)
    {
        FadeGraphic(graphic, active ? activeLampAlpha : inactiveLampAlpha, fadeDuration);
    }

    private void SetGraphicAlpha(Graphic graphic, float value)
    {
        if (graphic == null)
            return;

        Color color = graphic.color;
        color.a = value;
        graphic.color = color;
    }

    private void FadeGraphic(Graphic graphic, float alpha, float duration)
    {
        if (graphic == null)
            return;

        graphic.DOKill();
        graphic.DOFade(alpha, duration);
    }

    private string RiskName(JackpotRiskLevel level)
    {
        return level switch
        {
            JackpotRiskLevel.Low => "низкий",
            JackpotRiskLevel.Medium => "средний",
            JackpotRiskLevel.High => "высокий",
            JackpotRiskLevel.Critical => "критический",
            _ => "низкий"
        };
    }

    private void OnDestroy()
    {
        if (riskGlowLow != null) riskGlowLow.DOKill();
        if (riskGlowMedium != null) riskGlowMedium.DOKill();
        if (riskGlowHigh != null) riskGlowHigh.DOKill();
        if (machineNoiseOverlay != null) machineNoiseOverlay.DOKill();
        if (debtFlashOverlay != null) debtFlashOverlay.DOKill();
        if (machineEyeLight != null) machineEyeLight.DOKill();
        if (reelShadowOverlay != null) reelShadowOverlay.DOKill();
    }
}

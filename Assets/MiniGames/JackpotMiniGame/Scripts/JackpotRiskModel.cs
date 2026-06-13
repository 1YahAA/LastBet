using UnityEngine;

public sealed class JackpotRiskModel : MonoBehaviour
{
    [Header("Пороги риска")]
    [SerializeField] private int mediumRiskThreshold = 35;
    [SerializeField] private int highRiskThreshold = 70;
    [SerializeField] private int criticalRiskThreshold = 100;

    [Header("Ограничения")]
    [SerializeField] private int maxSpins = 6;
    [SerializeField] private int maxDebt = 3;

    public int SpinCount { get; private set; }
    public int Reward { get; private set; }
    public int Debt { get; private set; }
    public int RiskScore { get; private set; }
    public JackpotRiskLevel CurrentRiskLevel { get; private set; } = JackpotRiskLevel.Low;

    public bool HasRewardToTake => Reward > 0;
    public bool ShouldForceEnd => RiskScore >= criticalRiskThreshold || Debt >= maxDebt || SpinCount >= maxSpins;
    public int MaxSpins => maxSpins;

    public void ResetModel()
    {
        SpinCount = 0;
        Reward = 0;
        Debt = 0;
        RiskScore = 0;
        CurrentRiskLevel = JackpotRiskLevel.Low;
    }

    public void ApplySpin(JackpotSpinResult result)
    {
        if (result == null)
            return;

        SpinCount++;
        Reward = Mathf.Max(0, Reward + result.RewardDelta);
        Debt = Mathf.Max(0, Debt + result.DebtDelta);
        RiskScore = Mathf.Max(0, RiskScore + result.RiskDelta);
        CurrentRiskLevel = ResolveRiskLevel(RiskScore);
    }

    private JackpotRiskLevel ResolveRiskLevel(int score)
    {
        if (score >= criticalRiskThreshold)
            return JackpotRiskLevel.Critical;

        if (score >= highRiskThreshold)
            return JackpotRiskLevel.High;

        if (score >= mediumRiskThreshold)
            return JackpotRiskLevel.Medium;

        return JackpotRiskLevel.Low;
    }
}

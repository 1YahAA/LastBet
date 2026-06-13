using System;

[Serializable]
public sealed class JackpotFinalResult
{
    public JackpotOutcome Outcome { get; }
    public JackpotBehaviourToken Token { get; }
    public JackpotLeoRelationState LeoRelationState { get; }
    public JackpotRiskLevel RiskLevel { get; }
    public int SpinCount { get; }
    public int Reward { get; }
    public int Debt { get; }
    public int RiskScore { get; }
    public bool StoppedByPlayer { get; }
    public bool SawHairpin { get; }
    public bool SawDebt { get; }

    public JackpotFinalResult(
        JackpotOutcome outcome,
        JackpotBehaviourToken token,
        JackpotLeoRelationState leoRelationState,
        JackpotRiskLevel riskLevel,
        int spinCount,
        int reward,
        int debt,
        int riskScore,
        bool stoppedByPlayer,
        bool sawHairpin,
        bool sawDebt)
    {
        Outcome = outcome;
        Token = token;
        LeoRelationState = leoRelationState;
        RiskLevel = riskLevel;
        SpinCount = spinCount;
        Reward = reward;
        Debt = debt;
        RiskScore = riskScore;
        StoppedByPlayer = stoppedByPlayer;
        SawHairpin = sawHairpin;
        SawDebt = sawDebt;
    }
}

using UnityEngine;

public sealed class JackpotResultResolver : MonoBehaviour
{
    [Header("Правила результата")]
    [SerializeField] private int earlyStopMaxSpins = 2;
    [SerializeField] private int riskyDefianceMinSpins = 3;
    [SerializeField] private int coldDebtThreshold = 2;

    public JackpotFinalResult Resolve(JackpotRiskModel risk, JackpotNarrativeState narrative, bool stoppedByPlayer)
    {
        if (risk == null)
        {
            return new JackpotFinalResult(
                JackpotOutcome.ControlledExit,
                JackpotBehaviourToken.Analysis,
                JackpotLeoRelationState.Ambiguous,
                JackpotRiskLevel.Low,
                0,
                0,
                0,
                0,
                stoppedByPlayer,
                false,
                false
            );
        }

        bool sawHairpin = narrative != null && narrative.SawHairpin;
        bool sawDebt = narrative != null && narrative.SawDebt;
        bool forced = narrative != null && narrative.WasForcedToStop;

        JackpotOutcome outcome = ResolveOutcome(risk, stoppedByPlayer, forced);
        JackpotBehaviourToken token = ResolveToken(risk, outcome, stoppedByPlayer);
        JackpotLeoRelationState leoState = ResolveLeoState(risk, outcome, stoppedByPlayer);

        return new JackpotFinalResult(
            outcome,
            token,
            leoState,
            risk.CurrentRiskLevel,
            risk.SpinCount,
            risk.Reward,
            risk.Debt,
            risk.RiskScore,
            stoppedByPlayer,
            sawHairpin,
            sawDebt
        );
    }

    private JackpotOutcome ResolveOutcome(JackpotRiskModel risk, bool stoppedByPlayer, bool forced)
    {
        if (risk.Debt >= coldDebtThreshold)
            return JackpotOutcome.TrappedByDebt;

        if (forced || risk.CurrentRiskLevel == JackpotRiskLevel.Critical)
            return JackpotOutcome.ForcedStop;

        if (stoppedByPlayer && risk.SpinCount <= earlyStopMaxSpins)
            return JackpotOutcome.ControlledExit;

        return JackpotOutcome.RiskyDefiance;
    }

    private JackpotBehaviourToken ResolveToken(JackpotRiskModel risk, JackpotOutcome outcome, bool stoppedByPlayer)
    {
        if (outcome == JackpotOutcome.ControlledExit)
            return JackpotBehaviourToken.Analysis;

        if (outcome == JackpotOutcome.TrappedByDebt || outcome == JackpotOutcome.ForcedStop)
            return JackpotBehaviourToken.Obedience;

        if (stoppedByPlayer && risk.SpinCount >= riskyDefianceMinSpins)
            return JackpotBehaviourToken.Revolt;

        return JackpotBehaviourToken.Analysis;
    }

    private JackpotLeoRelationState ResolveLeoState(JackpotRiskModel risk, JackpotOutcome outcome, bool stoppedByPlayer)
    {
        if (outcome == JackpotOutcome.ControlledExit)
            return JackpotLeoRelationState.Friendly;

        if (outcome == JackpotOutcome.TrappedByDebt || (int)risk.CurrentRiskLevel >= (int)JackpotRiskLevel.High)
            return JackpotLeoRelationState.SilentObserver;

        return JackpotLeoRelationState.Ambiguous;
    }
}

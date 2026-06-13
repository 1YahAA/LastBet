using UnityEngine;

public sealed class JackpotGameStateAdapter : MonoBehaviour
{
    [Header("Интеграция")]
    [SerializeField] private bool finishThroughGameManager = true;
    [SerializeField] private bool showDebugLogs = true;

    public void ApplyResult(JackpotFinalResult result)
    {
        if (result == null)
            return;

        if (GameManager.Instance == null || GameManager.Instance.gameState == null)
        {
            Debug.LogWarning("[JackpotGameStateAdapter] GameManager или GameState не найден.");
            return;
        }

        GameManager.Instance.gameState.ApplyJackpotResult(
            result.Outcome.ToString(),
            result.Token.ToString(),
            result.LeoRelationState.ToString(),
            result.RiskLevel.ToString(),
            result.SpinCount,
            result.Reward,
            result.Debt,
            result.RiskScore,
            result.StoppedByPlayer,
            result.SawHairpin,
            result.SawDebt
        );

        if (showDebugLogs)
        {
            Debug.Log(
                $"[JackpotGameStateAdapter] Saved only | Outcome={result.Outcome}, " +
                $"Token={result.Token}, LeoNext={result.LeoRelationState}, " +
                $"Hairpin={result.SawHairpin}, Debt={result.Debt}"
            );
        }
    }

    public void FinishMiniGame(JackpotFinalResult result)
    {
        if (result == null)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[JackpotGameStateAdapter] GameManager.Instance не найден. Результат не может быть завершён через основную игру.");
            return;
        }

        if (finishThroughGameManager)
        {
            GameManager.Instance.FinishJackpotMiniGame(
                result.Outcome.ToString(),
                result.Token.ToString(),
                result.LeoRelationState.ToString(),
                result.RiskLevel.ToString(),
                result.SpinCount,
                result.Reward,
                result.Debt,
                result.RiskScore,
                result.StoppedByPlayer,
                result.SawHairpin,
                result.SawDebt
            );
            return;
        }

        ApplyResult(result);
    }
}
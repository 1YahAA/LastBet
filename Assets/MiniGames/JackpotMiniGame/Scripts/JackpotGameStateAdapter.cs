using UnityEngine;

public sealed class JackpotGameStateAdapter : MonoBehaviour
{
    [SerializeField] private bool showDebugLogs = true;

    public void ApplyResult(JackpotFinalResult result)
    {
        if (result == null) return;

        if (GameManager.Instance == null || GameManager.Instance.gameState == null)
        {
            Debug.LogWarning("[JackpotGameStateAdapter] GameManager или GameState не найден.");
            return;
        }

        GameManager.Instance.gameState.ApplyJackpotResult(
            result.Outcome.ToString(),
            result.JokerCardObtained,
            result.RevoltDelta,
            result.ObedienceDelta,
            result.AnalysisDelta);

        if (showDebugLogs)
            Debug.Log($"[JackpotAdapter] Outcome={result.Outcome} Joker={result.JokerCardObtained} Spins={result.SpinCount}");
    }

    public void FinishMiniGame(JackpotFinalResult result)
    {
        if (result == null) return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[JackpotGameStateAdapter] GameManager.Instance не найден.");
            return;
        }

        ApplyResult(result);
        OfficeDirector.SetMiniGameResult(result.IsJackpot);
        GameManager.Instance.ReturnFromMiniGame();
    }
}
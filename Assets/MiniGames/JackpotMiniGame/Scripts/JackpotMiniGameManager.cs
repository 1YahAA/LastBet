using System.Collections;
using UnityEngine;

public sealed class JackpotMiniGameManager : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private JackpotSlotMachine slotMachine;
    [SerializeField] private JackpotResultResolver resultResolver;
    [SerializeField] private JackpotGameStateAdapter gameStateAdapter;

    [Header("UI")]
    [SerializeField] private JackpotUiPanel uiPanel;
    [SerializeField] private JackpotMessagePanel messagePanel;

    [Header("Настройки")]
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private int maxSpinCount = 3;

    public JackpotMiniGameState State { get; private set; } = JackpotMiniGameState.NotStarted;
    public JackpotFinalResult FinalResult { get; private set; }

    private int _spinCount;
    private Coroutine _spinRoutine;

    private void Awake() => BindButtons();

    private void Start()
    {
        if (startOnAwake) StartMiniGame();
    }

    public void StartMiniGame()
    {
        State = JackpotMiniGameState.Idle;
        FinalResult = null;
        _spinCount = 0;
        _spinRoutine = null;

        uiPanel?.Initialize();
        messagePanel?.ShowIntro();
    }

    public void Spin()
    {
        if (State != JackpotMiniGameState.Idle) return;
        if (_spinRoutine != null) return;

        if (slotMachine == null || resultResolver == null)
        {
            Debug.LogWarning("[Jackpot] slotMachine или resultResolver не назначены.", this);
            return;
        }

        _spinRoutine = StartCoroutine(SpinFlow());
    }

    private IEnumerator SpinFlow()
    {
        State = JackpotMiniGameState.Spinning;
        _spinCount++;

        bool mustRollJackpot = _spinCount >= maxSpinCount;

        uiPanel?.OnSpinStarted();
        messagePanel?.Show($"Крутка {_spinCount} из {maxSpinCount}. Барабаны крутятся...");

        JackpotSpinResult spinResult = null;
        yield return slotMachine.SpinRoutine(_spinCount, mustRollJackpot, result => spinResult = result);

        JackpotFinalResult resolved = resultResolver.ResolveSpin(spinResult, _spinCount);

        if (resolved != null && resolved.IsJackpot)
        {
            yield return JackpotFlow(resolved);
            _spinRoutine = null;
            yield break;
        }

        messagePanel?.ShowSpinResult(resolved);

        State = JackpotMiniGameState.Idle;
        uiPanel?.OnSpinEnded(_spinCount < maxSpinCount);

        _spinRoutine = null;
    }

    private IEnumerator JackpotFlow(JackpotFinalResult result)
    {
        State = JackpotMiniGameState.ShowingResult;
        FinalResult = result;

        uiPanel?.SetSpinEnabled(false);
        uiPanel?.SetContinueVisible(false);

        messagePanel?.Show("ДЖЕКПОТ! Автомат выдаёт карту.");
        yield return new WaitForSeconds(0.45f);

        messagePanel?.Show("Эвелин замирает: «Автомат выдаёт карты?»");

        bool animationDone = false;

        if (uiPanel != null)
        {
            uiPanel.AnimateJokerCard(() => animationDone = true);

            float timeout = 3f;
            while (!animationDone && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.35f);
        messagePanel?.Show("Эвелин убирает карту в карман.");

        State = JackpotMiniGameState.Completed;
        uiPanel?.ShowFinalState();
    }

    public void ContinueAfterResult()
    {
        if (FinalResult == null) return;


        if (gameStateAdapter != null)
        {
            gameStateAdapter.FinishMiniGame(FinalResult);
            return;
        }

        if (GameManager.Instance != null)
        {
            OfficeDirector.SetMiniGameResult(FinalResult.IsJackpot);
            GameManager.Instance.ReturnFromMiniGame();
        }
    }

    private void BindButtons()
    {
        if (uiPanel == null) return;

        if (uiPanel.SpinButton != null)
        {
            uiPanel.SpinButton.onClick.RemoveAllListeners();
            uiPanel.SpinButton.onClick.AddListener(Spin);
        }

        if (uiPanel.ContinueButton != null)
        {
            uiPanel.ContinueButton.onClick.RemoveAllListeners();
            uiPanel.ContinueButton.onClick.AddListener(ContinueAfterResult);
        }
    }
}
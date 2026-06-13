using System.Collections;
using UnityEngine;

public sealed class JackpotMiniGameManager : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private JackpotSlotMachine slotMachine;
    [SerializeField] private JackpotRiskModel riskModel;
    [SerializeField] private JackpotNarrativeState narrativeState;
    [SerializeField] private JackpotResultResolver resultResolver;
    [SerializeField] private JackpotGameStateAdapter gameStateAdapter;

    [Header("UI")]
    [SerializeField] private JackpotUiPanel uiPanel;
    [SerializeField] private JackpotMessagePanel messagePanel;
    [SerializeField] private JackpotTutorialOverlay tutorialOverlay;

    [Header("Настройки")]
    [SerializeField] private bool startOnAwake = false;
    [SerializeField] private bool showTutorialOnStart = true;

    public JackpotMiniGameState State { get; private set; } = JackpotMiniGameState.NotStarted;
    public JackpotFinalResult FinalResult { get; private set; }

    private Coroutine _spinRoutine;

    private void Awake()
    {
        BindButtons();
    }

    private void Start()
    {
        if (startOnAwake)
            StartMiniGame();
    }

    public void StartMiniGame()
    {
        State = JackpotMiniGameState.Intro;
        FinalResult = null;

        riskModel?.ResetModel();
        narrativeState?.ResetState();
        uiPanel?.Initialize();
        uiPanel?.UpdateState(riskModel);
        messagePanel?.ShowIntro();

        if (showTutorialOnStart && tutorialOverlay != null)
            tutorialOverlay.Show();

        State = JackpotMiniGameState.Idle;
    }

    public void Spin()
    {
        if (State != JackpotMiniGameState.Idle && State != JackpotMiniGameState.Decision)
            return;

        if (slotMachine == null || riskModel == null)
        {
            Debug.LogWarning("[JackpotMiniGameManager] Не назначены slotMachine или riskModel.");
            return;
        }

        if (_spinRoutine != null)
            StopCoroutine(_spinRoutine);

        _spinRoutine = StartCoroutine(SpinFlow());
    }

    public void StopAndTakeResult()
    {
        if (State != JackpotMiniGameState.Decision && State != JackpotMiniGameState.Idle)
            return;

        narrativeState?.RegisterPlayerStop();
        Complete(stoppedByPlayer: true);
    }

    public void ContinueToMainGame()
    {
        if (FinalResult == null)
            return;

        gameStateAdapter?.FinishMiniGame(FinalResult);
    }

    private IEnumerator SpinFlow()
    {
        State = JackpotMiniGameState.Spinning;
        uiPanel?.SetSpinEnabled(false);
        uiPanel?.SetStopEnabled(false);
        uiPanel?.OnSpinStarted();
        messagePanel?.ShowSpinStarted();

        JackpotSpinResult spinResult = null;
        yield return slotMachine.SpinRoutine(riskModel, result => spinResult = result);

        uiPanel?.OnSpinEnded();

        State = JackpotMiniGameState.ResolvingSpin;
        riskModel.ApplySpin(spinResult);
        narrativeState?.RegisterSpin(spinResult, riskModel.SpinCount);

        uiPanel?.UpdateState(riskModel);
        messagePanel?.ShowSpinResult(spinResult, riskModel);

        if (spinResult != null && spinResult.HasDebt)
            uiPanel?.ShowDebtFlash();

        if (spinResult != null && spinResult.HasHairpin)
            uiPanel?.ShowHairpinInterference();

        if (riskModel.ShouldForceEnd)
        {
            narrativeState?.RegisterForcedStop();
            messagePanel?.ShowForcedEnd();
            Complete(stoppedByPlayer: false);
            _spinRoutine = null;
            yield break;
        }

        State = JackpotMiniGameState.Decision;
        messagePanel?.ShowDecision(riskModel);
        uiPanel?.SetSpinEnabled(true);
        uiPanel?.SetStopEnabled(riskModel.HasRewardToTake || riskModel.SpinCount > 0);
        _spinRoutine = null;
    }

    private void Complete(bool stoppedByPlayer)
    {
        if (State == JackpotMiniGameState.Completed)
            return;

        State = JackpotMiniGameState.Completed;
        uiPanel?.SetSpinEnabled(false);
        uiPanel?.SetStopEnabled(false);

        if (resultResolver == null)
        {
            Debug.LogWarning("[JackpotMiniGameManager] resultResolver не назначен.");
            return;
        }

        FinalResult = resultResolver.Resolve(riskModel, narrativeState, stoppedByPlayer);
        uiPanel?.ShowResult(FinalResult);
    }

    private void BindButtons()
    {
        if (uiPanel == null)
            return;

        if (uiPanel.SpinButton != null)
        {
            uiPanel.SpinButton.onClick.RemoveAllListeners();
            uiPanel.SpinButton.onClick.AddListener(Spin);
        }

        if (uiPanel.StopButton != null)
        {
            uiPanel.StopButton.onClick.RemoveAllListeners();
            uiPanel.StopButton.onClick.AddListener(StopAndTakeResult);
        }

        if (uiPanel.ContinueButton != null)
        {
            uiPanel.ContinueButton.onClick.RemoveAllListeners();
            uiPanel.ContinueButton.onClick.AddListener(ContinueToMainGame);
        }
    }
}

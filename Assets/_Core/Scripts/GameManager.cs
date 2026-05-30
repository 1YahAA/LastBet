using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Данные игры")]
    public GameState gameState;

    [Header("Порядок игровых сцен")]
    public string[] sceneOrder =
    {
        "Scene1_Cabaret",
        "Scene2_Dressing",
        "Scene3_Bar",
        "Scene4_Casino",
        "Scene5_Backstage",
        "Scene6_Office",
        "Scene7_FinalStake"
    };

    public GameplayState CurrentState { get; private set; } = GameplayState.MainMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (SaveSystem.HasSave())
            SaveSystem.Load(gameState);
        else
            gameState.ResetAll();

        SetState(GameplayState.MainMenu);
        SceneTransition.Instance?.FadeIn();
    }

 
    public void StartNewGame()
    {
        gameState.ResetAll();
        SaveSystem.Delete();
        LoadSceneByIndex(0);
    }

    public void ContinueGame()
    {
        LoadSceneByIndex(gameState.currentSceneIndex);
    }

 
    public void LoadNextScene()
    {
        LoadSceneByIndex(gameState.currentSceneIndex + 1);
    }

    public void LoadSceneByIndex(int index)
    {
        if (index >= sceneOrder.Length)
        {
            LoadEnding();
            return;
        }

        gameState.currentSceneIndex = index;
        SaveSystem.Save(gameState);
        SetState(GameplayState.Playing);
        SceneTransition.Instance.FadeToScene(sceneOrder[index]);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SetState(GameplayState.MainMenu);
        SceneTransition.Instance.FadeToScene("MainMenu");
    }

 
    public void LoadMiniGame(string miniGameSceneName, MiniGameType miniGameType)
    {
        gameState.returnSceneName = sceneOrder[gameState.currentSceneIndex];
        gameState.currentMiniGame = miniGameType;
        SetState(GameplayState.MiniGame);
        SceneTransition.Instance.FadeToScene(miniGameSceneName);
    }

    public void ReturnFromMiniGame()
    {
        SaveSystem.Save(gameState);
        SetState(GameplayState.Playing);
        SceneTransition.Instance.FadeToScene(gameState.returnSceneName);
    }

    public void FinishMiniGame(bool won)
    {
        gameState.AddToken(won ? TokenType.Revolt :
            gameState.currentMiniGame == MiniGameType.CardGame
                ? TokenType.Obedience
                : TokenType.Analysis);

        SaveSystem.Save(gameState);
        SetState(GameplayState.Playing);
        SceneTransition.Instance.FadeToScene(gameState.returnSceneName);
    }

    public void FinishMiniGame(bool won, TokenType token)
    {
        gameState.AddToken(token);
        SaveSystem.Save(gameState);
        SetState(GameplayState.Playing);
        SceneTransition.Instance.FadeToScene(gameState.returnSceneName);
    }

 
    public void LoadEnding()
    {
        SetState(GameplayState.Ending);
        SaveSystem.Delete();
        string endingScene = gameState.GetEnding().ToString();
        Debug.Log($"[GameManager] Концовка: {endingScene}");
        SceneTransition.Instance.FadeToScene(endingScene);
    }


    public void Pause()
    {
        if (CurrentState != GameplayState.Playing &&
            CurrentState != GameplayState.Dialogue) return;
        SetState(GameplayState.Paused);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (CurrentState != GameplayState.Paused) return;
        SetState(GameplayState.Playing);
        Time.timeScale = 1f;
    }


    public void OnDialogueStart() => SetState(GameplayState.Dialogue);
    public void OnDialogueEnd() => SetState(GameplayState.Playing);

    public bool IsPlaying => CurrentState == GameplayState.Playing;
    public bool IsPaused => CurrentState == GameplayState.Paused;
    public bool IsInDialogue => CurrentState == GameplayState.Dialogue;
    public bool IsInEnding => CurrentState == GameplayState.Ending;

    private void SetState(GameplayState newState)
    {
        CurrentState = newState;
    }
}
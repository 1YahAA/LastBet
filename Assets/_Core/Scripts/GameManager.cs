// Главный менеджер игры

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header(" Данные игры ")]
    public GameState gameState;

    [Header(" Порядок игровых сцен ")]
    public string[] sceneOrder =
    {
        "Scene1_Cabaret", // 0 — Выступление
        "Scene2_Dressing", // 1 — Гримёрная (коктейль, дверь, автомат)
        "Scene3_Bar",  // 2 — Бар (встреча с Лео)
        "Scene4_Casino", // 3 — Казино
        "Scene5_Backstage", // 4 — Закулисье
        "Scene6_Office", // 5 — Кабинет Виктора
        "Scene7_FinalStake" // 6 — Финальная ставка
    };

    // Текущее состояние игры — влияет на то, что можно делать
    public GameplayState CurrentState { get; private set; } = GameplayState.MainMenu;

    // ИНИЦИАЛИЗАЦИЯ

    private void Awake()
    {
        Debug.Log($"[GameManager] Awake | name={name} | scene={gameObject.scene.name} | active={gameObject.activeInHierarchy}", this);

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameManager] Duplicate destroyed", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log($"[GameManager] Start | Instance={Instance}", this);
        
        if (SaveSystem.HasSave())
        {
            SaveSystem.Load(gameState);
            Debug.Log("[GameManager] Сохранение загружено");
        }
        else
        {
            gameState.ResetAll();
        }
        
        SceneTransition.Instance.FadeToScene("MainMenu");
    }
    
    private void OnDestroy()
    {
        Debug.LogWarning($"[GameManager] OnDestroy | name={name} | scene={gameObject.scene.name}", this);
    }

    // ЗАПУСК/ПРОДОЛЖЕНИЕ ИГРЫ

    // Новая игра — сбросить всё и начать с первой сцены
    public void StartNewGame()
    {
        gameState.ResetAll();
        SaveSystem.Delete();
        LoadSceneByIndex(0);
    }

    // Продолжить — загрузить сцену из сохранения
    public void ContinueGame()
    {
        LoadSceneByIndex(gameState.currentSceneIndex);
    }

    // НАВИГАЦИЯ ПО СЦЕНАМ

    // Перейти к следующей сцене по порядку
    public void LoadNextScene()
    {
        LoadSceneByIndex(gameState.currentSceneIndex + 1);
    }

    // Автоматически сохраняет прогресс
    public void LoadSceneByIndex(int index)
    {
        if (index >= sceneOrder.Length)
        {
            LoadEnding();
            return;
        }
        gameState.currentSceneIndex = index;
        SaveSystem.Save(gameState); // автосохранение
        SetState(GameplayState.Playing);
        SceneTransition.Instance.FadeToScene(sceneOrder[index]);
    }

    // Вернуться на главное меню (из паузы или концовки)
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // снять паузу на всякий случай
        SetState(GameplayState.MainMenu);
        SceneTransition.Instance.FadeToScene("MainMenu");
    }

    // МИНИ-ИГРЫ

    // Запустить мини-игру
    // Запоминает из какой сцены ушли, чтобы вернуться
    public void LoadMiniGame(string miniGameSceneName, MiniGameType miniGameType)
    {
        gameState.returnSceneName = sceneOrder[gameState.currentSceneIndex];
        gameState.currentMiniGame = miniGameType;
        SetState(GameplayState.MiniGame);
        SceneTransition.Instance.FadeToScene(miniGameSceneName);
    }

    // Завершить мини-игру и вернуться в сцену
    public void FinishMiniGame(bool won)
    {
        if (won)
        {
            gameState.AddToken(TokenType.Revolt);
        }
        else
        {
            // Разные жетоны при поражении для разных мини-игр
            switch (gameState.currentMiniGame)
            {
                case MiniGameType.CardGame:
                    gameState.AddToken(TokenType.Obedience); break;
                case MiniGameType.Roulette:
                    gameState.AddToken(TokenType.Analysis); break;
            }
        }
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

    // КОНЦОВКИ

    // Загрузить концовку по накопленным жетонам
    public void LoadEnding()
    {
        SetState(GameplayState.Ending);
        SaveSystem.Delete(); // после концовки сохранение сбрасываем
        string endingScene = gameState.GetEnding().ToString();
        Debug.Log($"[GameManager] Концовка: {endingScene} (Б:{gameState.revolt} П:{gameState.obedience} А:{gameState.analysis})");
        SceneTransition.Instance.FadeToScene(endingScene);
    }

    // ПАУЗА

    // Поставить паузу
    public void Pause()
    {
        if (CurrentState != GameplayState.Playing &&
            CurrentState != GameplayState.Dialogue) return;
        SetState(GameplayState.Paused);
        Time.timeScale = 0f;
    }

    // Снять паузу
    public void Resume()
    {
        if (CurrentState != GameplayState.Paused) return;
        SetState(GameplayState.Playing);
        Time.timeScale = 1f;
    }

    // ДИАЛОГ

    public void OnDialogueStart() => SetState(GameplayState.Dialogue);

    // Вызывать когда диалог Yarn Spinner завершился
    public void OnDialogueEnd() => SetState(GameplayState.Playing);

    // Удобные проверки
    public bool IsPlaying => CurrentState == GameplayState.Playing;
    public bool IsPaused => CurrentState == GameplayState.Paused;
    public bool IsInDialogue => CurrentState == GameplayState.Dialogue;
    public bool IsInEnding => CurrentState == GameplayState.Ending;

    private void SetState(GameplayState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] Состояние: {newState}");
    }
}
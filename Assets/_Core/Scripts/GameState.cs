using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "Game/GameState")]
public class GameState : ScriptableObject
{

    [Header("Жетоны (скрытые от игрока)")]
    [Tooltip("Бунт — за сопротивление и дерзкие выборы")]
    public int revolt;

    [Tooltip("Послушание — за подчинение и согласие")]
    public int obedience;

    [Tooltip("Анализ — за молчание и наблюдение")]
    public int analysis;

    [Header("Прогресс прохождения")]
    [Tooltip("Индекс текущей сцены в массиве sceneOrder[] в GameManager")]
    public int currentSceneIndex;

    [Tooltip("Имя сцены для возврата после мини-игры")]
    public string returnSceneName;

    [Tooltip("Тип запущенной мини-игры — чтобы знать какой жетон давать")]
    public MiniGameType currentMiniGame;


    [Header("Постоянные эффекты (из сценария)")]
    [Tooltip("Выпила ли Эвелин коктейль хоть раз — влияет на реплики и сложность")]
    public bool cocktailDrunk;

    [Tooltip("Сколько раз выпила коктейль — больше 1 даёт двойной штраф")]
    public int cocktailCount;


    public void AddToken(TokenType type, int amount = 1)
    {
        switch (type)
        {
            case TokenType.Revolt: revolt += amount; break;
            case TokenType.Obedience: obedience += amount; break;
            case TokenType.Analysis: analysis  += amount; break;
        }
    }

    public EndingType GetEnding()
    {
        if (revolt > obedience && revolt > analysis) return EndingType.Freedom;
        if (obedience > revolt && obedience > analysis) return EndingType.Submission;
        return EndingType.Death;
    }


    public void DrinkCocktail()
    {
        cocktailDrunk = true;
        cocktailCount++;
        AddToken(TokenType.Obedience);
        if (cocktailCount > 1) AddToken(TokenType.Analysis);
    }

    public void RefuseCocktail()
    {
        AddToken(TokenType.Revolt);
    }


    public void ResetAll()
    {
        revolt = 0;
        obedience = 0;
        analysis = 0;
        currentSceneIndex = 0;
        returnSceneName = "";
        cocktailDrunk = false;
        cocktailCount = 0;
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "Game/GameState")]
public class GameState : ScriptableObject
{
    // ЖЕТОНЫ (скрытые от игрока счётчики)
    // Накапливаются за каждый выбор в диалогах и мини-играх

    [Header("Жетоны (скрытые от игрока)")]
    [Tooltip("Бунт — за сопротивление и дерзкие выборы")]
    public int revolt;

    [Tooltip("Послушание — за подчинение и согласие")]
    public int obedience;

    [Tooltip("Анализ — за молчание и наблюдение")]
    public int analysis;

    // ПРОГРЕСС
    [Header("Прогресс прохождения")]
    [Tooltip("Индекс текущей сцены в массиве sceneOrder[] в GameManager")]
    public int currentSceneIndex;

    [Tooltip("Имя сцены для возврата после мини-игры")]
    public string returnSceneName;

    [Tooltip("Тип запущенной мини-игры — чтобы знать какой жетон давать")]
    public MiniGameType currentMiniGame;

    // ПОСТОЯННЫЕ ЭФФЕКТЫ
    // Коктейль из сценария: первое питьё → +1 Послушание.
    // Каждое следующее → +1 Послушание +1 Анализ.
    // Эффект влияет на всю игру: реплики Эвелин становятся злее,
    // мини-игры сложнее (реализуем через cocktailDrunk в логике сцен)

    [Header("Постоянные эффекты (из сценария)")]
    [Tooltip("Выпила ли Эвелин коктейль хоть раз — влияет на реплики и сложность")]
    public bool cocktailDrunk;

    [Tooltip("Сколько раз выпила коктейль — больше 1 даёт двойной штраф")]
    public int cocktailCount;

    // МЕТОДЫ — ЖЕТОНЫ

    /// Добавить жетон
    public void AddToken(TokenType type, int amount = 1)
    {
        switch (type)
        {
            case TokenType.Revolt: revolt += amount; break;
            case TokenType.Obedience: obedience += amount; break;
            case TokenType.Analysis: analysis  += amount; break;
        }
        // Лог виден в Console — удобно для отладки
        Debug.Log($"[Жетон] +{amount} {type} | Итого → Бунт:{revolt} Послушание:{obedience} Анализ:{analysis}");
    }

    /// Определить концовку по накопленным жетонам
    public EndingType GetEnding()
    {
        if (revolt > obedience && revolt > analysis) return EndingType.Freedom;
        if (obedience > revolt && obedience > analysis) return EndingType.Submission;
        return EndingType.Death;
    }

    // МЕТОДЫ — КОКТЕЙЛЬ

    // Вызывать когда Эвелин выпила коктейль
    public void DrinkCocktail()
    {
        cocktailDrunk = true;
        cocktailCount++;
        AddToken(TokenType.Obedience);
        if (cocktailCount > 1) AddToken(TokenType.Analysis);
        Debug.Log($"[Коктейль] Выпит раз: {cocktailCount}");
    }

    // Вызывать когда Эвелин отказалась от коктейля.
    public void RefuseCocktail()
    {
        AddToken(TokenType.Revolt);
        Debug.Log("[Коктейль] Отказ → +1 Бунт");
    }

    // СБРОС

    // Полный сброс — вызывается при старте новой игры
    public void ResetAll()
    {
        revolt = 0;
        obedience = 0;
        analysis = 0;
        currentSceneIndex = 0;
        returnSceneName = "";
        cocktailDrunk = false;
        cocktailCount = 0;
        Debug.Log("[GameState] Сброс выполнен");
    }
}
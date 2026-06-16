using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "Game/GameState")]
public class GameState : ScriptableObject
{
    [Header("Жетоны")]
    public int revolt;
    public int obedience;
    public int analysis;

    [Header("Прогресс")]
    public int currentSceneIndex;
    public string returnSceneName;
    public MiniGameType currentMiniGame;

    [Header("Гримёрка")]
    public bool cocktailDrunk;
    public bool cocktailInspected;
    public int cocktailCount;

    [Header("Бар")]
    public bool barMiniGameWon;
    public bool officeKeyObtained;
    public bool midnightPlanKnown;

    [Header("Джекпот")]
    public bool jackpotCompleted;
    public string jackpotOutcome;

    [Header("Джокер")]
    public bool jokerWon;
    public bool truthAvailable;

    [Header("Финал")]
    public bool finalBetWon;

    public void AddToken(TokenType type, int amount = 1)
    {
        switch (type)
        {
            case TokenType.Revolt: revolt += amount; break;
            case TokenType.Obedience: obedience += amount; break;
            case TokenType.Analysis: analysis += amount; break;
        }
        Debug.Log($"[Жетон] +{amount} {type} | Б:{revolt} П:{obedience} А:{analysis}");
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

    public void RefuseCocktail() => AddToken(TokenType.Revolt);

    public void InspectCocktail()
    {
        cocktailInspected = true;
        AddToken(TokenType.Analysis);
    }

    public void ApplyBarMiniGameResult(bool won)
    {
        barMiniGameWon = won;
        officeKeyObtained = true;
        midnightPlanKnown = won;
        AddToken(won ? TokenType.Analysis : TokenType.Obedience);
    }

    public void ApplyJackpotResult(
        string outcome, bool jokerObtained,
        int revoltDelta, int obedienceDelta, int analysisDelta)
    {
        jackpotCompleted = true;
        jackpotOutcome = outcome;
        if (revoltDelta > 0) AddToken(TokenType.Revolt, revoltDelta);
        if (obedienceDelta > 0) AddToken(TokenType.Obedience, obedienceDelta);
        if (analysisDelta > 0) AddToken(TokenType.Analysis, analysisDelta);
    }

    public void ApplyJokerResult(bool won)
    {
        jokerWon = won;
        if (won) { truthAvailable = true; AddToken(TokenType.Analysis); }
        else AddToken(TokenType.Obedience);
    }

    public void ApplyFinalBetResult(bool won)
    {
        finalBetWon = won;
        AddToken(won ? TokenType.Analysis : TokenType.Obedience);
    }

    public void ResetAll()
    {
        revolt = 0; obedience = 0; analysis = 0;
        currentSceneIndex = 0; returnSceneName = "";
        currentMiniGame = MiniGameType.CardGame;
        cocktailDrunk = false; cocktailInspected = false; cocktailCount = 0;
        barMiniGameWon = false; officeKeyObtained = false; midnightPlanKnown = false;
        jackpotCompleted = false; jackpotOutcome = "";
        jokerWon = false; truthAvailable = false;
        finalBetWon = false;
        Debug.Log("[GameState] Сброс выполнен");
    }
}
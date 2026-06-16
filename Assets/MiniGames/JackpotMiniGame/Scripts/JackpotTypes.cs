using System;

public enum JackpotMiniGameState
{
    NotStarted   = 0,
    Idle         = 1,
    Spinning     = 2,
    ResolvingSpin = 3,
    ShowingResult = 4,
    Completed    = 5
}

public enum JackpotSymbolType
{
    Blank       = 0,
    Bird        = 1,
    Cage        = 2,
    Eye         = 3,
    Cocktail    = 4,
    Microphone  = 5
}

public enum JackpotOutcome
{
    Refused     = 0,
    Combination = 1,
    Jackpot     = 2
}

public enum JackpotBehaviourToken
{
    None        = 0,
    Revolt      = 1,
    Obedience   = 2,
    Analysis    = 3,
    Mixed       = 4
}

[Serializable]
public struct JackpotWeightedSymbol
{
    public JackpotSymbolType symbol;
    public int weight;
}

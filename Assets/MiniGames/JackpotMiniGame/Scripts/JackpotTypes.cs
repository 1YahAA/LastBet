using System;

public enum JackpotMiniGameState
{
    NotStarted = 0,
    Intro = 1,
    Idle = 2,
    Spinning = 3,
    ResolvingSpin = 4,
    Decision = 5,
    Completed = 6
}

public enum JackpotSymbolType
{
    Blank = 0,
    Coin = 1,
    DoubleCoin = 2,
    Debt = 3,
    Hairpin = 4
}

public enum JackpotRiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum JackpotOutcome
{
    ControlledExit = 0,
    RiskyDefiance = 1,
    TrappedByDebt = 2,
    ForcedStop = 3
}

public enum JackpotLeoRelationState
{
    Friendly = 0,
    Ambiguous = 1,
    SilentObserver = 2
}

public enum JackpotBehaviourToken
{
    Revolt = 0,
    Obedience = 1,
    Analysis = 2
}

[Serializable]
public struct JackpotWeightedSymbol
{
    public JackpotSymbolType symbol;
    public int weight;
}

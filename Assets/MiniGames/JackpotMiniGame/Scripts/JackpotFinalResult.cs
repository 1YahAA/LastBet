using System;

[Serializable]
public sealed class JackpotFinalResult
{
    public string Title              { get; }
    public string Description        { get; }
    public bool Refused              { get; }
    public bool IsJackpot            { get; }
    public bool JokerCardObtained    { get; }
    public JackpotOutcome Outcome    { get; }
    public JackpotBehaviourToken Token { get; }
    public JackpotSymbolType LeftSymbol   { get; }
    public JackpotSymbolType CenterSymbol { get; }
    public JackpotSymbolType RightSymbol  { get; }
    public int RevoltDelta     { get; }
    public int ObedienceDelta  { get; }
    public int AnalysisDelta   { get; }
    public int SpinCount       { get; }

    public JackpotFinalResult(
        string title,
        string description,
        bool refused,
        bool isJackpot,
        bool jokerCardObtained,
        JackpotOutcome outcome,
        JackpotBehaviourToken token,
        JackpotSymbolType leftSymbol,
        JackpotSymbolType centerSymbol,
        JackpotSymbolType rightSymbol,
        int revoltDelta,
        int obedienceDelta,
        int analysisDelta,
        int spinCount)
    {
        Title             = title;
        Description       = description;
        Refused           = refused;
        IsJackpot         = isJackpot;
        JokerCardObtained = jokerCardObtained;
        Outcome           = outcome;
        Token             = token;
        LeftSymbol        = leftSymbol;
        CenterSymbol      = centerSymbol;
        RightSymbol       = rightSymbol;
        RevoltDelta       = revoltDelta;
        ObedienceDelta    = obedienceDelta;
        AnalysisDelta     = analysisDelta;
        SpinCount         = spinCount;
    }
}

using System;

[Serializable]
public sealed class JackpotSpinResult
{
    public JackpotSymbolType LeftSymbol { get; }
    public JackpotSymbolType CenterSymbol { get; }
    public JackpotSymbolType RightSymbol { get; }

    public bool IsJackpot { get; }

    public JackpotSpinResult(
        JackpotSymbolType leftSymbol,
        JackpotSymbolType centerSymbol,
        JackpotSymbolType rightSymbol,
        bool isJackpot)
    {
        LeftSymbol = leftSymbol;
        CenterSymbol = centerSymbol;
        RightSymbol = rightSymbol;
        IsJackpot = isJackpot;
    }

    public JackpotSymbolType[] Symbols => new[]
    {
        LeftSymbol,
        CenterSymbol,
        RightSymbol
    };
}
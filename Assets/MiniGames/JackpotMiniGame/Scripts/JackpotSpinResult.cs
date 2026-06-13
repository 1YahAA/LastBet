using System;
using System.Linq;

[Serializable]
public sealed class JackpotSpinResult
{
    public JackpotSymbolType[] Symbols { get; }
    public int RewardDelta { get; }
    public int RiskDelta { get; }
    public int DebtDelta { get; }

    public bool HasDebt => Symbols != null && Symbols.Contains(JackpotSymbolType.Debt);
    public bool HasHairpin => Symbols != null && Symbols.Contains(JackpotSymbolType.Hairpin);
    public bool HasDoubleCoin => Symbols != null && Symbols.Contains(JackpotSymbolType.DoubleCoin);
    public bool HasAnyReward => RewardDelta > 0;

    public JackpotSpinResult(JackpotSymbolType[] symbols, int rewardDelta, int riskDelta, int debtDelta)
    {
        Symbols = symbols ?? Array.Empty<JackpotSymbolType>();
        RewardDelta = rewardDelta;
        RiskDelta = riskDelta;
        DebtDelta = debtDelta;
    }
}

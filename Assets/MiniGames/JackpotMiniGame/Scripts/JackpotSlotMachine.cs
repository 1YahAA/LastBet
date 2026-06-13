using System.Collections;
using UnityEngine;

public sealed class JackpotSlotMachine : MonoBehaviour
{
    [Header("Барабаны")]
    [SerializeField] private JackpotReelController[] reels;

    [Header("Базовые веса символов")]
    [SerializeField] private JackpotWeightedSymbol[] baseWeights =
    {
        new JackpotWeightedSymbol { symbol = JackpotSymbolType.Blank, weight = 35 },
        new JackpotWeightedSymbol { symbol = JackpotSymbolType.Coin, weight = 35 },
        new JackpotWeightedSymbol { symbol = JackpotSymbolType.DoubleCoin, weight = 18 },
        new JackpotWeightedSymbol { symbol = JackpotSymbolType.Debt, weight = 8 },
        new JackpotWeightedSymbol { symbol = JackpotSymbolType.Hairpin, weight = 4 }
    };

    [Header("Эскалация")]
    [SerializeField] private int extraDebtWeightPerSpin = 3;
    [SerializeField] private int extraHairpinWeightAfterRisk = 65;
    [SerializeField] private float delayBetweenReels = 0.12f;

    public IEnumerator SpinRoutine(JackpotRiskModel riskModel, System.Action<JackpotSpinResult> onCompleted)
    {
        JackpotSymbolType[] symbols = RollSymbols(riskModel);
        JackpotSpinResult result = BuildResult(symbols, riskModel);

        if (reels != null)
        {
            for (int i = 0; i < reels.Length; i++)
            {
                if (reels[i] == null)
                    continue;

                JackpotSymbolType symbol = i < symbols.Length ? symbols[i] : JackpotSymbolType.Blank;
                yield return reels[i].SpinTo(symbol);

                if (delayBetweenReels > 0f)
                    yield return new WaitForSeconds(delayBetweenReels);
            }
        }

        onCompleted?.Invoke(result);
    }

    private JackpotSymbolType[] RollSymbols(JackpotRiskModel riskModel)
    {
        int count = reels != null && reels.Length > 0 ? reels.Length : 3;
        JackpotSymbolType[] result = new JackpotSymbolType[count];

        for (int i = 0; i < count; i++)
            result[i] = RollOne(riskModel);

        return result;
    }

    private JackpotSymbolType RollOne(JackpotRiskModel riskModel)
    {
        int total = 0;

        foreach (var weighted in baseWeights)
            total += Mathf.Max(0, GetRuntimeWeight(weighted, riskModel));

        if (total <= 0)
            return JackpotSymbolType.Blank;

        int roll = Random.Range(0, total);
        int cumulative = 0;

        foreach (var weighted in baseWeights)
        {
            cumulative += Mathf.Max(0, GetRuntimeWeight(weighted, riskModel));
            if (roll < cumulative)
                return weighted.symbol;
        }

        return JackpotSymbolType.Blank;
    }

    private int GetRuntimeWeight(JackpotWeightedSymbol weighted, JackpotRiskModel riskModel)
    {
        int weight = weighted.weight;

        if (riskModel == null)
            return weight;

        if (weighted.symbol == JackpotSymbolType.Debt)
            weight += riskModel.SpinCount * extraDebtWeightPerSpin;

        if (weighted.symbol == JackpotSymbolType.Hairpin && riskModel.RiskScore >= extraHairpinWeightAfterRisk)
            weight += 8;

        return weight;
    }

    private JackpotSpinResult BuildResult(JackpotSymbolType[] symbols, JackpotRiskModel riskModel)
    {
        int reward = 0;
        int debt = 0;
        int risk = 12;

        foreach (var symbol in symbols)
        {
            switch (symbol)
            {
                case JackpotSymbolType.Coin:
                    reward += 1;
                    risk += 3;
                    break;
                case JackpotSymbolType.DoubleCoin:
                    reward += 2;
                    risk += 6;
                    break;
                case JackpotSymbolType.Debt:
                    debt += 1;
                    reward -= 1;
                    risk += 22;
                    break;
                case JackpotSymbolType.Hairpin:
                    risk += 16;
                    break;
            }
        }

        if (riskModel != null && (int)riskModel.CurrentRiskLevel >= (int)JackpotRiskLevel.High)
            risk += 8;

        return new JackpotSpinResult(symbols, reward, risk, debt);
    }
}

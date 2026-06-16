using System.Collections;
using UnityEngine;

public sealed class JackpotSlotMachine : MonoBehaviour
{
    [Header("Барабаны")]
    [SerializeField] private JackpotReelController leftReel;
    [SerializeField] private JackpotReelController centerReel;
    [SerializeField] private JackpotReelController rightReel;

    [Header("Вес символов")]
    [SerializeField] private JackpotWeightedSymbol[] weightedSymbols =
    {
        new JackpotWeightedSymbol { symbol = JackpotSymbolType.Bird, weight = 20 },
        new JackpotWeightedSymbol { symbol = JackpotSymbolType.Cage, weight = 20 },
        new JackpotWeightedSymbol { symbol = JackpotSymbolType.Eye, weight = 20 },
        new JackpotWeightedSymbol { symbol = JackpotSymbolType.Cocktail, weight = 20 },
        new JackpotWeightedSymbol { symbol = JackpotSymbolType.Microphone, weight = 20 }
    };

    [Header("Джекпот")]
    [Range(0f, 1f)]
    [SerializeField] private float firstSpinJackpotChance = 0.2f;

    [Range(0f, 1f)]
    [SerializeField] private float secondSpinJackpotChance = 0.35f;

    [SerializeField] private bool treatAnyTripleAsJackpot = true;

    [Header("Длительность барабанов")]
    [SerializeField] private float leftSpinDuration = 1.1f;
    [SerializeField] private float centerSpinDuration = 1.35f;
    [SerializeField] private float rightSpinDuration = 1.6f;

    public IEnumerator SpinRoutine(int spinIndex, bool mustRollJackpot, System.Action<JackpotSpinResult> onComplete)
    {
        bool isJackpot = mustRollJackpot || ShouldTriggerEarlyJackpot(spinIndex);

        JackpotSymbolType left;
        JackpotSymbolType center;
        JackpotSymbolType right;

        if (isJackpot)
        {
            JackpotSymbolType jackpotSymbol = RollSymbol();
            left = jackpotSymbol;
            center = jackpotSymbol;
            right = jackpotSymbol;
        }
        else
        {
            left = RollSymbol();
            center = RollSymbol();
            right = RollSymbol();

            if (treatAnyTripleAsJackpot && IsTriple(left, center, right))
                isJackpot = true;
        }

        bool leftDone = leftReel == null;
        bool centerDone = centerReel == null;
        bool rightDone = rightReel == null;

        if (leftReel != null)
            StartCoroutine(SpinReel(leftReel, left, leftSpinDuration, () => leftDone = true));

        if (centerReel != null)
            StartCoroutine(SpinReel(centerReel, center, centerSpinDuration, () => centerDone = true));

        if (rightReel != null)
            StartCoroutine(SpinReel(rightReel, right, rightSpinDuration, () => rightDone = true));

        while (!leftDone || !centerDone || !rightDone)
            yield return null;

        onComplete?.Invoke(new JackpotSpinResult(left, center, right, isJackpot));
    }

    private IEnumerator SpinReel(
        JackpotReelController reel,
        JackpotSymbolType result,
        float duration,
        System.Action onDone)
    {
        yield return reel.SpinTo(result, duration);
        reel.SetSymbolInstant(result);
        onDone?.Invoke();
    }

    private bool ShouldTriggerEarlyJackpot(int spinIndex)
    {
        if (spinIndex <= 1)
            return Random.value <= firstSpinJackpotChance;

        if (spinIndex == 2)
            return Random.value <= secondSpinJackpotChance;

        return false;
    }

    private JackpotSymbolType RollSymbol()
    {
        if (weightedSymbols == null || weightedSymbols.Length == 0)
            return JackpotSymbolType.Bird;

        int totalWeight = 0;

        foreach (var item in weightedSymbols)
        {
            if (item.weight > 0)
                totalWeight += item.weight;
        }

        if (totalWeight <= 0)
            return JackpotSymbolType.Bird;

        int roll = Random.Range(1, totalWeight + 1);
        int current = 0;

        foreach (var item in weightedSymbols)
        {
            if (item.weight <= 0)
                continue;

            current += item.weight;

            if (roll <= current)
                return item.symbol;
        }

        return JackpotSymbolType.Bird;
    }

    private static bool IsTriple(JackpotSymbolType left, JackpotSymbolType center, JackpotSymbolType right)
    {
        return left == center && center == right;
    }
}

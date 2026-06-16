using UnityEngine;

public sealed class JackpotResultResolver : MonoBehaviour
{
    public JackpotFinalResult ResolveRefusal()
    {
        return new JackpotFinalResult(
            title: "Отказ",
            description: "Эвелин проходит мимо автомата. Не каждая игра заслуживает участия.",
            refused: true,
            isJackpot: false,
            jokerCardObtained: false,
            outcome: JackpotOutcome.Refused,
            token: JackpotBehaviourToken.Revolt,
            leftSymbol: JackpotSymbolType.Blank,
            centerSymbol: JackpotSymbolType.Blank,
            rightSymbol: JackpotSymbolType.Blank,
            revoltDelta: 1,
            obedienceDelta: 0,
            analysisDelta: 0,
            spinCount: 0
        );
    }

    public JackpotFinalResult ResolveSpin(JackpotSpinResult spinResult, int spinCount)
    {
        if (spinResult == null)
        {
            return new JackpotFinalResult(
                "Автомат молчит",
                "Старый механизм не отвечает.",
                false,
                false,
                false,
                JackpotOutcome.Combination,
                JackpotBehaviourToken.None,
                JackpotSymbolType.Blank,
                JackpotSymbolType.Blank,
                JackpotSymbolType.Blank,
                0,
                0,
                0,
                spinCount
            );
        }

        int revolt = 0;
        int obedience = 0;
        int analysis = 0;

        string description = ResolveCombinationText(
            spinResult.LeftSymbol,
            spinResult.CenterSymbol,
            spinResult.RightSymbol,
            out revolt,
            out obedience,
            out analysis
        );

        JackpotBehaviourToken token = ResolveMainToken(revolt, obedience, analysis);

        if (spinResult.IsJackpot)
        {
            return new JackpotFinalResult(
                title: "Джекпот",
                description: "Автомат выдаёт карту Джокера. Эвелин убирает её в карман.",
                refused: false,
                isJackpot: true,
                jokerCardObtained: true,
                outcome: JackpotOutcome.Jackpot,
                token: token,
                leftSymbol: spinResult.LeftSymbol,
                centerSymbol: spinResult.CenterSymbol,
                rightSymbol: spinResult.RightSymbol,
                revoltDelta: revolt,
                obedienceDelta: obedience,
                analysisDelta: analysis,
                spinCount: spinCount
            );
        }

        return new JackpotFinalResult(
            title: "Комбинация",
            description: description,
            refused: false,
            isJackpot: false,
            jokerCardObtained: false,
            outcome: JackpotOutcome.Combination,
            token: token,
            leftSymbol: spinResult.LeftSymbol,
            centerSymbol: spinResult.CenterSymbol,
            rightSymbol: spinResult.RightSymbol,
            revoltDelta: revolt,
            obedienceDelta: obedience,
            analysisDelta: analysis,
            spinCount: spinCount
        );
    }

    private string ResolveCombinationText(
        JackpotSymbolType left,
        JackpotSymbolType center,
        JackpotSymbolType right,
        out int revolt,
        out int obedience,
        out int analysis)
    {
        revolt = 0;
        obedience = 0;
        analysis = 0;

        int bird = Count(left, center, right, JackpotSymbolType.Bird);
        int cage = Count(left, center, right, JackpotSymbolType.Cage);
        int eye = Count(left, center, right, JackpotSymbolType.Eye);
        int cocktail = Count(left, center, right, JackpotSymbolType.Cocktail);
        int micro = Count(left, center, right, JackpotSymbolType.Microphone);

        if (bird == 3)
        {
            revolt = 2;
            return "Клетка открыта. Осталось только выйти.";
        }

        if (cage == 3)
        {
            obedience = 2;
            return "Иногда безопаснее остаться внутри.";
        }

        if (eye == 3)
        {
            analysis = 2;
            return "Впервые всё начинает складываться в систему.";
        }

        if (cocktail == 3)
        {
            obedience = 2;
            return "Некоторые решения принимаются раньше первого глотка.";
        }

        if (micro == 3)
        {
            analysis = 2;
            return "Кажется, кто-то давно написал этот номер за неё.";
        }

        if (Has(left, center, right, JackpotSymbolType.Bird)
            && Has(left, center, right, JackpotSymbolType.Eye)
            && Has(left, center, right, JackpotSymbolType.Microphone))
        {
            revolt = 1;
            analysis = 1;
            return "Чтобы изменить правила, нужно сначала увидеть сцену целиком.";
        }

        if (Has(left, center, right, JackpotSymbolType.Cage)
            && Has(left, center, right, JackpotSymbolType.Eye)
            && Has(left, center, right, JackpotSymbolType.Microphone))
        {
            obedience = 1;
            analysis = 1;
            return "Понимание не всегда даёт силы возразить.";
        }

        if (Has(left, center, right, JackpotSymbolType.Cocktail)
            && Has(left, center, right, JackpotSymbolType.Eye))
        {
            analysis = 1;
            return "Сладкий след снова ведёт туда, где кто-то решил за неё.";
        }

        if (Has(left, center, right, JackpotSymbolType.Bird)
            && Has(left, center, right, JackpotSymbolType.Cage))
        {
            analysis = 1;
            return "Свобода и безопасность требуют разной цены.";
        }

        if (bird >= 2)
        {
            revolt = 1;
            return "Страх ещё рядом, но уже не управляет выбором.";
        }

        if (cage >= 2)
        {
            obedience = 1;
            return "Привычная клетка кажется безопаснее неизвестности.";
        }

        if (eye >= 2 || micro >= 2)
        {
            analysis = 1;
            return "За красивыми декорациями проступает механизм.";
        }

        if (cocktail >= 2)
        {
            obedience = 1;
            return "Мысли становятся мягче, когда кто-то заранее смешал напиток.";
        }

        return "Автомат молчит. Только старый механизм продолжает дышать.";
    }

    private int Count(JackpotSymbolType left, JackpotSymbolType center, JackpotSymbolType right, JackpotSymbolType symbol)
    {
        int count = 0;

        if (left == symbol) count++;
        if (center == symbol) count++;
        if (right == symbol) count++;

        return count;
    }

    private bool Has(JackpotSymbolType left, JackpotSymbolType center, JackpotSymbolType right, JackpotSymbolType symbol)
    {
        return left == symbol || center == symbol || right == symbol;
    }

    private JackpotBehaviourToken ResolveMainToken(int revolt, int obedience, int analysis)
    {
        int active = 0;

        if (revolt > 0) active++;
        if (obedience > 0) active++;
        if (analysis > 0) active++;

        if (active > 1)
            return JackpotBehaviourToken.Mixed;

        if (revolt > 0)
            return JackpotBehaviourToken.Revolt;

        if (obedience > 0)
            return JackpotBehaviourToken.Obedience;

        if (analysis > 0)
            return JackpotBehaviourToken.Analysis;

        return JackpotBehaviourToken.None;
    }
}
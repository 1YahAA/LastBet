using System.Collections.Generic;

// Формирует последствия выбора
public static class LastBetResultResolver
{
    public static LastBetStrategyToken ResolveToken(LastBetSuspect suspect, int information, int suspicion, int suspicionLimit)
    {
        if (suspect == LastBetSuspect.Helga)
            return LastBetStrategyToken.Revolt;

        if (suspect == LastBetSuspect.Victor)
            return LastBetStrategyToken.Obedience;

        if (suspect == LastBetSuspect.Marie)
            return LastBetStrategyToken.Analysis;

        return information >= 3 && suspicion < suspicionLimit
            ? LastBetStrategyToken.Analysis
            : LastBetStrategyToken.Obedience;
    }

    public static bool IsInformationUseful(int information, int minInformationToChoose, int suspicion, int suspicionLimit)
    {
        return information >= minInformationToChoose && suspicion < suspicionLimit;
    }

    public static string BuildTitle(bool useful)
    {
        return useful ? "ВЕРСИЯ ЗАФИКСИРОВАНА" : "ВЕРСИЯ ОСТАЛАСЬ ШУМНОЙ";
    }

    public static string BuildBody(LastBetSuspect suspect, LastBetStrategyToken token, bool useful, IReadOnlyList<LastBetStoryClue> clues)
    {
        string baseText = suspect switch
        {
            LastBetSuspect.Helga => "Эвелин допускает, что Хэльга не враг, а человек, который пытается предупредить её обходным путём.",
            LastBetSuspect.Victor => "Эвелин видит в Викторе самый заметный центр давления. Эта версия безопасна для толпы, но слишком удобна.",
            LastBetSuspect.Marie => "Эвелин обращает внимание на тихие служебные следы. Такая версия выглядит слабее внешне, но лучше объясняет мелкие совпадения.",
            _ => "Эвелин не успела собрать устойчивую версию. Останется только ощущение чужого вмешательства."
        };

        string quality = useful
            ? "Сведения сохранены и смогут повлиять на финальное понимание дела."
            : "Сведений слишком мало или подозрение стало слишком высоким. Часть наблюдений сохранится, но доверять им полностью опасно.";

        return $"{baseText}\n\n{quality}";
    }
}

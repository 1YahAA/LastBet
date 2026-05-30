using System.Collections.Generic;

public static class CardGameScoring
{
    public static RoundScoreResult CalculateRoundScore(CardData[] cards, RuntimeCustomer customer)
    {
        if (cards == null || cards.Length == 0)
            return RoundScoreResult.Failed("Карты не выбраны");

        if (customer == null)
            return RoundScoreResult.Failed("Клиент не найден");

        bool usedRiskCard = HasRiskCard(cards);
        bool usedDamagedCard = HasRawType(cards, CocktailType.Damaged);

        for (int i = 0; i < cards.Length; i++)
        {
            CardData card = cards[i];
            if (card == null) continue;

            if (card.effectType == CardEffectType.LoseIfDamagedNearby &&
                HasAdjacentType(cards, i, CocktailType.Damaged))
                return RoundScoreResult.Fatal("Токсичный микс уничтожил напиток", usedRiskCard, usedDamagedCard);
        }

        if (customer.RuleType == CustomerRuleType.NoDamagedCards && usedDamagedCard)
            return RoundScoreResult.Failed("Клиент отказался от испорченного ингредиента", usedRiskCard, usedDamagedCard);

        if (customer.RequiredType != CocktailType.None && !HasEffectiveType(cards, customer.RequiredType))
            return RoundScoreResult.Failed($"Нет нужного ингредиента: {TypeName(customer.RequiredType)}", usedRiskCard, usedDamagedCard);

        int customerRuleBonus = 0;

        if (customer.RuleType == CustomerRuleType.WantsRainbow && IsRainbow(cards))
            customerRuleBonus += 2;

        if (customer.RuleType == CustomerRuleType.WantsTriplet && IsTriplet(cards))
            customerRuleBonus += 2;

        int baseScore = BaseScore(cards);
        int adjacencyScore = customer.RuleType == CustomerRuleType.NoAdjacencyBonus ? 0 : AdjacencyBonus(cards);
        int recipeScore = RecipeBonus(cards, customer.PreferredType);
        int preferredScore = customer.PreferredType != CocktailType.None && HasEffectiveType(cards, customer.PreferredType)
            ? customer.BonusForPreferred
            : 0;

        int bonusScore = adjacencyScore + recipeScore + preferredScore + customerRuleBonus;
        return RoundScoreResult.Success(baseScore + bonusScore, baseScore, bonusScore, usedRiskCard, usedDamagedCard);
    }

    private static int BaseScore(CardData[] cards)
    {
        int score = 0;

        for (int i = 0; i < cards.Length; i++)
        {
            CardData card = cards[i];
            if (card == null) continue;

            int value = card.points;

            if (card.effectType == CardEffectType.MoldCenterPenalty && i == 1)
                value += card.effectAmount;

            if (card.effectType == CardEffectType.ZeroIfNoTargetNearby && !HasAdjacentType(cards, i, card.effectTarget))
                value = 0;

            if (card.effectType == CardEffectType.PenaltyIfTargetNearby && HasAdjacentType(cards, i, card.effectTarget))
                value += card.effectAmount;

            score += value;
        }

        for (int i = 0; i < cards.Length; i++)
        {
            CardData card = cards[i];
            if (card == null || card.effectType != CardEffectType.AddToNeighbor) continue;

            if (i > 0) score += card.effectAmount;
            if (i < cards.Length - 1) score += card.effectAmount;
        }

        return score;
    }

    public static int AdjacencyBonus(CardData[] cards)
    {
        int bonus = 0;

        for (int i = 0; i < cards.Length; i++)
        {
            CardData card = cards[i];
            if (card == null || card.adjacencyBonus == 0) continue;
            if (IsAdjacencyBrokenByNeighbor(cards, i)) continue;

            bool leftOk = card.requiredLeft == CocktailType.None || (i > 0 && MatchesAdjacency(cards, i - 1, card.requiredLeft));
            bool rightOk = card.requiredRight == CocktailType.None || (i < cards.Length - 1 && MatchesAdjacency(cards, i + 1, card.requiredRight));

            if (!leftOk || !rightOk) continue;

            int cardBonus = card.adjacencyBonus;

            if (i > 0 && cards[i - 1] != null && cards[i - 1].effectType == CardEffectType.DoubleRightAdjacency)
                cardBonus *= 2;

            bonus += cardBonus;
        }

        return bonus;
    }

    public static int RecipeBonus(CardData[] cards, CocktailType preferredType)
    {
        foreach (CardData card in cards)
        {
            if (card != null && card.effectType == CardEffectType.CancelRecipeBonus)
                return 0;
        }

        List<CocktailType> types = GetRecipeTypes(cards);
        if (types.Count == 0) return 0;

        if (IsTriplet(cards))
        {
            CocktailType type = types[0];
            return type == preferredType ? 4 : 3;
        }

        for (int i = 0; i < cards.Length - 1; i++)
        {
            CocktailType a = EffectiveType(cards, i);
            CocktailType b = EffectiveType(cards, i + 1);
            if (IsRecipeType(a) && a == b)
                return a == preferredType ? 2 : 1;
        }

        if (IsRainbow(cards))
            return types.Contains(preferredType) ? 3 : 2;

        return 0;
    }

    private static bool HasRiskCard(CardData[] cards)
    {
        foreach (CardData card in cards)
        {
            if (card == null) continue;
            if (card.effectType == CardEffectType.ZeroIfNoTargetNearby ||
                card.effectType == CardEffectType.PenaltyIfTargetNearby ||
                card.effectType == CardEffectType.LoseIfDamagedNearby)
                return true;
        }
        return false;
    }

    private static bool HasRawType(CardData[] cards, CocktailType type)
    {
        foreach (CardData card in cards)
            if (card != null && card.cocktailType == type) return true;
        return false;
    }

    private static bool HasEffectiveType(CardData[] cards, CocktailType type)
    {
        for (int i = 0; i < cards.Length; i++)
            if (EffectiveType(cards, i) == type) return true;
        return false;
    }

    private static bool HasAdjacentType(CardData[] cards, int index, CocktailType type)
    {
        if (index > 0 && EffectiveType(cards, index - 1) == type) return true;
        if (index < cards.Length - 1 && EffectiveType(cards, index + 1) == type) return true;
        return false;
    }

    private static bool MatchesAdjacency(CardData[] cards, int index, CocktailType required)
    {
        CardData card = cards[index];
        if (card == null) return false;

        if (card.effectType == CardEffectType.AnyTypeForAdjacency)
            return required == CocktailType.Bitter || required == CocktailType.Lemonchello || required == CocktailType.Absinthe;

        return EffectiveType(cards, index) == required;
    }

    private static bool IsAdjacencyBrokenByNeighbor(CardData[] cards, int index)
    {
        if (index > 0 && cards[index - 1] != null && cards[index - 1].effectType == CardEffectType.BreakAdjacentBonuses) return true;
        if (index < cards.Length - 1 && cards[index + 1] != null && cards[index + 1].effectType == CardEffectType.BreakAdjacentBonuses) return true;
        return false;
    }

    private static CocktailType EffectiveType(CardData[] cards, int index)
    {
        CardData card = cards[index];
        if (card == null) return CocktailType.None;

        if (card.effectType == CardEffectType.CopyNeighborType)
        {
            if (index > 0 && IsRecipeType(cards[index - 1].cocktailType)) return cards[index - 1].cocktailType;
            if (index < cards.Length - 1 && IsRecipeType(cards[index + 1].cocktailType)) return cards[index + 1].cocktailType;
            return CocktailType.None;
        }

        return card.cocktailType;
    }

    private static List<CocktailType> GetRecipeTypes(CardData[] cards)
    {
        var result = new List<CocktailType>();
        for (int i = 0; i < cards.Length; i++)
        {
            CardData card = cards[i];
            if (card == null || card.effectType == CardEffectType.ExcludeFromRainbow) continue;
            CocktailType type = EffectiveType(cards, i);
            if (IsRecipeType(type)) result.Add(type);
        }
        return result;
    }

    private static bool IsTriplet(CardData[] cards)
    {
        List<CocktailType> types = GetRecipeTypes(cards);
        return types.Count == 3 && types[0] == types[1] && types[1] == types[2];
    }

    private static bool IsRainbow(CardData[] cards)
    {
        List<CocktailType> types = GetRecipeTypes(cards);
        return types.Count == 3 && types[0] != types[1] && types[0] != types[2] && types[1] != types[2];
    }

    private static bool IsRecipeType(CocktailType type)
    {
        return type == CocktailType.Bitter || type == CocktailType.Lemonchello || type == CocktailType.Absinthe;
    }

    private static string TypeName(CocktailType type)
    {
        return type switch
        {
            CocktailType.Bitter => "Биттер",
            CocktailType.Lemonchello => "Лимончелло",
            CocktailType.Absinthe => "Абсент",
            CocktailType.Damaged => "Испорченная",
            _ => "любой"
        };
    }
}

using NUnit.Framework;
using UnityEngine;

public class CardGameTests
{
    private static CardData Card(
        CocktailType type,
        int points,
        CocktailType requiredLeft = CocktailType.None,
        CocktailType requiredRight = CocktailType.None,
        int adjacencyBonus = 0,
        CardEffectType effectType = CardEffectType.None,
        CocktailType effectTarget = CocktailType.None,
        int effectAmount = 0)
    {
        CardData card = ScriptableObject.CreateInstance<CardData>();
        
        card.cocktailType = type;
        card.points = points;
        
        card.requiredLeft = requiredLeft;
        card.requiredRight = requiredRight;
        card.adjacencyBonus = adjacencyBonus;
        
        card.effectType = effectType;
        card.effectTarget = effectTarget;
        card.effectAmount = effectAmount;
        
        return card;
    }

    private static RuntimeCustomer Customer(
        CocktailType required = CocktailType.None,
        CocktailType preferred = CocktailType.None,
        CustomerRuleType rule = CustomerRuleType.None)
    {
        return new RuntimeCustomer
        {
            RequiredType = required,
            PreferredType = preferred,
            RuleType = rule,
            BonusForPreferred = 1
        };
    }

    [Test]
    public void RequiredTypeMissing_FailsRound()
    {
        CardData[] cards =
        {
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Absinthe, 2),
            Card(CocktailType.Bitter, 1)
        };

        RoundScoreResult result = CardGameScoring.CalculateRoundScore(cards, Customer(required: CocktailType.Lemonchello));

        Assert.IsTrue(result.IsFailed);
        Assert.IsFalse(result.IsFatal);
        Assert.AreEqual(0, result.Score);
    }

    [Test]
    public void RequiredTypePresent_ReturnsScore()
    {
        CardData[] cards =
        {
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Absinthe, 1)
        };

        RoundScoreResult result = CardGameScoring.CalculateRoundScore(cards, Customer(required: CocktailType.Absinthe, preferred: CocktailType.Bitter));

        Assert.IsFalse(result.IsFailed);
        Assert.AreEqual(8, result.Score);
    }

    [Test]
    public void AdjacencyBonus_Works()
    {
        CardData[] cards =
        {
            Card(CocktailType.Lemonchello, 1),
            Card(CocktailType.Bitter, 2, requiredLeft: CocktailType.Lemonchello, adjacencyBonus: 1),
            Card(CocktailType.Absinthe, 1)
        };

        Assert.AreEqual(1, CardGameScoring.AdjacencyBonus(cards));
    }

    [Test]
    public void ToxicMixNearDamaged_IsFatal()
    {
        CardData[] cards =
        {
            Card(CocktailType.Damaged, 6, effectType: CardEffectType.LoseIfDamagedNearby),
            Card(CocktailType.Damaged, -1),
            Card(CocktailType.Bitter, 2)
        };

        RoundScoreResult result = CardGameScoring.CalculateRoundScore(cards, Customer(required: CocktailType.Bitter));

        Assert.IsTrue(result.IsFatal);
    }
}

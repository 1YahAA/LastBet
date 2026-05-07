using NUnit.Framework;
using UnityEngine;

public class CardGameTests
{
    static CardData Card(CardColor color, int value,
        CardColor leftAdj  = CardColor.None,
        CardColor rightAdj = CardColor.None,
        int adjBonus       = 0)
    {
        var d = ScriptableObject.CreateInstance<CardData>();
        d.color              = color;
        d.value              = value;
        d.leftAdjacencyColor  = leftAdj;
        d.rightAdjacencyColor = rightAdj;
        d.adjacencyBonus      = adjBonus;
        return d;
    }

    // ТЕСТЫ БОНУСОВ РЕЦЕПТА
    [Test]
    public void Triplet_SameColor_GivesBonus4()
    {
        var cards = new[] {
            Card(CardColor.Red, 1),
            Card(CardColor.Red, 2),
            Card(CardColor.Red, 1)
        };
        // Все 3 красных → Triplet → бонус +4
        int bonus = CardGameScoring.RecipeBonus(cards, CardColor.Blue, CardColor.Yellow);
        Assert.AreEqual(4, bonus, "Triplet должен давать +4");
    }

    [Test]
    public void Triplet_PreferredColor_GivesBonus8()
    {
        var cards = new[] {
            Card(CardColor.Yellow, 2),
            Card(CardColor.Yellow, 1),
            Card(CardColor.Yellow, 2)
        };
        // Triplet + preferred Yellow → 4 × 2 = 8
        int bonus = CardGameScoring.RecipeBonus(cards, CardColor.Blue, CardColor.Yellow);
        Assert.AreEqual(8, bonus, "Triplet preferred должен давать +8");
    }

    [Test]
    public void Twin_Adjacent_GivesBonus2()
    {
        var cards = new[] {
            Card(CardColor.Red, 2),
            Card(CardColor.Red, 1),
            Card(CardColor.Blue, 2)
        };
        // Слоты 0 и 1 — оба Red, рядом → Twin → +2
        int bonus = CardGameScoring.RecipeBonus(cards, CardColor.Blue, CardColor.Yellow);
        Assert.AreEqual(2, bonus, "Twin рядом должен давать +2");
    }

    [Test]
    public void Twin_NotAdjacent_GivesNoBonus()
    {
        // Red на 0 и 2, между ними Blue → НЕ рядом → не Twin
        var cards = new[] {
            Card(CardColor.Red,  2),
            Card(CardColor.Blue, 1),
            Card(CardColor.Red,  1)
        };
        int bonus = CardGameScoring.RecipeBonus(cards, CardColor.Blue, CardColor.Yellow);
        // Не Triplet, не Twin рядом, не Rainbow (2 уникальных цвета) → 0
        Assert.AreEqual(0, bonus, "Red не рядом — бонуса нет");
    }

    [Test]
    public void Twin_Preferred_GivesBonus4()
    {
        var cards = new[] {
            Card(CardColor.Blue, 2),
            Card(CardColor.Blue, 2),
            Card(CardColor.Red,  1)
        };
        // Twin Blue рядом, Blue — preferred → 2 × 2 = 4
        int bonus = CardGameScoring.RecipeBonus(cards, CardColor.Red, CardColor.Blue);
        Assert.AreEqual(4, bonus, "Twin preferred должен давать +4");
    }

    [Test]
    public void Rainbow_AllDifferent_GivesBonus3()
    {
        var cards = new[] {
            Card(CardColor.Red,    1),
            Card(CardColor.Yellow, 1),
            Card(CardColor.Blue,   1)
        };
        // Все 3 разные → Rainbow → +3 (preferred Blue не в cards → нет доп.)
        int bonus = CardGameScoring.RecipeBonus(cards, CardColor.Red, CardColor.Blue);
        // Blue есть → +1 дополнительно
        Assert.AreEqual(4, bonus, "Rainbow + preferred среди цветов → +4");
    }

    [Test]
    public void Rainbow_PreferredNotPresent_GivesBonus3()
    {
        var cards = new[] {
            Card(CardColor.Red,    1),
            Card(CardColor.Yellow, 1),
            Card(CardColor.Blue,   1)
        };
        // preferred = Black, которого нет → только базовый +3
        int bonus = CardGameScoring.RecipeBonus(cards, CardColor.Red, CardColor.Black);
        Assert.AreEqual(3, bonus, "Rainbow без preferred → +3");
    }

    // ТЕСТЫ БОНУСА СОСЕДСТВА
    [Test]
    public void Adjacency_LeftConditionMet_GivesBonus()
    {
        // cards[1] требует Yellow слева → cards[0] Yellow → бонус +1
        var cards = new[] {
            Card(CardColor.Yellow, 2),
            Card(CardColor.Red,    1, leftAdj: CardColor.Yellow, adjBonus: 1),
            Card(CardColor.Blue,   2)
        };
        int adj = CardGameScoring.AdjacencyBonus(cards);
        Assert.AreEqual(1, adj, "Условие слева выполнено → +1");
    }

    [Test]
    public void Adjacency_LeftConditionNotMet_NoBonus()
    {
        // cards[1] требует Yellow слева, но слева Blue → бонус 0
        var cards = new[] {
            Card(CardColor.Blue,   2),
            Card(CardColor.Red,    1, leftAdj: CardColor.Yellow, adjBonus: 1),
            Card(CardColor.Yellow, 2)
        };
        int adj = CardGameScoring.AdjacencyBonus(cards);
        Assert.AreEqual(0, adj, "Условие слева не выполнено → 0");
    }

    // ТЕСТЫ ПОЛНОГО РАУНДА (CalculateRoundScore)
    [Test]
    public void RoundScore_NoRequiredColor_ReturnsMinusOne()
    {
        var cards = new[] {
            Card(CardColor.Red,  2),
            Card(CardColor.Blue, 1),
            Card(CardColor.Red,  1)
        };
        // required = Yellow, которого нет → -1 (поражение)
        int score = CardGameScoring.CalculateRoundScore(cards, CardColor.Yellow, CardColor.Blue);
        Assert.AreEqual(-1, score, "Нет required цвета → -1");
    }

    [Test]
    public void RoundScore_TwinWithPreferred_CorrectTotal()
    {
        // 2+2 Red рядом, required=Red, preferred=Red
        // Базовые: 2+2+1=5. Adj: 0. Twin preferred: +4. Итого: 9
        var cards = new[] {
            Card(CardColor.Red,  2),
            Card(CardColor.Red,  2),
            Card(CardColor.Blue, 1)
        };
        int score = CardGameScoring.CalculateRoundScore(cards, CardColor.Red, CardColor.Red);
        Assert.AreEqual(9, score);
    }
}

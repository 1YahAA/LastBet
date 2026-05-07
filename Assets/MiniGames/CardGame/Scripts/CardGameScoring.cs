using System.Collections.Generic;

public static class CardGameScoring
{
    // Считает итоговые очки за один ряд из 3 карт
    public static int CalculateRoundScore(
        CardData[] cards,
        CardColor required,
        CardColor preferred)
    {
        // Проверка: есть ли required в ряду
        bool hasRequired = false;
        foreach (var c in cards)
            if (c.color == required) { hasRequired = true; break; }
        if (!hasRequired) return -1;

        int score = 0;

        // 1. Базовые значения карт
        foreach (var c in cards) score += c.value;

        // 2. Бонус соседства
        score += AdjacencyBonus(cards);

        // 3. Бонус рецепта
        score += RecipeBonus(cards, required, preferred);

        return score;
    }

    // Бонус соседства: карта проверяет своих соседей слева и справа.
    // Если условие выполнено с обеих нужных сторон — бонус начисляется
    public static int AdjacencyBonus(CardData[] cards)
    {
        int bonus = 0;
        for (int i = 0; i < cards.Length; i++)
        {
            var card = cards[i];
            if (card.adjacencyBonus == 0) continue;

            // Если leftAdjacencyColor = None → условие по левому соседу не требуется
            bool leftOk = card.leftAdjacencyColor == CardColor.None
                       || (i > 0 && cards[i - 1].color == card.leftAdjacencyColor);

            // Если rightAdjacencyColor = None → условие по правому соседу не требуется
            bool rightOk = card.rightAdjacencyColor == CardColor.None
                        || (i < cards.Length - 1 && cards[i + 1].color == card.rightAdjacencyColor);

            if (leftOk && rightOk)
                bonus += card.adjacencyBonus;
        }
        return bonus;
    }

    // Бонус рецепта
    // Правила для 3 карт:
    //   Triplet  = все 3 одного цвета → +4 (×2 если preferred)
    //   Twin = 2 одного цвета РЯДОМ → +2 (×2 если preferred)
    //   Rainbow  = все 3 цвета разные → +3 (+1 за каждый preferred среди них)
    public static int RecipeBonus(CardData[] cards, CardColor required, CardColor preferred)
    {
        // Считаем, сколько раз встречается каждый цвет
        var counts = new Dictionary<CardColor, int>();
        foreach (var c in cards)
        {
            if (c.color == CardColor.None) continue;
            if (!counts.ContainsKey(c.color)) counts[c.color] = 0;
            counts[c.color]++;
        }

        // Triplet (3 одного цвета)
        foreach (var kv in counts)
        {
            if (kv.Value == 3)
            {
                int bonus = 4;
                if (kv.Key == preferred) bonus *= 2; // preferred → удвоить
                return bonus;
            }
        }

        // Twin (2 одного цвета, обязательно РЯДОМ) 
        // При 3 картах возможен только один Twin: [0,1] или [1,2]
        for (int i = 0; i < 2; i++)
        {
            if (cards[i].color == cards[i + 1].color && cards[i].color != CardColor.None)
            {
                int bonus = 2;
                if (cards[i].color == preferred) bonus *= 2;
                return bonus;
            }
        }

        // Rainbow (все разные)
        if (counts.Count == 3)
        {
            int bonus = 3;
            if (counts.ContainsKey(preferred)) bonus += 1; // любой preferred среди цветов
            return bonus;
        }

        return 0;
    }
}

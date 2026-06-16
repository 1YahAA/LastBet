using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LastBetDeckService
{
    public static List<LastBetCardData> BuildDeck(IEnumerable<LastBetCardData> templates)
    {
        List<LastBetCardData> deck = new List<LastBetCardData>();

        if (templates != null)
            deck.AddRange(templates.Where(card => card != null));

        if (deck.Count == 0)
            deck.AddRange(CreateFallbackDeck());

        foreach (LastBetCardData card in deck)
            card?.NormalizeValuesFromSymbol();

        Shuffle(deck);
        EnsureJokerNotFirst(deck);
        return deck;
    }

    private static void EnsureJokerNotFirst(List<LastBetCardData> deck)
    {
        if (deck == null || deck.Count < 2)
            return;

        if (deck[0] == null || !deck[0].IsJoker)
            return;

        int swapIndex = Random.Range(1, deck.Count);
        LastBetCardData temp = deck[0];
        deck[0] = deck[swapIndex];
        deck[swapIndex] = temp;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        if (list == null)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private static IEnumerable<LastBetCardData> CreateFallbackDeck()
    {
        return new[]
        {
            Create(LastBetSymbolType.Bird, "ПТИЦА", "Свобода ближе, чем кажется.",
                "Птица всегда мечтает о небе. Пока не вспоминает, кто её кормит."),

            Create(LastBetSymbolType.Bird, "ПТИЦА", "Желание выйти из клетки.",
                "Птица всегда мечтает о небе. Пока не вспоминает, кто её кормит."),

            Create(LastBetSymbolType.Cage, "КЛЕТКА", "Безопасность, похожая на плен.",
                "Дом узнаёт своих. Даже если они делают вид, что хотят уйти."),

            Create(LastBetSymbolType.Cocktail, "КОКТЕЙЛЬ", "Мягкий голос чужого контроля.",
                "Дом узнаёт своих. Даже если они делают вид, что хотят уйти."),

            Create(LastBetSymbolType.Eye, "ГЛАЗ", "Внимание к тому, что скрыто.",
                "Осторожнее, дорогая. Не все истины стоит произносить при публике."),

            Create(LastBetSymbolType.Microphone, "МИКРОФОН", "Правда может быть сказана вслух.",
                "Осторожнее, дорогая. Не все истины стоит произносить при публике."),

            Create(LastBetSymbolType.Joker, "ДЖОКЕР", "Сбой. Давление. Чужая рука в раскладе.",
                "Вот видишь? Даже судьба противится твоей дерзости.")
        };
    }

    private static LastBetCardData Create(LastBetSymbolType symbol, string title, string description, string victorLine)
    {
        LastBetCardData data = new LastBetCardData
        {
            symbolType = symbol,
            title = title,
            cardDescription = description,
            victorLine = victorLine
        };

        data.NormalizeValuesFromSymbol();
        return data;
    }
}

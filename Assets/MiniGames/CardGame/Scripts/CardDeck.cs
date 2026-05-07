using System.Collections.Generic;
using UnityEngine;

public class CardDeck : MonoBehaviour
{
    [Header("Колода")]
    [Tooltip("Перетащить сюда все 12 CardData.asset из Assets/MiniGames/CardGame/Data/Cards/")]
    public List<CardData> allCards = new();

    // Рабочая колода — копия allCards после перемешивания
    private List<CardData> _deck = new();

    // Перемешать колоду алгоритмом Fisher-Yates
    public void Shuffle()
    {
        _deck = new List<CardData>(allCards);
        for (int i = _deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
        }
    }

    // Взять <paramref name="count"/> карт с верха колоды.
    // Если карт меньше — вернёт сколько есть
    public List<CardData> Draw(int count)
    {
        var drawn = new List<CardData>();
        for (int i = 0; i < count && _deck.Count > 0; i++)
        {
            drawn.Add(_deck[0]);
            _deck.RemoveAt(0);
        }
        return drawn;
    }

    // Сколько карт осталось в колоде
    public int Remaining => _deck.Count;
}

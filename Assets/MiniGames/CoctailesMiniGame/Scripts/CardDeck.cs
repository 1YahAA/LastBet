using System.Collections.Generic;
using UnityEngine;

public class CardDeck : MonoBehaviour
{
    [Header("Все карты")]
    public List<CardData> allCards = new();

    private readonly List<CardData> _drawPile = new();
    private readonly List<CardData> _discardPile = new();

    public int DrawPileCount => _drawPile.Count;
    public int DiscardPileCount => _discardPile.Count;

    public void Initialize()
    {
        _drawPile.Clear();
        _discardPile.Clear();

        foreach (var card in allCards)
        {
            if (card != null)
                _drawPile.Add(card);
        }

        Shuffle(_drawPile);
    }

    public List<CardData> Draw(int count)
    {
        var drawn = new List<CardData>();

        for (int i = 0; i < count; i++)
        {
            if (_drawPile.Count == 0)
                ReshuffleDiscardIntoDrawPile();

            if (_drawPile.Count == 0)
                break;

            drawn.Add(_drawPile[0]);
            _drawPile.RemoveAt(0);
        }

        return drawn;
    }

    public void AddManyToDiscard(IEnumerable<CardData> cards)
    {
        foreach (var card in cards)
        {
            if (card != null)
                _discardPile.Add(card);
        }
    }

    private void ReshuffleDiscardIntoDrawPile()
    {
        if (_discardPile.Count == 0)
            return;

        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        Shuffle(_drawPile);
        Debug.Log("[CardDeck] Сброс перемешан обратно в колоду");
    }

    private void Shuffle(List<CardData> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }
}

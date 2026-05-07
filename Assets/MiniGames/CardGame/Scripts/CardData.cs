using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "CardGame/CardData")]
public class CardData : ScriptableObject
{
    [Header("Основные параметры")]
    [Tooltip("Цвет карты (ингредиент коктейля)")]
    public CardColor color;

    [Tooltip("Базовое значение карты в очках")]
    [Range(1, 3)]
    public int value = 1;

    [Tooltip("Спрайт карты")]
    public Sprite cardSprite;

    [Header("Бонус соседства (adjacency)")]
    [Tooltip("Какой цвет должен стоять СЛЕВА, чтобы сработал бонус. None = бонуса нет")]
    public CardColor leftAdjacencyColor = CardColor.None;

    [Tooltip("Какой цвет должен стоять СПРАВА, чтобы сработал бонус. None = бонуса нет")]
    public CardColor rightAdjacencyColor = CardColor.None;

    [Tooltip("Сколько очков добавить при выполнении условия соседства")]
    [Range(0, 3)]
    public int adjacencyBonus = 0;
}

// Цвета карт. None используется только как «пусто» в adjacency.
public enum CardColor
{
    None = 0,
    Red = 1, // 🔴 Spirits — крепкий алкоголь
    Yellow = 2, // 🟡 Citrus  — цитрус
    Blue = 3, // 🔵 Bitter  — биттер
    Black = 4 // ⚫ Spoiled — испорченное
}

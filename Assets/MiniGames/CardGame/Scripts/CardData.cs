using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "CardGame/CardData")]
public class CardData : ScriptableObject
{

    [Header("Отображение")]
    public string displayName;

    [Header("Основные параметры")]
    public CocktailType cocktailType = CocktailType.None;

    [Range(-3, 6)]
    public int points = 1;

    public Sprite cardSprite;

    [Header("Бонус если рядом")]
    public CocktailType requiredLeft = CocktailType.None;
    public CocktailType requiredRight = CocktailType.None;

    [Range(-3, 6)]
    public int adjacencyBonus = 0;

    [Header("Особый эффект")]
    public CardEffectType effectType = CardEffectType.None;
    public CocktailType effectTarget = CocktailType.None;
    public int effectAmount = 0;
}

public enum CocktailType
{
    None = 0,
    Bitter = 1,
    Lemonchello = 2,
    Absinthe = 3,
    Damaged = 4
}

public enum CardEffectType
{
    None = 0,
    BreakAdjacentBonuses = 1,
    ExcludeFromRainbow = 2,
    CancelRecipeBonus = 3,
    MoldCenterPenalty = 4,
    CopyNeighborType = 10,
    AnyTypeForAdjacency = 11,
    AddToNeighbor = 12,
    DoubleRightAdjacency = 13,
    ZeroIfNoTargetNearby = 20,
    PenaltyIfTargetNearby = 21,
    LoseIfDamagedNearby = 22
}

public enum CocktailStrategy
{
    Revolt = 0,
    Obedience = 1,
    Analysis = 2
}

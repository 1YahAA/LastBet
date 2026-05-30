using UnityEngine;

[CreateAssetMenu(fileName = "Customer", menuName = "CardGame/CustomerData")]
public class CustomerData : ScriptableObject
{
    [Header("Основные параметры")]
    public string customerName;
    public Sprite portraitSprite;

    [TextArea(2, 4)]
    public string requestText;

    [Header("Пожелания")]
    public CocktailType requiredType = CocktailType.None;
    public CocktailType preferredType = CocktailType.None;
    public int bonusForPreferred = 1;

    [Header("Особое правило")]
    public CustomerRuleType ruleType = CustomerRuleType.None;
}

public enum CustomerRuleType
{
    None = 0,
    NoDamagedCards = 1,
    WantsRainbow = 2,
    WantsTriplet = 3,
    NoAdjacencyBonus = 4
}

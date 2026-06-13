using UnityEngine;

public class RuntimeCustomer
{
    public string Name;
    public Sprite PortraitSprite;
    public string RequestText;
    public CocktailType RequiredType;
    public CocktailType PreferredType;
    public int BonusForPreferred;
    public CustomerRuleType RuleType;

    // Нужен только для внутренней защиты от повторов реплик.
    public string RequestKey;
}

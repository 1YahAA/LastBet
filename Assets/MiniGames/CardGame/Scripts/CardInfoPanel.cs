using TMPro;
using UnityEngine;

public class CardInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    public void Show(CardData card)
    {
        if (card == null)
        {
            Hide();
            return;
        }

        gameObject.SetActive(true);

        if (titleText != null)
            titleText.text = string.IsNullOrWhiteSpace(card.displayName)
                ? card.name
                : card.displayName;

        if (descriptionText != null)
            descriptionText.text = BuildDescription(card);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private string BuildDescription(CardData card)
    {
        string text =
            $"Тип: {TypeName(card.cocktailType)}\n" +
            $"Очки: {card.points}\n";

        if (card.requiredLeft != CocktailType.None ||
            card.requiredRight != CocktailType.None)
        {
            text += "\nБонус соседства:\n";

            if (card.requiredLeft != CocktailType.None)
                text += $"Слева нужен {TypeName(card.requiredLeft)}\n";

            if (card.requiredRight != CocktailType.None)
                text += $"Справа нужен {TypeName(card.requiredRight)}\n";

            text += $"Бонус: +{card.adjacencyBonus}\n";
        }

        string effect = EffectDescription(card);

        if (!string.IsNullOrEmpty(effect))
        {
            text += "\nЭффект:\n";
            text += effect;
        }

        return text;
    }

    private string EffectDescription(CardData card)
    {
        return card.effectType switch
        {
            CardEffectType.BreakAdjacentBonuses =>
                "Ломает бонусы соседних карт.",

            CardEffectType.ExcludeFromRainbow =>
                "Не участвует в Радуге.",

            CardEffectType.CancelRecipeBonus =>
                "Отменяет бонус рецепта.",

            CardEffectType.MoldCenterPenalty =>
                $"Если в центре: {card.effectAmount}.",

            CardEffectType.CopyNeighborType =>
                "Копирует тип соседа.",

            CardEffectType.AnyTypeForAdjacency =>
                "Подходит к любому бонусу соседства.",

            CardEffectType.AddToNeighbor =>
                $"+{card.effectAmount} к соседним картам.",

            CardEffectType.DoubleRightAdjacency =>
                "Удваивает бонус карты справа.",

            CardEffectType.ZeroIfNoTargetNearby =>
                $"Если рядом нет {TypeName(card.effectTarget)} → 0 очков.",

            CardEffectType.PenaltyIfTargetNearby =>
                $"Если рядом {TypeName(card.effectTarget)}: {card.effectAmount}.",

            CardEffectType.LoseIfDamagedNearby =>
                "Если рядом испорченная карта → поражение.",

            _ => ""
        };
    }

    private string TypeName(CocktailType type)
    {
        return type switch
        {
            CocktailType.Bitter => "Биттер",
            CocktailType.Lemonchello => "Лимончелло",
            CocktailType.Absinthe => "Абсент",
            CocktailType.Damaged => "Испорченная",
            _ => "Нет"
        };
    }
}
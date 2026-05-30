using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Создаёт и перемешивает колоду. Менеджер не должен знать детали fallback-карт
public static class LastBetDeckService
{
    public static List<LastBetCardData> BuildDeck(IEnumerable<LastBetCardData> templates)
    {
        List<LastBetCardData> deck = new List<LastBetCardData>();

        if (templates != null)
            deck.AddRange(templates.Where(card => card != null));

        if (deck.Count == 0)
            deck.AddRange(CreateFallbackDeck());

        Shuffle(deck);
        return deck;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        if (list == null)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static IEnumerable<LastBetCardData> CreateFallbackDeck()
    {
        return new[]
        {
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.LighterAtServiceDoor,
                informationValue = 1,
                suspicionValue = 0,
                title = "ЗАЖИГАЛКА",
                cardDescription = "Чужая вещь у служебного входа.",
                evidencePanelDescription = "Тяжёлая металлическая зажигалка с потёртой гравировкой. Её нашли возле двери, которой пользуется персонал кабаре.",
                croupierLine = "Интересная находка. Но вещь у двери ещё не говорит, кто её оставил."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.VipBillWithoutNumber,
                informationValue = 1,
                suspicionValue = 0,
                title = "VIP-БИЛЕТ",
                cardDescription = "След закрытой ложи.",
                evidencePanelDescription = "Билет из закрытой ложи второго этажа. На обороте осталась едва заметная пометка карандашом.",
                croupierLine = "Закрытая ложа многое скрывает. Иногда слишком многое."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.RewrittenLetter,
                informationValue = 1,
                suspicionValue = 0,
                title = "ЗАПИСКА",
                cardDescription = "Фраза без подписи.",
                evidencePanelDescription = "«После последней ставки дверь будет открыта». Записка сложена слишком аккуратно, будто её подготовили заранее.",
                croupierLine = "Бумага терпит любые слова. Подпись обычно говорит больше."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.FalseTrail,
                storyClue = LastBetStoryClue.MaskWithPowder,
                informationValue = 1,
                suspicionValue = 1,
                title = "МАСКА",
                cardDescription = "Снята слишком поспешно.",
                evidencePanelDescription = "Чёрная сценическая маска с надорванной лентой. На внутренней стороне остался след грима.",
                croupierLine = "Маска слишком заметна. Иногда такие следы оставляют специально."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.Shield,
                storyClue = LastBetStoryClue.HelgaWarning,
                informationValue = 1,
                suspicionValue = 0,
                title = "ЗАВЯЗКА",
                cardDescription = "След ткани и грима.",
                evidencePanelDescription = "Тонкая лента пропитана запахом духов и театрального грима. Похожую ткань используют за сценой.",
                croupierLine = "За сценой всё пахнет гримом. Но не каждый след ведёт к врагу."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.Doubt,
                storyClue = LastBetStoryClue.BettingOrderChanged,
                informationValue = 1,
                suspicionValue = 1,
                title = "СТАВКИ",
                cardDescription = "Последовательность изменена.",
                evidencePanelDescription = "Кто-то исправил порядок карточных ставок прямо перед началом игры. Исправления сделаны уверенной рукой.",
                croupierLine = "В ставках нет случайностей. Есть только те, кто умеет ждать."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.ForgedEvidence,
                informationValue = 1,
                suspicionValue = 0,
                title = "ПОМЕТКА",
                cardDescription = "Знак персонала.",
                evidencePanelDescription = "Небольшой знак на полях документа совпадает с внутренними отметками персонала кабаре.",
                croupierLine = "Персонал всегда знает больше гостей. Но не всегда говорит правду."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.SecretVipCorridor,
                informationValue = 1,
                suspicionValue = 0,
                title = "КОРИДОР",
                cardDescription = "Маршрут за кулисы.",
                evidencePanelDescription = "На схеме отмечен служебный проход, ведущий мимо сцены и VIP-комнат. Обычные гости о нём не знают.",
                croupierLine = "Двери для гостей и двери для своих редко ведут в одно место."
            },
            new LastBetCardData
            {
                cardType = LastBetCardType.Joker,
                storyClue = LastBetStoryClue.JokerManipulatedTable,
                informationValue = 0,
                suspicionValue = 1,
                title = "ДЖОКЕР",
                cardDescription = "Карта вмешательства.",
                evidencePanelDescription = string.Empty,
                croupierLine = "Джокер любит появляться там, где выводы становятся слишком удобными."
            }
        };
    }
}

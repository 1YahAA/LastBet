using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Создаёт и перемешивает колоду карт для мини-игры «Последняя ставка»
public static class LastBetDeckService
{
    // Собирает колоду из шаблонов или fallback-карт, перемешивает
    public static List<LastBetCardData> BuildDeck(IEnumerable<LastBetCardData> templates)
    {
        List<LastBetCardData> deck = new List<LastBetCardData>();

        if (templates != null)
            deck.AddRange(templates.Where(card => card != null));

        if (deck.Count == 0)
            deck.AddRange(CreateFallbackDeck());

        Shuffle(deck);

        // Джокер не должен быть первой картой — иначе эффект затуманивания
        EnsureJokerNotFirst(deck);

        return deck;
    }

    // Перемещает Джокера с первой позиции на случайную позицию начиная со второй
    private static void EnsureJokerNotFirst(List<LastBetCardData> deck)
    {
        if (deck.Count < 2)
            return;

        if (deck[0] == null || !deck[0].IsJoker)
            return;

        // Меняем Джокера с любой картой начиная со второй позиции
        int swapIndex = Random.Range(1, deck.Count);
        (deck[0], deck[swapIndex]) = (deck[swapIndex], deck[0]);
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

    // Встроенная колода на случай если Inspector не заполнен
    private static IEnumerable<LastBetCardData> CreateFallbackDeck()
    {
        return new[]
        {
            // Намек на Мари
            // Служебный вход, внутренние маршруты — знает только персонал
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.LighterAtServiceDoor,
                informationValue = 1,
                suspicionValue = 0,
                title = "ЗАЖИГАЛКА",
                cardDescription = "Чужая вещь у служебного входа.",
                evidencePanelDescription =
                    "Тяжёлая металлическая зажигалка у служебной двери. " +
                    "Гости кабаре этим входом не пользуются — он для тех, кто знает внутренний распорядок. " +
                    "Зажигалку не забывают случайно. Её оставляют намеренно или теряют в спешке.",
                croupierLine = "Интересная находка. Но вещь у двери ещё не говорит, кто её оставил."
            },

            // Намек на Виктора
            // VIP-ложа, статус, публичная власть — его территория
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.VipBillWithoutNumber,
                informationValue = 1,
                suspicionValue  = 0,
                title = "VIP-БИЛЕТ",
                cardDescription = "След закрытой ложи.",
                evidencePanelDescription =
                    "Билет из закрытой ложи второго этажа — туда пускают только тех, кого хозяин кабаре считает нужным. " +
                    "На обороте едва заметная пометка карандашом: время и короткое слово, похожее на имя. " +
                    "Такие билеты не попадают в чужие руки просто так.",
                croupierLine = "Закрытая ложа многое скрывает. Иногда слишком многое."
            },

            // Без четкого акцента
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.RewrittenLetter,
                informationValue = 1,
                suspicionValue = 0,
                title = "ЗАПИСКА",
                cardDescription = "Фраза без подписи.",
                evidencePanelDescription =
                    "«После последней ставки дверь будет открыта». " +
                    "Почерк аккуратный, почти без наклона — так пишут люди, которых учили скрывать эмоции. " +
                    "Записка сложена слишком ровно, будто её готовили заранее и долго держали при себе.",
                croupierLine = "Бумага терпит любые слова. Подпись обычно говорит больше."
            },

            // Ложный след на Хэльгу
            // Маска слишком заметна — такие следы оставляют специально
            new LastBetCardData
            {
                cardType = LastBetCardType.FalseTrail,
                storyClue = LastBetStoryClue.MaskWithPowder,
                informationValue = 1,
                suspicionValue = 1,
                title = "МАСКА",
                cardDescription = "Снята слишком поспешно.",
                evidencePanelDescription =
                    "Чёрная сценическая маска с надорванной лентой — такие носят артисты в определённых номерах. " +
                    "На внутренней стороне след грима и слабый запах театральных духов. " +
                    "Маска слишком заметна для случайной улики. " +
                    "Тот, кто её оставил, либо торопился — либо хотел, чтобы её нашли.",
                croupierLine = "Маска слишком заметна. Иногда такие следы оставляют специально."
            },

            // Ложный след на Хэльгу
            // Грим и театральная ткань — её работа, не улика против неё
            new LastBetCardData
            {
                cardType = LastBetCardType.Shield,
                storyClue = LastBetStoryClue.HelgaWarning,
                informationValue = 1,
                suspicionValue = 0,
                title = "ЗАВЯЗКА",
                cardDescription = "След ткани и грима.",
                evidencePanelDescription =
                    "Тонкая лента пропитана запахом театрального грима и чужих духов. " +
                    "Такую ткань используют за кулисами — она часть работы, не улика. " +
                    "Но кто-то оставил её там, где артисты обычно не ходят. " +
                    "Или хотел, чтобы так подумали.",
                croupierLine = "За сценой всё пахнет гримом. Но не каждый след ведёт к врагу."
            },

            // Намек на Виктора
            // Порядок ставок — его контроль над столом
            new LastBetCardData
            {
                cardType = LastBetCardType.Doubt,
                storyClue = LastBetStoryClue.BettingOrderChanged,
                informationValue = 1,
                suspicionValue = 1,
                title = "СТАВКИ",
                cardDescription = "Последовательность изменена.",
                evidencePanelDescription =
                    "Порядок карточных ставок исправлен прямо перед началом игры. " +
                    "Исправления сделаны уверенной рукой — человек знал что меняет и зачем. " +
                    "Изменить порядок ставок без ведома хозяина невозможно. " +
                    "Или это сделал сам хозяин.",
                croupierLine = "В ставках нет случайностей. Есть только те, кто умеет ждать."
            },

            // Намек на Мари
            // Внутренняя пометка — знание системы изнутри
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.ForgedEvidence,
                informationValue = 1,
                suspicionValue  = 0,
                title = "ПОМЕТКА",
                cardDescription = "Знак персонала.",
                evidencePanelDescription =
                    "Небольшой знак на полях документа — такие метки используют внутри кабаре для сортировки бумаг. " +
                    "Гость этого не знает. Случайный человек тоже. " +
                    "Так помечает тот, кто давно работает в системе и знает её изнутри.",
                croupierLine = "Персонал всегда знает больше гостей. Но не всегда говорит правду."
            },

            // ── Намёк на Мари ─────────────────────────────────────────────
            // Служебный коридор — её маршруты, её знание здания.
            new LastBetCardData
            {
                cardType = LastBetCardType.StableClue,
                storyClue = LastBetStoryClue.SecretVipCorridor,
                informationValue = 1,
                suspicionValue = 0,
                title = "КОРИДОР",
                cardDescription = "Маршрут за кулисы.",
                evidencePanelDescription =
                    "На схеме отмечен служебный проход мимо сцены и VIP-комнат. " +
                    "Обычные гости о нём не знают. Большинство артистов — тоже. " +
                    "Знает тот, кто пользуется этим маршрутом регулярно и незаметно.",
                croupierLine = "Двери для гостей и двери для своих редко ведут в одно место."
            },

            // Джокер — вмешательство
            // Кто-то заметил что Эвелин ищет. Скорее всего — Мари
            new LastBetCardData
            {
                cardType = LastBetCardType.Joker,
                storyClue = LastBetStoryClue.JokerManipulatedTable,
                informationValue = 0,
                suspicionValue = 1,
                title = "ДЖОКЕР",
                cardDescription = "Карта вмешательства.",
                evidencePanelDescription = string.Empty,
                croupierLine =
                    "Джокер любит появляться там, где выводы становятся слишком удобными. " +
                    "Кто-то заметил что Эвелин ищет — и решил запутать след."
            }
        };
    }
}

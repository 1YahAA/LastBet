/// Типы жетонов
public enum TokenType
{
    Revolt, // Бунт — сопротивление, дерзость → концовка Свобода
    Obedience, // Послушание — подчинение, согласие → концовка Подчинение
    Analysis // Анализ — наблюдение, молчание → концовка Смерть
}

// Три концовки игры
public enum EndingType
{
    Freedom, // Свобода — сцена "Freedom"
    Submission, // Подчинение — сцена "Submission"
    Death // Смерть — сцена "Death"
}

// Какую мини-игру сейчас запустили
public enum MiniGameType
{
    CardGame, // Карточная игра
    Roulette // Рулетка
}

// Текущее состояние игры, чтобы блокировать/разрешать разные действия.
public enum GameplayState
{
    MainMenu, // Мы на главном экране
    Playing, // Игровой процесс, ждём клики игрока
    Paused, // Пауза (Escape)
    Dialogue, // Идёт диалог Yarn Spinner
    MiniGame, // Мы в мини-игре (отдельная сцена)
    Ending // Концовка — пауза недоступна
}
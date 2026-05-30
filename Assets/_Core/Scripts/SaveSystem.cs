// Сохранение и загрузка данных через PlayerPrefs
using UnityEngine;

public static class SaveSystem
{
    // Ключи для PlayerPrefs
    private const string KEY_REVOLT = "s_revolt";
    private const string KEY_OBEDIENCE = "s_obedience";
    private const string KEY_ANALYSIS = "s_analysis";
    private const string KEY_SCENE_INDEX = "s_sceneIndex";
    private const string KEY_COCKTAIL = "s_cocktailDrunk";
    private const string KEY_COCKTAIL_CNT = "s_cocktailCount";
    private const string KEY_HAS_SAVE = "s_hasSave"; // флаг: есть ли сохранение

    // Сохранить текущие данные из GameState в PlayerPrefs
    public static void Save(GameState state)
    {
        PlayerPrefs.SetInt(KEY_REVOLT, state.revolt);
        PlayerPrefs.SetInt(KEY_OBEDIENCE, state.obedience);
        PlayerPrefs.SetInt(KEY_ANALYSIS, state.analysis);
        PlayerPrefs.SetInt(KEY_SCENE_INDEX, state.currentSceneIndex);
        PlayerPrefs.SetInt(KEY_COCKTAIL, state.cocktailDrunk ? 1 : 0);
        PlayerPrefs.SetInt(KEY_COCKTAIL_CNT, state.cocktailCount);
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.Save();
        Debug.Log("[Сохранение] Выполнено");
    }

    // Загрузить данные из PlayerPrefs в GameState
    public static void Load(GameState state)
    {
        if (!HasSave()) return;
        state.revolt = PlayerPrefs.GetInt(KEY_REVOLT, 0);
        state.obedience = PlayerPrefs.GetInt(KEY_OBEDIENCE, 0);
        state.analysis = PlayerPrefs.GetInt(KEY_ANALYSIS, 0);
        state.currentSceneIndex = PlayerPrefs.GetInt(KEY_SCENE_INDEX, 0);
        state.cocktailDrunk = PlayerPrefs.GetInt(KEY_COCKTAIL, 0) == 1;
        state.cocktailCount = PlayerPrefs.GetInt(KEY_COCKTAIL_CNT, 0);
        Debug.Log("[Сохранение] Загружено");
    }

    // Удалить сохранение. Вызывается при старте новой игры и после концовки
    public static void Delete()
    {
        PlayerPrefs.DeleteKey(KEY_REVOLT);
        PlayerPrefs.DeleteKey(KEY_OBEDIENCE);
        PlayerPrefs.DeleteKey(KEY_ANALYSIS);
        PlayerPrefs.DeleteKey(KEY_SCENE_INDEX);
        PlayerPrefs.DeleteKey(KEY_COCKTAIL);
        PlayerPrefs.DeleteKey(KEY_COCKTAIL_CNT);
        PlayerPrefs.DeleteKey(KEY_HAS_SAVE);
        PlayerPrefs.Save();
        Debug.Log("[Сохранение] Удалено");
    }

    // Есть ли сохранение? чтобы показать/скрыть кнопку "Продолжить"
    public static bool HasSave() => PlayerPrefs.GetInt(KEY_HAS_SAVE, 0) == 1;
}
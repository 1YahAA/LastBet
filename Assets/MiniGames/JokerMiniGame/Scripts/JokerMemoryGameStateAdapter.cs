using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class JokerMemoryGameStateAdapter : MonoBehaviour
{
    [Header("Опционально: прямые ссылки")]
    [SerializeField] private ScriptableObject gameState;
    [SerializeField] private MonoBehaviour gameManager;

    [Header("Переход")]
    [SerializeField] private bool returnThroughGameManager = true;
    [SerializeField] private string fallbackSceneName = "";

    public void ApplyJokerMemoryResult(bool won)
    {
        ApplyToGameState(won);
        NotifyGameManager(won);
    }

    public void ReturnToMainScene()
    {
        if (returnThroughGameManager && TryInvoke(gameManager, "ReturnFromMiniGame"))
            return;

        if (!string.IsNullOrWhiteSpace(fallbackSceneName))
            SceneManager.LoadScene(fallbackSceneName);
    }

    private void ApplyToGameState(bool won)
    {
        UnityEngine.Object target = gameState;
        if (target == null)
            target = FindObjectOfTypeByName("GameState");

        if (target == null)
        {
            Debug.LogWarning("[JokerMemory] GameState не найден. Добавь ссылку в JokerMemoryGameStateAdapter или поля в текущий GameState.");
            return;
        }

        SetBool(target, "jokerWon", won);
        SetBool(target, "truthAvailable", won);

        if (won)
        {
            AddToken(target, "Analysis", 1);
        }
        else
        {
            SetBool(target, "distortionIncreased", true);
            TryInvoke(target, "IncreaseDistortion");
            TryInvoke(target, "IncreaseDistortion", 1);
        }

        TryInvoke(target, "Save");
    }

    private void NotifyGameManager(bool won)
    {
        UnityEngine.Object target = gameManager;
        if (target == null)
            target = FindObjectOfTypeByName("GameManager");

        if (target == null)
            return;

        if (TryInvoke(target, "FinishJokerMiniGame", won))
            return;

        if (TryInvoke(target, "FinishJokerMemoryMiniGame", won))
            return;

        TryInvoke(target, "FinishMiniGame", won);
    }

    private static void AddToken(UnityEngine.Object target, string tokenName, int amount)
    {
        if (TryInvoke(target, "AddToken", tokenName, amount))
            return;

        if (TryInvoke(target, "AddToken", tokenName))
            return;

        if (TryInvoke(target, "AddAnalysis", amount))
            return;

        FieldInfo field = target.GetType().GetField("analysis", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? target.GetType().GetField("Analysis", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? target.GetType().GetField("analysisTokens", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field != null && field.FieldType == typeof(int))
        {
            int current = (int)field.GetValue(target);
            field.SetValue(target, current + amount);
            return;
        }

        Debug.LogWarning("[JokerMemory] Не удалось начислить Analysis +1. Добавь метод AddToken/AddAnalysis или поле analysis в GameState.");
    }

    private static bool SetBool(UnityEngine.Object target, string memberName, bool value)
    {
        Type type = target.GetType();

        FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null && field.FieldType == typeof(bool))
        {
            field.SetValue(target, value);
            return true;
        }

        PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
        {
            property.SetValue(target, value);
            return true;
        }

        Debug.LogWarning($"[JokerMemory] В GameState не найден bool {memberName}.");
        return false;
    }

    private static UnityEngine.Object FindObjectOfTypeByName(string typeName)
    {
        foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll<UnityEngine.Object>())
        {
            if (obj == null)
                continue;

            if (obj.GetType().Name == typeName)
                return obj;
        }

        return null;
    }

    private static bool TryInvoke(UnityEngine.Object target, string methodName)
    {
        if (target == null)
            return false;

        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        if (method == null)
            return false;

        method.Invoke(target, null);
        return true;
    }

    private static bool TryInvoke(UnityEngine.Object target, string methodName, bool value)
    {
        if (target == null)
            return false;

        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(bool) }, null);
        if (method == null)
            return false;

        method.Invoke(target, new object[] { value });
        return true;
    }

    private static bool TryInvoke(UnityEngine.Object target, string methodName, int value)
    {
        if (target == null)
            return false;

        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
        if (method == null)
            return false;

        method.Invoke(target, new object[] { value });
        return true;
    }

    private static bool TryInvoke(UnityEngine.Object target, string methodName, string value)
    {
        if (target == null)
            return false;

        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
        if (method == null)
            return false;

        method.Invoke(target, new object[] { value });
        return true;
    }

    private static bool TryInvoke(UnityEngine.Object target, string methodName, string value, int amount)
    {
        if (target == null)
            return false;

        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(int) }, null);
        if (method == null)
            return false;

        method.Invoke(target, new object[] { value, amount });
        return true;
    }
}

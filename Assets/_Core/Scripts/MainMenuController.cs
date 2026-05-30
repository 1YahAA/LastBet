// Управляет главным меню игры
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Кнопки меню")]
    public GameObject continueButton;

    public GameObject quitButton;

    void Start()
    {
        Debug.Log($"[MainMenu] GameManager.Instance = {GameManager.Instance}");
        Debug.Log($"[MainMenu] SceneTransition.Instance = {SceneTransition.Instance}");
        // Показываем "Продолжить" только если есть сохранение
        if (continueButton != null)
            continueButton.SetActive(SaveSystem.HasSave());
        
        // Скрываем "Выход" в WebGL
        if (quitButton != null)
        {
#if UNITY_WEBGL
            quitButton.SetActive(false);
#endif
        }
    }

    public void OnNewGameClicked()
    {
        GameManager.Instance.StartNewGame();
    }

    public void OnContinueClicked()
    {
        GameManager.Instance.ContinueGame();
    }

    public void OnQuitClicked()
    {
#if !UNITY_WEBGL
        Application.Quit();
#endif
    }
}
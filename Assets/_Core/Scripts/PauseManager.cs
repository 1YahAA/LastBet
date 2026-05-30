// Управляет паузой: Escape открывает/закрывает меню
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI паузы")]
    public GameObject pauseMenuRoot;

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameManager.Instance.IsPaused)
                OnResumeClicked();
            else if (GameManager.Instance.IsPlaying || GameManager.Instance.IsInDialogue)
                OnPauseClicked();
            // В состоянии Ending, MiniGame, MainMenu — Escape не работает
        }
    }

    // Вызывается автоматически по Escape или вручную по кнопке
    public void OnPauseClicked()
    {
        GameManager.Instance.Pause();
        pauseMenuRoot.SetActive(true);
    }

    public void OnResumeClicked()
    {
        GameManager.Instance.Resume();
        pauseMenuRoot.SetActive(false);
    }

    public void OnMainMenuClicked()
    {
        pauseMenuRoot.SetActive(false);
        GameManager.Instance.ReturnToMainMenu();
    }

    public void OnRestartClicked()
    {
        pauseMenuRoot.SetActive(false);
        GameManager.Instance.StartNewGame();
    }
}
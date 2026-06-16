using TMPro;
using UnityEngine;

public sealed class JackpotMessagePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    public void ShowIntro()
    {
        Show("Нажми «ПУСК». Автомат допускает не больше трёх прокруток.");
    }

    public void ShowSpinStarted()
    {
        Show("Барабаны начинают движение...");
    }

    public void ShowSpinResult(JackpotFinalResult result)
    {
        if (result == null)
        {
            Show("Автомат молчит.");
            return;
        }

        Show(result.Description);
    }

    public void Show(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CabaretDirector : MonoBehaviour
{
    [Header("Силуэты в зале")]
    [Tooltip("4 силуэта: Viktor, Leo, Helga, Mari.\nПорядок не важен.")]
    public SilhouetteInteractable[] silhouettes;

    [Header("UI")]
    [Tooltip("Панель с персонажем и мыслями")]
    public CharacterPanel characterPanel;

    [Tooltip("Кнопка Продолжить — выключена пока не все силуэты просмотрены")]
    public Button continueButton;

    [Header("Музыка")]
    [Tooltip("AudioSource с Gloomy_Sunday.mp3.\nLoop: включить, Play On Awake: выключить")]
    public AudioSource musicSource;

    [Header("Субтитры")]
    [Tooltip("DialogueTrigger для субтитров песни.\nЗапускает узел Cabaret_Song")]
    public DialogueTrigger dialogueTrigger;

    [Tooltip("Запускать субтитры автоматически при старте сцены")]
    public bool autoStartSubtitles = true;

    void Start()
    {
        // Кнопка скрыта до просмотра всех силуэтов
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        // Запускаем музыку
        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.Play();
        }

        // Запускаем субтитры
        if (autoStartSubtitles && dialogueTrigger != null)
            StartCoroutine(StartSubtitlesDelayed());
    }

    // Небольшая задержка перед субтитрами — сцена успевает загрузиться
    private IEnumerator StartSubtitlesDelayed()
    {
        yield return new WaitForSeconds(1f);
        dialogueTrigger.StartDialogueNode("Cabaret_Song");
    }

    // Силуэты

    // Вызывается из SilhouetteInteractable при клике
    public void OnSilhouetteClicked(SilhouetteInteractable silhouette)
    {
        if (characterPanel == null) return;

        characterPanel.Show(silhouette.characterId, onClose: () =>
        {
            // Отмечаем силует как просмотренный
            silhouette.MarkAsViewed();
            CheckAllViewed();
        });
    }

    // Проверяем все ли силуэты просмотрены
    private void CheckAllViewed()
    {
        foreach (var s in silhouettes)
            if (!s.IsViewed) return;

        // Все просмотрены — показываем кнопку продолжить
        Debug.Log("[Cabaret] Все персонажи просмотрены → кнопка Продолжить");
        if (continueButton != null)
            continueButton.gameObject.SetActive(true);
    }

    // Переход

    // Кнопка "Продолжить" → переход в Scene2_Dressing
    public void OnContinueClicked()
    {
        if (musicSource != null)
            musicSource.Stop();

        GameManager.Instance.LoadNextScene();
    }
}
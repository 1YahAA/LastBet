using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private Image fadeImage;

    [SerializeField] private float fadeOutDuration = 0.5f; 
    [SerializeField] private float fadeInDuration = 1.2f; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 1f);
            fadeImage.raycastTarget = true;
        }
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutRoutine(sceneName));
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeOutRoutine(string sceneName)
    {
        if (fadeImage != null) fadeImage.raycastTarget = true;
        yield return StartCoroutine(Fade(0f, 1f, fadeOutDuration));
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        yield return StartCoroutine(Fade(1f, 0f, fadeInDuration));
        if (fadeImage != null) fadeImage.raycastTarget = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        fadeImage.color = new Color(0, 0, 0, from);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, to);
    }
}
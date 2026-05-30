using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.4f;

    private void Awake()
    {
        Debug.Log($"[SceneTransition] Awake | object={name} | activeSelf={gameObject.activeSelf} | scene={gameObject.scene.name}", this);

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[SceneTransition] Duplicate destroyed: {name}", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log($"[SceneTransition] Instance set | fadeImage={fadeImage}", this);

        if (fadeImage == null)
        {
            Debug.LogError("[SceneTransition] fadeImage is NULL", this);
            return;
        }

        var c = fadeImage.color;
        fadeImage.color = new Color(c.r, c.g, c.b, 0f);
        fadeImage.raycastTarget = false;
    }

    public void FadeToScene(string sceneName)
    {
        Debug.Log($"[SceneTransition] FadeToScene: {sceneName} | fadeImage={fadeImage}", this);

        if (fadeImage == null)
        {
            Debug.LogError("[SceneTransition] FadeToScene aborted: fadeImage is NULL", this);
            return;
        }

        fadeImage.raycastTarget = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
        fadeImage.DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                SceneManager.LoadScene(sceneName);
            });
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (fadeImage == null)
        {
            Debug.LogError("[SceneTransition] OnSceneLoaded: fadeImage is NULL", this);
            return;
        }

        fadeImage.DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => fadeImage.raycastTarget = false);
    }
}
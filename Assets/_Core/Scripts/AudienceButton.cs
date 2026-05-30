using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudienceButton : MonoBehaviour
{
    [Tooltip("Дочерний объект с пузырём")]
    public GameObject bubble;

    [Tooltip("Image фона пузыря")]
    public Image bubbleImage;

    [Tooltip("Текст внутри пузыря")]
    public TextMeshProUGUI bubbleText;

    [Tooltip("Фраза этого зрителя")]
    [TextArea(2, 4)]
    public string thought;

    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.4f;
    public float hideDelay = 3f;

    private Coroutine _activeCoroutine;

    void Start()
    {
        if (bubble != null) bubble.SetActive(false);

        SetAlpha(0f);

        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnClicked);
    }

    public void OnClicked()
    {
        if (bubble == null) return;
        if (bubbleText != null) bubbleText.text = thought;

        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(ShowAndHide());
    }

    public void ForceHide()
    {
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator ShowAndHide()
    {
        bubble.SetActive(true);
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(hideDelay);
        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, elapsed / fadeInDuration));
            yield return null;
        }
        SetAlpha(1f);
    }

    private IEnumerator FadeOut()
    {
        float startAlpha = GetAlpha();
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration));
            yield return null;
        }
        SetAlpha(0f);
        if (bubble != null) bubble.SetActive(false);
    }

    private void SetAlpha(float a)
    {
        if (bubbleImage != null)
        {
            Color c = bubbleImage.color;
            bubbleImage.color = new Color(c.r, c.g, c.b, a);
        }
        if (bubbleText != null)
        {
            Color c = bubbleText.color;
            bubbleText.color = new Color(c.r, c.g, c.b, a);
        }
    }

    private float GetAlpha()
    {
        if (bubbleImage != null) return bubbleImage.color.a;
        if (bubbleText != null) return bubbleText.color.a;
        return 1f;
    }
}
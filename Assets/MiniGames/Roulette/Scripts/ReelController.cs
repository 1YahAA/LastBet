using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ReelController : MonoBehaviour
{
    [Header("Символы")]
    public Image[] symbolImages;

    [Header("Параметры")]
    public float symbolHeight = 128f;
    public float spinSpeed    = 800f;
    public float slowdownDuration = 0.6f;

    public bool IsSpinning { get; private set; }

    private RectTransform _rt;
    private Sprite[] _sprites;
    private int _count;
    private float _cycleHeight; // высота одного набора символов
    private Coroutine _spinCoroutine;

    // Инициализация

    public void Initialize(Sprite[] sprites)
    {
        _rt = GetComponent<RectTransform>();
        _sprites = sprites;
        _count = sprites.Length;
        _cycleHeight = symbolHeight * _count;

        CreateSymbolImages();
        SetPositionY(0f);
    }

    // Создаём 3 копии набора символов подряд
    // Это позволяет крутить вверх и зациклиться без видимого прыжка
    private void CreateSymbolImages()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        const int copies = 3;
        symbolImages = new Image[_count * copies];

        for (int i = 0; i < symbolImages.Length; i++)
        {
            var go = new GameObject($"Symbol_{i}");
            go.transform.SetParent(transform, false);

            var img = go.AddComponent<Image>();
            img.sprite = _sprites[i % _count];
            img.preserveAspect = true;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(symbolHeight, symbolHeight);
            rt.anchoredPosition = new Vector2(0f, -i * symbolHeight);

            symbolImages[i] = img;
        }
    }

    // Вращение
    public void StartSpin()
    {
        if (IsSpinning) return;
        KillTween();
        if (_spinCoroutine != null) StopCoroutine(_spinCoroutine);
        _spinCoroutine = StartCoroutine(SpinLoop());
    }

    public void StopSpin(int symbolIndex)
    {
        if (!IsSpinning) return;
        if (_spinCoroutine != null) StopCoroutine(_spinCoroutine);
        KillTween();
        _spinCoroutine = StartCoroutine(StopRoutine(symbolIndex));
    }

    // ForceSymbol теперь тоже через StopSpin — плавно, без телепорта
    public void ForceSymbol(int symbolIndex)
    {
        StopSpin(symbolIndex);
    }

    // Барабан едет ВВЕРХ (Y увеличивается), зацикливается в пределах одного набора
    private IEnumerator SpinLoop()
    {
        IsSpinning = true;
        while (true)
        {
            float y = _rt.anchoredPosition.y + spinSpeed * Time.deltaTime;
            // Зацикливаем: как только прокрутили один полный набор — возвращаемся
            if (y >= _cycleHeight)
                y -= _cycleHeight;
            SetPositionY(y);
            yield return null;
        }
    }

    // Плавная остановка: нормализуем позицию и едем к цели через DOTween
    private IEnumerator StopRoutine(int symbolIndex)
    {
        IsSpinning = true;

        // Нормализуем текущую позицию в [0, _cycleHeight)
        float currentY = Mathf.Repeat(_rt.anchoredPosition.y, _cycleHeight);
        SetPositionY(currentY);

        // Целевая позиция в пределах одного цикла
        float targetY = symbolIndex * symbolHeight;

        // Нам нужно ехать ВПЕРЁД (вверх) к цели
        // Если цель уже позади — добавляем один оборот
        float destination = targetY;
        if (destination <= currentY)
            destination += _cycleHeight;

        // Добавляем минимум один полный оборот — без этого барабан почти не крутится
        destination += _cycleHeight;

        // Время торможения пропорционально дистанции — скорость замедления одинакова всегда
        float distance = destination - currentY;
        float duration = distance / spinSpeed * 1.2f;
        duration = Mathf.Clamp(duration, 0.5f, 1.5f);

        yield return _rt.DOAnchorPosY(destination, duration)
            .SetEase(Ease.OutCubic)
            .WaitForCompletion();

        // Снап к точной позиции
        SetPositionY(targetY);
        IsSpinning = false;
        _spinCoroutine = null;
    }

    // Утилиты

    private void SetPositionY(float y)
    {
        _rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, y);
    }

    private void KillTween()
    {
        _rt?.DOKill();
    }

    public void PlayJackpotEffect()
    {
        foreach (var img in symbolImages)
        {
            if (img == null) continue;
            img.DOKill();
            img.DOColor(new Color(1f, 0.85f, 0.2f), 0.25f)
               .SetLoops(6, LoopType.Yoyo)
               .SetUpdate(true);
        }
    }
}
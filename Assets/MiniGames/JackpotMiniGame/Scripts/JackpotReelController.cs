using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[Serializable]
public sealed class JackpotSymbolSprite
{
    public JackpotSymbolType symbol;
    public Sprite sprite;
}

public sealed class JackpotReelController : MonoBehaviour
{
    [Header("Viewport")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform contentRoot;

    [Header("Символы")]
    [SerializeField] private JackpotSymbolSprite[] symbolSprites;

    [Header("Прокрутка")]
    [SerializeField] private int symbolsInSpin = 14;
    [SerializeField] private float defaultSpinDuration = 1.2f;
    [SerializeField] private Ease spinEase = Ease.OutCubic;

    public JackpotSymbolType CurrentSymbol { get; private set; } = JackpotSymbolType.Blank;

    private readonly List<GameObject> _spawnedItems = new();

    private void Awake()
    {
        if (viewport == null)
            viewport = transform as RectTransform;

        if (viewport != null && viewport.GetComponent<RectMask2D>() == null)
            viewport.gameObject.AddComponent<RectMask2D>();

        if (contentRoot == null)
            contentRoot = CreateContentRoot();
    }

    public IEnumerator SpinTo(JackpotSymbolType result)
    {
        yield return SpinTo(result, defaultSpinDuration);
    }

    public IEnumerator SpinTo(JackpotSymbolType result, float duration)
    {
        if (viewport == null)
            yield break;

        if (contentRoot == null)
            contentRoot = CreateContentRoot();

        contentRoot.DOKill();
        ClearSpawnedItems();

        float itemHeight = viewport.rect.height;
        float itemWidth = viewport.rect.width;

        if (itemHeight <= 0f)
            itemHeight = 180f;

        if (itemWidth <= 0f)
            itemWidth = 180f;

        List<JackpotSymbolType> stripSymbols = BuildSpinStrip(result);

        for (int i = 0; i < stripSymbols.Count; i++)
        {
            CreateSymbolItem(stripSymbols[i], i, itemWidth, itemHeight);
        }

        contentRoot.anchoredPosition = Vector2.zero;

        float targetY = (stripSymbols.Count - 1) * itemHeight;

        Tween tween = contentRoot
            .DOAnchorPosY(targetY, duration)
            .SetEase(spinEase);

        yield return tween.WaitForCompletion();

        CurrentSymbol = result;
    }

    public void SetSymbolInstant(JackpotSymbolType symbol)
    {
        if (viewport == null)
            viewport = transform as RectTransform;

        if (contentRoot == null)
            contentRoot = CreateContentRoot();

        contentRoot.DOKill();
        ClearSpawnedItems();

        float itemHeight = viewport != null && viewport.rect.height > 0f ? viewport.rect.height : 180f;
        float itemWidth = viewport != null && viewport.rect.width > 0f ? viewport.rect.width : 180f;

        CreateSymbolItem(symbol, 0, itemWidth, itemHeight);
        contentRoot.anchoredPosition = Vector2.zero;

        CurrentSymbol = symbol;
    }

    private List<JackpotSymbolType> BuildSpinStrip(JackpotSymbolType finalSymbol)
    {
        List<JackpotSymbolType> result = new();

        int count = Mathf.Max(4, symbolsInSpin);

        for (int i = 0; i < count - 1; i++)
            result.Add(RandomSymbolForAnimation());

        result.Add(finalSymbol);

        return result;
    }

    private void CreateSymbolItem(JackpotSymbolType symbol, int index, float width, float height)
    {
        GameObject obj = new GameObject($"Symbol_{index}_{symbol}", typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(contentRoot, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(0f, -index * height);
        rt.localScale = Vector3.one;

        Image image = obj.GetComponent<Image>();
        image.sprite = GetSprite(symbol);
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.enabled = image.sprite != null;

        _spawnedItems.Add(obj);
    }

    private RectTransform CreateContentRoot()
    {
        GameObject obj = new GameObject("ReelContent", typeof(RectTransform));
        obj.transform.SetParent(transform, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = viewport != null ? viewport.rect.size : new Vector2(180f, 180f);

        return rt;
    }

    private JackpotSymbolType RandomSymbolForAnimation()
    {
        int value = UnityEngine.Random.Range(0, 5);

        return value switch
        {
            0 => JackpotSymbolType.Bird,
            1 => JackpotSymbolType.Cage,
            2 => JackpotSymbolType.Eye,
            3 => JackpotSymbolType.Cocktail,
            _ => JackpotSymbolType.Microphone
        };
    }

    private Sprite GetSprite(JackpotSymbolType symbol)
    {
        if (symbolSprites == null)
            return null;

        foreach (var item in symbolSprites)
        {
            if (item != null && item.symbol == symbol)
                return item.sprite;
        }

        return null;
    }

    private void ClearSpawnedItems()
    {
        foreach (GameObject item in _spawnedItems)
        {
            if (item != null)
                Destroy(item);
        }

        _spawnedItems.Clear();
    }

    private void OnDestroy()
    {
        if (contentRoot != null)
            contentRoot.DOKill();
    }
}
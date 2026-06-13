using System;
using System.Collections;
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
    [Header("UI")]
    [SerializeField] private Image symbolImage;

    [Header("Символы")]
    [SerializeField] private JackpotSymbolSprite[] symbolSprites;

    [Header("Анимация")]
    [SerializeField] private float spinDuration = 0.75f;
    [SerializeField] private float tickInterval = 0.06f;
    [SerializeField] private float punchScale = 0.08f;

    public JackpotSymbolType CurrentSymbol { get; private set; } = JackpotSymbolType.Blank;

    public IEnumerator SpinTo(JackpotSymbolType result)
    {
        if (symbolImage == null)
            yield break;

        float elapsed = 0f;

        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * punchScale, spinDuration, 8, 0.5f);

        while (elapsed < spinDuration)
        {
            SetSymbol(RandomSymbolForAnimation());
            elapsed += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }

        SetSymbol(result);
    }

    public void SetSymbol(JackpotSymbolType symbol)
    {
        CurrentSymbol = symbol;

        if (symbolImage == null)
            return;

        symbolImage.sprite = GetSprite(symbol);
        symbolImage.enabled = symbolImage.sprite != null;
        symbolImage.color = Color.white;
        symbolImage.preserveAspect = true;
    }

    private JackpotSymbolType RandomSymbolForAnimation()
    {
        int value = UnityEngine.Random.Range(0, 5);

        return value switch
        {
            0 => JackpotSymbolType.Coin,
            1 => JackpotSymbolType.DoubleCoin,
            2 => JackpotSymbolType.Blank,
            3 => JackpotSymbolType.Debt,
            _ => JackpotSymbolType.Hairpin
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

    private void OnDestroy()
    {
        transform.DOKill();
    }
}

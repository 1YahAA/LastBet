using UnityEngine;
using UnityEngine.UI;

public sealed class JackpotSymbolView : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private JackpotSymbolSprite[] sprites;

    public JackpotSymbolType CurrentSymbol { get; private set; }

    public void SetSymbol(JackpotSymbolType symbol)
    {
        CurrentSymbol = symbol;

        if (image == null)
            return;

        image.sprite = FindSprite(symbol);
        image.enabled = image.sprite != null;
        image.preserveAspect = true;
    }

    private Sprite FindSprite(JackpotSymbolType symbol)
    {
        if (sprites == null)
            return null;

        foreach (var item in sprites)
        {
            if (item != null && item.symbol == symbol)
                return item.sprite;
        }

        return null;
    }
}

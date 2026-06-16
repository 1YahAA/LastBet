using System;
using UnityEngine;

[Serializable]
public sealed class LastBetCardData
{
    [Header("Symbol")]
    public LastBetSymbolType symbolType = LastBetSymbolType.None;

    [Header("Visual")]
    public Sprite symbolSprite;
    public string title;
    [TextArea(1, 3)] public string cardDescription;

    [Header("Victor Line")]
    [TextArea(1, 3)] public string victorLine;

    [Header("Score Values")]
    public int freedomValue;
    public int cageValue;
    public int truthValue;
    public int pressureValue;

    public bool IsJoker => symbolType == LastBetSymbolType.Joker;

    public void NormalizeValuesFromSymbol()
    {
        freedomValue = 0;
        cageValue = 0;
        truthValue = 0;
        pressureValue = 0;

        switch (symbolType)
        {
            case LastBetSymbolType.Bird:
                freedomValue = 1;
                break;

            case LastBetSymbolType.Cage:
            case LastBetSymbolType.Cocktail:
                cageValue = 1;
                break;

            case LastBetSymbolType.Eye:
            case LastBetSymbolType.Microphone:
                truthValue = 1;
                break;

            case LastBetSymbolType.Joker:
                pressureValue = 1;
                break;
        }
    }
}

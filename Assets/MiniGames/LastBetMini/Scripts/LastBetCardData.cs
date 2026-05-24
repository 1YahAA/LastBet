using System;
using UnityEngine;

[Serializable]
public class LastBetCardData
{
    [Header("Logic")]
    public LastBetCardType cardType;
    public LastBetStoryClue storyClue;
    public int informationValue = 1;
    public int suspicionValue = 0;

    [Header("Visual")]
    public Sprite clueSprite;
    public string title;
    [TextArea(1, 3)] public string cardDescription;
    [TextArea(2, 5)] public string evidencePanelDescription;
    [TextArea(1, 3)] public string croupierLine;

    public bool IsJoker => cardType == LastBetCardType.Joker;
    public bool AddsEvidence => !IsJoker && storyClue != LastBetStoryClue.None;
}
using System.Collections.Generic;
using UnityEngine;

// Класс отвечает только за правила: время, подозрение, сведения и собранные улики
public sealed class LastBetRoundModel
{
    private readonly List<LastBetStoryClue> _collectedClues = new List<LastBetStoryClue>();

    public int Information { get; private set; }
    public int Suspicion { get; private set; }
    public float TimeLeft { get; private set; }
    public bool Active { get; private set; }
    public bool ChoiceOpened { get; private set; }
    public LastBetSuspect SelectedSuspect { get; private set; } = LastBetSuspect.None;
    public IReadOnlyList<LastBetStoryClue> CollectedClues => _collectedClues;

    public void Start(float roundTime)
    {
        Information = 0;
        Suspicion = 0;
        TimeLeft = roundTime;
        Active = true;
        ChoiceOpened = false;
        SelectedSuspect = LastBetSuspect.None;
        _collectedClues.Clear();
    }

    public void Tick(float deltaTime)
    {
        if (!Active || ChoiceOpened || TimeLeft <= 0f)
            return;

        TimeLeft = Mathf.Max(0f, TimeLeft - deltaTime);
    }

    public LastBetCardApplyResult ApplyCard(LastBetCardData card, int suspicionLimit)
    {
        LastBetCardApplyResult result = new LastBetCardApplyResult();

        if (card == null)
            return result;

        Information += Mathf.Max(0, card.informationValue);
        Suspicion += Mathf.Max(0, card.suspicionValue);
        Suspicion = Mathf.Clamp(Suspicion, 0, suspicionLimit);

        result.IsJoker = card.IsJoker;
        result.AddedEvidence = card.AddsEvidence;
        result.OpenChoiceBecauseSuspicionLimit = Suspicion >= suspicionLimit;

        if (card.AddsEvidence && !_collectedClues.Contains(card.storyClue))
            _collectedClues.Add(card.storyClue);

        return result;
    }

    public bool HasEnoughInformation(int minInformationToChoose)
    {
        return Information >= minInformationToChoose;
    }

    public void OpenChoice()
    {
        ChoiceOpened = true;
        Active = false;
    }

    public void SelectSuspect(LastBetSuspect suspect)
    {
        SelectedSuspect = suspect;
    }
}

public struct LastBetCardApplyResult
{
    public bool IsJoker;
    public bool AddedEvidence;
    public bool OpenChoiceBecauseSuspicionLimit;
}

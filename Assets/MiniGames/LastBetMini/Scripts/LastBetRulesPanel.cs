using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Стартовое окно правил
public sealed class LastBetRulesPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    public void Configure(GameObject root, Button button, TMP_Text title, TMP_Text body)
    {
        if (panelRoot == null) panelRoot = root;
        if (startButton == null) startButton = button;
        if (titleText == null) titleText = title;
        if (bodyText == null) bodyText = body;
    }

    public bool Exists => GetRoot() != null;

    public void Show(int minInformationToChoose, int suspicionLimit, Action onStart)
    {
        SetupText(minInformationToChoose, suspicionLimit);
        WireButton(onStart);
        LastBetUiUtility.SetPanelVisible(GetRoot(), true);
        GetRoot()?.transform.SetAsLastSibling();
    }

    public void Hide()
    {
        LastBetUiUtility.SetPanelVisible(GetRoot(), false);
    }

    private void SetupText(int minInformationToChoose, int suspicionLimit)
    {
        if (titleText == null)
            titleText = LastBetSceneLookup.FindText("RulesTitleText");
        if (bodyText == null)
            bodyText = LastBetSceneLookup.FindText("RulesBodyText");

        if (titleText != null)
            titleText.text = "Правила последней ставки";

        if (bodyText != null)
        {
            bodyText.text =
                "В кабаре слишком много масок и слишком мало правды.\n\n" +
                "Открывайте карты, собирайте сведения и следите за тем, как меняется атмосфера за столом.\n\n" +
                "Когда версия начнёт складываться — заберите сведения и решите, кому Эвелин готова поверить.\n\n" +
                "Но помните:\n" +
                "в «Последней ставке» уверенность бывает опаснее лжи.";
        }
    }

    private void WireButton(Action onStart)
    {
        if (startButton == null)
            startButton = LastBetSceneLookup.FindButton("RulesStartButton");
        if (startButton == null)
            return;

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => onStart?.Invoke());
    }

    private GameObject GetRoot()
    {
        if (panelRoot == null)
            panelRoot = LastBetSceneLookup.FindObjectIncludeInactive("RulesPanel");
        return panelRoot;
    }
}

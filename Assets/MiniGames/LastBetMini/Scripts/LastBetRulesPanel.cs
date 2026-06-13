using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Вступительная панель перед мини-игрой
public sealed class LastBetRulesPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    public bool Exists => GetRoot() != null;

    public void Configure(GameObject root, Button button, TMP_Text title, TMP_Text body)
    {
        if (root != null) panelRoot = root;
        if (button != null) startButton = button;
        if (title != null) titleText = title;
        if (body != null) bodyText = body;
    }

    public void Show(Action onStart, int minInformationToChoose, int suspicionLimit)
    {
        GameObject root = GetRoot();
        if (root == null)
            return;

        LastBetUiUtility.SetPanelVisible(root, true);
        root.transform.SetAsLastSibling();

        if (titleText == null)
            titleText = LastBetSceneLookup.FindText("RulesTitleText");

        if (bodyText == null)
            bodyText = LastBetSceneLookup.FindText("RulesBodyText");

        if (startButton == null)
            startButton = LastBetSceneLookup.FindButton("RulesStartButton");

        if (titleText != null)
            titleText.text = "Последняя ставка";

        if (bodyText != null)
        {
            bodyText.text =
                "В кабаре слишком много масок и слишком мало правды.\n\n" +
                "Открывайте карты, собирайте следы и слушайте крупье. Его слова не всегда помогают, но иногда выдают больше, чем сами улики.\n\n" +
                "Каждая новая карта может прояснить версию или привлечь лишнее внимание к Эвелин. Когда картина начнёт складываться, остановите партию и решите, чьему следу она поверит.\n\n" +
                "В «Последней ставке» опасна не только ложь. Опасна уверенность, появившаяся слишком рано.";
        }

        WireButton(onStart);
    }

    public void Hide()
    {
        LastBetUiUtility.SetPanelVisible(GetRoot(), false);
    }

    private void WireButton(Action onStart)
    {
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

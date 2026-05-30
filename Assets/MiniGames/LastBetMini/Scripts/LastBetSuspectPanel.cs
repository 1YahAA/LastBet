using System;
using UnityEngine;
using UnityEngine.UI;

// Панель выбора версии
public sealed class LastBetSuspectPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private LastBetChoiceButton helgaChoice;
    [SerializeField] private LastBetChoiceButton victorChoice;
    [SerializeField] private LastBetChoiceButton marieChoice;

    private Action<LastBetSuspect> _onSelected;
    private LastBetSuspect _selected = LastBetSuspect.None;

    public void Configure(GameObject root, LastBetChoiceButton helga, LastBetChoiceButton victor, LastBetChoiceButton marie)
    {
        if (panelRoot == null) panelRoot = root;
        if (helgaChoice == null) helgaChoice = helga;
        if (victorChoice == null) victorChoice = victor;
        if (marieChoice == null) marieChoice = marie;
    }

    public void Initialize(Action<LastBetSuspect> onSelected)
    {
        _onSelected = onSelected;
        BindChoice(ref helgaChoice, "SuspectedHelga", LastBetSuspect.Helga);
        BindChoice(ref victorChoice, "SuspectedVictor", LastBetSuspect.Victor);
        BindChoice(ref marieChoice, "SuspectedMari", LastBetSuspect.Marie);
        Hide();
    }

    public void Show()
    {
        LastBetUiUtility.SetPanelVisible(GetRoot(), true);
        GameObject root = GetRoot();
        if (root != null)
            root.transform.SetAsLastSibling();
        RefreshSelection();
    }

    public void Hide()
    {
        LastBetUiUtility.SetPanelVisible(GetRoot(), false);
    }

    public void SetSelected(LastBetSuspect suspect)
    {
        _selected = suspect;
        RefreshSelection();
    }

    private void BindChoice(ref LastBetChoiceButton choice, string objectName, LastBetSuspect suspect)
    {
        GameObject go = choice != null ? choice.gameObject : LastBetSceneLookup.FindObjectIncludeInactive(objectName);
        if (go == null)
            return;

        if (choice == null)
            choice = go.GetComponent<LastBetChoiceButton>() ?? go.AddComponent<LastBetChoiceButton>();

        choice.BindDefaults();
        Button button = go.GetComponent<Button>() ?? go.AddComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onSelected?.Invoke(suspect));
    }

    private void RefreshSelection()
    {
        if (helgaChoice != null) helgaChoice.SetSelected(_selected == LastBetSuspect.Helga);
        if (victorChoice != null) victorChoice.SetSelected(_selected == LastBetSuspect.Victor);
        if (marieChoice != null) marieChoice.SetSelected(_selected == LastBetSuspect.Marie);
    }

    private GameObject GetRoot()
    {
        if (panelRoot == null)
            panelRoot = LastBetSceneLookup.FindObjectIncludeInactive("SuspectPanel");
        return panelRoot;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Хранилище найденных улик
public sealed class LastBetEvidencePanel : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private LastBetClueSlotView clueSlotPrefab;
    [SerializeField] private LastBetTooltip tooltip;

    [Header("Behaviour")]
    [SerializeField] private bool allowRemoveFromPanel = true;
    [SerializeField] private bool debugLogs = true;

    private readonly List<LastBetClueSlotView> _slots = new List<LastBetClueSlotView>();

    public int VisibleCount => _slots.Count;

    public void Configure(Transform parent, LastBetClueSlotView prefab, LastBetTooltip tooltipReference = null)
    {
        if (parent != null)
            contentParent = parent;

        if (prefab != null)
            clueSlotPrefab = prefab;

        if (tooltipReference != null)
            tooltip = tooltipReference;

        if (tooltip == null)
            tooltip = FindAnyObjectByType<LastBetTooltip>(FindObjectsInactive.Include);

        if (contentParent == null)
            Debug.LogError("[LastBet] EvidencePanel.Configure: contentParent is still null. " +
                           "Переименуй объект Content в сцене в 'EvidenceContent' " +
                           "и назначь его в поле Evidence Content Parent в Inspector.");

        if (clueSlotPrefab == null)
            Debug.LogError("[LastBet] EvidencePanel.Configure: clueSlotPrefab is null. " +
                           "Назначь префаб ClueSlot в Inspector компонента LastBetMiniGameManager.");
    }

    public void Clear()
    {
        tooltip?.Hide();
        _slots.Clear();
        LastBetUiUtility.ClearChildren(GetContentParent());

        if (debugLogs)
            Debug.Log("[LastBet] EvidencePanel cleared.");
    }

    public void AddEvidence(LastBetCardData data)
    {
        if (data == null || !data.AddsEvidence)
            return;

        Transform parent = GetContentParent();
        if (parent == null)
        {
            Debug.LogError("[LastBet] AddEvidence: contentParent не назначен. " +
                           "Улика не добавлена. Назначь EvidenceContent в Inspector.");
            return;
        }

        if (clueSlotPrefab == null)
        {
            Debug.LogError("[LastBet] AddEvidence: clueSlotPrefab не назначен. " +
                           "Улика не добавлена. Назначь ClueSlot Prefab в Inspector.");
            return;
        }

        LastBetClueSlotView slot = Instantiate(clueSlotPrefab, parent);
        slot.gameObject.SetActive(true);
        slot.transform.SetAsLastSibling();
        slot.Setup(
            data.clueSprite,
            MakeReadableTitle(data.title),
            data.evidencePanelDescription,
            tooltip,
            allowRemoveFromPanel ? RemoveSlot : (System.Action<LastBetClueSlotView>)null
        );

        _slots.Add(slot);

        RebuildLayout(parent);

        if (debugLogs)
        {
            Debug.Log(
                "[LastBet] Evidence added: " +
                $"storyClue={data.storyClue} | title={data.title} | " +
                $"parent={parent.name} | children={parent.childCount} | visibleSlots={_slots.Count}"
            );
        }
    }

    public void MarkLastEvidenceAsUnstable()
    {
        if (_slots.Count == 0)
            return;

        LastBetClueSlotView slot = _slots[_slots.Count - 1];
        if (slot != null)
            slot.SetUnstable(true);
    }

    private void RemoveSlot(LastBetClueSlotView slot)
    {
        if (slot == null)
            return;

        _slots.Remove(slot);
        Destroy(slot.gameObject);
        RebuildLayout(GetContentParent());

        if (debugLogs)
            Debug.Log($"[LastBet] Evidence hidden from panel. visibleSlots={_slots.Count}");
    }

    private Transform GetContentParent()
    {
        if (contentParent != null)
            return contentParent;

        Debug.LogError("[LastBet] GetContentParent: contentParent == null. " +
                       "Улики некуда добавлять. Проверь назначение в Inspector.");
        return null;
    }

    private static void RebuildLayout(Transform parent)
    {
        if (parent == null)
            return;

        if (parent is RectTransform parentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
    }

    private static string MakeReadableTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Улика";

        return value.Trim().ToLowerInvariant() switch
        {
            "зажигалка" => "Зажигалка",
            "vip-билет" => "VIP-билет",
            "записка" => "Записка",
            "маска" => "Маска",
            "завязка" => "Завязка маски",
            "ставки" => "Порядок ставок",
            "пометка" => "Служебная пометка",
            "коридор" => "Закрытый коридор",
            _ => value.Trim()
        };
    }
}

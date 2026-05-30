using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Панель найденных улик
public class LastBetEvidencePanel : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private LastBetClueSlotView clueSlotPrefab;

    [Header("Debug")]
    [SerializeField] private bool debugVisualState = true;
    [SerializeField] private LastBetTooltip tooltip;

    public void Configure(Transform parent, LastBetClueSlotView prefab)
    {
        if (parent != null)
            contentParent = parent;

        if (prefab != null)
            clueSlotPrefab = prefab;
    }

    public void Clear()
    {
        if (contentParent == null)
        {
            Debug.LogWarning("[LastBet] EvidencePanel.Clear skipped: contentParent is null.");
            return;
        }

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        Debug.Log("[LastBet] EvidencePanel cleared.");
    }

    public void AddEvidence(LastBetCardData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[LastBet] AddEvidence skipped: data is null.");
            return;
        }

        if (data.IsJoker)
        {
            Debug.Log("[LastBet] AddEvidence skipped: joker has no evidence slot.");
            return;
        }

        if (contentParent == null)
        {
            Debug.LogError("[LastBet] AddEvidence failed: contentParent is null. Assign EvidenceTable.");
            return;
        }

        if (clueSlotPrefab == null)
        {
            Debug.LogError("[LastBet] AddEvidence failed: clueSlotPrefab is null.");
            return;
        }

        LastBetClueSlotView slot = Instantiate(clueSlotPrefab, contentParent);
        slot.gameObject.SetActive(true);
        slot.transform.SetAsLastSibling();
        slot.Setup(
            data.clueSprite,
            data.title,
            data.evidencePanelDescription,
            tooltip
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);

        Debug.Log(
            "[LastBet] Evidence added: " +
            $"storyClue={data.storyClue} | " +
            $"title={data.title} | " +
            $"descriptionEmpty={string.IsNullOrWhiteSpace(data.evidencePanelDescription)} | " +
            $"parent={contentParent.name} | " +
            $"children={contentParent.childCount}"
        );

        if (debugVisualState)
        {
            DebugEvidenceSlot(slot.gameObject, contentParent);
            DebugEvidenceChildren(slot.gameObject);
            DebugEvidenceParentChain(contentParent);
        }
    }

    private void DebugEvidenceSlot(GameObject slotObject, Transform parent)
    {
        RectTransform slotRect = slotObject.GetComponent<RectTransform>();
        CanvasGroup slotCanvasGroup = slotObject.GetComponent<CanvasGroup>();

        Debug.Log(
            "[LastBet] Evidence visual debug\n" +
            $"slot={slotObject.name}\n" +
            $"activeSelf={slotObject.activeSelf}, activeInHierarchy={slotObject.activeInHierarchy}\n" +
            $"parent={parent?.name}\n" +
            $"parentActive={parent?.gameObject.activeInHierarchy}\n" +
            $"slotSize={(slotRect != null ? slotRect.rect.size.ToString() : "no rect")}\n" +
            $"slotAnchoredPos={(slotRect != null ? slotRect.anchoredPosition.ToString() : "no rect")}\n" +
            $"slotScale={slotObject.transform.localScale}\n" +
            $"slotCanvasAlpha={(slotCanvasGroup != null ? slotCanvasGroup.alpha.ToString("0.###") : "no slot canvas group")}"
        );
    }

    private void DebugEvidenceChildren(GameObject slotObject)
    {
        Graphic[] graphics = slotObject.GetComponentsInChildren<Graphic>(true);
        TMP_Text[] texts = slotObject.GetComponentsInChildren<TMP_Text>(true);

        Debug.Log($"[LastBet] Evidence child graphics count={graphics.Length}, texts count={texts.Length}");

        foreach (Graphic graphic in graphics)
        {
            RectTransform rect = graphic.GetComponent<RectTransform>();

            Debug.Log(
                $"[LastBet] Evidence graphic: {graphic.name} | " +
                $"type={graphic.GetType().Name} | " +
                $"activeSelf={graphic.gameObject.activeSelf} | " +
                $"activeInHierarchy={graphic.gameObject.activeInHierarchy} | " +
                $"enabled={graphic.enabled} | " +
                $"color={graphic.color} | " +
                $"alpha={graphic.color.a:0.###} | " +
                $"size={(rect != null ? rect.rect.size.ToString() : "no rect")}"
            );
        }

        foreach (TMP_Text text in texts)
        {
            Debug.Log(
                $"[LastBet] Evidence text: {text.name} | " +
                $"activeSelf={text.gameObject.activeSelf} | " +
                $"activeInHierarchy={text.gameObject.activeInHierarchy} | " +
                $"enabled={text.enabled} | " +
                $"alpha={text.color.a:0.###} | " +
                $"value='{text.text}'"
            );
        }
    }

    private void DebugEvidenceParentChain(Transform start)
    {
        Transform current = start;

        while (current != null)
        {
            RectTransform rect = current.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = current.GetComponent<CanvasGroup>();
            Mask mask = current.GetComponent<Mask>();
            RectMask2D rectMask = current.GetComponent<RectMask2D>();
            LayoutGroup layoutGroup = current.GetComponent<LayoutGroup>();
            ContentSizeFitter fitter = current.GetComponent<ContentSizeFitter>();

            Debug.Log(
                $"[LastBet] Evidence parent chain: {current.name} | " +
                $"activeSelf={current.gameObject.activeSelf} | " +
                $"activeInHierarchy={current.gameObject.activeInHierarchy} | " +
                $"size={(rect != null ? rect.rect.size.ToString() : "no rect")} | " +
                $"alpha={(canvasGroup != null ? canvasGroup.alpha.ToString("0.###") : "no canvas group")} | " +
                $"mask={(mask != null)} | rectMask={(rectMask != null)} | " +
                $"layout={(layoutGroup != null ? layoutGroup.GetType().Name : "none")} | " +
                $"fitter={(fitter != null ? fitter.horizontalFit + "/" + fitter.verticalFit : "none")}"
            );

            current = current.parent;
        }
    }
}
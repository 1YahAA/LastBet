using UnityEngine;

public class HandFanLayout : MonoBehaviour
{
    [Header("Веер")]
    public float cardSpacing = 130f;
    public float maxRotation = 10f;
    public float arcHeight = 45f;

    [Header("Смещение всего веера")]
    public Vector2 fanOffset = Vector2.zero;

    public void Refresh()
    {
        int count = transform.childCount;

        if (count == 0)
            return;

        float totalWidth = (count - 1) * cardSpacing;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < count; i++)
        {
            RectTransform rt = transform.GetChild(i) as RectTransform;

            if (rt == null)
                continue;

            float x = startX + i * cardSpacing;

            float normalized = 0f;

            if (count > 1 && totalWidth > 0f)
                normalized = x / (totalWidth * 0.5f);

            float y = -Mathf.Abs(normalized) * arcHeight;
            float rotation = -normalized * maxRotation;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = fanOffset + new Vector2(x, y);

            rt.localRotation = Quaternion.Euler(0f, 0f, rotation);
            
            rt.localScale = Vector3.one;
        }
    }
}
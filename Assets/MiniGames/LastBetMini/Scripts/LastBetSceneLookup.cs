using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Временный автопоиск объектов сцены по именам
public static class LastBetSceneLookup
{
    public static GameObject FindObject(string name)
    {
        return GameObject.Find(name);
    }

    public static GameObject FindObjectIncludeInactive(string name)
    {
        GameObject direct = GameObject.Find(name);
        if (direct != null)
            return direct;

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform transform in transforms)
        {
            if (transform == null || transform.gameObject == null)
                continue;

            if (transform.gameObject.name == name && transform.gameObject.scene.IsValid())
                return transform.gameObject;
        }

        return null;
    }

    public static Transform FindTransform(string name)
    {
        GameObject go = FindObjectIncludeInactive(name);
        return go != null ? go.transform : null;
    }

    public static TMP_Text FindText(string name)
    {
        GameObject go = FindObjectIncludeInactive(name);
        return go != null ? go.GetComponent<TMP_Text>() : null;
    }

    public static Button FindButton(string name)
    {
        GameObject go = FindObjectIncludeInactive(name);
        return go != null ? go.GetComponent<Button>() : null;
    }
}

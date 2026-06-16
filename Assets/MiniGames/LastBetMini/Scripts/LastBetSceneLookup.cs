using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class LastBetSceneLookup
{
    public static GameObject FindObjectIncludeInactive(string name)
    {
        GameObject direct = GameObject.Find(name);
        if (direct != null)
            return direct;

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform item in transforms)
        {
            if (item == null || item.gameObject == null)
                continue;

            if (item.gameObject.name == name && item.gameObject.scene.IsValid())
                return item.gameObject;
        }

        return null;
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

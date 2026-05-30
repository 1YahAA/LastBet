using UnityEngine;

public class BlackMarkCardInstance
{
    public ClueType ClueType;
    public bool IsJoker;
    public Sprite FrontSprite;

    public BlackMarkCardInstance(ClueType clueType, bool isJoker, Sprite frontSprite)
    {
        ClueType = clueType;
        IsJoker = isJoker;
        FrontSprite = frontSprite;
    }
}

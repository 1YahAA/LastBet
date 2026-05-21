public enum ClueType
{
    BlackMark,
    Mask,
    Glass,
    Key,
    Joker
}

public enum BlackMarkState { Idle, ShowHint, Gameplay, JokerEvent, Win, Lose, Exit }
public enum JokerEffectType { Fog, Swap, FalseLead, Panic }
public enum BlackMarkStrategy { Revolt, Obedience, Analysis }
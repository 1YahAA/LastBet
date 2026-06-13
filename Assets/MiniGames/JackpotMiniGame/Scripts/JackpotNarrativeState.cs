using UnityEngine;

public sealed class JackpotNarrativeState : MonoBehaviour
{
    public bool SawHairpin { get; private set; }
    public bool SawDebt { get; private set; }
    public bool StoppedByPlayer { get; private set; }
    public bool WasForcedToStop { get; private set; }
    public int HairpinSpinIndex { get; private set; } = -1;
    public int DebtAppearances { get; private set; }

    public void ResetState()
    {
        SawHairpin = false;
        SawDebt = false;
        StoppedByPlayer = false;
        WasForcedToStop = false;
        HairpinSpinIndex = -1;
        DebtAppearances = 0;
    }

    public void RegisterSpin(JackpotSpinResult spinResult, int spinIndex)
    {
        if (spinResult == null)
            return;

        if (spinResult.HasHairpin)
        {
            SawHairpin = true;
            if (HairpinSpinIndex < 0)
                HairpinSpinIndex = spinIndex;
        }

        if (spinResult.HasDebt)
        {
            SawDebt = true;
            DebtAppearances++;
        }
    }

    public void RegisterPlayerStop()
    {
        StoppedByPlayer = true;
    }

    public void RegisterForcedStop()
    {
        WasForcedToStop = true;
    }
}

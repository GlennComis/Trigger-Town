using UnityEngine;

public class QTEManager : MonoBehaviour
{
    [SerializeField] private TimingQTE timingQTE;
    [SerializeField] private RevolverChamberQTE revolverChamberQTE;
    // Future QTEs:
    // [SerializeField] private MashingQTE mashingQTE;
    // [SerializeField] private HoldReleaseQTE holdReleaseQTE;

    public IQTE GetQTE(QTEType type)
    {
        return type switch
        {
            QTEType.Timing => timingQTE,
            QTEType.RevolverChamber => revolverChamberQTE,
            // QTEType.Mashing => mashingQTE,
            // QTEType.HoldRelease => holdReleaseQTE,
            _ => null,
        };
    }
}

public enum QTEResult
{
    Miss,
    Good,
    Perfect
}

public enum QTEType
{
    Timing,
    Mashing,
    HoldRelease,
    RevolverChamber
}

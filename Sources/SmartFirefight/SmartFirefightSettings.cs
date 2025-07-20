using Verse;

namespace SmartFirefight;

public class SmartFirefightSettings : ModSettings
{
    public bool ExtinguishFiresTouchingHomeArea = true;
    public int MaxFireDistance = FireTracker.DefaultMaxDistance;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref ExtinguishFiresTouchingHomeArea, nameof(ExtinguishFiresTouchingHomeArea), true);
        Scribe_Values.Look(ref MaxFireDistance, nameof(MaxFireDistance), FireTracker.DefaultMaxDistance);
    }
}
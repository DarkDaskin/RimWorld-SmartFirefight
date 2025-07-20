using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace SmartFirefight;

[UsedImplicitly]
public class SmartFirefightMod : Mod
{
    public SmartFirefightMod(ModContentPack content) : base(content)
    {
        GetSettings<SmartFirefightSettings>();
    }

    public override string SettingsCategory() => nameof(SettingsCategory).Translate();

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var settings = GetSettings<SmartFirefightSettings>();

        var listing = new Listing_Standard();
        listing.Begin(inRect);

        listing.CheckboxLabeled(nameof(settings.ExtinguishFiresTouchingHomeArea).Translate(), ref settings.ExtinguishFiresTouchingHomeArea,
            $"{nameof(settings.ExtinguishFiresTouchingHomeArea)}_Desc".Translate());
        if (settings.ExtinguishFiresTouchingHomeArea)
        {
            settings.MaxFireDistance = (int)listing.SliderLabeled($"{nameof(settings.MaxFireDistance).Translate()}: {settings.MaxFireDistance}", 
                settings.MaxFireDistance, 1, 10, tooltip: $"{nameof(settings.MaxFireDistance)}_Desc".Translate());
        }

        listing.End();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();

        var settings = GetSettings<SmartFirefightSettings>();

        FireTracker.Instance.IsEnabled = settings.ExtinguishFiresTouchingHomeArea;
        FireTracker.Instance.MaxDistance = settings.MaxFireDistance;
    }
}

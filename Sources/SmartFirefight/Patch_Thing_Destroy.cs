using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace SmartFirefight;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
[HarmonyPatch(typeof(Thing), nameof(Thing.Destroy))]
internal static class Patch_Thing_Destroy
{
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public static void Prefix(Thing __instance)
    {
        if (__instance is Fire fire)
            FireTracker.Instance.RemoveFire(fire);
    }
}
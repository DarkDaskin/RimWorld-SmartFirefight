using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace SmartFirefight;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
[HarmonyPatch(typeof(GenSpawn), nameof(GenSpawn.Spawn), [typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool), typeof(bool)])]
internal static class Patch_GenSpawn_Spawn
{
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public static void Postfix(Thing __result)
    {
        if (__result is Fire fire)
            FireTracker.Instance.AddFire(fire);
    }
}
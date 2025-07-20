using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace SmartFirefight;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
[HarmonyPatch(typeof(Designator_Cancel), nameof(Designator_Cancel.DesignateSingleCell))]
internal static class Patch_Designator_Cancel_DesignateSingleCell
{
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public static void Postfix(IntVec3 c, Designator __instance)
    {
        foreach (var fire in c.GetThingList(__instance.Map).OfType<Fire>())
            FireTracker.Instance.SetExtinguishDesignation(fire, false);
    }
}
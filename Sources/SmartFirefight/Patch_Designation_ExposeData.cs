using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace SmartFirefight;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
[HarmonyPatch(typeof(Designation), nameof(Designation.ExposeData))]
internal static class Patch_Designation_ExposeData
{
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public static void Postfix(Designation __instance)
    {
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;

        if (__instance.def != SmartFirefightDefs.ExtinguishFiresDesignationDef || 
            !__instance.target.HasThing || __instance.target.Thing is not Fire fire)
            return;


        FireTracker.Instance.SetExtinguishDesignation(fire, true);
    }
}
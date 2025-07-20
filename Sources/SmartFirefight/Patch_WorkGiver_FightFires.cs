using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace SmartFirefight;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
[HarmonyPatch("RimWorld.WorkGiver_FightFires", "HasJobOnThing")]
internal class Patch_WorkGiver_FightFires
{
    // ReSharper disable InconsistentNaming
    private static readonly MethodInfo? Thing_Map_get = typeof(Thing).GetProperty(nameof(Thing.Map))?.GetGetMethod();
    private static readonly FieldInfo? Map_areaManager = typeof(Map).GetField(nameof(Map.areaManager));
    private static readonly MethodInfo? AreaManager_Home_get = typeof(AreaManager).GetProperty(nameof(AreaManager.Home))?.GetGetMethod();
    private static readonly MethodInfo? Thing_Position_get = typeof(Thing).GetProperty(nameof(Thing.Position))?.GetGetMethod();
    private static readonly MethodInfo? Area_Item_get = typeof(Area).GetProperty("Item", [typeof(IntVec3)])?.GetGetMethod();
    // ReSharper restore InconsistentNaming

    private static readonly object?[] AllAccessedMembers = [Thing_Map_get, Map_areaManager, AreaManager_Home_get, Thing_Position_get, Area_Item_get];
    
    [UsedImplicitly]
    public static bool Prepare() => AllAccessedMembers.All(o => o != null);

    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return instructions.ReplaceInstructions([
            i => i.IsLdarg(1),
            i => i.Calls(Thing_Map_get),
            i => i.LoadsField(Map_areaManager),
            i => i.Calls(AreaManager_Home_get),
            i => i.IsLdloc(),
            i => i.Calls(Thing_Position_get),
            i => i.Calls(Area_Item_get),
        ], [
            CodeInstruction.LoadArgument(1),
            CodeInstruction.LoadArgument(2),
            CodeInstruction.Call(typeof(Patch_WorkGiver_FightFires), nameof(IsAtHomeAreaOrConnected)),
        ]);
    }

    private static bool IsAtHomeAreaOrConnected(Pawn pawn, Thing thing)
    {
        if (thing is not Fire fire)
            return false;

        if (pawn.Map.areaManager.Home[fire.Position])
            return true;

        return FireTracker.Instance.ShouldExtinguish(fire);
    }
}
using HarmonyLib;
using Verse;

namespace SmartFirefight;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        var harmony = new Harmony("SmartFirefight");
        harmony.PatchAll(typeof(Startup).Assembly);
    }
}
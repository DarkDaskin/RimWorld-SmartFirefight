using HarmonyLib;
using Verse;

namespace SmartFirefight;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        Harmony.DEBUG = true;
        var harmony = new Harmony("SmartFirefight");
        harmony.PatchAll(typeof(Startup).Assembly);
    }
}
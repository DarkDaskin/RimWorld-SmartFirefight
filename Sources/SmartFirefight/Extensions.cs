using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace SmartFirefight;

internal static class Extensions
{
    public static IEnumerable<CodeInstruction> ReplaceInstructions(this IEnumerable<CodeInstruction> instructions, 
        IReadOnlyList<Func<CodeInstruction, bool>> predicates, IReadOnlyList<CodeInstruction> replacements)
    {
        var cutInstrunctions = new List<CodeInstruction>();
        var cutLabels = new List<Label>();
        var labelsEmitted = false;

        foreach (var instruction in instructions)
        {
            if (cutInstrunctions.Count < predicates.Count)
            {
                if (predicates[cutInstrunctions.Count].Invoke(instruction))
                {
                    cutInstrunctions.Add(instruction);
                    cutLabels.AddRange(instruction.labels);
                    continue;
                }

                foreach (var cutInstrunction in cutInstrunctions)
                    yield return cutInstrunction;
            }
            else
            {
                foreach (var replacement in replacements)
                {
                    if (!labelsEmitted && cutLabels.Count > 0)
                    {
                        yield return replacement.Clone().WithLabels(cutLabels);
                        labelsEmitted = true;
                    }
                    else
                        yield return replacement;
                }
            }

            cutInstrunctions.Clear();
            cutLabels.Clear();
            labelsEmitted = false;

            yield return instruction;
        }
    }

    public static TaggedString TranslateNS(this string key) => $"{nameof(SmartFirefight)}.{key}".Translate();
}
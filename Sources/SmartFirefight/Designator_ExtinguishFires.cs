using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace SmartFirefight;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class Designator_ExtinguishFires : Designator
{
    public Designator_ExtinguishFires()
    {
        defaultLabel = "ExtinguishFires".Translate();
        defaultDesc = "ExtinguishFires_Desc".Translate();
        icon = ContentFinder<Texture2D>.Get("Designator_ExtinguishFires");
        useMouseIcon = true;
        soundDragSustain = SoundDefOf.Designate_DragStandard;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        soundSucceeded = SmartFirefightDefs.ExtinguishFiresSoundDef;
        soundFailed = SoundDefOf.Designate_Failed;
    }

    protected override DesignationDef Designation => SmartFirefightDefs.ExtinguishFiresDesignationDef;

    public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Orders;

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        if (!loc.InBounds(Map) || loc.Fogged(Map))
            return false;

        return Map.thingGrid.CellContains(loc, ThingDefOf.Fire);
    }

    public override AcceptanceReport CanDesignateThing(Thing t)
    {
        var acceptanceReport = base.CanDesignateThing(t);
        if (!acceptanceReport.Accepted)
            return acceptanceReport;

        if (t is not Fire fire)
            return false;

        if (Map.designationManager.DesignationOn(t, Designation) != null)
            return false;

        return true;
    }

    public override void DesignateSingleCell(IntVec3 c) => DesignateThing(Map.thingGrid.ThingAt<Fire>(c));

    public override void DesignateThing(Thing t)
    {
        if (t is not Fire fire)
            return;

        Map.designationManager.TryRemoveDesignationOn(t, Designation);
        Map.designationManager.AddDesignation(new Designation(t, Designation));

        FireTracker.Instance.SetExtinguishDesignation(fire, true);
    }
}
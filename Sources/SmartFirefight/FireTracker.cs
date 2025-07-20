using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace SmartFirefight;

public class FireTracker
{
    private const int CleanupIntervalTicks = 300;
    internal const int DefaultMaxDistance = 2;

    public static readonly FireTracker Instance = new();

    private readonly Dictionary<Map, List<FireGroup>> _fireGroups = [];
    private readonly HashSet<Fire> _firesDesignatedToExtinguishWhileLoading = [];
    private int _lastCleanupTick;
    private int _maxDistance;
    private int _maxDistancePlusOneSquared;

    public bool IsAutoExtinguishEnabled { get; set; } = true;

    public int MaxDistance
    {
        get => _maxDistance;
        set
        {
            _maxDistance = value;
            var maxDistancePlusOne = value + 1;
            _maxDistancePlusOneSquared = maxDistancePlusOne * maxDistancePlusOne;
        }
    }
    public FireTracker()
    {
        MaxDistance = DefaultMaxDistance;
    }

    public void AddFire(Fire fire)
    {
        var list = GetFireGropus(fire.Map);
        var groups = list.Where(fg => fg.IsNear(fire.Position)).ToArray();
        switch (groups.Length)
        {
            case 0:
                var newGroup = new FireGroup(this, fire.Map);
                AddFireToGroup(newGroup);
                list.Add(newGroup);
                break;
            case 1:
                AddFireToGroup(groups[0]);
                break;
            default:
                AddFireToGroup(groups[0]);

                for (var i = 1; i < groups.Length; i++)
                {
                    groups[0].UnionWith(groups[i]);
                    list.Remove(groups[i]);
                }

                break;
        }


        void AddFireToGroup(FireGroup group)
        {
            group.AddFire(fire.Position);
            var isDesignatedToExtinguish = _firesDesignatedToExtinguishWhileLoading.Remove(fire);
            if (isDesignatedToExtinguish)
                group.DesignateToExtinguish();
        }
    }

    public void RemoveFire(Fire fire)
    {
        var list = GetFireGropus(fire.Map);
        var group = list.FirstOrDefault(fg => fg.HasFire(fire.Position));
        if (group == null)
            Log.Warning($"Trying to remove untracked fire at {fire.Position} on {fire.Map}.");
        else
        {
            group.RemoveFire(fire.Position);

            if (group.IsEmpty)
                list.Remove(group);
        }
    }

    public void SetExtinguishDesignation(Fire fire, bool isDesignated)
    {
        if (fire.Map == null)
        {
            if (isDesignated)
                _firesDesignatedToExtinguishWhileLoading.Add(fire);

            return;
        }

        var list = GetFireGropus(fire.Map);
        var group = list.FirstOrDefault(fg => fg.HasFire(fire.Position));
        if (group == null)
            Log.Warning($"Trying to set designation on untracked fire at {fire.Position} on {fire.Map}.");
        else if (isDesignated)
            group.DesignateToExtinguish();
        else
            group.RemoveDesignationToExtinguish();
    }

    public bool ShouldExtinguish(Fire fire) => 
        IsAutoExtinguishEnabled && GetFireGropus(fire.Map).Any(fg => fg.HasOverlappedHomeArea && fg.HasFire(fire.Position)) ||
        GetFireGropus(fire.Map).Any(fg => fg.IsDesignatedToExtinguish && fg.HasFire(fire.Position));

    private ICollection<FireGroup> GetFireGropus(Map map)
    {
        var shouldCleanup = Find.TickManager.TicksGame - _lastCleanupTick > CleanupIntervalTicks;
        if (shouldCleanup)
        {
            var keysToRemove = _fireGroups.Keys.Where(key => key.Disposed).ToArray();
            foreach (var key in keysToRemove) 
                _fireGroups.Remove(key);

            _lastCleanupTick = Find.TickManager.TicksGame;
        }

        if (!_fireGroups.TryGetValue(map, out var list))
        {
            list = [];
            _fireGroups.Add(map, list);
        }

        return list;
    }


    private class FireGroup(FireTracker tracker, Map map)
    {
        private readonly HashSet<IntVec3> _positions = [];

        public bool IsEmpty => _positions.Count == 0;

        // If fire ever touched home area, the whole group should be extinguished.
        public bool HasOverlappedHomeArea { get; private set; }

        public bool IsDesignatedToExtinguish { get; private set; }

        public void AddFire(IntVec3 position)
        {
            if (!_positions.Add(position))
                return;

            if (map.areaManager.Home[position])
                HasOverlappedHomeArea = true;

            if (IsDesignatedToExtinguish)
                TryDesignateToExtinguish(position);
        }

        public void RemoveFire(IntVec3 position)
        {
            if (!_positions.Remove(position))
                return;

            if (IsDesignatedToExtinguish)
                TryRemoveDesignationToExtinguish(position);
        }

        public bool HasFire(IntVec3 position) => _positions.Contains(position);

        public bool IsNear(IntVec3 position) => _positions.Any(p => (p - position).LengthHorizontalSquared < tracker._maxDistancePlusOneSquared);

        public void UnionWith(FireGroup other)
        {
            _positions.UnionWith(other._positions);
            HasOverlappedHomeArea |= other.HasOverlappedHomeArea;

            if (other.IsDesignatedToExtinguish && !IsDesignatedToExtinguish)
                DesignateToExtinguish();
        }

        public void DesignateToExtinguish()
        {
            IsDesignatedToExtinguish = true;

            foreach (var position in _positions)
                TryDesignateToExtinguish(position);
        }

        public void RemoveDesignationToExtinguish()
        {
            IsDesignatedToExtinguish = false;

            foreach (var position in _positions)
                TryRemoveDesignationToExtinguish(position);
        }

        private void TryDesignateToExtinguish(IntVec3 position)
        {
            if (map.designationManager.DesignationOn(map.thingGrid.ThingAt<Fire>(position), SmartFirefightDefs.ExtinguishFiresDesignationDef) == null)
                    map.designationManager.AddDesignation(new Designation(
                        map.thingGrid.ThingAt<Fire>(position), SmartFirefightDefs.ExtinguishFiresDesignationDef));
        }
        private void TryRemoveDesignationToExtinguish(IntVec3 position) => 
            map.designationManager.TryRemoveDesignationOn(map.thingGrid.ThingAt<Fire>(position), SmartFirefightDefs.ExtinguishFiresDesignationDef);
    }
}
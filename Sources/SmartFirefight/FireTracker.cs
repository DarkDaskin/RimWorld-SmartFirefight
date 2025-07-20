using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace SmartFirefight;

public class FireTracker
{
    private const int CleanupIntervalTicks = 300;
    private const int DefaultMaxDistance = 2;

    public static readonly FireTracker Instance = new();

    private readonly Dictionary<Map, List<FireGroup>> _fireGroups = [];
    private int _lastCleanupTick;
    private int _maxDistance;
    private int _maxDistancePlusOneSquared;

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
                newGroup.AddFire(fire.Position);
                list.Add(newGroup);
                break;
            case 1:
                groups[0].AddFire(fire.Position);
                break;
            default:
                groups[0].AddFire(fire.Position);

                for (var i = 1; i < groups.Length; i++)
                {
                    groups[0].UnionWith(groups[i]);
                    list.Remove(groups[i]);
                }

                break;
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

    public bool ShouldExtinguish(Fire fire) => GetFireGropus(fire.Map).Any(fg => fg.HasOverlappedHomeArea && fg.HasFire(fire.Position));

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

        public void AddFire(IntVec3 position)
        {
            if (!_positions.Add(position))
                return;

            if (map.areaManager.Home[position])
                HasOverlappedHomeArea = true;
        }

        public void RemoveFire(IntVec3 position)
        {
            if (!_positions.Remove(position))
                return;

            //if (map.areaManager.Home[position])
            //    _homeAreaOverlaps--;
        }

        public bool HasFire(IntVec3 position) => _positions.Contains(position);
        

        public bool IsNear(IntVec3 position) => _positions.Any(p => (p - position).LengthHorizontalSquared < tracker._maxDistancePlusOneSquared);

        public void UnionWith(FireGroup other)
        {
            _positions.UnionWith(other._positions);
            HasOverlappedHomeArea |= other.HasOverlappedHomeArea;
        }
    }
}
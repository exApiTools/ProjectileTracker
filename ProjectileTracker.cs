using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Helpers;
using SharpDX;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace ProjectileTracker;

public class ProjectileTracker : BaseSettingsPlugin<ProjectileTrackerSettings>
{
    private readonly HashSet<Entity> _trackedEntities = new HashSet<Entity>();
    private readonly Stopwatch _refreshSw = Stopwatch.StartNew();

    public override bool Initialise()
    {
        return true;
    }

    public override void AreaChange(AreaInstance area)
    {
        _trackedEntities.Clear();
    }

    public override Job Tick()
    {
        if (_refreshSw.Elapsed > TimeSpan.FromMilliseconds(Settings.FullRefreshPeriod))
        {
            _refreshSw.Restart();
            _trackedEntities.Clear();
            var entitiesToAdd = GameController.EntityListWrapper.Entities.Where(IsMatchingEntity).ToList();
            foreach (var entity in entitiesToAdd)
            {
                _trackedEntities.Add(entity);
            }
        }

        return null;
    }

    public override void Render()
    {
        var playerPos = GameController.Player.GridPosNum;
        foreach (var trackedEntity in _trackedEntities.Where(x => x.IsValid))
        {
            var entityPos = trackedEntity.GridPosNum;
            var targetPos = trackedEntity.GetComponent<Positioned>().TravelTarget.WorldToGrid();
            var xy = entityPos - targetPos;
            var xz = entityPos - playerPos;
            var xylsq = xy.LengthSquared();
            float minDistance;
            if (xylsq == 0)
            {
                minDistance = 0;
            }
            else
            {
                var t = Math.Clamp(Vector2.Dot(xy, xz) / xylsq, 0, 1);
                minDistance = (Vector2.Lerp(entityPos, targetPos, t) - playerPos).Length();
            }

            var color = minDistance <= Settings.MinDistance
                ? Settings.MaxDangerLineColor.Value.ToImguiVec4()
                : Vector4.Lerp(Settings.MediumDangerLineColor.Value.ToImguiVec4(), Settings.LeastDangerLineColor.Value.ToImguiVec4(),
                    Math.Clamp((minDistance - Settings.MinDistance) / (Settings.MaxDistance - Settings.MinDistance), 0, 1));
            var entityScreenPos = GameController.IngameState.Data.GetGridScreenPosition(entityPos);
            var targetScreenPos = GameController.IngameState.Data.GetGridScreenPosition(targetPos);
            Graphics.DrawLine(entityScreenPos, targetScreenPos, 10, color.ToSharpColor());
        }
    }

    public override void EntityAdded(Entity entity)
    {
        if (IsMatchingEntity(entity))
        {
            _trackedEntities.Add(entity);
        }
    }

    private bool IsMatchingEntity(Entity entity)
    {
        return entity.HasComponent<Positioned>() && Settings.EntityPatterns.Select(x => new Regex(x)).Any(x => x.IsMatch(entity.Path));
    }
}
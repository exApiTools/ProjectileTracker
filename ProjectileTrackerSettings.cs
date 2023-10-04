using System.Collections.Generic;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ImGuiNET;
using Newtonsoft.Json;
using SharpDX;

namespace ProjectileTracker;

public class ProjectileTrackerSettings : ISettings
{
    public ProjectileTrackerSettings()
    {
        PatternEditor = new CustomNode(() =>
        {
            for (int i = 0; i < EntityPatterns.Count; i++)
            {
                var str = EntityPatterns[i];
                if (ImGui.InputText($"##{i}", ref str, 2000))
                {
                    EntityPatterns[i] = str;
                }

                ImGui.SameLine();
                if (ImGui.Button("X"))
                {
                    EntityPatterns.RemoveAt(i);
                }
            }

            if (ImGui.Button("+"))
            {
                EntityPatterns.Add("^$");
            }
        });
    }

    public ToggleNode Enable { get; set; } = new ToggleNode(false);
    public ColorNode MaxDangerLineColor { get; set; } = Color.Red;
    public ColorNode MediumDangerLineColor { get; set; } = Color.DarkGoldenrod;
    public ColorNode LeastDangerLineColor { get; set; } = Color.DarkGoldenrod with { A = 128 };

    public RangeNode<float> MinDistance { get; set; } = new RangeNode<float>(10, 0, 100);
    public RangeNode<float> MaxDistance { get; set; } = new RangeNode<float>(40, 0, 100);
    public RangeNode<int> FullRefreshPeriod { get; set; } = new RangeNode<int>(250, 1, 1000);

    [JsonIgnore]
    public CustomNode PatternEditor { get; set; }

    public List<string> EntityPatterns = new List<string>();
}
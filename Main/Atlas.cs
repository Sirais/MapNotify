using ExileCore;
using ImGuiNET;
using System.Numerics;

namespace MapNotify
{
    public partial class MapNotify
    {
        public void AtlasRender()
        {
            if (!Settings.TargetRegions)
            {
                return;
            }

            ImGui.SetNextWindowPos(new Vector2(Settings.AtlasX, Settings.AtlasY), ImGuiCond.Once, Vector2.Zero);
            ImGui.Begin("TargetedRegions",
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoFocusOnAppearing |
                ImGuiWindowFlags.AlwaysAutoResize |
                ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.NoCollapse);
            var pos = ImGui.GetWindowPos();
            Settings.AtlasX = (int)pos.X;
            Settings.AtlasY = (int)pos.Y;
            ImGui.Text("Targeted Regions");
            ImGui.Separator();
            ImGui.Checkbox("Haewark Hamlet", ref Settings.TargetingHaewarkHamlet);
            ImGui.Checkbox("Valdo's Rest", ref Settings.TargetingValdosRest);
            ImGui.Checkbox("Glennach Cairns", ref Settings.TargetingGlennachCairns);
            ImGui.Checkbox("Lira Arthain", ref Settings.TargetingLiraArthain);
            ImGui.End();
        }
        public bool CheckRegionTarget(string region)
        {
            switch (region)
            {
                case "Haewark Hamlet":
                    return Settings.TargetingHaewarkHamlet;
                case "Valdo's Rest":
                    return Settings.TargetingValdosRest;
                case "Glennach Cairns":
                    return Settings.TargetingGlennachCairns;
                case "Lira Arthain":
                    return Settings.TargetingLiraArthain;
                default:
                    return false;
            }
        }
    }
}

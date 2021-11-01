﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.Shared.Nodes;
using ImGuiNET;
using nuVector4 = System.Numerics.Vector4;

namespace MapNotify
{
    partial class MapNotify
    {
        private bool _Debug;
        private bool Maven;
        private bool _Comp;
        public static List<string> HoverMods = new List<string>();

        public static void HelpMarker(string desc)
        {
            ImGui.TextDisabled("(?)");
            if (!ImGui.IsItemHovered())
            {
                return;
            }

            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }

        public static int IntSlider(string labelString, RangeNode<int> setting)
        {
            var refValue = setting.Value;
            ImGui.SliderInt(labelString, ref refValue, setting.Min, setting.Max);
            return refValue;
        }

        public static nuVector4 ColorButton(string labelString, nuVector4 setting)
        {
            var refValue = setting;
            ImGui.ColorEdit4(labelString, ref refValue);
            return refValue;
        }

        public static bool Checkbox(string labelString, bool boolValue)
        {
            ImGui.Checkbox(labelString, ref boolValue);
            return boolValue;
        }

        public static void DebugHover()
        {
            var uiHover = _ingameState.UIHover;
            if (uiHover == null || !uiHover.IsVisible)
            {
                return;
            }

            var inventoryItemIcon = uiHover?.AsObject<NormalInventoryItem>();
            if (inventoryItemIcon == null)
            {
                return;
            }

            var tooltip = inventoryItemIcon?.Tooltip;
            var entity = inventoryItemIcon?.Item;

            if (tooltip != null && entity.Address != 0 && entity.IsValid)
            {
                var modsComponent = entity.GetComponent<Mods>();
                if (modsComponent == null)
                {
                    HoverMods.Clear();
                }
                else if (modsComponent.ItemMods.Any())
                {
                    HoverMods.Clear();

                    var itemMods = modsComponent?.ItemMods ?? null;
                    if (itemMods == null || itemMods.Count == 0)
                    {
                        HoverMods.Clear();
                        return;
                    }

                    foreach (var mod in itemMods)
                    {
                        if (!HoverMods.Contains(
                                $"{mod.RawName} : {mod.Value1}, {mod.Value2}, {mod.Value3}, {mod.Value4}"))
                        {
                            HoverMods.Add($"{mod.RawName} : {mod.Value1}, {mod.Value2}, {mod.Value3}, {mod.Value4}");
                        }
                    }
                }
            }
            else
            {
                HoverMods.Clear();
            }
        }

        public override void DrawSettings()
        {
            ImGui.Text("Plugin by Lachrymatory. https://github.com/Lachrymatory/");
            if (ImGui.Button("Lachrymatory's GitHub"))
            {
                Process.Start("https://github.com/Lachrymatory/");
            }

            ImGui.Separator();


            if (ImGui.TreeNodeEx("Core Settings", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                Settings.AlwaysShowTooltip.Value =
                    Checkbox("Show Tooltip Even Without Warnings", Settings.AlwaysShowTooltip);
                ImGui.SameLine();
                HelpMarker(
                    "This will show a tooltip even if there are no mods to warn you about on the map.\nThis means you will always be able to see tier, completion, quantity, mod count, etc.");
                Settings.HorizontalLines.Value = Checkbox("Show Horizontal Lines", Settings.HorizontalLines);
                ImGui.SameLine();
                HelpMarker("Add a Horizontal Line above actual mod information.");
                Settings.ShowForZanaMaps.Value = Checkbox("Display for Zana Missions", Settings.ShowForZanaMaps);
                Settings.ShowLineForZanaMaps.Value = Checkbox("Display Horizontal Line in Zana Missions Info",
                    Settings.ShowLineForZanaMaps);
                Settings.ShowForWatchstones.Value = Checkbox("Display for Watchstones", Settings.ShowForWatchstones);
                Settings.ShowForHeist.Value = Checkbox("Display for Contracts and Blueprints", Settings.ShowForHeist);
                Settings.ShowForInvitations.Value =
                    Checkbox("Display for Maven Invitations", Settings.ShowForInvitations);
                Settings.AlwaysShowCompletionBorder.Value = Checkbox("Style tooltip border on incomplete maps",
                    Settings.AlwaysShowCompletionBorder);
                Settings.BoxForBricked.Value = Checkbox("Border on bricked maps in inventory", Settings.BoxForBricked);
                ImGui.SameLine();
                HelpMarker("Add ';true' after a line in the config files to mark it as a bricked mod.");
            }

            if (ImGui.TreeNodeEx("Map Tooltip Settings", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                Settings.ShowMapName.Value = Checkbox("Show Map Name", Settings.ShowMapName);
                Settings.ShowCompletion.Value = Checkbox("Show Completion Status", Settings.ShowCompletion);
                if (Settings.ShowCompletion)
                {
                    Settings.ShowMapName.Value = true;
                }

                ImGui.SameLine();
                HelpMarker(
                    "Requires map names.\nDisplays a red letter for each missing completion.\nA for Awakened Completion\nB for Bonus Completion\nC for Completion.");
                Settings.ShowMapRegion.Value = Checkbox("Show Region Name", Settings.ShowMapRegion);
                Settings.TargetRegions.Value = Checkbox("Enable Region Targetting ", Settings.TargetRegions);
                ImGui.SameLine();
                HelpMarker("Open the Atlas and tick the regions you want to highlight. Requires Show Region Name.");
                if (Settings.TargetRegions)
                {
                    Settings.ShowMapRegion.Value = true;
                }

                Settings.ShowModWarnings.Value = Checkbox("Show Mod Warnings", Settings.ShowModWarnings);
                ImGui.SameLine();
                HelpMarker("Configured in 'ModWarnings.txt' in the plugin folder, created if missing.");
                Settings.ShowModCount.Value = Checkbox("Show Number of Mods on Map", Settings.ShowModCount);
                Settings.ShowPackSizePercent.Value = Checkbox("Show Pack Size %", Settings.ShowPackSizePercent);
                Settings.ShowQuantityPercent.Value = Checkbox("Show Item Quantity %", Settings.ShowQuantityPercent);
                Settings.ColorQuantityPercent.Value =
                    Checkbox("Warn Below Quantity Percentage", Settings.ColorQuantityPercent);
                Settings.ColorQuantity.Value = IntSlider("##ColorQuantity", Settings.ColorQuantity);
                ImGui.SameLine();
                HelpMarker("The colour of the quantity text will be red below this amount and green above it.");
                Settings.NonUnchartedList.Value = Checkbox("Display Maven Boss List for non-uncharted regions",
                    Settings.NonUnchartedList);
                ImGui.SameLine();
                HelpMarker(
                    "This will show (up to) all 10 bosses you have slain in a normal region as a full list.\nDisplays a count for normal regions otherwise.");
            }

            if (ImGui.TreeNodeEx("Borders and Colours", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                Settings.BorderThickness.Value =
                    IntSlider("Border Thickness##BorderThickness", Settings.BorderThickness);
                Settings.BorderThickness.Value = IntSlider("Completion Border Thickness##BorderThickness",
                    Settings.BorderThickness);

                Settings.DefaultBorderTextColor =
                    ColorButton("Text colour for maps with borders", Settings.DefaultBorderTextColor);
                Settings.StyleTextForBorder.Value =
                    Checkbox("Use border colour for text colour", Settings.StyleTextForBorder);
                ImGui.SameLine();
                HelpMarker("i.e. if you have Harvest in green, 'Harvest' will be written in green in the tooltip.");

                Settings.ElderGuardianBorder.Value = Checkbox("##elder", Settings.ElderGuardianBorder);
                ImGui.SameLine();
                Settings.ElderGuardian = ColorButton("Elder Guardian", Settings.ElderGuardian);

                Settings.ShaperGuardianBorder.Value = Checkbox("##shaper", Settings.ShaperGuardianBorder);
                ImGui.SameLine();
                Settings.ShaperGuardian = ColorButton("Shaper Guardian", Settings.ShaperGuardian);

                Settings.HarvestBorder.Value = Checkbox("##harvest", Settings.HarvestBorder);
                ImGui.SameLine();
                Settings.Harvest = ColorButton("Harvest", Settings.Harvest);

                Settings.DeliriumBorder.Value = Checkbox("##delirium", Settings.DeliriumBorder);
                ImGui.SameLine();
                Settings.Delirium = ColorButton("Delirium", Settings.Delirium);

                Settings.BlightedBorder.Value = Checkbox("##blighted", Settings.BlightedBorder);
                ImGui.SameLine();
                Settings.Blighted = ColorButton("Blighted Map", Settings.Blighted);

                Settings.BlightEncounterBorder.Value = Checkbox("##blightenc", Settings.BlightEncounterBorder);
                ImGui.SameLine();
                Settings.BlightEncounter = ColorButton("Blight in normal map", Settings.BlightEncounter);

                Settings.MetamorphBorder.Value = Checkbox("##metamorph", Settings.MetamorphBorder);
                ImGui.SameLine();
                Settings.Metamorph = ColorButton("Metamorph", Settings.Metamorph);

                Settings.LegionBorder.Value = Checkbox("##legion", Settings.LegionBorder);
                ImGui.SameLine();
                Settings.Legion = ColorButton("Legion Monolith", Settings.Legion);

                Settings.CompletionBorder.Value = Checkbox("Show borders for lack of completion##completion",
                    Settings.CompletionBorder);
                Settings.Incomplete = ColorButton("Incomplete", Settings.Incomplete);
                Settings.BonusIncomplete = ColorButton("Bonus Incomplete", Settings.BonusIncomplete);
                Settings.AwakenedIncomplete = ColorButton("Awakened Incomplete", Settings.AwakenedIncomplete);

                Settings.Bricked = ColorButton("Bricked Map", Settings.Bricked);
            }

            if (!ImGui.TreeNodeEx("Config Files and Other", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                return;
            }

            if (ImGui.Button("Reload Warnings Text Files"))
            {
                WarningDictionary = LoadConfigs();
            }

            if (ImGui.Button("Recreate Default Warnings Text Files"))
            {
                ResetConfigs();
            }

            ImGui.SameLine();
            HelpMarker("This will irreversibly delete all your existing warnings config files!");
            Settings.PadForNinjaPricer.Value = Checkbox("Pad for Ninja Pricer", Settings.PadForNinjaPricer);
            ImGui.SameLine();
            HelpMarker(
                "This will move the tooltip down vertically to allow room for the Ninja Pricer tooltip to be rendered. Only needed with that plugin active.");
            Settings.PadForAltPricer.Value = Checkbox("Pad for Personal Pricer", Settings.PadForAltPricer);
            ImGui.SameLine();
            HelpMarker("It's unlikely you'll need this.");
            ImGui.Spacing();

            _Debug = Checkbox("Debug Features", _Debug);
            ImGui.SameLine();
            HelpMarker(
                "Show mod names for quickly adding them to your ModWarnings.txt\nYou only need the start of a mod to match it, for example: 'MapBloodlinesModOnMagicsMapWorlds' would be matched with:\nMapBloodlines;Bloodlines;FF7F00FF");
            if (!_Debug)
            {
                return;
            }

            Maven = Checkbox("Maven Debug", Maven);
            if (Maven)
            {
                ImGui.Text("Maven Witnessed:");
                foreach (var map in MavenAreas)
                {
                    ImGui.TextColored(new nuVector4(0.5F, 0.5F, 1.2F, 1F), $"{map.Name}");
                }

                ImGui.Text("Maven Regions:");
                foreach (var region in MavenDict)
                {
                    ImGui.TextColored(new nuVector4(0.5F, 0.5F, 1.2F, 1F), $"{region.Key}");
                    ImGui.SameLine();
                    ImGui.TextColored(new nuVector4(1.2F, 0.5F, 0.5F, 1F), $"{region.Value}");
                }
            }

            _Comp = Checkbox("Completion Debug", _Comp);

            if (_Comp)
            {
                ImGui.Text($"Bonus ({BonusAreas.Count}): ");
                foreach (var map in BonusAreas)
                {
                    ImGui.TextColored(new nuVector4(0.5F, 0.5F, 1.2F, 1F), $"{map.Name}");
                }

                ImGui.Text($"Completion ({CompletedAreas.Count}): ");
                foreach (var map in CompletedAreas)
                {
                    ImGui.TextColored(new nuVector4(0.5F, 0.5F, 1.2F, 1F), $"{map.Name}");
                }
            }

            DebugHover();
            ImGui.Text("Last Hovered item's mods:");
            if (HoverMods.Count > 0)
            {
                foreach (var mod in HoverMods)
                {
                    ImGui.TextColored(new nuVector4(0.5F, 0.5F, 1.2F, 1F), mod);
                }
            }
        }
    }
}
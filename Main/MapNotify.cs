using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ImGuiNET;
using SharpDX;
using Map = ExileCore.PoEMemory.Components.Map;
using nuVector2 = System.Numerics.Vector2;
using nuVector4 = System.Numerics.Vector4;

namespace MapNotify
{
    public partial class MapNotify : BaseSettingsPlugin<MapNotifySettings>
    {
        private RectangleF _WindowArea;
        private static GameController _gameController;
        private static IngameState _ingameState;
        public static Dictionary<string, StyledText> WarningDictionary;

        public override bool Initialise()
        {
            base.Initialise();
            Name = "Map Mod Notifications";
            _WindowArea = GameController.Window.GetWindowRectangle();
            WarningDictionary = LoadConfigs();
            _gameController = GameController;
            _ingameState = _gameController.IngameState;
            BuildRegions();
            return true;
        }

        public static nuVector2 BoxSize;
        public static float MaxSize;
        public static float RowSize;
        public static int LastCol;

        public nuVector4? GetObjectiveColor(ObjectiveType rarity)
        {
            switch (rarity)
            {
                case ObjectiveType.None:
                    return null;
                case ObjectiveType.ElderGuardian:
                    return Settings.ElderGuardianBorder ? Settings.ElderGuardian : (nuVector4?)null;
                case ObjectiveType.ShaperGuardian:
                    return Settings.ShaperGuardianBorder ? Settings.ShaperGuardian : (nuVector4?)null;
                case ObjectiveType.Harvest:
                    return Settings.HarvestBorder ? Settings.Harvest : (nuVector4?)null;
                case ObjectiveType.Delirium:
                    return Settings.DeliriumBorder ? Settings.Delirium : (nuVector4?)null;
                case ObjectiveType.Blighted:
                    return Settings.BlightedBorder ? Settings.Blighted : (nuVector4?)null;
                case ObjectiveType.Metamorph:
                    return Settings.MetamorphBorder ? Settings.Metamorph : (nuVector4?)null;
                case ObjectiveType.Legion:
                    return Settings.LegionBorder ? Settings.Legion : (nuVector4?)null;
                case ObjectiveType.BlightEncounter:
                    return Settings.BlightEncounterBorder ? Settings.BlightEncounter : (nuVector4?)null;
                default:
                    return null;
            }
        }

        public void RenderItem(NormalInventoryItem item, Entity entity, bool isZanaMissionInventory = false,
            int missionIndex = 0)
        {
            if (entity.Address == 0 || !entity.IsValid)
            {
                return;
            }

            var baseType = _gameController.Files.BaseItemTypes.Translate(entity.Path);
            var classId = baseType.ClassName ?? string.Empty;

            if (!entity.HasComponent<Map>() && !classId.Equals(string.Empty) &&
                !entity.Path.Contains("BreachFragment") && !entity.Path.Contains("CurrencyElderFragment") &&
                !entity.Path.Contains("ShaperFragment") && !entity.Path.Contains("VaalFragment2_") &&
                !classId.Contains("HeistContract") && !classId.Contains("HeistBlueprint") &&
                !classId.Contains("AtlasRegionUpgradeItem") && !entity.Path.Contains("MavenMap") ||
                (classId.Contains("HeistContract") || classId.Contains("HeistBlueprint")) &&
                entity.GetComponent<Mods>()?.ItemRarity == ItemRarity.Normal)
            {
                return;
            }

            if (!Settings.ShowForHeist && (classId.Contains("HeistContract") || classId.Contains("HeistBlueprint")))
            {
                return;
            }

            if (!Settings.ShowForWatchstones && classId.Contains("AtlasRegionUpgradeItem"))
            {
                return;
            }

            if (!Settings.ShowForInvitations && (classId.Contains("MavenMap") || classId.Contains("MiscMapItem")))
            {
                return;
            }

            var itemDetails = entity.GetHudComponent<ItemDetails>();
            if (itemDetails == null)
            {
                itemDetails = new ItemDetails(item, entity);
                entity.SetHudComponent(itemDetails);
            }

            if (!Settings.AlwaysShowTooltip && itemDetails.ActiveWarnings.Count <= 0)
            {
                return;
            }

            if ((classId.Contains("AtlasRegionUpgradeItem") || classId.Contains("HeistContract") ||
                 classId.Contains("HeistBlueprint")) && itemDetails.ActiveWarnings.Count == 0)
            {
                return;
            }


            nuVector2 boxOrigin;
            if (Settings.PadForAltPricer && itemDetails.NeedsPadding)
            {
                boxOrigin = new nuVector2(MouseLite.GetCursorPositionVector().X + 24,
                    MouseLite.GetCursorPositionVector().Y + 30);
            }
            else if (Settings.PadForNinjaPricer && itemDetails.NeedsPadding)
            {
                boxOrigin = new nuVector2(MouseLite.GetCursorPositionVector().X + 24,
                    MouseLite.GetCursorPositionVector().Y + 56);
            }
            else
            {
                boxOrigin = new nuVector2(MouseLite.GetCursorPositionVector().X + 24,
                    MouseLite.GetCursorPositionVector().Y);
            }

            if (isZanaMissionInventory)
            {
                if (missionIndex < LastCol)
                {
                    BoxSize = nuVector2.Zero;
                    RowSize += MaxSize + 2;
                    MaxSize = 0;
                }

                var framePos = _ingameState.UIHover.Parent.GetClientRect().TopRight;
                framePos.X += 10 + BoxSize.X;
                framePos.Y -= 200;
                boxOrigin = new nuVector2(framePos.X, framePos.Y + RowSize);
            }

            ImGui.PushStyleColor(ImGuiCol.WindowBg, 0xFF3F3F3F);

            var opened = true;
            if (ImGui.Begin($"{entity.Address}", ref opened,
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoNavInputs))
            {
                ImGui.BeginGroup();
                if (!classId.Contains("HeistContract") && !classId.Contains("MapFragment") &&
                    !classId.Contains("HeistBlueprint") && !classId.Contains("AtlasRegionUpgradeItem") &&
                    !classId.Contains("QuestItem") && !classId.Contains("MiscMapItem"))
                {
                    if (isZanaMissionInventory || Settings.ShowMapName)
                    {
                        if (itemDetails.LacksCompletion || !Settings.ShowCompletion)
                        {
                            ImGui.TextColored(itemDetails.ItemColor, $"{itemDetails.MapName}");
                        }
                        else
                        {
                            ImGui.TextColored(itemDetails.ItemColor, $"{itemDetails.MapName}");
                            if (!itemDetails.Completed)
                            {
                                ImGui.TextColored(new nuVector4(1f, 0f, 0f, 1f), "C");
                                ImGui.SameLine();
                                ImGui.TextColored(new nuVector4(1f, 0f, 0f, 1f), "B");
                                ImGui.SameLine();
                                ImGui.TextColored(new nuVector4(1f, 0f, 0f, 1f), "A");
                            }
                            else
                            {
                                if (!itemDetails.Bonus)
                                {
                                    ImGui.TextColored(new nuVector4(1f, 0f, 0f, 1f), "B");
                                }

                                if (!itemDetails.Awakened)
                                {
                                    if (!itemDetails.Bonus)
                                    {
                                        ImGui.SameLine();
                                    }

                                    ImGui.TextColored(new nuVector4(1f, 0f, 0f, 1f), "A");
                                }
                            }

                            if (itemDetails.MavenDetails.MavenCompletion)
                            {
                                ImGui.TextColored(new nuVector4(0.9f, 0f, 0.77f, 1f), "Witnessed");
                            }

                            if (itemDetails.MavenDetails.MavenUncharted)
                            {
                                ImGui.TextColored(new nuVector4(0.0f, 0.9f, 0.77f, 1f), "Uncharted");
                            }

                            ImGui.PushStyleColor(ImGuiCol.Separator, new nuVector4(1f, 1f, 1f, 0.2f));
                        }

                        if (Settings.ShowMapRegion)
                        {
                            var regionColor = Settings.TargetRegions && CheckRegionTarget(itemDetails.MapRegion)
                                ? new nuVector4(1f, 1f, 1f, 1f)
                                : new nuVector4(1f, 0f, 1f, 1f);
                            ImGui.TextColored(regionColor, $"{itemDetails.MapRegion}");
                        }
                    }
                }

                if (classId.Contains("QuestItem") || classId.Contains("MiscMapItem") || classId.Contains("MapFragment"))
                {
                    ImGui.TextColored(new nuVector4(0.9f, 0f, 0.77f, 1f), $"{itemDetails.MapName}");
                    if (!Settings.NonUnchartedList && !entity.Path.Contains("MavenMapVoid") &&
                        !entity.Path.Contains("MapFragment"))
                    {
                        ImGui.TextColored(new nuVector4(0f, 1f, 0f, 1f),
                            $"{itemDetails.MavenDetails.MavenBosses.Count} Bosses Witnessed");
                    }
                    else
                    {
                        foreach (var (boss, complete) in itemDetails.MavenDetails.MavenBosses)
                        {
                            ImGui.TextColored(
                                complete ? new nuVector4(0f, 1f, 0f, 1f) : new nuVector4(1f, 0.8f, 0.8f, 1f),
                                $"{boss}");
                        }
                    }
                }
                else if (itemDetails.MavenDetails.MavenRegion != string.Empty && Input.GetKeyState(Keys.Menu))
                {
                    foreach (var (boss, complete) in itemDetails.MavenDetails.MavenBosses)
                    {
                        ImGui.TextColored(complete ? new nuVector4(0f, 1f, 0f, 1f) : new nuVector4(1f, 0.8f, 0.8f, 1f),
                            $"{boss}");
                    }
                }

                if (isZanaMissionInventory)
                {
                    var bCol = GetObjectiveColor(itemDetails.ZanaMissionType);
                    if (bCol.HasValue)
                    {
                        if (Settings.StyleTextForBorder)
                        {
                            ImGui.TextColored(bCol.Value, $"{itemDetails.ZanaMod?.Text ?? "Zana Mod was null!"}");
                        }
                        else
                        {
                            ImGui.TextColored(Settings.DefaultBorderTextColor,
                                $"{itemDetails.ZanaMod?.Text ?? "Zana Mod was null!"}");
                        }
                    }
                    else
                    {
                        ImGui.TextColored(new nuVector4(0.9f, 0.85f, 0.65f, 1f),
                            $"{itemDetails.ZanaMod?.Text ?? "Zana Mod was null!"}");
                    }
                }

                if (!classId.Contains("HeistContract") && !classId.Contains("HeistBlueprint") &&
                    !classId.Contains("AtlasRegionUpgradeItem"))
                {
                    var qCol = Settings.ColorQuantityPercent
                        ? itemDetails.Quantity < Settings.ColorQuantity ? new nuVector4(1f, 0.4f, 0.4f, 1f) :
                        new nuVector4(0.4f, 1f, 0.4f, 1f)
                        : new nuVector4(1f, 1f, 1f, 1f);

                    if (Settings.ShowQuantityPercent && itemDetails.Quantity != 0 && Settings.ShowPackSizePercent &&
                        itemDetails.PackSize != 0)
                    {
                        ImGui.TextColored(qCol, $"{itemDetails.Quantity}%% Quant");
                        ImGui.SameLine();
                        ImGui.TextColored(new nuVector4(1f, 1f, 1f, 1f), $"{itemDetails.PackSize}%% Pack Size");
                    }
                    else if (Settings.ShowQuantityPercent && itemDetails.Quantity != 0)
                    {
                        ImGui.TextColored(qCol, $"{itemDetails.Quantity}%% Quantity");
                    }
                    else if (Settings.ShowPackSizePercent && itemDetails.PackSize != 0)
                    {
                        ImGui.TextColored(new nuVector4(1f, 1f, 1f, 1f), $"{itemDetails.PackSize}%% Pack Size");
                    }

                    if (Settings.HorizontalLines && itemDetails.ActiveWarnings.Count > 0 &&
                        (Settings.ShowModCount || Settings.ShowModWarnings))
                    {
                        if (Settings.ShowLineForZanaMaps && isZanaMissionInventory || !isZanaMissionInventory)
                        {
                            ImGui.Separator();
                        }
                    }
                }

                if (Settings.ShowModCount && itemDetails.ModCount != 0 && !classId.Contains("AtlasRegionUpgradeItem"))
                {
                    if (entity.GetComponent<Base>().isCorrupted)
                    {
                        ImGui.TextColored(new nuVector4(1f, 0f, 0f, 1f),
                            $"{(isZanaMissionInventory ? itemDetails.ModCount - 1 : itemDetails.ModCount)} Mods, Corrupted");
                    }
                    else
                    {
                        ImGui.TextColored(new nuVector4(1f, 1f, 1f, 1f),
                            $"{(isZanaMissionInventory ? itemDetails.ModCount - 1 : itemDetails.ModCount)} Mods");
                    }
                }


                if (Settings.ShowModWarnings)
                {
                    foreach (var styledText in itemDetails.ActiveWarnings.OrderBy(x => x.Color.ToString()).ToList())
                    {
                        ImGui.TextColored(SharpToNu(styledText.Color), styledText.Text);
                    }
                }

                ImGui.EndGroup();

                if (itemDetails.Bricked || entity.HasComponent<Map>() &&
                    (isZanaMissionInventory || Settings.AlwaysShowCompletionBorder))
                {
                    var min = ImGui.GetItemRectMin();
                    min.X -= 8;
                    min.Y -= 8;
                    var max = ImGui.GetItemRectMax();
                    max.X += 8;
                    max.Y += 8;
                    var zanaMissionColor = GetObjectiveColor(itemDetails.ZanaMissionType);

                    if (itemDetails.Bricked)
                    {
                        ImGui.GetForegroundDrawList().AddRect(min, max, ColorToUint(Settings.Bricked), 0f, 0,
                            Settings.BorderThickness.Value);
                    }
                    else if (itemDetails.ZanaMissionType != ObjectiveType.None && zanaMissionColor.HasValue)
                    {
                        ImGui.GetForegroundDrawList().AddRect(min, max, ColorToUint(zanaMissionColor.Value), 0f, 0,
                            Settings.BorderThickness.Value);
                    }
                    else if (Settings.CompletionBorder && !itemDetails.Completed)
                    {
                        ImGui.GetForegroundDrawList().AddRect(min, max, ColorToUint(Settings.Incomplete));
                    }
                    else if (Settings.CompletionBorder && !itemDetails.Bonus)
                    {
                        ImGui.GetForegroundDrawList().AddRect(min, max, ColorToUint(Settings.BonusIncomplete));
                    }
                    else if (Settings.CompletionBorder && !itemDetails.Awakened)
                    {
                        ImGui.GetForegroundDrawList().AddRect(min, max, ColorToUint(Settings.AwakenedIncomplete));
                    }
                    else if (isZanaMissionInventory)
                    {
                        ImGui.GetForegroundDrawList().AddRect(min, max, 0xFF4A4A4A);
                    }
                }

                var size = ImGui.GetWindowSize();
                if (boxOrigin.X + size.X > _WindowArea.Width)
                {
                    ImGui.SetWindowPos(
                        new nuVector2(boxOrigin.X - (boxOrigin.X + size.X - _WindowArea.Width) - 4, boxOrigin.Y + 24),
                        ImGuiCond.Always);
                }
                else
                {
                    ImGui.SetWindowPos(boxOrigin, ImGuiCond.Always);
                }

                if (isZanaMissionInventory)
                {
                    BoxSize.X += (int)size.X + 2;
                    if (MaxSize < size.Y)
                    {
                        MaxSize = size.Y;
                    }

                    LastCol = missionIndex;
                }
            }

            ImGui.End();
        }

        public override void Render()
        {
            if (_ingameState.IngameUi.Atlas.IsVisible)
            {
                AtlasRender();
            }

            var uiHover = _ingameState.UIHover;
            if (_ingameState.UIHover?.IsVisible ?? false)
            {
                var itemType = uiHover.AsObject<HoverItemIcon>()?.ToolTipType;
                if (itemType != null)
                {
                    if (itemType != ToolTipType.ItemInChat && itemType != ToolTipType.None)
                    {
                        var hoverItem = uiHover.AsObject<NormalInventoryItem>();
                        if (hoverItem.Item?.Path != null && (hoverItem.Tooltip?.IsValid ?? false))
                        {
                            RenderItem(hoverItem, hoverItem.Item);
                        }
                    }
                    else if (Settings.ShowForZanaMaps && itemType == ToolTipType.None)
                    {
                        var npcInv = _ingameState.Data.ServerData.NPCInventories;
                        if (npcInv == null || npcInv.Count == 0)
                        {
                            return;
                        }

                        foreach (var inv in npcInv)
                        {
                            if (uiHover.Parent.ChildCount == inv.Inventory.InventorySlotItems.Count)
                            {
                                BoxSize = new nuVector2(0f, 0f);
                                MaxSize = 0;
                                RowSize = 0;
                                LastCol = 0;
                                foreach (var item in inv.Inventory.InventorySlotItems.OrderBy(x => x.PosY)
                                             .ThenBy(x => x.PosX))
                                {
                                    RenderItem(null, item.Item, true, item.PosY);
                                }
                            }
                        }
                    }
                }
            }

            if (_ingameState.IngameUi.InventoryPanel.IsVisible)
            {
                foreach (var item in _ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory]
                             .VisibleInventoryItems)
                {
                    if (!item.Item.HasComponent<Map>())
                    {
                        continue;
                    }

                    var ItemDetails = item.Item.GetHudComponent<ItemDetails>() ?? null;
                    if (ItemDetails == null)
                    {
                        ItemDetails = new ItemDetails(item, item.Item);
                        item.Item.SetHudComponent(ItemDetails);
                    }

                    if (ItemDetails.Bricked)
                    {
                        Graphics.DrawFrame(item.GetClientRect(), Color.Red, 2);
                    }
                }
            }
        }
    }
}
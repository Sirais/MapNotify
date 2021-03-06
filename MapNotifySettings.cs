﻿
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace MapNotify
{
    public class MapNotifySettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);
        public ToggleNode ShowModCount { get; set; } = new ToggleNode(true);
        public ToggleNode ShowQuantityPercent { get; set; } = new ToggleNode(true);
        public ToggleNode ShowPackSizePercent { get; set; } = new ToggleNode(true);
        public ToggleNode ShowModWarnings { get; set; } = new ToggleNode(true);
    }
}

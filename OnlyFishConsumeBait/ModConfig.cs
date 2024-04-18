using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace OnlyFishConsumeBait;

public sealed class ModConfig
{
    public Keybind ToggleModButton { get; set; } = new Keybind(SButton.None);
    public bool EnableMod { get; set; } = true;
}
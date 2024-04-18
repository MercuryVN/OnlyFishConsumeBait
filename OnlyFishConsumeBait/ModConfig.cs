using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace OnlyFishConsumeBait;

public sealed class ModConfig
{
    public Keybind ToggleModButton { get; set; } = new Keybind(SButton.Q);
    public bool EnableMod { get; set; } = true;
}
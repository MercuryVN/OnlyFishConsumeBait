using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace OnlyFishConsumeBait;

internal sealed class ModEntry : Mod
{
    private IModHelper? helper;
    private IGenericModConfigMenuApi? configMenu;
    private ModConfig? config;
    private Farmer? player;
    private Object? bait;
    private int isFish = -1;

    public override void Entry(IModHelper helper)
    {
        this.helper = helper;
        config = helper.ReadConfig<ModConfig>();
        helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
    }

    private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu == null) return;
        configMenu.Register(
            mod: ModManifest,
            reset: () => config = new ModConfig(),
            save: () => helper!.WriteConfig(config!)
        );
        configMenu.AddKeybind(
            mod: ModManifest,
            getValue: () => config!.ToggleModButton.Buttons[0],
            setValue: value => config!.ToggleModButton.Buttons[0] = value,
            name: () => "Toggle mod button"
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            getValue: () => config!.EnableMod,
            setValue: value => config!.EnableMod = value,
            name: () => "Enable mod"
        );
    }

    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        _ = Task.Run(AssignPlayerAsync);
    }

    private void Input_ButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (helper == null || config == null || player == null) return;
        if (e.Button == config.ToggleModButton.Buttons[0])
        {
            config.EnableMod = !config.EnableMod;
            helper.WriteConfig(config);
            HUDMessage hudMessage = new("Default message")
            {
                message = config.EnableMod ? "Trash won't comsume bait" : "Trash will consume bait",
                timeLeft = 1500f,
                noIcon = true,
            };
            Game1.addHUDMessage(hudMessage);
        }
    }

    private void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (helper == null || config == null || player == null) return;
        if (!config.EnableMod) return;
        if (player.CurrentTool is not FishingRod rod) return;
        if (rod.isFishing && rod.GetBait() != null && bait == null)
        {
            Object tempBait = rod.GetBait();
            bait = new(tempBait.ItemId, tempBait.Stack, false, tempBait.Price, tempBait.Quality);
        }
        if (rod.pullingOutOfWater && rod.whichFish != null && bait != null && isFish < 0)
        {
            if (rod.whichFish.GetParsedData().Category == -4) isFish = 1;
            else
            {
                rod.GetBait().Stack++;
                isFish = 0;
            }
        }
        if (!rod.isFishing && !rod.pullingOutOfWater && !rod.fishCaught && (bait != null || isFish >= 0))
        {
            if (isFish == 0) rod.attachments[0] = bait;
            if (bait != null) bait = null;
            if (isFish >= 0) isFish = -1;
        }
    }

    private async Task AssignPlayerAsync()
    {
        while (Game1.player == null) await Task.Delay(500);
        player = Game1.player;
    }
}

using HarmonyLib;
using MetalRecycling.Configuration;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace MetalRecycling;
public class MetalRecyclingModSystem : ModSystem
{
    public static MetalRecyclingConfig Config { get; private set; }

    private Harmony _harmony;

    public override void StartServerSide(ICoreServerAPI api)
    {
        _harmony = new Harmony(ModConstants.ModId);

        Config = ModConfig.ReadConfig(api);

        _harmony.PatchCategory("Server");
    }

    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return forSide == EnumAppSide.Server;
    }

    public override void Dispose()
    {
        base.Dispose();

        _harmony.UnpatchAll();
    }
}

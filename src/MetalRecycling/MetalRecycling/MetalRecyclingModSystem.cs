using HarmonyLib;
using MetalRecycling.Configuration;
using Vintagestory.API.Common;

namespace MetalRecycling;
public class MetalRecyclingModSystem : ModSystem
{
    public static MetalRecyclingConfig Config { get; private set; }

    private Harmony _harmony;

    // Called on server and client
    // Useful for registering block/entity classes on both sides
    public override void Start(ICoreAPI api)
    {
        _harmony = new Harmony(ModConstants.ModId);

        if (api.Side != EnumAppSide.Server)
        {
            return;
        }

        Config = ModConfig.ReadConfig(api);

        _harmony.PatchCategory("Server");
    }

    public override void Dispose()
    {
        base.Dispose();

        _harmony.UnpatchAll();
    }
}

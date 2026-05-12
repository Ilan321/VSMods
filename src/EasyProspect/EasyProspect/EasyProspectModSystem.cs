using EasyProspect.Configuration;
using HarmonyLib;
using Vintagestory.API.Common;

namespace EasyProspect;
public class EasyProspectModSystem : ModSystem
{
    internal static EasyProspectConfig Config { get; private set; }

    private Harmony _harmony;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        Config = ModConfig.ReadConfig(api);

        _harmony = new Harmony(ModConstants.ModId);

        _harmony.PatchAll();
    }

    public override void Dispose()
    {
        base.Dispose();

        _harmony.UnpatchAll();
    }
}

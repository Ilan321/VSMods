using HarmonyLib;
using Vintagestory.API.Common;

namespace MetalUnitTooltip;

// ReSharper disable once UnusedMember.Global
public class MetalUnitTooltipModSystem : ModSystem
{
    private Harmony _harmony;

    public override void Start(ICoreAPI api)
    {
        _harmony = new Harmony(ModConstants.ModId);

        if (api.Side == EnumAppSide.Client)
        {
            _harmony.PatchCategory("Client");
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        _harmony.UnpatchAll();
    }
}
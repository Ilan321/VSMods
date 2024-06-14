using HarmonyLib;
using Vintagestory.API.Common;

namespace EasyProspect;
public class EasyProspectModSystem : ModSystem
{
    private Harmony _harmony;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        _harmony = new Harmony(ModConstants.ModId);

        _harmony.PatchAll();
    }

    public override void Dispose()
    {
        base.Dispose();

        _harmony.UnpatchAll();
    }
}

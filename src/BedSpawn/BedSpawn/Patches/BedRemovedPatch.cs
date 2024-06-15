using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BedSpawn.Patches;

[HarmonyPatchCategory("Server")]
[HarmonyPatch(typeof(BlockBed), nameof(BlockBed.OnBlockRemoved))]
// ReSharper disable once UnusedMember.Global
public class BedRemovedPatch
{
    // ReSharper disable once UnusedMember.Global
    public static void Postfix(
        // ReSharper disable once InconsistentNaming
        BlockBed __instance,
        IWorldAccessor world,
        BlockPos pos
    )
    {
        if (world.Api.Side != EnumAppSide.Server)
        {
            return;
        }

        var modSystem = world.Api.ModLoader.GetModSystem<BedSpawnModSystem>();

        modSystem.OnBlockRemoved(
            __instance,
            world,
            pos
        );
    }
}

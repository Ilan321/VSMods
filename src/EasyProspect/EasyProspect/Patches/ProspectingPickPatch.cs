using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace EasyProspect.Patches;

[HarmonyPatch(typeof(ItemProspectingPick), "ProbeBlockDensityMode")]
public class ProspectingPickPatch
{
    private static MethodInfo PrintProbeResultsInfo = typeof(ItemProspectingPick).GetMethod("PrintProbeResults", BindingFlags.NonPublic | BindingFlags.Instance);
    private static MethodInfo IsPropickableInfo = typeof(ItemProspectingPick).GetMethod("isPropickable", BindingFlags.NonPublic | BindingFlags.Instance);

    public static bool Prefix(
        ItemProspectingPick __instance,
        IWorldAccessor world,
        Entity byEntity,
        ItemSlot itemslot,
        BlockSelection blockSel
    )
    {
        IPlayer byPlayer = null;

        if (byEntity is EntityPlayer player)
        {
            byPlayer = world.PlayerByUid(player.PlayerUID);
        }

        Block block = world.BlockAccessor.GetBlock(blockSel.Position);

        float dropMul = block.BlockMaterial switch
        {
            EnumBlockMaterial.Ore => 0,
            EnumBlockMaterial.Stone => 0,
            _ => 1f
        };

        block.OnBlockBroken(
            world,
            blockSel.Position,
            byPlayer,
            dropMul
        );

        if (!isPropickable(__instance, block))
        {
            return false;
        }

        if (byPlayer is not IServerPlayer splr)
        {
            return false;
        }

        // Skip all the sample count logic..

        PrintProbeResults(
            __instance,
            world,
            splr,
            itemslot,
            blockSel.Position
        );

        if (__instance.DamagedBy == null || !__instance.DamagedBy.Contains(EnumItemDamageSource.BlockBreaking))
        {
            return false;
        }

        // Damage the item twice, since it's damaged anyway in the original method

        __instance.DamageItem(
            world,
            byEntity,
            itemslot,
            2
        );

        return false;
    }


    // Ugly reflection hacks :( 

    private static void PrintProbeResults(
        ItemProspectingPick instance,
        IWorldAccessor world,
        IServerPlayer splr,
        ItemSlot itemslot,
        BlockPos pos
    ) => PrintProbeResultsInfo.Invoke(
        instance,
        new object[]
        {
            world,
            splr,
            itemslot,
            pos
        }
    );

    private static bool isPropickable(ItemProspectingPick instance, Block block) => IsPropickableInfo.Invoke(instance, new object[] { block }) as bool? ?? false;
}
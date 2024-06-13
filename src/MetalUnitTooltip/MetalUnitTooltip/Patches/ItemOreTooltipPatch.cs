using System.Text;
using HarmonyLib;
using MetalUnitTooltip.Logic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace MetalUnitTooltip.Patches;

[HarmonyPatchCategory("Client")]
[HarmonyPatch(typeof(ItemOre), nameof(ItemOre.GetHeldItemInfo))]
// ReSharper disable once UnusedMember.Global
public class ItemOreTooltipPatch
{
    // ReSharper disable once UnusedMember.Global
    public static void Postfix(
        ItemSlot inSlot,
        StringBuilder dsc,
        IWorldAccessor world,
        bool withDebugInfo
    )
    {
        var tooltipText = MetalUnitTooltipLogic.GetOreTooltipText(inSlot.Itemstack, world.Api);

        if (string.IsNullOrEmpty(tooltipText))
        {
            return;
        }

        dsc.AppendLine(tooltipText);
    }
}
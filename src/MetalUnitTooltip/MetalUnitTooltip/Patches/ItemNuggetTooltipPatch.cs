using System.Text;
using HarmonyLib;
using MetalUnitTooltip.Logic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace MetalUnitTooltip.Patches;

[HarmonyPatch(typeof(ItemNugget), nameof(ItemNugget.GetHeldItemInfo))]
[HarmonyPatchCategory("Client")]
// ReSharper disable once UnusedMember.Global
public class ItemNuggetTooltipPatch
{
    // ReSharper disable once UnusedMember.Global
    public static void Postfix(
        ItemSlot inSlot,
        StringBuilder dsc,
        IWorldAccessor world,
        bool withDebugInfo
    )
    {
        var tooltipText = MetalUnitTooltipLogic.GetNuggetTooltipText(inSlot.Itemstack);

        if (string.IsNullOrEmpty(tooltipText))
        {
            return;
        }

        dsc.AppendLine(tooltipText);
    }
}
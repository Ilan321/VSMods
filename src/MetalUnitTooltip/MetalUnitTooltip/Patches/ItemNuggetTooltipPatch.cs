using System.Text;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
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
        if (inSlot.Itemstack.Item is null)
        {
            return;
        }

        var totalMetalUnits = inSlot.Itemstack.StackSize * 5;

        var tooltip = Lang.Get($"{ModConstants.ModId}:tooltipText", totalMetalUnits);

        if (inSlot.Itemstack.Item.CombustibleProps is { } smeltInfo)
        {
            float itemsPerIngot = smeltInfo.SmeltedStack.ResolvedItemstack.StackSize
                                  * 100f
                                  / smeltInfo.SmeltedRatio;

            var totalIngots = inSlot.Itemstack.StackSize / itemsPerIngot;

            tooltip += Lang.Get($"{ModConstants.ModId}:ingotText", totalIngots.ToString("0.#"));
        }

        dsc.AppendLine(tooltip);
    }
}
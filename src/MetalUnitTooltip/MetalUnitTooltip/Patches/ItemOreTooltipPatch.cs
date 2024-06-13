using System.Text;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace MetalUnitTooltip.Patches;

[HarmonyPatchCategory("Client")]
[HarmonyPatch(typeof(ItemOre), nameof(ItemOre.GetHeldItemInfo))]
// ReSharper disable once UnusedMember.Global
public class ItemOreTooltipPatch
{
    private const string MetalUnitsAttributeKey = "metalUnits";
    private const string NuggetItemCodePrefix = "nugget-";

    // ReSharper disable once UnusedMember.Global
    public static void Postfix(
        ItemSlot inSlot,
        StringBuilder dsc,
        IWorldAccessor world,
        bool withDebugInfo
    )
    {
        var item = inSlot.Itemstack.Item;

        if (item.CombustibleProps?.SmeltedStack?.ResolvedItemstack is not null)
        {
            return;
        }

        if (!item.Attributes[MetalUnitsAttributeKey].Exists)
        {
            return;
        }

        var metalUnits = item.Attributes[MetalUnitsAttributeKey]
                             .AsInt();

        var totalMetalUnits = inSlot.Itemstack.StackSize * metalUnits;

        var tooltip = Lang.Get($"{ModConstants.ModId}:tooltipText", totalMetalUnits);

        var nuggetItem = TryGetNugget(item, world.Api);

        if (nuggetItem.CombustibleProps is { } smeltInfo)
        {
            float itemsPerIngot = smeltInfo.SmeltedStack.ResolvedItemstack.StackSize
                                  * 100f
                                  / smeltInfo.SmeltedRatio;

            var totalIngots = inSlot.Itemstack.StackSize / itemsPerIngot;

            tooltip += Lang.Get($"{ModConstants.ModId}:ingotText", totalIngots.ToString("0.#"));
        }

        dsc.AppendLine(tooltip);
    }

    private static Item TryGetNugget(Item item, ICoreAPI api)
    {
        var oreType = item.LastCodePart(1);

        if (oreType.Contains("_"))
        {
            oreType = oreType.Split('_')[1];
        }

        return api.World.GetItem(new AssetLocation(NuggetItemCodePrefix + oreType));
    }
}
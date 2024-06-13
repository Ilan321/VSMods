using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace MetalUnitTooltip.Logic;

internal static class MetalUnitTooltipLogic
{
    private const string MetalUnitsAttributeKey = "metalUnits";
    private const string NuggetItemCodePrefix = "nugget-";

    public static string? GetNuggetTooltipText(ItemStack stack)
    {
        var totalMetalUnits = stack.StackSize * 5;

        var tooltip = Lang.Get($"{ModConstants.ModId}:tooltipText", totalMetalUnits);

        if (stack.Item.CombustibleProps is { } smeltInfo)
        {
            var totalIngots = stack.StackSize / (float) smeltInfo.SmeltedRatio;

            tooltip += GetIngotString(totalIngots);
        }

        return tooltip;
    }

    public static string? GetOreTooltipText(ItemStack stack, ICoreAPI api)
    {
        var item = stack.Item;

        if (item.CombustibleProps?.SmeltedStack?.ResolvedItemstack is not null)
        {
            return null;
        }

        if (!item.Attributes[MetalUnitsAttributeKey].Exists)
        {
            return null;
        }

        var metalUnits = item.Attributes[MetalUnitsAttributeKey]
                             .AsInt();

        var totalMetalUnits = stack.StackSize * metalUnits;

        var tooltip = Lang.Get($"{ModConstants.ModId}:tooltipText", totalMetalUnits);

        var nuggetItem = TryGetOreNugget(item, api);

        if (nuggetItem.CombustibleProps is { } smeltInfo)
        {
            var totalNuggets = stack.StackSize * (metalUnits / 5f);
            var totalIngots = totalNuggets / smeltInfo.SmeltedRatio;

            tooltip += GetIngotString(totalIngots);
        }

        return tooltip;
    }

    private static string? GetIngotString(float totalIngots) => totalIngots switch
    {
        _ when Math.Abs(totalIngots - 1) < 0.001 => Lang.Get($"{ModConstants.ModId}:tooltipIngotOne"),
        _ => Lang.Get($"{ModConstants.ModId}:tooltipIngotMany", totalIngots.ToString("0.##"))
    };

    private static Item TryGetOreNugget(Item item, ICoreAPI api)
    {
        var oreType = item.LastCodePart(1);

        if (oreType.Contains("_"))
        {
            oreType = oreType.Split('_')[1];
        }

        return api.World.GetItem(new AssetLocation(NuggetItemCodePrefix + oreType));
    }
}
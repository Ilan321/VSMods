using System;
using HarmonyLib;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MetalRecycling.Configuration;
using MetalRecycling.Helpers;
using Vintagestory.API.Common;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace MetalRecycling.Patches;

[HarmonyPatchCategory("Server")]
[HarmonyPatch(typeof(InventoryCraftingGrid), "FoundMatch")]
public static class InventoryCraftingGridPatch
{
    public static bool Prefix(InventoryCraftingGrid __instance, GridRecipe recipe, out int __state)
    {
        __state = 0;

        if (!MetalRecyclingModSystem.Config.ReduceChiselingBits) return true;

        var workItem = __instance.FirstOrDefault(f => f.Itemstack?.Item.Code.PathStartsWith("workitem-") == true);

        if (workItem is null) return true;

        var isOutputtingBits = recipe.Output.Code.PathStartsWith("metalbit-");

        if (!isOutputtingBits) return true;

        var recycleCount = workItem.Itemstack.Attributes.GetFloat(ModConstants.Attributes.RecycleCount);

        if (recycleCount <= 0) return true;

        __state = (int)recycleCount;

        return true;
    }

    public static void Postfix(
        InventoryCraftingGrid __instance,
        GridRecipe recipe,
        int __state
    )
    {
        if (!MetalRecyclingModSystem.Config.ReduceChiselingBits) return;
        if (__state <= 0) return;

        var outputSlot = ReflectionHelpers.GetPrivateField<ItemSlot>(__instance, "outputSlot");

        outputSlot.Itemstack.StackSize = Math.Clamp(
            outputSlot.Itemstack.StackSize - __state,
            0,
            outputSlot.Itemstack.StackSize
        );
    }
}

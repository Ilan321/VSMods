﻿using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace MetalRecycling.Patches;

[HarmonyPatchCategory("Server")]
[HarmonyPatch(typeof(BlockEntityAnvil), nameof(BlockEntityAnvil.OnSplit))]
// ReSharper disable once UnusedMember.Global
// ReSharper disable once InconsistentNaming
public static class BEAnvilPatch
{
    private const string TemperatureAttributeKey = "temperature";
    private const string MetalBitCodePrefix = "metalbit-";
    private const string IronBitItemCode = "metalbit-iron";

    // ReSharper disable once UnusedMember.Global
    public static bool Prefix(BlockEntityAnvil __instance, Vec3i voxelPos)
    {
        if (__instance.Api.Side != EnumAppSide.Server)
        {
            return true;
        }

        var voxel = __instance.Voxels[voxelPos.X,
            voxelPos.Y,
            voxelPos.Z
        ];

        if (voxel != (byte) EnumVoxelMaterial.Metal)
        {
            // If it's not metal, just let the original method handle it

            return true;
        }

        var chance = GetRecycleChance(__instance.WorkItemStack);

        var rng = Random.Shared.NextDouble();

        var success = chance > rng;

        if (!success)
        {
            return true;
        }

        var bitStack = GetRecycledMetalBitStack(__instance);

        if (bitStack is null)
        {
            return true;
        }

        var temperatureAttribute = __instance.WorkItemStack
                                             .Attributes[TemperatureAttributeKey];

        if (temperatureAttribute is not null)
        {
            bitStack.Attributes[TemperatureAttributeKey] = temperatureAttribute.Clone();
        }

        var bitPos = __instance.Pos.AddCopy(
            0f,
            1f,
            0f
        );

        __instance.Api.World.SpawnItemEntity(bitStack, bitPos.ToVec3d());

        IncRecycleCount(__instance.WorkItemStack);

        return true;
    }

    /// <summary>
    /// Gets the metal bit ItemStack to eject out of the anvil. Can return null if no metal bit is to be ejected.
    /// </summary>
    private static ItemStack GetRecycledMetalBitStack(BlockEntityAnvil instance)
    {
        if (instance.WorkItemStack.Item is not ItemIronBloom)
        {
            var itemType = instance.WorkItemStack.Item.LastCodePart(); // workitem-copper -> copper

            var bitCode = MetalBitCodePrefix + itemType;

            var bitItem = instance.Api.World.GetItem(new AssetLocation(bitCode));

            return bitItem is null ? null : new ItemStack(bitItem);
        }

        if (!MetalRecyclingModSystem.Config.RecycleIronBlooms)
        {
            // If it's an iron bloom and we don't want to recycle them, skip

            return null;
        }

        return new ItemStack(instance.Api.World.GetItem(new AssetLocation(IronBitItemCode)));
    }

    /// <summary>
    /// Gets the factored recycle chance. The recycle chance goes down with every successful recycle.
    /// </summary>
    private static float GetRecycleChance(ItemStack stack)
    {
        if (stack.Attributes[ModConstants.Attributes.RecycleCount] is not FloatAttribute attrib)
        {
            attrib = new FloatAttribute(0f);

            stack.Attributes[ModConstants.Attributes.RecycleCount] = attrib;
        }

        if (attrib.value > MetalRecyclingModSystem.Config.MaxBitsPerWorkItem)
        {
            return 0;
        }

        var chance = MetalRecyclingModSystem.Config.MetalRecyclingChance;

        return chance * MathF.Pow(MetalRecyclingModSystem.Config.DiminishingReturnFactor, attrib.value);
    }

    private static void IncRecycleCount(ItemStack stack)
    {
        if (stack.Attributes[ModConstants.Attributes.RecycleCount] is not FloatAttribute attrib)
        {
            attrib = new FloatAttribute(0f);

            stack.Attributes[ModConstants.Attributes.RecycleCount] = attrib;
        }

        attrib.value++;
    }
}
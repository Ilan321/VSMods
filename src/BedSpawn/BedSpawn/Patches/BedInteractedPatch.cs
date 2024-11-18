using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace BedSpawn.Patches;

[HarmonyPatchCategory("Server")]
[HarmonyPatch(typeof(BlockBed), nameof(BlockBed.OnBlockInteractStart))]
public class BedInteractedPatch
{
    public static bool Prefix(
        BlockBed __instance, 
        IWorldAccessor world,
        IPlayer byPlayer,
        BlockSelection blockSel,
        out bool __state
    )
    {
        __state = false;
        
        if (world.Api.Side != EnumAppSide.Server)
        {
            return true;
        }

        var serverPlayer = (IServerPlayer) byPlayer;

        // Set the state to whether the player is sneaking
        
        __state = serverPlayer.Entity.ServerControls.Sneak;

        return true;
    }
    
    public static void Postfix(
        BlockBed __instance,
        IWorldAccessor world,
        IPlayer byPlayer,
        BlockSelection blockSel,
        ref bool __result,
        bool __state
    )
    {
        if (!__result)
        {
            return;
        }
        
        if (world.Api.Side != EnumAppSide.Server)
        {
            return;
        }

        var modSystem = world.Api.ModLoader.GetModSystem<BedSpawnModSystem>();

        modSystem.SetPlayerSpawn(byPlayer as IServerPlayer, blockSel, __state);
    }
}

using System.Linq;
using BedSpawn.Configuration;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace BedSpawn;

public class BedSpawnModSystem : ModSystem
{
    internal static BedSpawnConfig Config { get; private set; }
    internal static readonly Harmony _harmony = new(ModConstants.ModId);

    public override void StartServerSide(ICoreServerAPI api)
    {
        Config = ModConfig.ReadConfig(api);

        _harmony.PatchCategory("Server");

        api.ChatCommands
            .Create("set-fatigue")
            .WithDescription("Sets the calling player's fatigue to the specified amount (defaults to 8)")
            .WithArgs(
                new FloatArgParser(
                    "fatigue",
                    8f,
                    false
                )
            )
            .RequiresPrivilege(Privilege.root)
            .RequiresPlayer()
            .HandleWith(SetFatigueHandler);

        api.Event.PlayerRespawn += Event_OnPlayerRespawn;

        base.StartServerSide(api);
    }

    private TextCommandResult SetFatigueHandler(TextCommandCallingArgs args)
    {
        if (args.Caller.Player is not IServerPlayer player)
        {
            return TextCommandResult.Error("A player must run this command");
        }

        Mod.Logger.Debug("Setting fatigue for {0}", player.PlayerName);

        // && (double) behavior.Tiredness <= 8.0

        if (player.Entity.GetBehavior("tiredness") is not EntityBehaviorTiredness behavior)
        {
            return TextCommandResult.Error($"Could not update the player fatigue");
        }

        behavior.Tiredness = args[0] as float? ?? 8f;

        return TextCommandResult.Success($"Set fatigue to {behavior.Tiredness}");
    }

    /// <summary>
    /// Check if the block is a "used" bed, and if it is, set the player's spawn point to their original location.
    /// </summary>
    public void OnBlockRemoved(
        BlockBed block,
        IWorldAccessor world,
        BlockPos pos
    )
    {
        var normalizedPosition = GetNormalizedBedPosition(block, pos);

        var playersWithThisSpawn = world
            .AllPlayers
            .OfType<IServerPlayer>()
            .Where(f => f.GetSpawnPosition(false)
                            .AsBlockPos
                        == normalizedPosition
            )
            .ToList();

        Mod.Logger.Debug(
            "Found {0} players with spawn point set to removed bed at {1}, clearing spawn position for these players",
            playersWithThisSpawn.Count,
            normalizedPosition
        );

        foreach (var player in playersWithThisSpawn)
        {
            player.ClearSpawnPosition();

            player.WorldData.SetModData(ModWorldData.BedMissing, true);
            player.BroadcastPlayerData();
        }
    }

    /// <summary>
    /// Check if the block is a bed, and if it is, set the player's spawn point to the bed's location.
    /// </summary>
    public void SetPlayerSpawn(
        IServerPlayer player,
        BlockSelection sel,
        bool wasSneaking
    )
    {
        var block = player.Entity.World.BlockAccessor.GetBlock(sel.Position);

        if (block is not BlockBed)
        {
            return;
        }

        var blockCode = $"{block.FirstCodePart()}-{block.FirstCodePart(1)}";

        if (Config.BlacklistedBeds.Contains(blockCode))
        {
            return;
        }

        if (Config.RequireSneaking && !wasSneaking)
        {
            return;
        }

        var normalizedPosition = GetNormalizedBedPosition(block, sel.Position);

        var currentSpawnPos = player.GetSpawnPosition(false)
            .AsBlockPos;

        if (currentSpawnPos == normalizedPosition)
        {
            // They used the same bed, so don't do anything

            return;
        }

        if (
            Config.Rooms.Enabled &&
            !Config.Rooms.BedsThatDontRequireRooms.Contains(blockCode) &&
            !BlockInRoom(player.Entity.World.Api, normalizedPosition.UpCopy())
        )
        {
            // Bed needs to be in a room, and it isn't - don't do anything

            if (Config.EnableDebugMessages)
            {
                player.SendLocalisedMessage(0, $"{ModConstants.ModId}:dbgBedNotInRoom");
            }

            return;
        }

        if (
            Config.DisableBelowSeaLevel && 
            normalizedPosition.Y < player.Entity.World.SeaLevel
            )
        {
            if (Config.EnableDebugMessages)
            {
                player.SendLocalisedMessage(0, $"{ModConstants.ModId}:dbgBedUnderSeaLevel");
            }

            return;
        }

        if (Config.Cooldown.Enabled)
        {
            var prevDays = player.WorldData.GetModData(ModWorldData.LastSetTime, -1d);

            if (prevDays >= 0)
            {
                var nowDays = player.Entity.World.Calendar.TotalDays;
                var diff = nowDays - prevDays;

                if (diff < Config.Cooldown.CooldownDays!.Value)
                {
                    if (Config.EnableDebugMessages)
                    {
                        player.SendLocalisedMessage(0, $"{ModConstants.ModId}:dbgCooldownActive");
                    }

                    return;
                }
            }
        }

        Mod.Logger.Debug(
            "Setting spawn position for {0} to {1}",
            player.PlayerName,
            normalizedPosition
        );

        player.SetSpawnPosition(
            new(
                normalizedPosition.X,
                normalizedPosition.Y,
                normalizedPosition.Z
            )
        );

        player.WorldData.SetModData(ModWorldData.BedMissing, false);
        player.WorldData.SetModData(ModWorldData.LastSetTime, player.Entity.World.Calendar.TotalDays);

        player.BroadcastPlayerData();

        player.SendLocalisedMessage(0, $"{ModConstants.ModId}:msgSpawnSet");
    }

    private bool BlockInRoom(ICoreAPI api, BlockPos pos)
    {
        var roomRegistry = api.ModLoader.GetModSystem<RoomRegistry>();

        if (roomRegistry is null)
        {
            Mod.Logger.Warning("Could not get room registry");

            // Fail quietly

            return true;
        }

        var skyExposed = api.World.BlockAccessor.GetRainMapHeightAt(pos.X, pos.Z) <= pos.Y;

        if (skyExposed) return false;

        var room = roomRegistry.GetRoomForPosition(pos);

        if (room is null) return false;
        if (room.ExitCount != 0) return false;

        return true;
    }

    /// <summary>
    /// Returns the position of the bed's head. If the input block is the bed's "feet", will return the position of the head.
    /// </summary>
    private BlockPos GetNormalizedBedPosition(Block bed, BlockPos pos)
    {
        if (bed.Variant["part"] == "head")
        {
            return pos;
        }

        var currentSide = bed.Variant["side"];

        var headFacing = BlockFacing.FromCode(currentSide)
            .Opposite;

        return pos.AddCopy(headFacing);
    }

    private void Event_OnPlayerRespawn(IServerPlayer player)
    {
        if (player is null) return;
        if (!player.WorldData.GetModData<bool>(ModWorldData.BedMissing)) return;

        player.WorldData.SetModData(ModWorldData.BedMissing, false);
        player.BroadcastPlayerData();

        if (!Config.NotifyPlayerOnBedDestroyed) return;

        player.SendLocalisedMessage(0, $"{ModConstants.ModId}:msgBedMissing");
    }

    public override void Dispose()
    {
        base.Dispose();

        _harmony?.UnpatchAll();
    }
}
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

    private Harmony _harmony;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        _harmony = new Harmony(ModConstants.ModId);

        if (api.Side == EnumAppSide.Server)
        {
            Config = ModConfig.ReadConfig(api);

            _harmony.PatchCategory("Server");
        }
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        api.Event.DidUseBlock += OnBlockUsed;

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
                                   .Where(
                                       f => f.GetSpawnPosition(false)
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
        }
    }

    /// <summary>
    /// Check if the block is a bed, and if it is, set the player's spawn point to the bed's location.
    /// </summary>
    private void OnBlockUsed(IServerPlayer player, BlockSelection sel)
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

        var normalizedPosition = GetNormalizedBedPosition(block, sel.Position);

        var currentSpawnPos = player.GetSpawnPosition(false)
                                    .AsBlockPos;

        if (currentSpawnPos == normalizedPosition)
        {
            // They used the same bed, so don't do anything

            return;
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

        player.SendLocalisedMessage(0, $"{ModConstants.ModId}:msgSpawnSet");
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

    public override void Dispose()
    {
        base.Dispose();

        _harmony?.UnpatchAll();
    }
}
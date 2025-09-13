using System.Collections.Generic;

namespace BedSpawn.Configuration;

public class BedSpawnConfig
{
    /// <summary>
    /// Gets or sets the list of beds that are blacklisted from being used as spawn points.
    /// </summary>
    public List<string> BlacklistedBeds { get; set; } = [];

    /// <summary>
    /// Gets or sets whether sneaking is required to set a spawn point.
    /// </summary>
    public bool RequireSneaking { get; set; }

    public BedSpawnRoomConfig Rooms { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to show debug messages when attempting to use a bed (and it fails to set your spawn).
    /// </summary>
    public bool EnableDebugMessages { get; set; }

    /// <summary>
    /// Whether the player should be notified when their bed has been destroyed (when they respawn)
    /// </summary>
    public bool NotifyPlayerOnBedDestroyed { get; set; } = true;

    /// <summary>
    /// Whether the player can set their spawn using a bed, below sea level (e.g. in a cave, dungeon, ruin)
    /// </summary>
    public bool DisableBelowSeaLevel { get; set; }

    public BedSpawnCooldownConfig Cooldown { get; set; } = new();
}

public class BedSpawnRoomConfig
{
    /// <summary>
    /// Whether to require a room for the spawn to be set.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Any beds in this list will still set your spawn, even if they're not in a room.
    /// </summary>
    public List<string> BedsThatDontRequireRooms { get; set; } = [];
}

public class BedSpawnCooldownConfig
{
    public bool Enabled => CooldownDays.HasValue;

    /// <summary>
    /// How many days the player has to wait before they can set their spawn again. Optional.
    /// </summary>
    public float? CooldownDays { get; set; }
}

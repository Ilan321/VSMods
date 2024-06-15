using System.Collections.Generic;

namespace BedSpawn.Configuration;

public class BedSpawnConfig
{
    /// <summary>
    /// Gets or sets the list of beds that are blacklisted from being used as spawn points.
    /// </summary>
    public List<string> BlacklistedBeds { get; set; } = new();
}

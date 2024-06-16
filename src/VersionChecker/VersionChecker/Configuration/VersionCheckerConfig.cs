using System;
using System.Collections.Generic;

namespace VersionChecker.Configuration;

public class VersionCheckerConfig
{
    /// <summary>
    /// List of modids to ignore when checking for version updates. Defaults to the built-in game mods.
    /// </summary>
    public HashSet<string> IgnoredMods { get; set; } = new()
    {
        "game",
        "survival",
        "creative"
    };

    /// <summary>
    /// The snooze time in minutes. Defaults to 24 hours.
    /// </summary>
    public int SnoozeMinutes { get; set; } = 60 * 60 * 24;

    /// <summary>
    /// The last time the .versionchecker snooze command was called.
    /// </summary>
    public DateTime? SnoozeTime { get; set; }
}
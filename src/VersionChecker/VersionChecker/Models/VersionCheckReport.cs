using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;

namespace VersionChecker.Models;

public class VersionCheckReport
{
    public required List<VersionCheckMod> Mods { get; set; }
    public List<Mod> IgnoredMods { get; set; } = new();

    public bool AllUpdated => Mods.All(m => m.CurrentVersion >= m.LatestVersion);
}

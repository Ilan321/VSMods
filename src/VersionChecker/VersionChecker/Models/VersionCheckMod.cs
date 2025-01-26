using ProperVersion;

namespace VersionChecker.Models;

public class VersionCheckMod
{
    public required string ModId { get; set; }
    public required string ModName { get; set; }
    public required SemVer CurrentVersion { get; set; }
    public required SemVer LatestVersion { get; set; }
    public required string UrlAlias { get; set; }
}

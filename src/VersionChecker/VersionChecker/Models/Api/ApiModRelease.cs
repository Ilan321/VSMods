using System.Collections.Generic;

namespace VersionChecker.Models.Api;

public class ApiModRelease
{
    public required string ModVersion { get; set; }
    public required List<string> Tags { get; set; }
}
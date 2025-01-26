using System.Collections.Generic;

namespace VersionChecker.Models.Api;

public class ApiModDetails
{
    public required List<ApiModRelease> Releases { get; set; }
    public string UrlAlias { get; set; }
}
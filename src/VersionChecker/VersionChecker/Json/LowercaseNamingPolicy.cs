using System.Text.Json;

namespace VersionChecker.Json;

public class LowercaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToLower();
}

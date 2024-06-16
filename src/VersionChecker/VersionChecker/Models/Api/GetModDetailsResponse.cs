namespace VersionChecker.Models.Api;

public class GetModDetailsResponse
{
    public string StatusCode { get; set; } = default!;
    public ApiModDetails? Mod { get; set; }
}

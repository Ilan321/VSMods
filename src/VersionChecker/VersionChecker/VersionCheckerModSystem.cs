using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ProperVersion;
using VersionChecker.Configuration;
using VersionChecker.Json;
using VersionChecker.Models;
using VersionChecker.Models.Api;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Config;
using Vintagestory.Client;

namespace VersionChecker;

public class VersionCheckerModSystem : ModSystem
{
    private const string ModUpdateLinkProtocol = "versioncheckerupdatemod";
    private const string UpdateAllModsLinkProtocol = "versioncheckerupdateallmods";

    private static readonly HttpClient _httpClient = new();

    public VersionCheckerConfig Config { get; private set; } = default!;

    public override void StartClientSide(ICoreClientAPI api)
    {
        Config = ModConfig.ReadConfig(api);

        api.RegisterLinkProtocol(ModUpdateLinkProtocol, OnModUpdateLinkClicked);
        api.RegisterLinkProtocol(UpdateAllModsLinkProtocol, OnUpdateAllModsLinkClicked);

        api.Event.IsPlayerReady += (ref EnumHandling handling) =>
        {
            handling = EnumHandling.PassThrough;

            OnPlayerReady(api);

            return true;
        };

        api.ChatCommands
            .Create("versionchecker")
            .WithDesc("VersionChecker related commands")
            .BeginSubCommand("snooze")
            .WithDescription("Snoozes the version check alert (by default, for 24 hours)")
            .HandleWith(OnSnoozeCommandCalled)
            .EndSubCommand()
            .BeginSubCommand("unsnooze")
            .WithDescription("Resets the snooze on the version check alert")
            .HandleWith(OnUnsnoozeCommandCalled)
            .EndSubCommand();
    }

    private TextCommandResult OnUnsnoozeCommandCalled(TextCommandCallingArgs args)
    {
        Mod.Logger.Debug("Unsnooze command called");

        if (args.Caller.Player is not IClientPlayer clientPlayer)
        {
            return TextCommandResult.Error($"Could not snooze, caller is not a client");
        }

        Config.SnoozeTime = null;

        ModConfig.SaveOrCreateConfig(clientPlayer.Entity.Api, Config);

        return TextCommandResult.Success(Lang.Get($"{ModConstants.ModId}:cmdUnsnoozeSuccess"));
    }

    private TextCommandResult OnSnoozeCommandCalled(TextCommandCallingArgs args)
    {
        Mod.Logger.Debug("Snooze command called");

        if (args.Caller.Player is not IClientPlayer clientPlayer)
        {
            return TextCommandResult.Error($"Could not snooze, caller is not a client");
        }

        var now = DateTime.UtcNow;

        Config.SnoozeTime = now;

        ModConfig.SaveOrCreateConfig(clientPlayer.Entity.Api, Config);

        return TextCommandResult.Success(Lang.Get($"{ModConstants.ModId}:cmdSnoozeSuccess", Config.SnoozeMinutes));
    }

    private void OnModUpdateLinkClicked(LinkTextComponent obj)
    {
        Mod.Logger.Debug($"User clicked on mod update link: {obj.Href}");

        var identifier = obj.Href.Replace($"{ModUpdateLinkProtocol}://", string.Empty);

        ClientProgram.screenManager.InstallMod(identifier);
    }

    private void OnUpdateAllModsLinkClicked(LinkTextComponent obj)
    {
        Mod.Logger.Debug($"User clicked on updateallmods link: {obj.Href}");

        var screenManager = ClientProgram.screenManager;

        if (ReflectionUtils.GetField<GuiScreen>(screenManager, "CurrentScreen") is GuiScreenRunningGame
            runningGameScreen)
        {
            runningGameScreen.ExitOrRedirect(reason: "versionchecker updateallmods request");

            TyronThreadPool.QueueTask(
                (Action)(() =>
                {
                    var num = 0;

                    while (num++ < 1000 &&
                           ReflectionUtils.GetField<GuiScreen>(screenManager,
                               "CurrentScreen") is not GuiScreenMainRight)
                    {
                        Thread.Sleep(100);
                    }

                    ScreenManager.EnqueueMainThreadTask(() => OnUpdateAllModsLinkClicked(obj));
                }),
                "versionchecker updateallmods"
            );

            return;
        }

        var modids = obj.Href
            .Replace($"{UpdateAllModsLinkProtocol}://", string.Empty)
            .Split(',');

        string dataPathMods = GamePaths.DataPathMods;

        var mainScreen = ReflectionUtils.GetField<GuiScreenMainRight>(screenManager, "mainScreen");

        if (mainScreen is null)
        {
            Mod.Logger.Error("Could not get main screen from ScreenManager");

            return;
        }

        screenManager.LoadScreen(
            new GuiScreenDownloadMods(
                null,
                dataPathMods,
                modids.ToList(),
                screenManager,
                mainScreen
            )
        );
    }

    // Lord forgive me for this

    private async void OnPlayerReady(ICoreClientAPI api)
    {
        if (Config.SnoozeTime.HasValue)
        {
            var snoozeTime = Config.SnoozeTime.Value;
            var now = DateTime.UtcNow;

            var diff = now - snoozeTime;

            if (diff.TotalMinutes < Config.SnoozeMinutes)
            {
                Mod.Logger.Debug($"Snoozed version check, {diff.TotalMinutes} minutes left");

                return;
            }

            Mod.Logger.Debug("Snooze time expired, resetting snooze time");

            Config.SnoozeTime = null;

            ModConfig.SaveOrCreateConfig(api, Config);
        }

        Mod.Logger.Debug("Starting mod version check");

        var report = await GetLatestModVersion(api);

        if (report.AllUpdated)
        {
            Mod.Logger.Debug("All mods up to date, skipping client message");

            return;
        }

        api.Event.EnqueueMainThreadTask(
            () => ShowVersionReport(api, report),
            "versioncheckmessage"
        );
    }

    private void ShowVersionReport(ICoreClientAPI api, VersionCheckReport report)
    {
        var sb = new StringBuilder();

        var oldMods = report.Mods
            .Where(f => f.CurrentVersion < f.LatestVersion)
            .OrderBy(f => f.ModName)
            .ToList();

        Mod.Logger.Debug(
            $"Building version check report with {oldMods.Count} out-of-date mods: {string.Join(", ", oldMods.Select(f => f.ModId))}");
        Mod.Logger.Debug(
            $"(ignored {report.IgnoredMods.Count} mods: {string.Join(", ", report.IgnoredMods.Select(f => f.Info.ModID))})");

        if (oldMods.Count > 1)
        {
            var modids = oldMods.Select(f => $"{f.ModId}@{f.LatestVersion}");

            sb.AppendLine(
                Lang.Get(
                    $"{ModConstants.ModId}:foundOldMods",
                    oldMods.Count,
                    $"{UpdateAllModsLinkProtocol}://{string.Join(",", modids)}"
                )
            );
        }
        else
        {
            sb.AppendLine(Lang.Get($"{ModConstants.ModId}:foundOldMod"));
        }

        foreach (var mod in oldMods)
        {
            sb.AppendLine(
                Lang.Get(
                    $"{ModConstants.ModId}:modVersionItem",
                    mod.ModName,
                    mod.CurrentVersion,
                    mod.LatestVersion,
                    $"https://mods.vintagestory.at/{mod.UrlAlias}",
                    $"{ModUpdateLinkProtocol}://{mod.ModId}@{mod.LatestVersion}"
                )
            );
        }

        api.ShowChatMessage(sb.ToString());
    }

    private async Task<VersionCheckReport> GetLatestModVersion(ICoreClientAPI api)
    {
        var enabledMods = api.ModLoader.Mods;

        var bag = new ConcurrentBag<VersionCheckMod>();
        var filteredMods = new ConcurrentBag<Mod>();

        await Parallel.ForEachAsync(
            enabledMods,
            async (mod, _) =>
            {
                if (Config.IgnoredMods.Contains(mod.Info.ModID))
                {
                    filteredMods.Add(mod);

                    return;
                }

                var latestModVersion = await GetLatestModVersion(mod);

                if (latestModVersion is not null)
                {
                    bag.Add(latestModVersion);
                }
            }
        );

        return new()
        {
            IgnoredMods = filteredMods.ToList(),
            Mods = bag.ToList()
        };
    }

    private async Task<VersionCheckMod?> GetLatestModVersion(Mod mod)
    {
        try
        {
            var requestUrl = $"https://mods.vintagestory.at/api/mod/{mod.Info.ModID}";

            using var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                Mod.Logger.Warning(
                    $"Failed to fetch mod info for {mod.Info.ModID}, API returned non-success: {response.StatusCode}");

                return null;
            }

            var responseContent = await response.Content.ReadFromJsonAsync<GetModDetailsResponse>(
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = new LowercaseNamingPolicy()
                }
            );

            if (responseContent is not { StatusCode: "200" })
            {
                Mod.Logger.Warning(
                    $"Failed to fetch mod info for {mod.Info.ModID}, API returned non-success: {responseContent?.StatusCode}");

                return null;
            }

            var modInfo = responseContent.Mod;

            if (modInfo is null)
            {
                Mod.Logger.Warning($"Failed to fetch mod info for {mod.Info.ModID}, API returned no mod info");

                return null;
            }

            var currentGameVersion = SemVer.Parse(GameVersion.APIVersion);
            var currentVersion = SemVer.Parse(mod.Info.Version);

            var modReleases = modInfo.Releases;

            Mod.Logger.Debug(
                $"Found {modReleases.Count} releases for mod {mod.Info.ModID}, filtering by game version {currentGameVersion}");

            var modReleasesForThisGameVersion = modReleases
                .Where(f => f.Tags.Any(t =>
                    IsModTagCompatibleWithGameVersion(
                        currentGameVersion,
                        SemVer.Parse(t.TrimStart('v'))
                    )
                ))
                .ToList();

            Mod.Logger.Debug($"Applicable releases for {mod.Info.ModID} for game version {currentGameVersion}: {modReleasesForThisGameVersion.Count}");

            var latestVersion = modReleasesForThisGameVersion
                .Select(f => SemVer.Parse(f.ModVersion))
                .OrderByDescending(f => f)
                .FirstOrDefault();

            Mod.Logger.Debug($"Found latest version for {mod.Info.ModID}: {latestVersion}");

            if (latestVersion is null)
            {
                return null;
            }

            return new VersionCheckMod
            {
                ModId = mod.Info.ModID,
                ModName = mod.Info.Name,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                UrlAlias = modInfo.UrlAlias
            };
        }
        catch (Exception ex)
        {
            Mod.Logger.Error($"Failed to fetch mod info for {mod.Info.ModID}:");
            Mod.Logger.Error(ex);

            return null;
        }
    }

    private bool IsModTagCompatibleWithGameVersion(SemVer gameVersion, SemVer releaseVersion) =>
        gameVersion.Major == releaseVersion.Major && gameVersion.Minor == releaseVersion.Minor;
}
using Vintagestory.API.Common;

namespace VersionChecker.Configuration;

public static class ModConfig
{
    public static VersionCheckerConfig ReadConfig(ICoreAPI api)
    {
        try
        {
            var config = LoadConfig(api);

            if (config == null)
            {
                api.Logger.Debug("Generating configuration for BedSpawn..");

                SaveOrCreateConfig(api);

                config = LoadConfig(api);
            }

            SaveOrCreateConfig(api, config);

            return config!;
        }
        catch
        {
            SaveOrCreateConfig(api);

            return LoadConfig(api)!;
        }
    }

    private static VersionCheckerConfig? LoadConfig(ICoreAPI api)
    {
        return api.LoadModConfig<VersionCheckerConfig>(ModConstants.ConfigFileName);
    }

    internal static void SaveOrCreateConfig(ICoreAPI api, VersionCheckerConfig? config = default)
    {
        api.StoreModConfig(config ?? new(), ModConstants.ConfigFileName);
    }
}
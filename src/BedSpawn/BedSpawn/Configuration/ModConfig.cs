using Vintagestory.API.Common;

namespace BedSpawn.Configuration;

public static class ModConfig
{
    public static BedSpawnConfig ReadConfig(ICoreAPI api)
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

            // Re-save the config to add any missing properties to the json file

            SaveOrCreateConfig(api, config);

            return config;
        }
        catch
        {
            SaveOrCreateConfig(api);

            return LoadConfig(api);
        }
    }

    private static BedSpawnConfig LoadConfig(ICoreAPI api)
    {
        return api.LoadModConfig<BedSpawnConfig>(ModConstants.ConfigFileName);
    }

    private static void SaveOrCreateConfig(ICoreAPI api, BedSpawnConfig config = default)
    {
        api.StoreModConfig(config ?? new(), ModConstants.ConfigFileName);
    }
}

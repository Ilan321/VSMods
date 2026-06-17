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

                config = LoadConfigSafe(api);
            }

            // Re-save the config to add any missing properties to the json file

            SaveOrCreateConfig(api, config);

            return config;
        }
        catch
        {
            SaveOrCreateConfig(api);

            return LoadConfigSafe(api);
        }
    }

    private static BedSpawnConfig LoadConfig(ICoreAPI api)
    {
        return api.LoadModConfig<BedSpawnConfig>(ModConstants.ConfigFileName);
    }

    private static BedSpawnConfig LoadConfigSafe(ICoreAPI api)
    {
        var config = LoadConfig(api);

        if (config != null) return config;

        BedSpawnModSystem.Instance.Mod.Logger.Warning("LoadModConfig returned null! This is usually due to autoconfiglib changing how mod config loading works. Continuing with a default mod config, your custom configs will NOT TAKE EFFECT!");

        return new();
    }

    private static void SaveOrCreateConfig(ICoreAPI api, BedSpawnConfig config = default)
    {
        api.StoreModConfig(config ?? new(), ModConstants.ConfigFileName);
    }
}

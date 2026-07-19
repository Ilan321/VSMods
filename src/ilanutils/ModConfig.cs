using Vintagestory.API.Common;

namespace ilanutils;

public static class ModConfig
{
    public static T ReadConfig<T>(
        ICoreAPI api,
        string filename,
        ILogger log
    )
        where T : new()
    {
        try
        {
            var config = LoadConfig<T>(api, filename);

            if (config == null)
            {
                api.Logger.Debug($"Generating configuration at {filename}");

                SaveOrCreateConfig<T>(api, filename);

                config = LoadConfigSafe<T>(
                    api,
                    filename,
                    log
                );
            }

            // Re-save the config to add any missing properties to the json file

            SaveOrCreateConfig<T>(
                api,
                filename,
                config
            );

            return config;
        }
        catch
        {
            SaveOrCreateConfig<T>(api, filename);

            return LoadConfigSafe<T>(
                api,
                filename,
                log
            );
        }
    }

    private static T LoadConfig<T>(ICoreAPI api, string filename)
        where T : new()
    {
        return api.LoadModConfig<T>(filename);
    }

    private static T LoadConfigSafe<T>(
        ICoreAPI api,
        string filename,
        ILogger log
    )
        where T : new()
    {
        var config = LoadConfig<T>(api, filename);

        if (config != null) return config;

        log.Warning(
            "LoadModConfig returned null! This is usually due to autoconfiglib changing how mod config loading works. Continuing with a default mod config, your custom configs will NOT TAKE EFFECT!"
        );

        return new();
    }

    private static void SaveOrCreateConfig<T>(
        ICoreAPI api,
        string filename,
        T config = default
    )
        where T : new()
    {
        api.StoreModConfig(config ?? new(), filename);
    }
}
using Vintagestory.API.Common;

namespace EasyProspect.Configuration;

public static class ModConfig
{
    public static EasyProspectConfig ReadConfig(ICoreAPI api)
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

    private static EasyProspectConfig LoadConfig(ICoreAPI api)
    {
        return api.LoadModConfig<EasyProspectConfig>(ModConstants.ConfigFileName);
    }

    private static void SaveOrCreateConfig(ICoreAPI api, EasyProspectConfig config = default)
    {
        api.StoreModConfig(config ?? new(), ModConstants.ConfigFileName);
    }
}

public class EasyProspectConfig
{
    public bool TakeLessDurability { get; set; } = false;
}
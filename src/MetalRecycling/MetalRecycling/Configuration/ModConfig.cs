using Vintagestory.API.Common;

namespace MetalRecycling.Configuration;

public static class ModConfig
{
    public static MetalRecyclingConfig ReadConfig(ICoreAPI api)
    {
        try
        {
            var config = LoadConfig(api);

            if (config == null)
            {
                api.Logger.Debug("Generating configuration for Metal Recycling..");

                SaveOrCreateConfig(api);

                config = LoadConfig(api);
            }

            SaveOrCreateConfig(api, config);

            return config;
        }
        catch
        {
            SaveOrCreateConfig(api);

            return LoadConfig(api);
        }
    }

    private static MetalRecyclingConfig LoadConfig(ICoreAPI api)
    {
        return api.LoadModConfig<MetalRecyclingConfig>(ModConstants.ConfigFileName);
    }

    private static void SaveOrCreateConfig(ICoreAPI api, MetalRecyclingConfig config = default)
    {
        api.StoreModConfig(config ?? new(), ModConstants.ConfigFileName);
    }
}
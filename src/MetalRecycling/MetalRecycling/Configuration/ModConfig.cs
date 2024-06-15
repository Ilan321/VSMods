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

                GenerateConfig(api);

                config = LoadConfig(api);
            }

            return config;
        }
        catch
        {
            GenerateConfig(api);

            return LoadConfig(api);
        }
    }

    private static MetalRecyclingConfig LoadConfig(ICoreAPI api)
    {
        return api.LoadModConfig<MetalRecyclingConfig>(ModConstants.ConfigFileName);
    }

    private static void GenerateConfig(ICoreAPI api)
    {
        api.StoreModConfig(new MetalRecyclingConfig(), ModConstants.ConfigFileName);
    }
}
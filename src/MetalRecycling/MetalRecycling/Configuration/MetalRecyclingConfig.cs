namespace MetalRecycling.Configuration;

public class MetalRecyclingConfig
{
    /// <summary>
    /// The chance that a metal bit will be returned when splitting metal on the anvil. Defaults to 50% (0.5f).
    /// </summary>
    public float MetalRecyclingChance { get; set; } = 0.5f;

    /// <summary>
    /// How many metal bits you can get from a single work item. Defaults to 10.
    /// </summary>
    public int MaxBitsPerWorkItem { get; set; } = 10;

    /// <summary>
    /// The "factor" of diminishing returns that affects the recycle chance on every successful bit recovery. Defaults to 0.85.
    /// </summary>
    public float DiminishingReturnFactor { get; set; } = 0.85f;
}

using System.Text.Json.Serialization;

namespace EchoCat.Desktop.Skins;

public sealed class PetSkinManifest
{
    [JsonIgnore]
    public string BaseDirectory { get; set; } = string.Empty;

    public string Id { get; init; } = "soft-vector-cat";

    public string DisplayName { get; init; } = "Soft Vector Cat";

    public string Renderer { get; init; } = SkinRendererIds.SoftVectorCat;

    public string Version { get; init; } = "0.1.0";

    public string Author { get; init; } = "EchoCat";

    public PetSkinPaletteManifest Palette { get; init; } = new();

    public PetSpriteManifest? Sprite { get; init; }
}

public sealed class PetSpriteManifest
{
    public string DefaultFrame { get; init; } = string.Empty;

    public Dictionary<string, string> Frames { get; init; } = new();
}

public sealed class PetSkinPaletteManifest
{
    public string Fur { get; init; } = "#FFF8FA";

    public string FurShadow { get; init; } = "#F4DCE7";

    public string Ink { get; init; } = "#3B2E38";

    public string Blush { get; init; } = "#FFC4D5";

    public string BubbleBackground { get; init; } = "#F7FFFFFF";

    public string BubbleBorder { get; init; } = "#55A78FA0";

    public string PanelBackground { get; init; } = "#F4FFFFFF";

    public string PanelBorder { get; init; } = "#33A78FA0";

    public Dictionary<string, string> MoodAccents { get; init; } = new()
    {
        ["calm"] = "#8DE1C1",
        ["happy"] = "#FF8FB3",
        ["curious"] = "#7AC7FF",
        ["thinking"] = "#B69CFF",
        ["sleepy"] = "#9EB6C8"
    };
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(PetSkinManifest))]
internal sealed partial class PetSkinJsonContext : JsonSerializerContext;

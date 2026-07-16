using Avalonia.Media;
using EchoCat.Core;

namespace EchoCat.Desktop.Skins;

public sealed record PetSkinPalette(
    Color Fur,
    Color FurShadow,
    Color Ink,
    Color Accent,
    Color Blush,
    Color BubbleBackground,
    Color BubbleBorder,
    Color PanelBackground,
    Color PanelBorder)
{
    public static PetSkinPalette Default(PetMood mood)
    {
        var accent = mood switch
        {
            PetMood.Happy => Color.Parse("#FF8FB3"),
            PetMood.Curious => Color.Parse("#7AC7FF"),
            PetMood.Thinking => Color.Parse("#B69CFF"),
            PetMood.Sleepy => Color.Parse("#9EB6C8"),
            _ => Color.Parse("#8DE1C1")
        };

        return new PetSkinPalette(
            Color.Parse("#FFF8FA"),
            Color.Parse("#F4DCE7"),
            Color.Parse("#3B2E38"),
            accent,
            Color.Parse("#FFC4D5"),
            Color.Parse("#F7FFFFFF"),
            Color.Parse("#55A78FA0"),
            Color.Parse("#F4FFFFFF"),
            Color.Parse("#33A78FA0"));
    }

    public static PetSkinPalette FromManifest(PetSkinPaletteManifest manifest, PetMood mood)
    {
        return new PetSkinPalette(
            ParseOrDefault(manifest.Fur, "#FFF8FA"),
            ParseOrDefault(manifest.FurShadow, "#F4DCE7"),
            ParseOrDefault(manifest.Ink, "#3B2E38"),
            ParseOrDefault(ResolveAccent(manifest, mood), "#8DE1C1"),
            ParseOrDefault(manifest.Blush, "#FFC4D5"),
            ParseOrDefault(manifest.BubbleBackground, "#F7FFFFFF"),
            ParseOrDefault(manifest.BubbleBorder, "#55A78FA0"),
            ParseOrDefault(manifest.PanelBackground, "#F4FFFFFF"),
            ParseOrDefault(manifest.PanelBorder, "#33A78FA0"));
    }

    private static string ResolveAccent(PetSkinPaletteManifest manifest, PetMood mood)
    {
        var key = mood.ToString().ToLowerInvariant();
        if (manifest.MoodAccents.TryGetValue(key, out var color))
        {
            return color;
        }

        return manifest.MoodAccents.TryGetValue("calm", out var fallback)
            ? fallback
            : "#8DE1C1";
    }

    private static Color ParseOrDefault(string color, string fallback)
    {
        try
        {
            return Color.Parse(color);
        }
        catch
        {
            return Color.Parse(fallback);
        }
    }
}

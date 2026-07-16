using System.Text.Json;

namespace EchoCat.Desktop.Skins;

public sealed class PetSkinManifestLoader
{
    public IReadOnlyList<PetSkinManifest> LoadManifests(string skinsRoot)
    {
        if (!Directory.Exists(skinsRoot))
        {
            return [];
        }

        var manifests = new List<PetSkinManifest>();

        foreach (var skinDirectory in Directory.EnumerateDirectories(skinsRoot).Order())
        {
            var manifestPath = Path.Combine(skinDirectory, "skin.json");
            if (!File.Exists(manifestPath))
            {
                continue;
            }

            var manifest = LoadManifest(manifestPath);
            if (manifest is not null)
            {
                manifests.Add(manifest);
            }
        }

        return manifests;
    }

    private static PetSkinManifest? LoadManifest(string manifestPath)
    {
        try
        {
            using var stream = File.OpenRead(manifestPath);
            var manifest = JsonSerializer.Deserialize(stream, PetSkinJsonContext.Default.PetSkinManifest);
            if (manifest is not null)
            {
                manifest.BaseDirectory = Path.GetDirectoryName(manifestPath) ?? string.Empty;
            }

            return manifest;
        }
        catch
        {
            return null;
        }
    }
}

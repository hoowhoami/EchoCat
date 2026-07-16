namespace EchoCat.Desktop.Skins;

public sealed class PetSkinCatalog
{
    private readonly IReadOnlyList<IPetSkin> _skins;

    public PetSkinCatalog()
    {
        var loader = new PetSkinManifestLoader();
        var manifests = loader.LoadManifests(Path.Combine(AppContext.BaseDirectory, "skins"));
        var skins = manifests.Select(CreateSkin).OfType<IPetSkin>().ToList();

        if (skins.Count == 0)
        {
            skins.Add(new SoftVectorCatSkin());
        }

        _skins = skins;
    }

    public IReadOnlyList<IPetSkin> Skins => _skins;

    public IPetSkin ActiveSkin => _skins[0];

    public IPetSkin ActivePetSkin => _skins[0];

    public IPetSkin GetSkin(string id)
    {
        return _skins
            .FirstOrDefault(skin => skin.Id == id)
            ?? ActivePetSkin;
    }

    private static IPetSkin? CreateSkin(PetSkinManifest manifest)
    {
        return manifest.Renderer switch
        {
            SkinRendererIds.SoftVectorCat => new SoftVectorCatSkin(manifest),
            SkinRendererIds.SpriteSequence => new SpritePetSkin(manifest),
            _ => null
        };
    }
}

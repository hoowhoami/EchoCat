using EchoCat.Core;

namespace EchoCat.Desktop.Skins;

public interface IPetSkin
{
    string Id { get; }

    string DisplayName { get; }

    PetSkinPalette CreatePalette(PetMood mood);
}

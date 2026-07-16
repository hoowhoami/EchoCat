using Avalonia.Media;

namespace EchoCat.Desktop.Skins;

public interface IVectorPetSkin : IPetSkin
{
    void Render(DrawingContext context, PetRenderContext renderContext);
}

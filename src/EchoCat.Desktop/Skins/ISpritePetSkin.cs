using Avalonia.Media;

namespace EchoCat.Desktop.Skins;

public interface ISpritePetSkin : IPetSkin
{
    void Render(DrawingContext context, PetRenderContext renderContext);
}

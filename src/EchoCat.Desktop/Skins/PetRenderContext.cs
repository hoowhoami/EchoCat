using Avalonia;
using EchoCat.Core;

namespace EchoCat.Desktop.Skins;

public sealed record PetRenderContext(
    Rect Bounds,
    PetMood Mood,
    string Action,
    double BreathingPhase,
    PetSkinPalette Palette,
    double LookX,
    double LookY);

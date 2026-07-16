using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using EchoCat.Core;
using EchoCat.Desktop.Skins;

namespace EchoCat.Desktop.Controls;

public sealed class PetAvatarControl : Control
{
    public static readonly StyledProperty<PetMood> MoodProperty =
        AvaloniaProperty.Register<PetAvatarControl, PetMood>(nameof(Mood), PetMood.Calm);

    public static readonly StyledProperty<string> ActionProperty =
        AvaloniaProperty.Register<PetAvatarControl, string>(nameof(Action), "loaf");

    public static readonly StyledProperty<double> BreathingPhaseProperty =
        AvaloniaProperty.Register<PetAvatarControl, double>(nameof(BreathingPhase));

    public static readonly StyledProperty<double> LookXProperty =
        AvaloniaProperty.Register<PetAvatarControl, double>(nameof(LookX));

    public static readonly StyledProperty<double> LookYProperty =
        AvaloniaProperty.Register<PetAvatarControl, double>(nameof(LookY));

    public static readonly StyledProperty<PetSkinPalette> PaletteProperty =
        AvaloniaProperty.Register<PetAvatarControl, PetSkinPalette>(
            nameof(Palette),
            PetSkinPalette.Default(PetMood.Calm));

    public static readonly StyledProperty<IPetSkin> SkinProperty =
        AvaloniaProperty.Register<PetAvatarControl, IPetSkin>(
            nameof(Skin),
            new SoftVectorCatSkin());

    public PetMood Mood
    {
        get => GetValue(MoodProperty);
        set => SetValue(MoodProperty, value);
    }

    public string Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    public double BreathingPhase
    {
        get => GetValue(BreathingPhaseProperty);
        set => SetValue(BreathingPhaseProperty, value);
    }

    public double LookX
    {
        get => GetValue(LookXProperty);
        set => SetValue(LookXProperty, value);
    }

    public double LookY
    {
        get => GetValue(LookYProperty);
        set => SetValue(LookYProperty, value);
    }

    public PetSkinPalette Palette
    {
        get => GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public IPetSkin Skin
    {
        get => GetValue(SkinProperty);
        set => SetValue(SkinProperty, value);
    }

    static PetAvatarControl()
    {
        AffectsRender<PetAvatarControl>(
            MoodProperty,
            ActionProperty,
            BreathingPhaseProperty,
            LookXProperty,
            LookYProperty,
            PaletteProperty,
            SkinProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var renderContext = new PetRenderContext(Bounds, Mood, Action, BreathingPhase, Palette, LookX, LookY);
        switch (Skin)
        {
            case IVectorPetSkin vectorSkin:
                vectorSkin.Render(context, renderContext);
                break;
            case ISpritePetSkin spriteSkin:
                spriteSkin.Render(context, renderContext);
                break;
        }
    }
}

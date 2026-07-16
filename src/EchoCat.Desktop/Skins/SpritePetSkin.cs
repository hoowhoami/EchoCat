using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using EchoCat.Core;

namespace EchoCat.Desktop.Skins;

public sealed class SpritePetSkin : ISpritePetSkin
{
    private readonly Dictionary<string, Bitmap> _frames = new(StringComparer.OrdinalIgnoreCase);
    private readonly PetSkinManifest _manifest;

    public SpritePetSkin(PetSkinManifest manifest)
    {
        _manifest = manifest;
        LoadFrames();
    }

    public string Id => _manifest.Id;

    public string DisplayName => _manifest.DisplayName;

    public PetSkinPalette CreatePalette(PetMood mood)
    {
        return PetSkinPalette.FromManifest(_manifest.Palette, mood);
    }

    public void Render(DrawingContext context, PetRenderContext renderContext)
    {
        var frame = SelectFrame(renderContext);
        if (frame is null)
        {
            DrawMissingFrame(context, renderContext);
            return;
        }

        var destination = Fit(frame.Size.Width, frame.Size.Height, renderContext.Bounds);
        context.DrawImage(frame, destination);
    }

    private void LoadFrames()
    {
        var sprite = _manifest.Sprite;
        if (sprite is null)
        {
            return;
        }

        TryLoadFrame("default", sprite.DefaultFrame);
        foreach (var (key, path) in sprite.Frames)
        {
            TryLoadFrame(key, path);
        }
    }

    private void TryLoadFrame(string key, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var path = Path.Combine(_manifest.BaseDirectory, relativePath);
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            _frames[key] = new Bitmap(path);
        }
        catch
        {
            // Invalid skin assets should not prevent EchoCat from launching.
        }
    }

    private Bitmap? SelectFrame(PetRenderContext renderContext)
    {
        if (_frames.TryGetValue(renderContext.Action, out var byAction))
        {
            return byAction;
        }

        if (_frames.TryGetValue(renderContext.Mood.ToString().ToLowerInvariant(), out var byMood))
        {
            return byMood;
        }

        return _frames.GetValueOrDefault("default");
    }

    private static Rect Fit(double sourceWidth, double sourceHeight, Rect bounds)
    {
        var scale = Math.Min(bounds.Width / sourceWidth, bounds.Height / sourceHeight);
        var width = sourceWidth * scale;
        var height = sourceHeight * scale;
        return new Rect(
            bounds.X + (bounds.Width - width) / 2,
            bounds.Y + (bounds.Height - height) / 2,
            width,
            height);
    }

    private static void DrawMissingFrame(DrawingContext context, PetRenderContext renderContext)
    {
        var palette = renderContext.Palette;
        var bounds = renderContext.Bounds.Deflate(18);
        var fill = new SolidColorBrush(palette.Fur);
        var accent = new SolidColorBrush(palette.Accent);
        var pen = new Pen(new SolidColorBrush(palette.Ink), 4, lineCap: PenLineCap.Round);

        context.DrawRectangle(fill, pen, bounds, 28, 28);
        context.DrawEllipse(accent, null, bounds.Center, 18, 18);
        context.DrawLine(pen, bounds.TopLeft + new Point(34, 34), bounds.BottomRight - new Point(34, 34));
        context.DrawLine(pen, bounds.TopRight + new Point(-34, 34), bounds.BottomLeft + new Point(34, -34));
    }
}

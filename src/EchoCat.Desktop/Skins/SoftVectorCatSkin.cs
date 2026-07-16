using EchoCat.Core;
using Avalonia;
using Avalonia.Media;

namespace EchoCat.Desktop.Skins;

public sealed class SoftVectorCatSkin : IVectorPetSkin
{
    private readonly PetSkinManifest _manifest;

    public SoftVectorCatSkin()
        : this(new PetSkinManifest())
    {
    }

    public SoftVectorCatSkin(PetSkinManifest manifest)
    {
        _manifest = manifest;
    }

    public string Id => _manifest.Id;

    public string DisplayName => _manifest.DisplayName;

    public PetSkinPalette CreatePalette(PetMood mood)
    {
        return PetSkinPalette.FromManifest(_manifest.Palette, mood);
    }

    public void Render(DrawingContext context, PetRenderContext renderContext)
    {
        var bounds = renderContext.Bounds;
        var size = Math.Min(bounds.Width, bounds.Height);
        var center = new Point(bounds.Width / 2, bounds.Height / 2 + 8);
        var breath = Math.Sin(renderContext.BreathingPhase * Math.PI * 2) * 4;
        var palette = renderContext.Palette;

        var fur = new SolidColorBrush(palette.Fur);
        var furShadow = new SolidColorBrush(palette.FurShadow);
        var ink = new SolidColorBrush(palette.Ink);
        var accent = new SolidColorBrush(palette.Accent);
        var blush = new SolidColorBrush(palette.Blush);
        var softLine = new Pen(new SolidColorBrush(palette.Ink), 4, lineCap: PenLineCap.Round);

        var scale = size / 220;
        using var _ = context.PushTransform(Matrix.CreateScale(scale, scale)
            * Matrix.CreateTranslation(center.X - 110 * scale, center.Y - 110 * scale + breath));

        DrawTail(context, renderContext.Action, renderContext.BreathingPhase, accent, softLine);
        DrawEars(context, fur, furShadow, accent, softLine);
        DrawBody(context, renderContext.Action, fur, furShadow, softLine);
        DrawFace(context, renderContext.Mood, renderContext.Action, renderContext.LookX, renderContext.LookY, ink, blush, accent);
        DrawActionAccent(context, renderContext.Action, accent, ink);
    }

    private static void DrawTail(DrawingContext context, string action, double phase, IBrush accent, Pen line)
    {
        var sway = action is "purr" or "sway" or "stretch" or "peek"
            ? Math.Sin(phase * Math.PI * 2) * 7
            : 0;

        var tail = new StreamGeometry();
        using (var g = tail.Open())
        {
            g.BeginFigure(new Point(166, 128), false);
            g.CubicBezierTo(new Point(214 + sway, 112), new Point(216 + sway, 52), new Point(176, 58));
            g.CubicBezierTo(new Point(144, 63), new Point(156, 98), new Point(184 + sway, 88));
        }

        context.DrawGeometry(null, new Pen(accent, 15, lineCap: PenLineCap.Round), tail);
        context.DrawGeometry(null, line, tail);
    }

    private static void DrawEars(DrawingContext context, IBrush fur, IBrush shadow, IBrush accent, Pen line)
    {
        var leftEar = new StreamGeometry();
        using (var g = leftEar.Open())
        {
            g.BeginFigure(new Point(54, 72), true);
            g.LineTo(new Point(72, 22));
            g.LineTo(new Point(104, 70));
            g.EndFigure(true);
        }

        var rightEar = new StreamGeometry();
        using (var g = rightEar.Open())
        {
            g.BeginFigure(new Point(116, 70), true);
            g.LineTo(new Point(148, 22));
            g.LineTo(new Point(166, 74));
            g.EndFigure(true);
        }

        context.DrawGeometry(fur, line, leftEar);
        context.DrawGeometry(fur, line, rightEar);
        context.DrawEllipse(shadow, null, new Point(76, 58), 13, 18);
        context.DrawEllipse(accent, null, new Point(144, 58), 13, 18);
    }

    private static void DrawBody(DrawingContext context, string action, IBrush fur, IBrush shadow, Pen line)
    {
        context.DrawEllipse(shadow, null, new Point(112, 158), 74, 44);
        context.DrawEllipse(fur, line, new Point(110, 105), 66, 58);
        context.DrawEllipse(fur, line, new Point(110, 153), 76, 48);

        if (action == "stretch")
        {
            context.DrawLine(line, new Point(74, 155), new Point(47, 145));
            context.DrawLine(line, new Point(146, 155), new Point(173, 145));
            context.DrawEllipse(shadow, null, new Point(48, 145), 10, 7);
            context.DrawEllipse(shadow, null, new Point(172, 145), 10, 7);
            return;
        }

        context.DrawEllipse(shadow, null, new Point(82, 168), 18, 10);
        context.DrawEllipse(shadow, null, new Point(138, 168), 18, 10);
    }

    private static void DrawFace(
        DrawingContext context,
        PetMood mood,
        string action,
        double lookX,
        double lookY,
        IBrush ink,
        IBrush blush,
        IBrush accent)
    {
        if (mood == PetMood.Sleepy || action == "nap")
        {
            context.DrawLine(new Pen(ink, 4, lineCap: PenLineCap.Round), new Point(79, 100), new Point(96, 96));
            context.DrawLine(new Pen(ink, 4, lineCap: PenLineCap.Round), new Point(124, 96), new Point(141, 100));
        }
        else if (action == "blink")
        {
            context.DrawLine(new Pen(ink, 4, lineCap: PenLineCap.Round), new Point(80, 98), new Point(96, 98));
            context.DrawLine(new Pen(ink, 4, lineCap: PenLineCap.Round), new Point(124, 98), new Point(140, 98));
        }
        else
        {
            var eyeOffset = new Point(lookX * 2.4, lookY * 2);
            context.DrawEllipse(ink, null, new Point(88 + eyeOffset.X, 97 + eyeOffset.Y), 7, 10);
            context.DrawEllipse(ink, null, new Point(132 + eyeOffset.X, 97 + eyeOffset.Y), 7, 10);
            context.DrawEllipse(Brushes.White, null, new Point(91 + eyeOffset.X, 92 + eyeOffset.Y), 2.5, 3);
            context.DrawEllipse(Brushes.White, null, new Point(135 + eyeOffset.X, 92 + eyeOffset.Y), 2.5, 3);
        }

        var blushScale = action == "purr" ? 1.25 : 1;
        context.DrawEllipse(blush, null, new Point(68, 116), 11 * blushScale, 6 * blushScale);
        context.DrawEllipse(blush, null, new Point(152, 116), 11 * blushScale, 6 * blushScale);
        context.DrawEllipse(accent, null, new Point(110, 109), 5, 4);
        context.DrawLine(new Pen(ink, 3, lineCap: PenLineCap.Round), new Point(110, 113), new Point(110, 120));
        context.DrawLine(new Pen(ink, 3, lineCap: PenLineCap.Round), new Point(110, 120), new Point(101, 126));
        context.DrawLine(new Pen(ink, 3, lineCap: PenLineCap.Round), new Point(110, 120), new Point(119, 126));
    }

    private static void DrawActionAccent(DrawingContext context, string action, IBrush accent, IBrush ink)
    {
        switch (action)
        {
            case "listen":
            case "reply":
                context.DrawEllipse(accent, null, new Point(164, 38), 4, 4);
                context.DrawEllipse(accent, null, new Point(178, 30), 3, 3);
                context.DrawEllipse(accent, null, new Point(190, 42), 2.5, 2.5);
                break;
            case "nap":
                var pen = new Pen(ink, 3, lineCap: PenLineCap.Round);
                context.DrawLine(pen, new Point(162, 38), new Point(176, 38));
                context.DrawLine(pen, new Point(176, 38), new Point(162, 52));
                context.DrawLine(pen, new Point(162, 52), new Point(176, 52));
                break;
            case "stretch":
                context.DrawEllipse(accent, null, new Point(48, 126), 3, 3);
                context.DrawEllipse(accent, null, new Point(172, 126), 3, 3);
                context.DrawLine(new Pen(accent, 3, lineCap: PenLineCap.Round), new Point(39, 133), new Point(33, 127));
                context.DrawLine(new Pen(accent, 3, lineCap: PenLineCap.Round), new Point(181, 133), new Point(187, 127));
                break;
            case "peek":
                context.DrawEllipse(accent, null, new Point(110, 38), 3, 3);
                context.DrawEllipse(accent, null, new Point(122, 35), 2.5, 2.5);
                break;
        }
    }
}

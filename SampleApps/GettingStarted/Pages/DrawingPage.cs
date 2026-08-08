using System;
using Cairo;
using Gtk;

namespace GettingStarted.Pages
{
    /// <summary>
    /// "Custom drawing with Cairo". Gtk 4 renders through a retained scene
    /// graph, and the Cairo path is confined to GtkDrawingArea -- OnDrawn is
    /// gone. The body of a Gtk 3 draw handler ports across unchanged; only how
    /// the context arrives is different.
    /// </summary>
    public class DrawingPage : TourPage
    {
        private readonly DrawingArea _area = new DrawingArea();
        private double _angle = 0.25;
        private int _petals = 6;

        public DrawingPage() : base(
            "Drawing with Cairo",
            "Assign a DrawFunc to a DrawingArea. It is handed a Cairo context and the current "
          + "size, and QueueDraw asks for another pass when your state changes.")
        {
            _area.SetSizeRequest(320, 240);
            _area.Hexpand = true;

            // The size arrives as arguments rather than being read out of an
            // allocation, which is the only structural change from Gtk 3.
            _area.DrawFunc = Draw;

            Append(Group("DrawingArea.DrawFunc", DrawingDemo()));
            Append(Note(
                "Dispose every Cairo object you create. Path, ImageSurface, Context and Pattern "
              + "are IDisposable, and leaking one does not merely warn -- the finalizer takes "
              + "the process down at whatever moment the GC next runs, so the crash lands "
              + "nowhere near the cause. The context handed to a DrawFunc is not yours; "
              + "anything you create inside it is."));
        }

        private Widget DrawingDemo()
        {
            var column = Column();
            column.Append(new Frame { Child = _area });

            var petals = new Scale(Orientation.Horizontal, 3, 12, 1) { Hexpand = true, DrawValue = true };
            petals.Value = _petals;
            petals.ValueChanged += (o, e) =>
            {
                _petals = (int) petals.Value;
                _area.QueueDraw();               // state changed: ask for a redraw
                Report($"{_petals} petals, redrawn through QueueDraw.");
            };

            var angle = new Scale(Orientation.Horizontal, 0, 1, 0.01) { Hexpand = true };
            angle.Value = _angle;
            angle.ValueChanged += (o, e) =>
            {
                _angle = angle.Value;
                _area.QueueDraw();
            };

            var grid = new Grid { ColumnSpacing = 8, RowSpacing = 4 };
            grid.Attach(new Label("Petals") { Xalign = 0 }, 0, 0, 1, 1);
            grid.Attach(petals, 1, 0, 1, 1);
            grid.Attach(new Label("Rotation") { Xalign = 0 }, 0, 1, 1, 1);
            grid.Attach(angle, 1, 1, 1, 1);

            column.Append(grid);
            return column;
        }

        private void Draw(DrawingArea area, Context cr, int width, int height)
        {
            cr.SetSourceRGB(0.97, 0.97, 0.98);
            cr.Rectangle(0, 0, width, height);
            cr.Fill();

            var radius = Math.Min(width, height) / 2d - 12;
            cr.Translate(width / 2d, height / 2d);
            cr.Rotate(_angle * 2 * Math.PI);

            for (var i = 0; i < _petals; i++)
            {
                cr.Save();
                cr.Rotate(i * 2 * Math.PI / _petals);
                cr.SetSourceRGBA(0.75, 0.1, 0.2, 0.55);
                cr.MoveTo(0, 0);
                cr.CurveTo(radius * 0.4, -radius * 0.4, radius * 0.9, -radius * 0.2, radius, 0);
                cr.CurveTo(radius * 0.9, radius * 0.2, radius * 0.4, radius * 0.4, 0, 0);
                cr.FillPreserve();
                cr.SetSourceRGB(0.4, 0.05, 0.1);
                cr.LineWidth = 1.5;
                cr.Stroke();
                cr.Restore();
            }

            // Pango rather than Cairo's toy text API: it is the one that knows
            // about fonts, shaping and measurement.
            cr.Rotate(-_angle * 2 * Math.PI);
            using (var layout = Pango.CairoHelper.CreateLayout(cr))
            {
                layout.FontDescription = Pango.FontDescription.FromString("Sans Bold 11");
                layout.SetText($"{_petals}");
                layout.GetPixelSize(out var textWidth, out var textHeight);

                cr.SetSourceRGB(1, 1, 1);
                cr.Arc(0, 0, 16, 0, 2 * Math.PI);
                cr.Fill();

                cr.SetSourceRGB(0.2, 0.2, 0.2);
                cr.MoveTo(-textWidth / 2d, -textHeight / 2d);
                Pango.CairoHelper.ShowLayout(cr, layout);
            }
        }
    }
}

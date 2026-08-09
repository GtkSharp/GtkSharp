using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Cairo;
using Xunit;
using IOPath = System.IO.Path;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The surfaces that are not <c>ImageSurface</c>: the three paginated
    /// backends that write a file (PDF, PostScript, SVG), the recording surface
    /// that keeps operations instead of pixels, the subsurface view onto part of
    /// another surface, and <c>Cairo.Device</c>, which nothing could reach at all
    /// because no property handed one out.
    /// </summary>
    /// <remarks>
    /// The oracle here is deliberately outside the library. A vector backend's
    /// whole job is to write a file in a format somebody else specified, so the
    /// bytes on disk are checked against that format rather than against
    /// anything cairo remembers: the SVG is parsed as XML and its path data read
    /// back as numbers, the PostScript is checked against the DSC comments the
    /// test asked for and against page bounding boxes this file works out from
    /// the flipped y axis, and the PDF against its version header and the
    /// <c>/MediaBox</c> entries <c>SetSize</c> is supposed to produce.
    ///
    /// For the recording surface the oracle is arithmetic: the ink extents of a
    /// filled rectangle are that rectangle, and the ink extents of a stroke are
    /// the line grown by half the pen on each side. Replay is checked in pixels,
    /// each positive against a control coordinate that must stay blank.
    ///
    /// Nothing here touches Gtk, so nothing needs the fixture's thread; Cairo has
    /// no thread affinity of its own.
    /// </remarks>
    public class CairoSurfaceTests : GtkTestBase, IDisposable
    {
        private readonly string _dir;

        public CairoSurfaceTests(GtkFixture fixture) : base(fixture)
        {
            _dir = IOPath.Combine(IOPath.GetTempPath(), "gtksharp-cairo-surfaces-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, true); }
            catch (IOException) { /* the file is the test's evidence, not its subject */ }
        }

        private string PathFor(string name) => IOPath.Combine(_dir, name);

        /// <summary>Reads a pixel as (b, g, r, a) — ARGB32 is premultiplied and
        /// stored little-endian, so byte 0 is blue.</summary>
        private static (byte b, byte g, byte r, byte a) PixelAt(ImageSurface surface, int x, int y)
        {
            surface.Flush();
            var data = surface.Data;
            var offset = y * surface.Stride + x * 4;
            return (data[offset], data[offset + 1], data[offset + 2], data[offset + 3]);
        }

        // --------------------------------------------------------- recording

        /// <summary>
        /// A recording surface's ink extents are the bounding box of what has
        /// been drawn into it, in its own user space — so for a filled
        /// rectangle they are that rectangle, exactly. The empty surface is the
        /// control: with nothing recorded the box has to be empty, otherwise
        /// "the extents grew" would prove nothing.
        /// </summary>
        [Fact]
        public void Ink_extents_are_the_rectangle_that_was_filled()
        {
            using var recording = new RecordingSurface(Content.ColorAlpha);

            var empty = recording.InkExtents;
            Assert.Equal(0.0, empty.Width);
            Assert.Equal(0.0, empty.Height);

            using (var cr = new Context(recording))
            {
                cr.SetSourceRGB(0, 0, 1);
                cr.Rectangle(10, 20, 30, 40);
                cr.Fill();
            }

            var ink = recording.InkExtents;
            Assert.Equal(10.0, ink.X);
            Assert.Equal(20.0, ink.Y);
            Assert.Equal(30.0, ink.Width);
            Assert.Equal(40.0, ink.Height);
        }

        /// <summary>
        /// A stroke covers half the line width on each side of the path, so a
        /// vertical segment from (50,50) to (50,80) drawn with pen width w has
        /// ink extents (50 - w/2, 50, w, 30) — butt caps add nothing at the
        /// ends. Two pen widths are measured so the assertion pins the
        /// relationship rather than one number, and the four out-parameters
        /// cannot be permuted without failing: x and y differ, and so do width
        /// and height.
        /// </summary>
        [Theory]
        [InlineData(10.0)]
        [InlineData(2.0)]
        public void Ink_extents_of_a_stroke_grow_by_half_the_pen_on_each_side(double penWidth)
        {
            using var recording = new RecordingSurface(Content.ColorAlpha);

            using (var cr = new Context(recording))
            {
                cr.LineWidth = penWidth;
                cr.LineCap = LineCap.Butt;
                cr.MoveTo(50, 50);
                cr.LineTo(50, 80);
                cr.Stroke();
            }

            var ink = recording.InkExtents;
            Assert.Equal(50 - penWidth / 2, ink.X);
            Assert.Equal(50.0, ink.Y);
            Assert.Equal(penWidth, ink.Width);
            Assert.Equal(30.0, ink.Height);
        }

        /// <summary>
        /// The point of recording is replay: the same recording painted at two
        /// origins has to appear twice, at both. The gap between the two copies
        /// is the control — a recording that painted everywhere, or one that
        /// ignored the source offset, would fill it.
        /// </summary>
        [Fact]
        public void A_recording_replays_at_every_origin_it_is_painted_from()
        {
            using var recording = new RecordingSurface(Content.ColorAlpha);
            using (var cr = new Context(recording))
            {
                cr.SetSourceRGB(0, 1, 0);
                cr.Rectangle(0, 0, 5, 5);
                cr.Fill();
            }

            using var image = new ImageSurface(Format.ARGB32, 40, 40);
            using (var cr = new Context(image))
            {
                cr.SetSource(recording, 1, 1);
                cr.Paint();
                cr.SetSource(recording, 20, 20);
                cr.Paint();
            }

            Assert.Equal((byte)255, PixelAt(image, 3, 3).g);
            Assert.Equal((byte)255, PixelAt(image, 22, 22).g);
            Assert.Equal((byte)0, PixelAt(image, 12, 12).a);
        }

        /// <summary>
        /// A bounded recording keeps only what falls inside the extents it was
        /// created with. The unbounded surface beside it is the control: given
        /// the identical drawing it records the whole 100x100, so the 20x20 that
        /// comes back from the bounded one is the clip and not the drawing.
        /// </summary>
        [Fact]
        public void A_bounded_recording_clips_what_it_keeps_to_its_extents()
        {
            using (var unbounded = new RecordingSurface(Content.ColorAlpha))
            {
                using (var cr = new Context(unbounded))
                {
                    cr.Rectangle(0, 0, 100, 100);
                    cr.Fill();
                }

                Assert.Equal(100.0, unbounded.InkExtents.Width);
                Assert.Equal(100.0, unbounded.InkExtents.Height);
            }

            using var bounded = new RecordingSurface(Content.ColorAlpha, new Rectangle(0, 0, 20, 20));
            using (var cr = new Context(bounded))
            {
                cr.SetSourceRGB(1, 0, 0);
                cr.Rectangle(0, 0, 100, 100);
                cr.Fill();
            }

            Assert.Equal(20.0, bounded.InkExtents.Width);
            Assert.Equal(20.0, bounded.InkExtents.Height);

            using var image = new ImageSurface(Format.ARGB32, 40, 40);
            using (var cr = new Context(image))
            {
                cr.SetSource(bounded, 0, 0);
                cr.Paint();
            }

            Assert.Equal((byte)255, PixelAt(image, 10, 10).r);
            Assert.Equal((byte)0, PixelAt(image, 30, 30).a);
        }

        /// <summary>
        /// <c>GetExtents</c> is how a caller tells the two kinds apart, and it
        /// reports the rectangle the surface was created with rather than what
        /// has been drawn — the surface here is bounded at (5,6,20,30) and has
        /// nothing in it at all.
        /// </summary>
        [Fact]
        public void GetExtents_separates_a_bounded_recording_from_an_unbounded_one()
        {
            using (var unbounded = new RecordingSurface(Content.ColorAlpha))
            {
                Assert.False(unbounded.GetExtents(out _));
            }

            using var bounded = new RecordingSurface(Content.ColorAlpha, new Rectangle(5, 6, 20, 30));
            Assert.True(bounded.GetExtents(out var extents));
            Assert.Equal(5.0, extents.X);
            Assert.Equal(6.0, extents.Y);
            Assert.Equal(20.0, extents.Width);
            Assert.Equal(30.0, extents.Height);
        }

        /// <summary>
        /// <c>cairo_surface_get_type</c> returns 16 for a recording surface and
        /// 14 for a script one; <c>SurfaceType</c> stopped at Svg = 10, so both
        /// came back as an integer no member named and <c>Surface.Lookup</c>
        /// fell through to the base class. The numbers are asserted as well as
        /// the names because the enum is positional: inserting a member would
        /// silently renumber every later one.
        /// </summary>
        [Fact]
        public void Each_backend_reports_its_own_surface_type()
        {
            using var image = new ImageSurface(Format.ARGB32, 4, 4);
            Assert.Equal(SurfaceType.Image, image.SurfaceType);

            using var recording = new RecordingSurface(Content.ColorAlpha);
            Assert.Equal(SurfaceType.Recording, recording.SurfaceType);
            Assert.Equal(16, (int)SurfaceType.Recording);

            using (var pdf = new PdfSurface(PathFor("type.pdf"), 10, 10))
                Assert.Equal(SurfaceType.Pdf, pdf.SurfaceType);

            using (var ps = new PSSurface(PathFor("type.ps"), 10, 10))
                Assert.Equal(SurfaceType.PS, ps.SurfaceType);

            using (var svg = new SvgSurface(PathFor("type.svg"), 10, 10))
                Assert.Equal(SurfaceType.Svg, svg.SurfaceType);

            Assert.Equal(14, (int)SurfaceType.Script);

            using var looked = Surface.Lookup(recording.Handle, false);
            Assert.IsType<RecordingSurface>(looked);
        }

        // -------------------------------------------------------- subsurface

        /// <summary>
        /// A subsurface is a window onto a rectangle of its target: painting the
        /// whole of it fills exactly that rectangle of the parent and nothing
        /// else. Both controls matter — a pixel before the origin and a pixel
        /// past the far corner — because an offset that was dropped and a clip
        /// that was dropped fail differently.
        /// </summary>
        [Fact]
        public void Drawing_into_a_subsurface_lands_in_the_parent_at_the_offset()
        {
            using var image = new ImageSurface(Format.ARGB32, 40, 40);

            using (var sub = image.CreateForRectangle(10, 10, 20, 20))
            using (var cr = new Context(sub))
            {
                cr.SetSourceRGB(1, 0, 0);
                cr.Paint();
            }

            Assert.Equal((byte)255, PixelAt(image, 15, 15).r);
            Assert.Equal((byte)255, PixelAt(image, 29, 29).a);
            Assert.Equal((byte)0, PixelAt(image, 5, 5).a);
            Assert.Equal((byte)0, PixelAt(image, 35, 35).a);
        }

        /// <summary>
        /// The expectation that a subsurface would report
        /// <c>SurfaceType.Subsurface</c> was wrong, not the binding: cairo's
        /// <c>_cairo_surface_create_for_rectangle_int</c> copies the target's
        /// type onto the new surface, so a view onto an image surface says
        /// Image. The type therefore cannot be used to tell a subsurface from
        /// its target, and this pins that rather than the value 23.
        /// </summary>
        [Fact]
        public void A_subsurface_reports_the_type_of_the_surface_it_views()
        {
            using var image = new ImageSurface(Format.ARGB32, 40, 40);
            using var sub = image.CreateForRectangle(10, 10, 20, 20);

            Assert.Equal(image.SurfaceType, sub.SurfaceType);
            Assert.Equal(SurfaceType.Image, sub.SurfaceType);
            Assert.NotEqual(image.Handle, sub.Handle);
        }

        // --------------------------------------------------------------- svg

        private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

        private static double[] NumbersIn(string text) =>
            Regex.Matches(text, @"-?\d+(?:\.\d+)?")
                 .Cast<Match>()
                 .Select(m => double.Parse(m.Value, CultureInfo.InvariantCulture))
                 .ToArray();

        /// <summary>
        /// The SVG backend's output is checked as XML rather than as a string:
        /// the document declares the size it was created with, and the filled
        /// rectangle arrives as a path whose data contains all four corners the
        /// test chose. The second document is the control — same size, nothing
        /// drawn, and therefore no path at all — so "there is a path" is a fact
        /// about the drawing and not about the backend's boilerplate.
        /// </summary>
        [Fact]
        public void The_svg_written_for_a_fill_carries_the_corners_that_were_drawn()
        {
            var drawn = PathFor("drawn.svg");
            using (var surface = new SvgSurface(drawn, 100, 50))
            {
                using (var cr = new Context(surface))
                {
                    cr.SetSourceRGB(1, 0, 0);
                    cr.Rectangle(10, 20, 30, 25);
                    cr.Fill();
                }
                surface.Finish();
            }

            var document = XDocument.Load(drawn);
            Assert.Equal(Svg + "svg", document.Root.Name);
            Assert.Equal("100", document.Root.Attribute("width").Value);
            Assert.Equal("50", document.Root.Attribute("height").Value);
            Assert.Equal("0 0 100 50", document.Root.Attribute("viewBox").Value);

            var paths = document.Root.Descendants(Svg + "path").ToList();
            var corners = NumbersIn(Assert.Single(paths).Attribute("d").Value);

            // The rectangle is x in [10,40] and y in [20,45]; cairo emits it as
            // a closed polyline, so every corner coordinate has to be present.
            Assert.Contains(10.0, corners);
            Assert.Contains(20.0, corners);
            Assert.Contains(40.0, corners);
            Assert.Contains(45.0, corners);
            Assert.DoesNotContain(99.0, corners);

            var empty = PathFor("empty.svg");
            using (var surface = new SvgSurface(empty, 100, 50))
                surface.Finish();

            var blank = XDocument.Load(empty);
            Assert.Equal("100", blank.Root.Attribute("width").Value);
            Assert.Empty(blank.Root.Descendants(Svg + "path"));
        }

        /// <summary>
        /// The document unit is a label on the two size attributes and nothing
        /// more: user space, and so every coordinate in the path data, is
        /// untouched. Asserting the path data is byte-identical between the two
        /// files is what makes this a test of the unit rather than of the
        /// drawing.
        /// </summary>
        [Fact]
        public void The_document_unit_relabels_the_size_without_moving_the_geometry()
        {
            string Write(string name, SvgUnit? unit)
            {
                var path = PathFor(name);
                using (var surface = new SvgSurface(path, 100, 50))
                {
                    if (unit.HasValue)
                        surface.DocumentUnit = unit.Value;

                    Assert.Equal(unit ?? SvgUnit.User, surface.DocumentUnit);

                    using (var cr = new Context(surface))
                    {
                        cr.Rectangle(10, 20, 30, 25);
                        cr.Fill();
                    }
                    surface.Finish();
                }
                return path;
            }

            var plain = XDocument.Load(Write("unit-default.svg", null));
            var millimetres = XDocument.Load(Write("unit-mm.svg", SvgUnit.Mm));

            Assert.Equal("100", plain.Root.Attribute("width").Value);
            Assert.Equal("100mm", millimetres.Root.Attribute("width").Value);
            Assert.Equal("50mm", millimetres.Root.Attribute("height").Value);

            Assert.Equal(plain.Root.Attribute("viewBox").Value, millimetres.Root.Attribute("viewBox").Value);
            Assert.Equal(
                plain.Root.Descendants(Svg + "path").Single().Attribute("d").Value,
                millimetres.Root.Descendants(Svg + "path").Single().Attribute("d").Value);
        }

        // -------------------------------------------------------- postscript

        private static string[] Lines(string path) =>
            Encoding.ASCII.GetString(File.ReadAllBytes(path)).Split('\n');

        /// <summary>
        /// A DSC comment the caller adds has to reach the file's header. The
        /// control is a second document written the same way without the call:
        /// the title is absent from it, so its presence in the first is the
        /// comment and not something cairo writes anyway.
        /// </summary>
        [Fact]
        public void A_dsc_comment_reaches_the_postscript_header()
        {
            string Write(string name, string comment)
            {
                var path = PathFor(name);
                using (var surface = new PSSurface(path, 200, 100))
                {
                    if (comment != null)
                        surface.DscComment(comment);

                    using (var cr = new Context(surface))
                    {
                        cr.Rectangle(10, 10, 50, 50);
                        cr.Fill();
                    }
                    surface.Finish();
                }
                return path;
            }

            var titled = Lines(Write("titled.ps", "%%Title: a title the test chose"));
            var untitled = Lines(Write("untitled.ps", null));

            Assert.Equal("%!PS-Adobe-3.0", titled[0].TrimEnd('\r'));
            Assert.Contains("%%Title: a title the test chose", titled);
            Assert.DoesNotContain(untitled, line => line.StartsWith("%%Title: a title"));
        }

        /// <summary>
        /// Every <c>ShowPage</c> starts a new DSC page, and each page's
        /// <c>%%PageBoundingBox</c> is the ink of that page in PostScript
        /// coordinates — whose origin is bottom-left, so a rectangle at user y
        /// in [t, b] on a surface h tall lands at PS y in [h - b, h - t]. The
        /// test does that arithmetic itself; nothing is copied out of the file
        /// and compared with itself.
        /// </summary>
        [Fact]
        public void Each_page_of_a_postscript_document_declares_its_own_bounding_box()
        {
            const double height = 100;
            var path = PathFor("pages.ps");

            using (var surface = new PSSurface(path, 200, height))
            {
                using (var cr = new Context(surface))
                {
                    cr.Rectangle(10, 10, 50, 50);
                    cr.Fill();
                    cr.ShowPage();
                    cr.Rectangle(20, 20, 10, 10);
                    cr.Fill();
                }
                surface.Finish();
            }

            var lines = Lines(path).Select(l => l.TrimEnd('\r')).ToList();

            Assert.Contains("%%Pages: 2", lines);
            Assert.Contains("%%Page: 1 1", lines);
            Assert.Contains("%%Page: 2 2", lines);

            string Box(double x, double top, double w, double h) =>
                string.Format(CultureInfo.InvariantCulture, "%%PageBoundingBox: {0} {1} {2} {3}",
                              x, height - (top + h), x + w, height - top);

            var boxes = lines.Where(l => l.StartsWith("%%PageBoundingBox:")).ToList();
            Assert.Equal(new[] { Box(10, 10, 50, 50), Box(20, 20, 10, 10) }, boxes);
        }

        /// <summary>
        /// EPS is a different declaration in the very first line, and it
        /// restricts the document to one page. Both files here draw the same
        /// thing; only the flag differs, so the difference in the header is the
        /// flag.
        /// </summary>
        [Fact]
        public void Eps_mode_changes_the_leading_comment_and_allows_one_page()
        {
            string Write(string name, bool eps)
            {
                var path = PathFor(name);
                using (var surface = new PSSurface(path, 200, 100))
                {
                    Assert.False(surface.Eps);
                    surface.Eps = eps;
                    Assert.Equal(eps, surface.Eps);

                    using (var cr = new Context(surface))
                    {
                        cr.Rectangle(10, 10, 50, 50);
                        cr.Fill();
                    }
                    surface.Finish();
                }
                return path;
            }

            var encapsulated = Lines(Write("encapsulated.eps", true)).Select(l => l.TrimEnd('\r')).ToList();
            var plain = Lines(Write("plain.ps", false)).Select(l => l.TrimEnd('\r')).ToList();

            Assert.Equal("%!PS-Adobe-3.0 EPSF-3.0", encapsulated[0]);
            Assert.Equal("%!PS-Adobe-3.0", plain[0]);
            Assert.Contains("%%Pages: 1", encapsulated);

            // A whole-document %%BoundingBox is required of EPS and is the ink
            // of the single page, flipped the same way as above.
            Assert.Contains("%%BoundingBox: 10 40 60 90", encapsulated);
        }

        // --------------------------------------------------------------- pdf

        /// <summary>
        /// The version a PDF surface is restricted to is the version its header
        /// declares. Two files differing only in the restriction are compared,
        /// and the trailer is checked as well so the assertion is about a
        /// complete document rather than about the first eight bytes of a
        /// truncated one.
        /// </summary>
        [Theory]
        [InlineData(PdfVersion.OnePointFour, "%PDF-1.4")]
        [InlineData(PdfVersion.OnePointFive, "%PDF-1.5")]
        [InlineData(PdfVersion.OnePointSeven, "%PDF-1.7")]
        public void The_pdf_version_restriction_is_what_the_header_declares(PdfVersion version, string header)
        {
            var path = PathFor("v" + (int)version + ".pdf");
            using (var surface = new PdfSurface(path, 200, 100))
            {
                surface.RestrictToVersion(version);
                using (var cr = new Context(surface))
                {
                    cr.Rectangle(10, 10, 50, 50);
                    cr.Fill();
                }
                surface.Finish();
            }

            var bytes = File.ReadAllBytes(path);
            Assert.Equal(header, Encoding.ASCII.GetString(bytes, 0, header.Length));
            Assert.Equal("%%EOF\n", Encoding.ASCII.GetString(bytes, bytes.Length - 6, 6));
        }

        /// <summary>
        /// <c>SetSize</c> applies to the page that has not been emitted yet, so
        /// a document whose size is changed after the first <c>ShowPage</c> ends
        /// up with two differently sized pages. PDF 1.4 keeps the page tree as
        /// plain text, so the <c>/MediaBox</c> entries can be read straight out
        /// of the file — and there must be exactly two of them, in the order the
        /// pages were written.
        /// </summary>
        [Fact]
        public void SetSize_gives_the_next_pdf_page_its_own_media_box()
        {
            var path = PathFor("resized.pdf");
            using (var surface = new PdfSurface(path, 200, 100))
            {
                surface.RestrictToVersion(PdfVersion.OnePointFour);
                using (var cr = new Context(surface))
                {
                    cr.Rectangle(10, 10, 50, 50);
                    cr.Fill();
                    cr.ShowPage();
                    surface.SetSize(300, 400);
                    cr.Rectangle(20, 20, 10, 10);
                    cr.Fill();
                }
                surface.Finish();
            }

            // Latin1 maps every byte to the code point of the same value, so a
            // PDF's binary sections cannot throw off the search.
            var text = Encoding.Latin1.GetString(File.ReadAllBytes(path));
            var boxes = Regex.Matches(text, @"/MediaBox \[ ([^\]]+)\]")
                             .Cast<Match>()
                             .Select(m => m.Groups[1].Value.Trim())
                             .ToList();

            Assert.Equal(new List<string> { "0 0 200 100", "0 0 300 400" }, boxes);
            Assert.Contains("/Count 2", text);
        }

        // ------------------------------------------------------------ device

        /// <summary>
        /// <c>Cairo.Device</c> had no way in: its only constructor is internal
        /// and no property returned one, so nothing in the assembly could ever
        /// hand one to a caller. The image surface is the control — its backend
        /// has no device, and the property has to say so rather than wrap a null
        /// pointer.
        /// </summary>
        [SkippableFact]
        public void Only_a_backend_that_has_a_device_hands_one_out()
        {
            using (var image = new ImageSurface(Format.ARGB32, 4, 4))
                Assert.Null(image.Device);

            // The script backend is a build option of the installed cairo, not
            // of this binding; where it is absent the symbol is missing and the
            // delegate is null.
            Skip.IfNot(Script.IsSupported, "this cairo was built without the script backend");

            using var script = new Script(PathFor("device.cs2"));
            Assert.Equal(DeviceType.Script, script.Type);
            Assert.Equal(Status.Success, script.Status);

            using var surface = new ScriptSurface(script, Content.ColorAlpha, 10, 10);
            using var fromSurface = surface.Device;

            Assert.NotNull(fromSurface);
            Assert.Equal(DeviceType.Script, fromSurface.Type);
            // Same device, reached the other way round — the surface does not
            // get a copy of its own.
            Assert.Equal(script.Handle, fromSurface.Handle);
        }

        /// <summary>
        /// The script backend writes the operations out as text, which makes it
        /// the one backend whose file can be read as a transcript of what the
        /// context did. The control is a script surface that draws nothing: its
        /// device file has the magic line and no operations, so the operators in
        /// the other file are the drawing.
        /// </summary>
        [SkippableFact]
        public void A_script_surface_writes_down_the_operations_it_was_given()
        {
            Skip.IfNot(Script.IsSupported, "this cairo was built without the script backend");

            string Write(string name, bool draw)
            {
                var path = PathFor(name);
                using (var device = new Script(path))
                {
                    using (var surface = new ScriptSurface(device, Content.ColorAlpha, 100, 100))
                    {
                        if (draw)
                        {
                            using var cr = new Context(surface);
                            cr.SetSourceRGB(0, 0, 1);
                            cr.Rectangle(3, 4, 5, 6);
                            cr.Fill();
                        }
                        surface.Finish();
                    }
                    device.Finish();
                }
                return path;
            }

            var drawn = File.ReadAllText(Write("drawn.cs2", true));
            var blank = File.ReadAllText(Write("blank.cs2", false));

            Assert.StartsWith("%!CairoScript", drawn);
            Assert.StartsWith("%!CairoScript", blank);

            Assert.Contains("3 4 5 6 rectangle", drawn);
            Assert.Contains("0 0 1 rgb set-source", drawn);
            Assert.Contains("fill", drawn);

            Assert.DoesNotContain("rectangle", blank);
            Assert.DoesNotContain("fill", blank);
        }
    }
}

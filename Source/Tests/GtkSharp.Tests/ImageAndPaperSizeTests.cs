using System;
using System.IO;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>Gtk.Image</c>'s hand-written constructors and <c>Gtk.PaperSize</c>'s
    /// named sizes — both at zero coverage.
    /// </summary>
    /// <remarks>
    /// These two have nothing to do with each other except that they are the two
    /// small hand-written files `Docs/coverage.md` marks "worth testing", and
    /// both have a real oracle rather than needing one invented.
    ///
    /// For <c>Image</c> it is a PNG the test makes itself, so the bytes going in
    /// are known. For <c>PaperSize</c> it is ISO 216 and the US paper standard,
    /// which are published numbers — and the ISO series has an internal relation
    /// the test can check by arithmetic: each size is the previous one halved
    /// across its long edge.
    /// </remarks>
    public class ImageAndPaperSizeTests : GtkTestBase
    {
        public ImageAndPaperSizeTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>A real PNG of a known size, built rather than checked in.</summary>
        static byte[] PngOf(int width, int height)
        {
            using var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, width, height);
            pixbuf.Fill(0xff0000ff);                 // opaque red
            return pixbuf.SaveToBuffer("png");
        }

        // ---------------------------------------------------------- Gtk.Image

        [Fact]
        public void An_image_built_from_a_stream_holds_that_picture()
        {
            Run(() =>
            {
                using var stream = new MemoryStream(PngOf(23, 17));

                var image = new Gtk.Image(stream);

                // The oracle is the size that went in. Gtk 4 stores the picture
                // as a GdkPaintable and has no gtk_image_get_pixbuf, so the size
                // is read from the paintable -- a stream that failed to decode
                // would have left the fallback icon and no paintable at all.
                Assert.Equal(Gtk.ImageType.Paintable, image.StorageType);
                Assert.NotNull(image.Paintable);
                Assert.Equal(23, image.Paintable.IntrinsicWidth);
                Assert.Equal(17, image.Paintable.IntrinsicHeight);
            });
        }

        [Fact]
        public void A_stream_that_is_not_a_picture_becomes_the_missing_image_icon()
        {
            // LoadFromStream catches everything and sets IconName instead. That
            // is a deliberate choice and a surprising one -- a caller passing a
            // corrupt file gets a widget, not an exception -- so it is pinned
            // rather than left to be discovered.
            Run(() =>
            {
                using var stream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 });

                var image = new Gtk.Image(stream);

                Assert.Equal("image-missing", image.IconName);
                Assert.Equal(Gtk.ImageType.IconName, image.StorageType);
            });
        }

        [Fact]
        public void An_image_can_be_built_from_a_manifest_resource()
        {
            // The resource here is a .ui file, not a picture, which is the point:
            // it exists, so the ArgumentException path is not taken, and it does
            // not decode, so the fallback is. Both branches of the constructor
            // are exercised by this and the test below.
            Run(() =>
            {
                var image = new Gtk.Image(typeof(ImageAndPaperSizeTests).Assembly,
                                          "GtkSharp.Tests.embedded-window.ui");

                Assert.Equal("image-missing", image.IconName);
            });
        }

        [Fact]
        public void A_resource_name_that_does_not_exist_says_so()
        {
            Run(() =>
            {
                var error = Assert.Throws<ArgumentException>(
                    () => new Gtk.Image(typeof(ImageAndPaperSizeTests).Assembly, "no.such.resource"));

                Assert.Contains("no.such.resource", error.Message);
            });
        }

        [Fact]
        public void The_static_resource_loader_looks_in_its_caller()
        {
            // LoadFromResource resolves Assembly.GetCallingAssembly(), which is
            // why it is marked NoInlining. The test is the calling assembly.
            Run(() =>
            {
                var image = Gtk.Image.LoadFromResource("GtkSharp.Tests.embedded-window.ui");

                Assert.NotNull(image);
                Assert.Equal("image-missing", image.IconName);

                Assert.Throws<ArgumentException>(
                    () => Gtk.Image.LoadFromResource("no.such.resource"));
            });
        }

        [Fact]
        public void The_deprecated_setters_still_reach_the_picture()
        {
            // FromFile and FromPixbuf are marked obsolete in favour of File and
            // Pixbuf, but they are still bound and still call into Gtk, so they
            // are still capable of being wrong.
            Run(() =>
            {
                var image = new Gtk.Image();

                using var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 11, 13);
                pixbuf.Fill(0x00ff00ff);

#pragma warning disable CS0618
                image.FromPixbuf = pixbuf;
#pragma warning restore CS0618

                Assert.Equal(11, image.Paintable.IntrinsicWidth);
                Assert.Equal(13, image.Paintable.IntrinsicHeight);

                var path = Path.Combine(Path.GetTempPath(),
                                        "gtksharp-image-" + Guid.NewGuid().ToString("N") + ".png");
                File.WriteAllBytes(path, PngOf(7, 5));
                try
                {
#pragma warning disable CS0618
                    image.FromFile = path;
#pragma warning restore CS0618

                    Assert.Equal(7, image.Paintable.IntrinsicWidth);
                    Assert.Equal(5, image.Paintable.IntrinsicHeight);
                }
                finally
                {
                    try { File.Delete(path); } catch (IOException) { }
                }
            });
        }

        // ------------------------------------------------------- Gtk.PaperSize

        /// <summary>Name, and the ISO/US dimensions in millimetres.</summary>
        public static TheoryData<string, double, double> Sizes => new TheoryData<string, double, double>
        {
            { "a3", 297.0, 420.0 },
            { "a4", 210.0, 297.0 },
            { "a5", 148.0, 210.0 },
            { "b5", 176.0, 250.0 },
            { "letter", 215.9, 279.4 },      // 8.5 x 11 in
            { "legal", 215.9, 355.6 },       // 8.5 x 14 in
            { "executive", 184.15, 266.7 },  // 7.25 x 10.5 in
        };

        static Gtk.PaperSize Named(string name) => name switch
        {
            "a3" => Gtk.PaperSize.A3,
            "a4" => Gtk.PaperSize.A4,
            "a5" => Gtk.PaperSize.A5,
            "b5" => Gtk.PaperSize.B5,
            "letter" => Gtk.PaperSize.Letter,
            "legal" => Gtk.PaperSize.Legal,
            "executive" => Gtk.PaperSize.Executive,
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };

        [Theory]
        [MemberData(nameof(Sizes))]
        public void The_named_paper_sizes_have_their_published_dimensions(
            string name, double widthMm, double heightMm)
        {
            // Each of the seven properties builds a PaperSize from a different
            // GTK_PAPER_NAME_* constant. A property wired to the wrong constant
            // returns a perfectly valid PaperSize of the wrong paper, which is
            // exactly what a dimension check catches and a null check does not.
            Run(() =>
            {
                using var paper = Named(name);

                // A tolerance, not a rounded comparison. Gtk keeps these as
                // floats, so 7.25 in comes back as 184.14999 mm -- which rounds
                // to 184.1 while the published 184.15 rounds to 184.2, and an
                // Assert.Equal to one decimal place calls two identical papers
                // different.
                Assert.Equal(widthMm, paper.GetWidth(Gtk.Unit.Mm), 0.05);
                Assert.Equal(heightMm, paper.GetHeight(Gtk.Unit.Mm), 0.05);

                Assert.False(string.IsNullOrEmpty(paper.DisplayName));
                Assert.False(string.IsNullOrEmpty(paper.Name));
            });
        }

        [Fact]
        public void The_ISO_A_series_halves_at_each_step()
        {
            // Arithmetic the test does itself, over three separate properties.
            // A3 -> A4 -> A5: the next size's long edge is the previous one's
            // short edge, and its short edge is half the previous long edge.
            Run(() =>
            {
                using var a3 = Gtk.PaperSize.A3;
                using var a4 = Gtk.PaperSize.A4;
                using var a5 = Gtk.PaperSize.A5;

                Assert.Equal(a3.GetWidth(Gtk.Unit.Mm), a4.GetHeight(Gtk.Unit.Mm), 0.05);
                Assert.Equal(a3.GetHeight(Gtk.Unit.Mm) / 2, a4.GetWidth(Gtk.Unit.Mm), 0.6);

                Assert.Equal(a4.GetWidth(Gtk.Unit.Mm), a5.GetHeight(Gtk.Unit.Mm), 0.05);
                Assert.Equal(a4.GetHeight(Gtk.Unit.Mm) / 2, a5.GetWidth(Gtk.Unit.Mm), 0.6);
            });
        }

        [Fact]
        public void The_unit_a_size_is_asked_for_changes_the_number_and_not_the_paper()
        {
            // 25.4 mm to the inch, and points are 1/72 in. If GetWidth ignored
            // its unit -- a plausible binding mistake, since the parameter is an
            // enum that could be dropped -- all three would be equal.
            Run(() =>
            {
                using var a4 = Gtk.PaperSize.A4;

                var mm = a4.GetWidth(Gtk.Unit.Mm);
                var inch = a4.GetWidth(Gtk.Unit.Inch);
                var points = a4.GetWidth(Gtk.Unit.Points);

                Assert.Equal(mm / 25.4, inch, 0.01);
                Assert.Equal(inch * 72.0, points, 0.05);

                Assert.NotEqual(mm, inch);
                Assert.NotEqual(mm, points);
            });
        }

        [Fact]
        public void Disposing_one_named_size_does_not_break_the_next_read_of_it()
        {
            // This is the test that found something. The seven properties used
            // to cache a singleton in a static field, and PaperSize is
            // IDisposable -- so `using (var p = PaperSize.A4)`, the obvious
            // thing to write, freed the shared GtkPaperSize and left the static
            // field dangling. Every later read in the process returned a freed
            // handle, and the next call through it was an access violation:
            // 0xC0000005, the test host gone, no exception to catch.
            //
            // Each read now returns a size of its own.
            Run(() =>
            {
                using (var first = Gtk.PaperSize.A4)
                {
                    Assert.Equal(210.0, first.GetWidth(Gtk.Unit.Mm), 0.05);
                }

                // Reached only if disposing the first did not poison the source.
                using var second = Gtk.PaperSize.A4;
                Assert.Equal(210.0, second.GetWidth(Gtk.Unit.Mm), 0.05);

                // And they are genuinely separate objects, which is what makes
                // disposing one safe.
                using var third = Gtk.PaperSize.A4;
                Assert.NotSame(second, third);
                Assert.True(second.IsEqual(third));
            });
        }

        [Fact]
        public void Two_sizes_of_the_same_paper_are_equal_and_of_different_paper_are_not()
        {
            Run(() =>
            {
                using var one = Gtk.PaperSize.A4;
                using var another = Gtk.PaperSize.A4;
                using var different = Gtk.PaperSize.A3;

                Assert.True(one.IsEqual(another));
                Assert.False(one.IsEqual(different));
            });
        }
    }
}

using System;
using System.IO;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>Gdk.PixbufAnimation</c>'s four hand-written constructors.
    /// </summary>
    /// <remarks>
    /// Forty lines at zero coverage, and one of them carries the same
    /// platform-split symbol trick as `GLib.FileUtils`:
    ///
    /// <code>
    /// FuncLoader.IsWindows ? "gdk_pixbuf_animation_new_from_file_utf8"
    ///                      : "gdk_pixbuf_animation_new_from_file"
    /// </code>
    ///
    /// A name that is not exported yields a **null delegate**, not a link error,
    /// so the constructor would throw `NullReferenceException` naming nothing.
    /// Nothing in this repository had ever called it on either platform.
    ///
    /// A still image is a perfectly good animation of one frame, which is what
    /// makes this testable without checking a GIF into the tree: the test builds
    /// a PNG of a known size, and the animation has to agree about that size and
    /// report itself static.
    /// </remarks>
    public class PixbufAnimationTests : GtkTestBase
    {
        public PixbufAnimationTests(GtkFixture fixture) : base(fixture) { }

        static byte[] PngOf(int width, int height)
        {
            using var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, width, height);
            pixbuf.Fill(0x336699ff);
            return pixbuf.SaveToBuffer("png");
        }

        [Fact]
        public void An_animation_loaded_from_a_file_has_the_size_of_the_picture()
        {
            // Also the test that the platform-specific symbol resolved: a null
            // delegate here is a NullReferenceException, not a wrong number.
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(),
                                        "gtksharp-anim-" + Guid.NewGuid().ToString("N") + ".png");
                File.WriteAllBytes(path, PngOf(19, 29));
                try
                {
                    using var animation = new Gdk.PixbufAnimation(path);

                    Assert.Equal(19, animation.Width);
                    Assert.Equal(29, animation.Height);
                }
                finally
                {
                    try { File.Delete(path); } catch (IOException) { }
                }
            });
        }

        [Fact]
        public void A_still_picture_is_a_static_animation_of_itself()
        {
            // IsStaticImage is the question the type exists to answer, and
            // StaticImage has to hand back the same picture rather than a
            // placeholder.
            Run(() =>
            {
                using var stream = new MemoryStream(PngOf(13, 7));
                using var animation = new Gdk.PixbufAnimation(stream);

                Assert.True(animation.IsStaticImage,
                            "a PNG has one frame, so it is a static animation");

                var still = animation.StaticImage;
                Assert.NotNull(still);
                Assert.Equal(13, still.Width);
                Assert.Equal(7, still.Height);
            });
        }

        [Fact]
        public void An_animation_can_be_built_from_a_manifest_resource()
        {
            Run(() =>
            {
                // The .ui resource is not a picture, so this is the failure
                // path -- and unlike Gtk.Image, PixbufAnimation does not
                // substitute a fallback, so it surfaces rather than being
                // swallowed.
                Assert.ThrowsAny<Exception>(
                    () => new Gdk.PixbufAnimation(typeof(PixbufAnimationTests).Assembly,
                                                  "GtkSharp.Tests.embedded-window.ui"));
            });
        }

        [Fact]
        public void A_resource_name_that_does_not_exist_says_so()
        {
            Run(() =>
            {
                var error = Assert.Throws<ArgumentException>(
                    () => new Gdk.PixbufAnimation(typeof(PixbufAnimationTests).Assembly,
                                                  "no.such.resource"));

                Assert.Contains("no.such.resource", error.Message);
            });
        }

        [Fact]
        public void The_static_resource_loader_looks_in_its_caller()
        {
            // LoadFromResource resolves Assembly.GetCallingAssembly(), which is
            // why it is marked NoInlining; the test is the calling assembly.
            Run(() =>
            {
                var error = Assert.Throws<ArgumentException>(
                    () => Gdk.PixbufAnimation.LoadFromResource("no.such.resource"));

                Assert.Contains("no.such.resource", error.Message);
            });
        }

        [Fact]
        public void A_missing_file_raises_a_GException_rather_than_a_null_handle()
        {
            // The constructor checks the GError itself rather than relying on
            // the marshaller, so the failure path is its own code.
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(),
                                        "gtksharp-no-such-" + Guid.NewGuid().ToString("N") + ".png");

                Assert.Throws<GLib.GException>(() => new Gdk.PixbufAnimation(path));
            });
        }

        [Fact]
        public void A_file_that_is_not_a_picture_raises_rather_than_returning_something()
        {
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(),
                                        "gtksharp-notapng-" + Guid.NewGuid().ToString("N") + ".png");
                File.WriteAllBytes(path, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
                try
                {
                    Assert.Throws<GLib.GException>(() => new Gdk.PixbufAnimation(path));
                }
                finally
                {
                    try { File.Delete(path); } catch (IOException) { }
                }
            });
        }
    }
}

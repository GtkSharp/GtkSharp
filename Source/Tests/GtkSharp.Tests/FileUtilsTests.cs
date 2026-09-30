using System;
using System.IO;
using System.Text;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.FileUtils.GetFileContents</c>, which had no coverage at all.
    /// </summary>
    /// <remarks>
    /// One method, and two reasons it is worth testing rather than assuming.
    ///
    /// It **branches on the platform** and loads a different native symbol on
    /// each side: <c>g_file_get_contents_utf8</c> on Windows,
    /// <c>g_file_get_contents</c> elsewhere. The Windows name is a real export —
    /// GLib's header redirects the ordinary name to it so filenames are UTF-8
    /// rather than the ANSI codepage — but a symbol that is not there produces a
    /// *null delegate*, not a link error, and a `NullReferenceException` at the
    /// call site naming nothing. At 0% coverage nothing in this repository had
    /// ever called it on either platform.
    ///
    /// And the oracle is free: the BCL can write the file and read it back.
    /// </remarks>
    public class FileUtilsTests : GtkTestBase
    {
        public FileUtilsTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>A file in the temp directory, deleted afterwards.</summary>
        static void WithFile(byte[] content, Action<string> body)
        {
            var path = Path.Combine(Path.GetTempPath(),
                                    "gtksharp-fileutils-" + Guid.NewGuid().ToString("N") + ".txt");
            File.WriteAllBytes(path, content);
            try
            {
                body(path);
            }
            finally
            {
                try { File.Delete(path); } catch (IOException) { }
            }
        }

        [Fact]
        public void The_contents_read_back_are_the_contents_written()
        {
            Run(() => WithFile(Encoding.UTF8.GetBytes("hello, file\nsecond line\n"), path =>
            {
                Assert.Equal(File.ReadAllText(path), GLib.FileUtils.GetFileContents(path));
            }));
        }

        [Fact]
        public void Non_ascii_content_survives_the_round_trip()
        {
            // The whole point of the Windows _utf8 entry point is that bytes are
            // UTF-8 rather than the ANSI codepage, so the test has to use
            // characters that would differ between the two.
            Run(() =>
            {
                const string text = "àèìòù 日本語 \U0001F600";

                WithFile(Encoding.UTF8.GetBytes(text), path =>
                {
                    Assert.Equal(text, GLib.FileUtils.GetFileContents(path));
                });
            });
        }

        [Fact]
        public void A_non_ascii_filename_survives_too()
        {
            // The other half of the same concern: the *name* goes through
            // g_strdup as UTF-8, and on Windows only the _utf8 entry point
            // interprets it that way. A file this test can only find again if
            // the encoding agreed.
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(),
                                        "gtksharp-è日-" + Guid.NewGuid().ToString("N") + ".txt");
                File.WriteAllText(path, "found me", new UTF8Encoding(false));
                try
                {
                    Assert.Equal("found me", GLib.FileUtils.GetFileContents(path));
                }
                finally
                {
                    try { File.Delete(path); } catch (IOException) { }
                }
            });
        }

        [Fact]
        public void An_empty_file_reads_as_an_empty_string_rather_than_null()
        {
            Run(() => WithFile(Array.Empty<byte>(), path =>
            {
                Assert.Equal(string.Empty, GLib.FileUtils.GetFileContents(path));
            }));
        }

        [Fact]
        public void A_missing_file_raises_a_GException_that_says_what_went_wrong()
        {
            // The failure path, which is the one that turns a null delegate into
            // a NullReferenceException if the symbol were missing -- so this
            // test doubles as the check that the platform branch loaded.
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(), "gtksharp-no-such-" + Guid.NewGuid().ToString("N"));

                var error = Assert.Throws<GLib.GException>(() => GLib.FileUtils.GetFileContents(path));

                Assert.False(string.IsNullOrEmpty(error.Message));
            });
        }

        [Fact]
        public void Content_after_an_embedded_NUL_is_not_returned()
        {
            // Not a defect to fix, but a limit worth pinning so it is not
            // discovered by surprise. g_file_get_contents is binary-safe and
            // reports a length; the wrapper discards that length and decodes the
            // buffer as a NUL-terminated string, so a file with a NUL in it
            // comes back truncated at the NUL.
            //
            // The method returns a string, so it could not do otherwise without
            // a different signature. Use System.IO for binary files.
            Run(() =>
            {
                var content = new byte[] { 0x61, 0x62, 0x00, 0x63, 0x64 };   // "ab\0cd"

                WithFile(content, path =>
                {
                    Assert.Equal("ab", GLib.FileUtils.GetFileContents(path));

                    // The BCL, which does not have that limit, sees all five bytes.
                    Assert.Equal(5, File.ReadAllBytes(path).Length);
                });
            });
        }
    }
}

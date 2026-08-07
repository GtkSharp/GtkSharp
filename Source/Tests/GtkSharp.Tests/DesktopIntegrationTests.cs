using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The parts of an application that talk to the desktop rather than to the
    /// screen: the Gtk 4 async dialogs, file filters, the launchers, the legacy
    /// <c>GtkFileChooser</c> interface, and the printing stack.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Gtk 4 replaced every dialog in this area with an async pair and deleted
    /// <c>gtk_dialog_run</c>, so this is exactly the shape a port gets wrong:
    /// the call compiles, the callback never arrives or arrives with a result
    /// the Finish method cannot read, and nothing says so. None of it had a
    /// test.
    /// </para>
    /// <para>
    /// Most of the oracles are outside the library. ISO 216 says an A4 sheet is
    /// 210 by 297 millimetres and ANSI says a Letter sheet is 8.5 by 11 inches;
    /// a printable page is its sheet less two margins, which is arithmetic this
    /// file does itself; print settings are written by Gtk and read back with
    /// <c>System.IO</c>, so what is asserted is what landed on disk; and a
    /// filter told about <c>*.txt</c> has to accept <c>notes.txt</c> and refuse
    /// <c>notes.png</c>.
    /// </para>
    /// <para>
    /// The dialogs that need a display are driven the only way a test without a
    /// user can drive them: presented, then cancelled through their
    /// <c>GCancellable</c>, which is the error path of every Finish method in
    /// the family. Nothing here launches a file or a URI —
    /// <c>Docs/testing.md</c> records why the LinkButton sample is skipped, and
    /// a suite that opens the developer's browser is worse than one that does
    /// not run.
    /// </para>
    /// </remarks>
    public class DesktopIntegrationTests : GtkTestBase
    {
        public DesktopIntegrationTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------ plumbing

        /// <summary>
        /// Runs the main loop until <paramref name="done"/> holds or the
        /// deadline passes, and reports which.
        /// </summary>
        /// <remarks>
        /// A blocking iteration parks forever when nothing is ready, so a
        /// callback that never arrives would hang the run rather than fail it.
        /// The ticking timeout is what makes the deadline reachable.
        /// </remarks>
        private static bool PumpUntil(Func<bool> done, int milliseconds = 10000)
        {
            var clock = Stopwatch.StartNew();
            var tick = GLib.Timeout.Add(10, () => true);
            try
            {
                while (!done() && clock.ElapsedMilliseconds < milliseconds)
                    GLib.MainContext.Iteration(true);
            }
            finally
            {
                GLib.Source.Remove(tick);
            }

            return done();
        }

        /// <summary>A directory that exists only while <paramref name="body"/> runs.</summary>
        private static void WithTempDir(Action<string> body)
        {
            var dir = Path.Combine(Path.GetTempPath(),
                                   "gtksharp-desktop-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                body(dir);
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch (IOException) { }
            }
        }

        /// <summary>Compares two paths as directory names, since Gtk and
        /// System.IO disagree about the trailing separator.</summary>
        private static void AssertSamePath(string expected, string actual)
        {
            Assert.Equal(expected.TrimEnd(Path.DirectorySeparatorChar),
                         actual?.TrimEnd(Path.DirectorySeparatorChar));
        }

        /// <summary>A GFileInfo carrying only what a GtkFileFilter reads.</summary>
        /// <remarks>
        /// <paramref name="contentType"/> is a *content type*, not a mime type:
        /// see <c>A_mime_filter_matches_a_content_type_rather_than_a_mime_type</c>
        /// for why the distinction is load-bearing on Windows.
        /// </remarks>
        private static GLib.FileInfo InfoFor(string displayName, string contentType = null)
        {
            var info = new GLib.FileInfo();
            info.SetAttributeString("standard::display-name", displayName);

            // Gtk asks a GFileInfo for its content type whenever a filter has a
            // mime rule, and Gio logs a CRITICAL if the attribute is absent. The
            // placeholder is a type no test filter names.
            info.SetAttributeString("standard::content-type",
                                    contentType ?? GLib.ContentType.FromMimeType("application/octet-stream")
                                                ?? "application/octet-stream");
            return info;
        }

        /// <summary>Round-trips a key file through its text, so nothing can be
        /// answered out of the writer's own memory.</summary>
        private static GLib.KeyFile Reread(GLib.KeyFile written)
        {
            var fresh = new GLib.KeyFile();
            Assert.True(fresh.LoadFromData(written.ToData(), GLib.KeyFileFlags.None));
            return fresh;
        }

        private static string TextOf(GLib.KeyFile file) => Encoding.UTF8.GetString(file.ToData());

        // ------------------------------------------------------------ PaperSize

        [Fact]
        public void A_named_paper_size_has_the_dimensions_its_standard_gives()
        {
            // ISO 216 fixes A4 at 210x297mm; ANSI fixes Letter at 8.5x11in. Gtk's
            // table is compiled into the library rather than read from the
            // system, so these are facts about Gtk and not about the machine.
            Run(() =>
            {
                var a4 = new Gtk.PaperSize("iso_a4");
                Assert.Equal(210.0, a4.GetWidth(Gtk.Unit.Mm), 3);
                Assert.Equal(297.0, a4.GetHeight(Gtk.Unit.Mm), 3);
                Assert.Equal("iso_a4", a4.Name);
                Assert.False(a4.IsCustom);

                var letter = new Gtk.PaperSize("na_letter");
                Assert.Equal(8.5, letter.GetWidth(Gtk.Unit.Inch), 3);
                Assert.Equal(11.0, letter.GetHeight(Gtk.Unit.Inch), 3);
            });
        }

        [Fact]
        public void Paper_dimensions_agree_across_every_unit()
        {
            // A point is 1/72 inch and an inch is 25.4mm by definition, so the
            // three readings are one number in three spellings. The tolerance is
            // for the double arithmetic Gtk does on the way, not for any
            // disagreement about the size.
            Run(() =>
            {
                var a4 = new Gtk.PaperSize("iso_a4");

                var mm = a4.GetWidth(Gtk.Unit.Mm);
                var inch = a4.GetWidth(Gtk.Unit.Inch);
                var points = a4.GetWidth(Gtk.Unit.Points);

                Assert.Equal(mm, inch * 25.4, 6);
                Assert.Equal(points, inch * 72.0, 6);
                Assert.Equal(mm, points * 25.4 / 72.0, 6);
            });
        }

        [Fact]
        public void A_custom_paper_size_keeps_what_it_was_given_and_can_be_resized()
        {
            // gtk_paper_size_set_size is guarded on is_custom, so a custom sheet
            // is the only kind a program may change under its own feet.
            Run(() =>
            {
                var size = new Gtk.PaperSize("test_slip", "Till slip", 80.0, 200.0, Gtk.Unit.Mm);

                Assert.True(size.IsCustom);
                Assert.Equal("Till slip", size.DisplayName);
                Assert.Equal(80.0, size.GetWidth(Gtk.Unit.Mm), 3);
                Assert.Equal(200.0, size.GetHeight(Gtk.Unit.Mm), 3);

                size.SetSize(4.0, 6.0, Gtk.Unit.Inch);

                Assert.Equal(4.0 * 25.4, size.GetWidth(Gtk.Unit.Mm), 3);
                Assert.Equal(6.0 * 25.4, size.GetHeight(Gtk.Unit.Mm), 3);
            });
        }

        [Fact]
        public void Paper_sizes_compare_by_name_rather_than_by_dimensions()
        {
            // gtk_paper_size_is_equal is a strcmp on the names, which is not the
            // comparison the name suggests: a custom sheet cut to exactly A4 is
            // *not* equal to A4, and two independently built A4s are.
            Run(() =>
            {
                var a4 = new Gtk.PaperSize("iso_a4");
                var alsoA4 = new Gtk.PaperSize("iso_a4");
                var a5 = new Gtk.PaperSize("iso_a5");

                Assert.True(a4.IsEqual(alsoA4));
                Assert.False(a4.IsEqual(a5));

                var lookalike = new Gtk.PaperSize("mine", "Mine", 210.0, 297.0, Gtk.Unit.Mm);
                Assert.Equal(a4.GetWidth(Gtk.Unit.Mm), lookalike.GetWidth(Gtk.Unit.Mm), 3);
                Assert.False(a4.IsEqual(lookalike));
            });
        }

        [Fact]
        public void A_custom_paper_size_round_trips_through_a_key_file()
        {
            // The key file goes out as text and comes back into a *fresh*
            // KeyFile, so nothing can be satisfied from what the writer still
            // holds in memory.
            Run(() =>
            {
                var original = new Gtk.PaperSize("test_slip", "Till slip", 80.0, 200.0, Gtk.Unit.Mm);

                var written = new GLib.KeyFile();
                original.ToKeyFile(written, "Paper");

                Assert.Contains("Till slip", TextOf(written));

                var restored = new Gtk.PaperSize(Reread(written), "Paper");

                Assert.Equal("test_slip", restored.Name);
                Assert.Equal("Till slip", restored.DisplayName);
                Assert.True(restored.IsCustom);
                Assert.Equal(80.0, restored.GetWidth(Gtk.Unit.Mm), 3);
                Assert.Equal(200.0, restored.GetHeight(Gtk.Unit.Mm), 3);
            });
        }

        [Fact]
        public void The_built_in_paper_size_list_contains_A4_at_its_standard_size()
        {
            // include_custom is deliberately false: custom sheets come from the
            // user's own gtk-custom-papers file, which is a fact about the
            // machine. The built-in table is compiled into Gtk.
            Run(() =>
            {
                var sizes = Gtk.PaperSize.GetPaperSizes(false);

                var a4 = sizes.FirstOrDefault(s => s.Name == "iso_a4");
                Assert.NotNull(a4);
                Assert.Equal(210.0, a4.GetWidth(Gtk.Unit.Mm), 3);

                Assert.Contains(sizes, s => s.Name == "na_letter");
                Assert.All(sizes, s => Assert.False(s.IsCustom));
            });
        }

        // ------------------------------------------------------------ PageSetup

        [Fact]
        public void A_portrait_page_is_the_sheet_less_its_side_margins()
        {
            // The one piece of arithmetic in GtkPageSetup, done here rather than
            // read back out of Gtk.
            Run(() =>
            {
                var setup = new Gtk.PageSetup();
                setup.PaperSize = new Gtk.PaperSize("iso_a4");
                setup.SetLeftMargin(15.0, Gtk.Unit.Mm);
                setup.SetRightMargin(20.0, Gtk.Unit.Mm);
                setup.SetTopMargin(5.0, Gtk.Unit.Mm);
                setup.SetBottomMargin(8.0, Gtk.Unit.Mm);

                Assert.Equal(210.0 - 15.0 - 20.0, setup.GetPageWidth(Gtk.Unit.Mm), 3);
                Assert.Equal(297.0 - 5.0 - 8.0, setup.GetPageHeight(Gtk.Unit.Mm), 3);
            });
        }

        [Fact]
        public void Rotating_the_sheet_changes_which_margins_bound_the_page()
        {
            // The trap, and it is a good one. A margin belongs to the *sheet* and
            // never moves: GetLeftMargin returns 15 in every orientation. The
            // page is what will be printed on the rotated sheet, so in landscape
            // its width is bounded by the top and bottom margins.
            //
            // A caller who computes "page width = paper width - left - right",
            // which is what the portrait case looks like and what the accessor
            // names invite, is wrong by exactly the difference between the two
            // pairs -- silently, and only in landscape. All four margins here
            // are different on purpose: with left+right equal to top+bottom the
            // two formulas agree and the test proves nothing.
            Run(() =>
            {
                var setup = new Gtk.PageSetup();
                setup.PaperSize = new Gtk.PaperSize("iso_a4");
                setup.SetLeftMargin(15.0, Gtk.Unit.Mm);
                setup.SetRightMargin(20.0, Gtk.Unit.Mm);
                setup.SetTopMargin(5.0, Gtk.Unit.Mm);
                setup.SetBottomMargin(8.0, Gtk.Unit.Mm);

                Assert.Equal(Gtk.PageOrientation.Portrait, setup.Orientation);
                Assert.Equal(210.0, setup.GetPaperWidth(Gtk.Unit.Mm), 3);

                setup.Orientation = Gtk.PageOrientation.Landscape;

                Assert.Equal(297.0, setup.GetPaperWidth(Gtk.Unit.Mm), 3);
                Assert.Equal(210.0, setup.GetPaperHeight(Gtk.Unit.Mm), 3);

                // The margins themselves did not move.
                Assert.Equal(15.0, setup.GetLeftMargin(Gtk.Unit.Mm), 3);
                Assert.Equal(20.0, setup.GetRightMargin(Gtk.Unit.Mm), 3);

                // But the page is bounded by the other pair now.
                Assert.Equal(297.0 - 5.0 - 8.0, setup.GetPageWidth(Gtk.Unit.Mm), 3);
                Assert.Equal(210.0 - 15.0 - 20.0, setup.GetPageHeight(Gtk.Unit.Mm), 3);
            });
        }

        [Fact]
        public void A_margin_reads_back_in_every_unit_it_was_not_set_in()
        {
            Run(() =>
            {
                var setup = new Gtk.PageSetup();
                setup.SetTopMargin(1.0, Gtk.Unit.Inch);

                Assert.Equal(1.0, setup.GetTopMargin(Gtk.Unit.Inch), 6);
                Assert.Equal(25.4, setup.GetTopMargin(Gtk.Unit.Mm), 6);
                Assert.Equal(72.0, setup.GetTopMargin(Gtk.Unit.Points), 6);
            });
        }

        [Fact]
        public void Setting_the_paper_size_with_default_margins_discards_the_margins_that_were_there()
        {
            // Two ways to install a paper size, and only one of them touches the
            // margins. Choosing the wrong one silently keeps margins that
            // belonged to a different sheet.
            Run(() =>
            {
                var setup = new Gtk.PageSetup();
                setup.SetTopMargin(40.0, Gtk.Unit.Mm);
                setup.PaperSize = new Gtk.PaperSize("iso_a4");

                Assert.Equal(40.0, setup.GetTopMargin(Gtk.Unit.Mm), 3);

                var a4 = new Gtk.PaperSize("iso_a4");
                setup.PaperSizeAndDefaultMargins = a4;

                Assert.Equal(a4.GetDefaultTopMargin(Gtk.Unit.Mm), setup.GetTopMargin(Gtk.Unit.Mm), 3);
                Assert.NotEqual(40.0, setup.GetTopMargin(Gtk.Unit.Mm));
            });
        }

        [Fact]
        public void A_page_setup_round_trips_through_a_key_file()
        {
            Run(() =>
            {
                var original = new Gtk.PageSetup();
                original.PaperSize = new Gtk.PaperSize("iso_a5");
                original.Orientation = Gtk.PageOrientation.ReverseLandscape;
                original.SetLeftMargin(11.0, Gtk.Unit.Mm);
                original.SetBottomMargin(13.0, Gtk.Unit.Mm);

                var written = new GLib.KeyFile();
                original.ToKeyFile(written, "Page Setup");
                var text = TextOf(written);

                Assert.Contains("MarginLeft=11", text);
                Assert.Contains("Orientation=reverse-landscape", text);

                // A standard sheet is written under its *PPD* name and its own
                // name is left out entirely, so "iso_a5" does not appear in the
                // file at all -- the name is recovered from Gtk's table on the
                // way back in.
                Assert.DoesNotContain("iso_a5", text);
                Assert.Contains("PPDName=A5", text);

                var restored = new Gtk.PageSetup(Reread(written), "Page Setup");

                Assert.Equal("iso_a5", restored.PaperSize.Name);
                Assert.Equal(Gtk.PageOrientation.ReverseLandscape, restored.Orientation);
                Assert.Equal(11.0, restored.GetLeftMargin(Gtk.Unit.Mm), 3);
                Assert.Equal(13.0, restored.GetBottomMargin(Gtk.Unit.Mm), 3);
            });
        }

        [Fact]
        public void A_copied_page_setup_does_not_follow_the_original()
        {
            Run(() =>
            {
                var original = new Gtk.PageSetup();
                original.PaperSize = new Gtk.PaperSize("iso_a4");
                original.SetTopMargin(5.0, Gtk.Unit.Mm);

                var copy = original.Copy();
                original.SetTopMargin(50.0, Gtk.Unit.Mm);
                original.Orientation = Gtk.PageOrientation.Landscape;

                Assert.Equal(5.0, copy.GetTopMargin(Gtk.Unit.Mm), 3);
                Assert.Equal(Gtk.PageOrientation.Portrait, copy.Orientation);
                Assert.Equal(50.0, original.GetTopMargin(Gtk.Unit.Mm), 3);
            });
        }

        // -------------------------------------------------------- PrintSettings

        [Fact]
        public void The_typed_setters_write_one_string_store_in_a_locale_independent_form()
        {
            // GtkPrintSettings is a string dictionary underneath and the typed
            // accessors are formatters over it. The oracle is the string that
            // ends up in the store: a double has to go out through
            // g_ascii_dtostr, or a machine with a comma decimal separator writes
            // settings no other machine can read.
            Run(() =>
            {
                var settings = new Gtk.PrintSettings();

                settings.SetBool("test-bool", true);
                settings.SetInt("test-int", 42);
                settings.SetDouble("test-double", 1.5);
                settings.Set("test-string", "plain");

                Assert.Equal("true", settings.Get("test-bool"));
                Assert.Equal("42", settings.Get("test-int"));
                Assert.Equal("1.5", settings.Get("test-double"));
                Assert.Equal("plain", settings.Get("test-string"));

                Assert.True(settings.GetBool("test-bool"));
                Assert.Equal(42, settings.GetInt("test-int"));
                Assert.Equal(1.5, settings.GetDouble("test-double"), 6);
            });
        }

        [Fact]
        public void A_length_is_stored_in_millimetres_whatever_unit_it_was_given_in()
        {
            // The unit belongs to the accessor, not to the value, so a length
            // written in inches and read in millimetres has to convert. The
            // stored string says which unit the store is in -- 0.5mm is chosen
            // because it is exactly representable, so the comparison is about
            // the unit and not about how many digits g_ascii_dtostr prints.
            Run(() =>
            {
                var settings = new Gtk.PrintSettings();
                settings.SetLength("margin", 1.0, Gtk.Unit.Inch);

                Assert.Equal(25.4, settings.GetLength("margin", Gtk.Unit.Mm), 6);
                Assert.Equal(1.0, settings.GetLength("margin", Gtk.Unit.Inch), 6);
                Assert.Equal(72.0, settings.GetLength("margin", Gtk.Unit.Points), 6);

                settings.SetLength("hairline", 0.5, Gtk.Unit.Mm);
                Assert.Equal("0.5", settings.Get("hairline"));
            });
        }

        [Fact]
        public void A_key_that_was_never_set_reads_as_the_default_it_was_asked_for()
        {
            // GetInt on a missing key is 0 and GetDouble is 0.0, which are also
            // perfectly good values -- so the *WithDefault pair is the only way
            // to tell "unset" from "set to zero".
            Run(() =>
            {
                var settings = new Gtk.PrintSettings();

                Assert.False(settings.HasKey("absent"));
                Assert.Equal(0, settings.GetInt("absent"));
                Assert.Equal(0.0, settings.GetDouble("absent"), 6);
                Assert.False(settings.GetBool("absent"));
                Assert.Null(settings.Get("absent"));

                Assert.Equal(7, settings.GetIntWithDefault("absent", 7));
                Assert.Equal(2.5, settings.GetDoubleWithDefault("absent", 2.5), 6);

                settings.SetInt("present", 0);
                Assert.True(settings.HasKey("present"));
                Assert.Equal(0, settings.GetIntWithDefault("present", 7));

                settings.Unset("present");
                Assert.False(settings.HasKey("present"));
                Assert.Equal(7, settings.GetIntWithDefault("present", 7));
            });
        }

        [Fact]
        public void Foreach_visits_exactly_the_keys_that_were_set()
        {
            // The callback is the only way to enumerate a GtkPrintSettings, so
            // it is the only way a program can copy or diff one.
            Run(() =>
            {
                var settings = new Gtk.PrintSettings();
                settings.Set("alpha", "one");
                settings.Set("beta", "two");
                settings.SetInt("gamma", 3);

                var seen = new Dictionary<string, string>();
                settings.Foreach((key, value) => seen[key] = value);

                Assert.Equal(3, seen.Count);
                Assert.Equal("one", seen["alpha"]);
                Assert.Equal("two", seen["beta"]);
                Assert.Equal("3", seen["gamma"]);
            });
        }

        [Fact]
        public void Print_settings_round_trip_through_a_file_on_disk()
        {
            // Written by Gtk, read back with System.IO, then parsed by Gtk out of
            // the file it wrote -- so the assertion is about the bytes, not about
            // what the settings object remembers.
            Run(() => WithTempDir(dir =>
            {
                var path = Path.Combine(dir, "settings.ini");

                var original = new Gtk.PrintSettings();
                original.Printer = "Test Printer";
                original.NCopies = 3;
                original.Collate = true;
                original.Reverse = true;
                original.Scale = 87.5;
                original.Quality = Gtk.PrintQuality.High;
                original.Duplex = Gtk.PrintDuplex.Horizontal;
                original.Orientation = Gtk.PageOrientation.Landscape;
                original.PaperSize = new Gtk.PaperSize("iso_a4");

                Assert.True(original.ToFile(path));

                var text = File.ReadAllText(path);
                Assert.Contains("Test Printer", text);
                Assert.Contains("87.5", text);

                var restored = new Gtk.PrintSettings(path);

                Assert.Equal("Test Printer", restored.Printer);
                Assert.Equal(3, restored.NCopies);
                Assert.True(restored.Collate);
                Assert.True(restored.Reverse);
                Assert.Equal(87.5, restored.Scale, 6);
                Assert.Equal(Gtk.PrintQuality.High, restored.Quality);
                Assert.Equal(Gtk.PrintDuplex.Horizontal, restored.Duplex);
                Assert.Equal(Gtk.PageOrientation.Landscape, restored.Orientation);
                Assert.Equal("iso_a4", restored.PaperSize.Name);
            }));
        }

        [Fact]
        public void The_paper_format_and_the_paper_width_are_two_independent_keys()
        {
            // Naming a standard sheet records its *name* and nothing else, so a
            // program that sets the paper size and then asks how wide the paper
            // is gets zero. The two accessors read different keys, and only the
            // getter for the size knows how to look a name up in Gtk's table.
            Run(() =>
            {
                var settings = new Gtk.PrintSettings();
                settings.PaperSize = new Gtk.PaperSize("iso_a4");

                Assert.Equal("iso_a4", settings.Get("paper-format"));
                Assert.Equal("iso_a4", settings.PaperSize.Name);
                Assert.Equal(210.0, settings.PaperSize.GetWidth(Gtk.Unit.Mm), 3);

                Assert.False(settings.HasKey("paper-width"));
                Assert.Equal(0.0, settings.GetPaperWidth(Gtk.Unit.Mm), 6);

                settings.SetPaperWidth(210.0, Gtk.Unit.Mm);
                Assert.Equal(210.0, settings.GetPaperWidth(Gtk.Unit.Mm), 3);
                Assert.Equal("iso_a4", settings.PaperSize.Name);
            });
        }

        [Fact]
        public void Every_page_range_survives_a_round_trip_through_the_settings()
        {
            // Both halves of this pair are arrays whose length is a separate
            // argument, and the api.xml has no way to say so: the getter used to
            // return one GtkPageRange and leak the rest of the block, and the
            // setter used to marshal one struct and tell Gtk to read three.
            // "Pages 1-3, 6 and 10-12" is the ordinary thing to type into a
            // print dialog, and only the first of the three ever arrived.
            Run(() =>
            {
                var settings = new Gtk.PrintSettings();
                settings.SetPageRanges(new[]
                {
                    new Gtk.PageRange { Start = 0, End = 2 },
                    new Gtk.PageRange { Start = 5, End = 5 },
                    new Gtk.PageRange { Start = 9, End = 11 },
                });

                var ranges = settings.GetPageRanges();

                Assert.Equal(3, ranges.Length);
                Assert.Equal(0, ranges[0].Start);
                Assert.Equal(2, ranges[0].End);
                Assert.Equal(5, ranges[1].Start);
                Assert.Equal(5, ranges[1].End);
                Assert.Equal(9, ranges[2].Start);
                Assert.Equal(11, ranges[2].End);

                // The store is text underneath, so the ranges are also readable
                // as the string a print dialog would show. A single-page range is
                // written without a dash.
                Assert.Equal("0-2,5,9-11", settings.Get("page-ranges"));

                Assert.Empty(new Gtk.PrintSettings().GetPageRanges());
            });
        }

        [Fact]
        public void Setting_x_and_y_resolutions_separately_leaves_the_combined_one_on_x()
        {
            // gtk_print_settings_set_resolution_xy writes three keys, and the
            // plain "resolution" it writes is the horizontal one. A caller that
            // reads Resolution back after asking for 300x1200 gets 300 -- not
            // 1200, and not an average.
            Run(() =>
            {
                var settings = new Gtk.PrintSettings();

                settings.Resolution = 600;
                Assert.Equal(600, settings.ResolutionX);
                Assert.Equal(600, settings.ResolutionY);

                settings.SetResolutionXy(300, 1200);
                Assert.Equal(300, settings.ResolutionX);
                Assert.Equal(1200, settings.ResolutionY);
                Assert.Equal(300, settings.Resolution);
            });
        }

        [Fact]
        public void A_copied_settings_object_does_not_follow_the_original()
        {
            Run(() =>
            {
                var original = new Gtk.PrintSettings();
                original.Set("shared", "before");
                original.NCopies = 2;

                var copy = original.Copy();
                original.Set("shared", "after");
                original.NCopies = 9;
                original.Set("added-later", "x");

                Assert.Equal("before", copy.Get("shared"));
                Assert.Equal(2, copy.NCopies);
                Assert.False(copy.HasKey("added-later"));
            });
        }

        // ------------------------------------------------------------ FileFilter

        [Fact]
        public void A_suffix_filter_matches_on_the_display_name_and_says_so()
        {
            // A GtkFileFilter is a GtkFilter over GFileInfo in Gtk 4, and it
            // declares which attributes it needs: get_attributes is what a caller
            // has to ask before it can build an info the filter can read at all.
            Run(() =>
            {
                var filter = new Gtk.FileFilter { Name = "Text" };
                filter.AddSuffix("txt");

                Assert.Contains("standard::display-name", filter.Attributes);

                using (var yes = InfoFor("notes.txt"))
                    Assert.True(filter.Match(yes.Handle));

                using (var no = InfoFor("notes.png"))
                    Assert.False(filter.Match(no.Handle));

                // A suffix is folded to a case-insensitive glob, and it anchors
                // at the end: "txt" in the middle of a name does not count.
                using (var upper = InfoFor("NOTES.TXT"))
                    Assert.True(filter.Match(upper.Handle));

                using (var middle = InfoFor("txt.png"))
                    Assert.False(filter.Match(middle.Handle));
            });
        }

        [Fact]
        public void A_mime_filter_matches_a_content_type_rather_than_a_mime_type()
        {
            // The distinction is not pedantry. AddMimeType stores
            // g_content_type_from_mime_type of what it was given, and matching
            // compares content types -- which on Windows are registry entries
            // like ".png" and on Linux are the mime strings themselves. So an
            // info built by putting the mime type straight into
            // standard::content-type matches on one platform and not the other,
            // and the portable way to write it is to convert on both sides, as
            // Gtk does internally.
            Run(() =>
            {
                var pngType = GLib.ContentType.FromMimeType("image/png");
                var textType = GLib.ContentType.FromMimeType("text/plain");
                Assert.NotNull(pngType);
                Assert.NotNull(textType);

                var filter = new Gtk.FileFilter();
                filter.AddMimeType("image/png");

                Assert.Contains("standard::content-type", filter.Attributes);
                Assert.DoesNotContain("standard::display-name", filter.Attributes);

                using (var yes = InfoFor("logo", pngType))
                    Assert.True(filter.Match(yes.Handle));

                using (var no = InfoFor("logo.png", textType))
                    Assert.False(filter.Match(no.Handle));
            });
        }

        [Fact]
        public void Two_rules_on_one_filter_are_a_union()
        {
            // Rules accumulate as alternatives: a filter offering "text and
            // markdown" is one filter with two rules, not two filters.
            Run(() =>
            {
                var filter = new Gtk.FileFilter();
                filter.AddSuffix("txt");
                filter.AddPattern("*.md");

                using (var a = InfoFor("notes.txt"))
                    Assert.True(filter.Match(a.Handle));
                using (var b = InfoFor("notes.md"))
                    Assert.True(filter.Match(b.Handle));
                using (var c = InfoFor("notes.rtf"))
                    Assert.False(filter.Match(c.Handle));

                // Adding a mime rule brings its attribute along with it without
                // disturbing the name rules already there.
                filter.AddMimeType("image/png");
                Assert.Contains("standard::display-name", filter.Attributes);
                Assert.Contains("standard::content-type", filter.Attributes);

                using (var d = InfoFor("notes.txt"))
                    Assert.True(filter.Match(d.Handle));
                using (var e = InfoFor("logo", GLib.ContentType.FromMimeType("image/png")))
                    Assert.True(filter.Match(e.Handle));
            });
        }

        [Fact]
        public void A_filter_round_trips_through_a_GVariant_with_the_same_rules()
        {
            // to_gvariant/new_from_gvariant is how a filter crosses a portal, so
            // the oracle is twofold: the serialised form has to come back
            // identical, and the rebuilt filter has to accept and refuse exactly
            // what the original did.
            //
            // No mime rule here, and that is Gtk's doing rather than a shortcut.
            // gtk_file_filter_to_gvariant writes the *content* type it stored,
            // while new_from_gvariant feeds what it reads back to
            // gtk_file_filter_add_mime_type, which converts again. Where the two
            // spellings coincide -- Linux -- that is harmless; on Windows a
            // content type is ".pdf", converting it a second time yields nothing,
            // and the rule comes back as "*", matching everything. Asserting
            // either outcome would be asserting the host.
            Run(() =>
            {
                var original = new Gtk.FileFilter { Name = "Documents" };
                original.AddSuffix("txt");
                original.AddPattern("*.md");

                var variant = original.ToGvariant();
                Assert.NotNull(variant);

                var restored = new Gtk.FileFilter(variant);

                Assert.Equal("Documents", restored.Name);
                Assert.Equal(variant.Print(true), restored.ToGvariant().Print(true));
                Assert.Equal(original.Attributes, restored.Attributes);

                using (var txt = InfoFor("a.txt"))
                    Assert.True(restored.Match(txt.Handle));
                using (var md = InfoFor("a.md"))
                    Assert.True(restored.Match(md.Handle));
                using (var png = InfoFor("a.png"))
                    Assert.False(restored.Match(png.Handle));
            });
        }

        [Fact]
        public void A_filter_built_from_a_buildable_description_matches_what_the_markup_listed()
        {
            // GtkFileFilter's buildable support is three custom child tags that
            // nothing else in Gtk uses, and a UI file is where an application's
            // filters usually live. Each tag is checked separately, because a
            // parser that silently ignored one of them would still produce a
            // filter that matched something.
            const string ui = @"<interface>
  <object class='GtkFileFilter' id='images'>
    <property name='name'>Pictures</property>
    <mime-types>
      <mime-type>image/png</mime-type>
    </mime-types>
    <patterns>
      <pattern>*.xcf</pattern>
    </patterns>
    <suffixes>
      <suffix>jpg</suffix>
    </suffixes>
  </object>
</interface>";

            Run(() =>
            {
                var builder = new Gtk.Builder();
                Assert.True(builder.AddFromString(ui));

                var filter = Assert.IsType<Gtk.FileFilter>(builder.GetObject("images"));

                Assert.Equal("Pictures", filter.Name);
                Assert.Contains("standard::display-name", filter.Attributes);
                Assert.Contains("standard::content-type", filter.Attributes);

                using (var png = InfoFor("logo", GLib.ContentType.FromMimeType("image/png")))
                    Assert.True(filter.Match(png.Handle));
                using (var xcf = InfoFor("art.xcf"))
                    Assert.True(filter.Match(xcf.Handle));
                using (var jpg = InfoFor("holiday.jpg"))
                    Assert.True(filter.Match(jpg.Handle));
                using (var txt = InfoFor("notes.txt"))
                    Assert.False(filter.Match(txt.Handle));
            });
        }

        // ------------------------------------------------------------ FileDialog

        [Fact]
        public void A_file_dialog_carries_its_filters_as_a_list_model()
        {
            // The Gtk 3 chooser owned its filters; the Gtk 4 dialog is handed a
            // GListModel it does not own, which is a different ownership story on
            // both sides of the property.
            Run(() =>
            {
                var text = new Gtk.FileFilter { Name = "Text" };
                text.AddSuffix("txt");
                var all = new Gtk.FileFilter { Name = "All" };
                all.AddPattern("*");

                var store = new GLib.ListStore(Gtk.FileFilter.GType);
                store.Append(text.Handle);
                store.Append(all.Handle);

                var dialog = new Gtk.FileDialog
                {
                    Title = "Pick something",
                    AcceptLabel = "_Take it",
                    Modal = false,
                    Filters = store,
                    DefaultFilter = text,
                };

                Assert.Equal("Pick something", dialog.Title);
                Assert.Equal("_Take it", dialog.AcceptLabel);
                Assert.False(dialog.Modal);

                var filters = dialog.Filters;
                Assert.Equal(2u, filters.NItems);
                Assert.Equal("Text", ((Gtk.FileFilter) filters.GetObject(0)).Name);
                Assert.Equal("All", ((Gtk.FileFilter) filters.GetObject(1)).Name);

                // One native object is one managed wrapper, so the default filter
                // comes back as the very object that went in.
                Assert.Same(text, dialog.DefaultFilter);
            });
        }

        [Fact]
        public void A_file_dialogs_initial_file_is_kept_as_a_folder_and_a_name_and_is_not_read_back()
        {
            // set_initial_file is documented as a shortcut for the other two
            // setters, and that is all it is: it stores nothing of its own, so
            // the property does not round-trip. Reading InitialFile back to find
            // out what was proposed returns null, and the answer is in the other
            // two properties.
            Run(() => WithTempDir(dir =>
            {
                var path = Path.Combine(dir, "report.txt");
                File.WriteAllText(path, "x");

                var dialog = new Gtk.FileDialog();
                dialog.InitialFile = GLib.FileFactory.NewForPath(path);

                Assert.Null(dialog.InitialFile);
                Assert.Equal("report.txt", dialog.InitialName);
                AssertSamePath(dir, dialog.InitialFolder.Path);
            }));
        }

        [Fact]
        public void An_open_that_is_cancelled_finishes_with_a_dialog_error()
        {
            // The whole point of the Gtk 4 dialog rewrite is that the answer
            // arrives through a GAsyncResult, and a cancel is the one branch a
            // test without a user can reach. It is also the branch a real program
            // hits most often, and the one whose Finish call is easiest to skip.
            Run(() =>
            {
                var dialog = new Gtk.FileDialog { Modal = false, Title = "cancel me" };
                var cancellable = new GLib.Cancellable();

                GLib.IAsyncResult result = null;
                GLib.Object source = null;
                dialog.Open(null, cancellable, (o, res, data) => { source = o; result = res; });

                cancellable.Cancel();

                Assert.True(PumpUntil(() => result != null), "the open callback never arrived");

                // GAsyncResult names the dialog the operation was started on, so
                // one callback can serve several dialogs.
                Assert.Same(dialog, source);

                var error = Assert.Throws<GLib.GException>(() => dialog.OpenFinish(result));
                Assert.Equal((int) Gtk.DialogError.Cancelled, error.Code);
                Assert.NotEqual((int) Gtk.DialogError.Dismissed, error.Code);
            });
        }

        // ----------------------------------------------------------- AlertDialog

        [Fact]
        public void An_alert_dialog_keeps_the_buttons_it_was_given_in_order()
        {
            // buttons is a "const char * const *" property, which is the family
            // GirToGapi used to collapse to a single string. The indices matter
            // as much as the strings: cancel-button and default-button are
            // positions in this array.
            Run(() =>
            {
                var dialog = new Gtk.AlertDialog("Discard the changes?")
                {
                    Detail = "They cannot be recovered.",
                    Buttons = new[] { "_Cancel", "_Discard", "_Save" },
                    CancelButton = 0,
                    DefaultButton = 2,
                    Modal = true,
                };

                Assert.Equal("Discard the changes?", dialog.Message);
                Assert.Equal("They cannot be recovered.", dialog.Detail);
                Assert.Equal(new[] { "_Cancel", "_Discard", "_Save" }, dialog.Buttons);
                Assert.Equal(0, dialog.CancelButton);
                Assert.Equal(2, dialog.DefaultButton);
                Assert.True(dialog.Modal);
            });
        }

        [Fact]
        public void A_message_carrying_a_percent_sign_is_not_a_format_string()
        {
            // GtkAlertDialog's only C constructor takes a printf format, so
            // codegen emitted nothing for it and left the class with a protected
            // void constructor -- the type that replaced GtkMessageDialog could
            // not be built at all from outside the assembly. The hand-written
            // constructor goes through g_object_new rather than the varargs entry
            // point, which is what makes this assertable: through
            // gtk_alert_dialog_new the message below is undefined behaviour.
            Run(() =>
            {
                var dialog = new Gtk.AlertDialog("Copied 50% of 3 files (100%)");
                Assert.Equal("Copied 50% of 3 files (100%)", dialog.Message);

                var withButtons = new Gtk.AlertDialog("Really?", "_No", "_Yes");
                Assert.Equal("Really?", withButtons.Message);
                Assert.Equal(new[] { "_No", "_Yes" }, withButtons.Buttons);

                var empty = new Gtk.AlertDialog();
                Assert.Empty(empty.Message ?? string.Empty);
            });
        }

        [Fact]
        public void An_alert_cancel_is_reported_in_a_different_error_domain_from_the_rest_of_the_family()
        {
            // Gtk is not consistent here, and a program that handles one of these
            // handles neither. GtkAlertDialog answers a cancel with
            // G_IO_ERROR_CANCELLED; GtkColorDialog -- and GtkFileDialog, and the
            // rest of the 4.10 family -- answers with GTK_DIALOG_ERROR_CANCELLED,
            // a different domain whose code happens to be 1. Comparing the code
            // alone, without the domain, mistakes one for G_IO_ERROR_NOT_FOUND.
            Run(() =>
            {
                var alert = new Gtk.AlertDialog("Pick one")
                {
                    Buttons = new[] { "_No", "_Yes" },
                    CancelButton = 0,
                    Modal = false,
                };

                var alertCancellable = new GLib.Cancellable();
                GLib.IAsyncResult alertResult = null;
                alert.Choose(null, alertCancellable, (o, res, data) => alertResult = res);
                alertCancellable.Cancel();
                Assert.True(PumpUntil(() => alertResult != null), "the choose callback never arrived");

                var alertError = Assert.Throws<GLib.GException>(() => alert.ChooseFinish(alertResult));
                Assert.Equal((int) GLib.IOErrorEnum.Cancelled, alertError.Code);

                var colour = new Gtk.ColorDialog { Modal = false };
                var colourCancellable = new GLib.Cancellable();
                GLib.IAsyncResult colourResult = null;
                var initial = new Gdk.RGBA();
                Assert.True(initial.Parse("#336699"));
                colour.ChooseRgba(null, initial, colourCancellable, (o, res, data) => colourResult = res);
                colourCancellable.Cancel();
                Assert.True(PumpUntil(() => colourResult != null), "the colour callback never arrived");

                var colourError = Assert.Throws<GLib.GException>(() => colour.ChooseRgbaFinish(colourResult));
                Assert.Equal((int) Gtk.DialogError.Cancelled, colourError.Code);

                Assert.NotEqual(alertError.Domain, colourError.Domain);
            });
        }

        // --------------------------------------------- ColorDialog / FontDialog

        [Fact]
        public void A_colour_dialog_keeps_its_properties_and_its_initial_colour_is_by_value()
        {
            // ChooseRgba takes a GdkRGBA by value, which the binding has to
            // allocate, hand over and free around the call: getting that wrong
            // shows up as a colour the dialog never opened on rather than as a
            // crash.
            Run(() =>
            {
                var dialog = new Gtk.ColorDialog
                {
                    Title = "Pick a colour",
                    WithAlpha = false,
                    Modal = false,
                };

                Assert.Equal("Pick a colour", dialog.Title);
                Assert.False(dialog.WithAlpha);
                Assert.False(dialog.Modal);

                dialog.WithAlpha = true;
                Assert.True(dialog.WithAlpha);

                var initial = new Gdk.RGBA();
                Assert.True(initial.Parse("#336699"));

                var cancellable = new GLib.Cancellable();
                GLib.IAsyncResult result = null;
                dialog.ChooseRgba(null, initial, cancellable, (o, res, data) => result = res);
                cancellable.Cancel();

                Assert.True(PumpUntil(() => result != null), "the colour callback never arrived");
                Assert.Throws<GLib.GException>(() => dialog.ChooseRgbaFinish(result));

                // The struct the binding marshalled is the caller's own and is
                // not touched by the call.
                Assert.Equal(0x33 / 255.0, initial.Red, 3);
                Assert.Equal(0x66 / 255.0, initial.Green, 3);
                Assert.Equal(0x99 / 255.0, initial.Blue, 3);
            });
        }

        [Fact]
        public void A_font_dialog_carries_a_language_and_a_filter_across_the_assembly_boundary()
        {
            // Three of GtkFontDialog's four properties are types from other
            // assemblies -- PangoLanguage is a boxed opaque with no identity map,
            // PangoFontMap is a GObject, and the filter is a GtkFilter -- so this
            // is really a test of the marshalling between them.
            Run(() =>
            {
                var dialog = new Gtk.FontDialog { Title = "Pick a font", Modal = false };

                Assert.Equal("Pick a font", dialog.Title);
                Assert.False(dialog.Modal);

                dialog.Language = Pango.Language.FromString("de-DE");
                Assert.Equal("de-de", dialog.Language.ToString());

                var map = new Gtk.Label("x").PangoContext.FontMap;
                dialog.FontMap = map;
                Assert.Same(map, dialog.FontMap);

                var filter = new Gtk.FileFilter();
                dialog.Filter = filter;
                Assert.Same(filter, dialog.Filter);

                dialog.Filter = null;
                Assert.Null(dialog.Filter);
            });
        }

        // -------------------------------------------------------- the launchers

        [Fact]
        public void A_uri_launcher_keeps_the_uri_it_was_built_with()
        {
            // Deliberately not launched. Docs/testing.md records why the
            // LinkButton sample is skipped: asking the desktop to open a URI
            // opens a browser on the machine running the suite.
            Run(() =>
            {
                var launcher = new Gtk.UriLauncher("https://example.invalid/page?a=1&b=2");

                Assert.Equal("https://example.invalid/page?a=1&b=2", launcher.Uri);

                launcher.Uri = "mailto:someone@example.invalid";
                Assert.Equal("mailto:someone@example.invalid", launcher.Uri);
            });
        }

        [Fact]
        public void A_file_launcher_holds_the_file_it_was_built_with()
        {
            Run(() => WithTempDir(dir =>
            {
                var path = Path.Combine(dir, "document.txt");
                File.WriteAllText(path, "content");

                var file = GLib.FileFactory.NewForPath(path);
                var launcher = new Gtk.FileLauncher(file);

                // A GInterfaceAdapter is a fresh wrapper every time, so the
                // comparison has to be g_file_equal rather than reference
                // identity -- the same trap GioDeepTests names.
                Assert.True(file.Equal(launcher.File));
                Assert.Equal(path, launcher.File.Path);

                Assert.False(launcher.AlwaysAsk);
                launcher.AlwaysAsk = true;
                Assert.True(launcher.AlwaysAsk);

                launcher.Writable = true;
                Assert.True(launcher.Writable);

                launcher.File = GLib.FileFactory.NewForPath(dir);
                AssertSamePath(dir, launcher.File.Path);
            }));
        }

        // ----------------------------------------------- the legacy FileChooser

        [Fact]
        public void A_chooser_reports_the_current_folder_as_the_GFile_it_was_given()
        {
            // In Gtk 3 a chooser spoke in filenames; in Gtk 4 it speaks in
            // GFiles. The metadata rule that retyped this one as a filename
            // survived the port, so the getter read a GObject's memory as a
            // NUL-terminated string and then g_free'd the object -- an
            // application that set a folder and read it back corrupted the heap.
            //
            // The pump is not incidental: a GtkFileChooserWidget loads the folder
            // through the main loop, so it reports nothing at all until the loop
            // has turned.
            Run(() => WithTempDir(dir =>
            {
                var chooser = new Gtk.FileChooserWidget(Gtk.FileChooserAction.Open);
                var folder = GLib.FileFactory.NewForPath(dir);

                Assert.True(chooser.SetCurrentFolder(folder));
                Assert.True(PumpUntil(() => chooser.CurrentFolder != null, 5000),
                            "the chooser never reported a current folder");

                var reported = chooser.CurrentFolder;
                Assert.True(folder.Equal(reported));
                AssertSamePath(dir, reported.Path);
            }));
        }

        [Fact]
        public void A_shortcut_folder_added_to_a_chooser_appears_in_its_shortcut_list()
        {
            // The same defect the other way round: the folder parameter was typed
            // as a filename, so Gtk was handed a char* to dereference as a GFile.
            Run(() => WithTempDir(dir =>
            {
                var chooser = new Gtk.FileChooserWidget(Gtk.FileChooserAction.Open);
                var folder = GLib.FileFactory.NewForPath(dir);

                var before = chooser.ShortcutFolders.NItems;

                Assert.True(chooser.AddShortcutFolder(folder));

                var after = chooser.ShortcutFolders;
                Assert.Equal(before + 1, after.NItems);
                AssertSamePath(dir,
                               GLib.FileAdapter.GetObject(after.GetObject(after.NItems - 1)).Path);

                Assert.True(chooser.RemoveShortcutFolder(folder));
                Assert.Equal(before, chooser.ShortcutFolders.NItems);
            }));
        }

        [Fact]
        public void Filters_added_to_a_chooser_are_listed_and_one_of_them_is_current()
        {
            Run(() =>
            {
                var chooser = new Gtk.FileChooserWidget(Gtk.FileChooserAction.Open);

                var text = new Gtk.FileFilter { Name = "Text" };
                text.AddSuffix("txt");
                var images = new Gtk.FileFilter { Name = "Images" };
                images.AddMimeType("image/png");

                chooser.AddFilter(text);
                chooser.AddFilter(images);

                var listed = chooser.Filters;
                Assert.Equal(2u, listed.NItems);
                Assert.Equal("Text", ((Gtk.FileFilter) listed.GetObject(0)).Name);

                chooser.Filter = images;
                Assert.Same(images, chooser.Filter);

                chooser.RemoveFilter(images);
                Assert.Equal(1u, chooser.Filters.NItems);
                Assert.Equal("Text", ((Gtk.FileFilter) chooser.Filters.GetObject(0)).Name);
            });
        }

        [Fact]
        public void A_choice_offered_by_a_chooser_round_trips_through_its_id()
        {
            // Two parallel string arrays -- the ids and their labels -- which is
            // the "const char * const *" shape again, twice in one call.
            Run(() =>
            {
                var chooser = new Gtk.FileChooserWidget(Gtk.FileChooserAction.Save);

                chooser.AddChoice("encoding", "Character encoding",
                                  new[] { "utf-8", "latin-1" },
                                  new[] { "Unicode (UTF-8)", "Western (ISO-8859-1)" });

                // A chooser starts on the first option it was offered.
                Assert.Equal("utf-8", chooser.GetChoice("encoding"));

                chooser.SetChoice("encoding", "latin-1");
                Assert.Equal("latin-1", chooser.GetChoice("encoding"));

                chooser.RemoveChoice("encoding");
                Assert.Null(chooser.GetChoice("encoding"));
            });
        }

        // -------------------------------------------------------- PrintOperation

        [Fact]
        public void A_print_operation_starts_out_initial_and_keeps_what_it_is_told()
        {
            Run(() =>
            {
                var setup = new Gtk.PageSetup();
                setup.PaperSize = new Gtk.PaperSize("iso_a4");

                var settings = new Gtk.PrintSettings();
                settings.NCopies = 4;

                var op = new Gtk.PrintOperation
                {
                    JobName = "Quarterly report",
                    NPages = 5,
                    Unit = Gtk.Unit.Mm,
                    UseFullPage = true,
                    ShowProgress = false,
                    DefaultPageSetup = setup,
                    PrintSettings = settings,
                };

                Assert.Equal("Quarterly report", op.JobName);
                Assert.Equal(5, op.NPages);
                Assert.Equal(Gtk.Unit.Mm, op.Unit);
                Assert.True(op.UseFullPage);
                Assert.Equal(4, op.PrintSettings.NCopies);
                Assert.Equal("iso_a4", op.DefaultPageSetup.PaperSize.Name);

                // Nothing has been printed, so the status is the one every
                // operation begins in and the page count is not yet known --
                // n-pages-to-print is -1, not 0, because 0 is a real answer.
                Assert.Equal(Gtk.PrintStatus.Initial, op.Status);
                Assert.False(op.IsFinished);
                Assert.Equal(-1, op.NPagesToPrint);
            });
        }

        [Fact]
        public void Exporting_a_print_operation_draws_every_page_in_order_and_writes_a_pdf()
        {
            // GTK_PRINT_OPERATION_ACTION_EXPORT is the whole print pipeline with
            // no printer and no dialog at either end, so it is the one way to run
            // the signal chain to completion in a test: begin-print, then
            // draw-page once per page in order, then end-print, then done.
            Run(() => WithTempDir(dir =>
            {
                var path = Path.Combine(dir, "export.pdf");

                var setup = new Gtk.PageSetup();
                setup.PaperSize = new Gtk.PaperSize("iso_a4");

                var op = new Gtk.PrintOperation
                {
                    ExportFilename = path,
                    JobName = "export test",
                    DefaultPageSetup = setup,
                };

                var order = new List<string>();
                var drawn = new List<int>();
                double contextWidth = 0;
                double contextDpi = 0;

                op.BeginPrint += (o, e) =>
                {
                    order.Add("begin");
                    op.NPages = 3;
                    contextWidth = e.Context.Width;
                    contextDpi = e.Context.DpiX;
                };

                op.DrawPage += (o, e) =>
                {
                    order.Add("draw" + e.PageNr);
                    drawn.Add(e.PageNr);

                    // The cairo context belongs to the print context, and the
                    // binding takes a reference when it wraps it, so the wrapper
                    // has to be disposed or the surface outlives the operation.
                    using (var cr = e.Context.CairoContext)
                    {
                        cr.MoveTo(20, 20 + 10 * e.PageNr);
                        cr.LineTo(120, 20 + 10 * e.PageNr);
                        cr.Stroke();
                    }
                };

                op.EndPrint += (o, e) => order.Add("end");
                op.Done += (o, e) => order.Add("done");

                var result = op.Run(Gtk.PrintOperationAction.Export, null);

                Assert.Equal(Gtk.PrintOperationResult.Apply, result);
                Assert.Equal(new[] { 0, 1, 2 }, drawn);
                Assert.Equal(new[] { "begin", "draw0", "draw1", "draw2", "end", "done" }, order);
                Assert.Equal(3, op.NPagesToPrint);

                // An export surface is a PDF at 72dpi, so the print context is
                // the printable page measured in points. One point of tolerance:
                // A4's 210mm is not a whole number of points.
                Assert.Equal(72.0, contextDpi, 6);
                Assert.Equal(setup.GetPageWidth(Gtk.Unit.Points), contextWidth, 1);

                // An export never reaches Finished. That status is reported by a
                // print backend watching a spooled job, and there is no backend
                // here -- so an application that waits for IsFinished after an
                // export waits forever, even though ::done has already run and
                // the file is complete.
                Assert.Equal(Gtk.PrintStatus.GeneratingData, op.Status);
                Assert.False(op.IsFinished);

                Assert.True(File.Exists(path), "the export wrote no file");
                var bytes = File.ReadAllBytes(path);
                Assert.True(bytes.Length > 1000, "the export wrote " + bytes.Length + " bytes");
                Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
            }));
        }

        [Fact]
        public void Cancelling_from_begin_print_stops_before_the_first_page_is_drawn()
        {
            // gtk_print_operation_cancel is documented for the begin-print and
            // paginate handlers, which is where an application discovers it has
            // nothing to print. ::done still runs -- that is the contract, and it
            // is where the result has to be read.
            Run(() => WithTempDir(dir =>
            {
                var path = Path.Combine(dir, "cancelled.pdf");

                var op = new Gtk.PrintOperation
                {
                    ExportFilename = path,
                    DefaultPageSetup = new Gtk.PageSetup(),
                };

                var drawn = 0;
                var doneFired = false;

                op.BeginPrint += (o, e) =>
                {
                    op.NPages = 2;
                    op.Cancel();
                };

                op.DrawPage += (o, e) =>
                {
                    drawn++;
                    using (var cr = e.Context.CairoContext)
                        cr.Paint();
                };

                op.Done += (o, e) => doneFired = true;

                Assert.Equal(Gtk.PrintOperationResult.Cancel,
                             op.Run(Gtk.PrintOperationAction.Export, null));

                Assert.Equal(0, drawn);
                Assert.True(doneFired, "::done did not run after a cancel");
                Assert.Equal(Gtk.PrintStatus.FinishedAborted, op.Status);
                Assert.True(op.IsFinished);
            }));
        }

        [Fact]
        public void An_export_with_no_filename_reports_failure_in_its_return_value_and_not_as_an_error()
        {
            // export-filename is the one property the export action cannot do
            // without. Gtk fails the g_return_val_if_fail and hands back
            // GTK_PRINT_OPERATION_RESULT_ERROR *without* filling in the GError,
            // so a caller that only catches GException sees an export that
            // silently did nothing. The return value is the only signal there is.
            Run(() =>
            {
                var op = new Gtk.PrintOperation { DefaultPageSetup = new Gtk.PageSetup() };
                var drawn = 0;
                op.BeginPrint += (o, e) => op.NPages = 1;
                op.DrawPage += (o, e) =>
                {
                    drawn++;
                    using (var cr = e.Context.CairoContext)
                        cr.Paint();
                };

                Assert.Equal(Gtk.PrintOperationResult.Error,
                             op.Run(Gtk.PrintOperationAction.Export, null));

                Assert.Equal(0, drawn);
                Assert.Equal(Gtk.PrintStatus.Initial, op.Status);

                // And nothing to find afterwards either: get_error has no error
                // to propagate.
                op.GetError();
            });
        }
    }
}

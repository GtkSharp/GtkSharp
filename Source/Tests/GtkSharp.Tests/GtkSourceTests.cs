using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// GtkSourceView beyond what <c>SatelliteAssemblyTests</c> reaches: source
    /// marks, style schemes, context classes, and loading and saving a buffer
    /// through <c>GtkSource.File</c>.
    /// </summary>
    /// <remarks>
    /// <c>GtkSourceSharp</c> binds 102 types and the suite named eleven of them.
    /// Its hand-written layer is twelve lines, so the coverage table in
    /// <c>Docs/coverage.md</c> says almost nothing about it — what matters is
    /// whether the generated surface actually works, and a generated wrapper for
    /// a missing or renamed export is a null delegate rather than a link error.
    ///
    /// The oracles here are outside the library wherever one exists: bytes the
    /// test wrote to disk and read back, line numbers it counted itself, and the
    /// language ids GtkSourceView publishes.
    /// </remarks>
    public class GtkSourceTests : GtkTestBase
    {
        public GtkSourceTests(GtkFixture fixture) : base(fixture) { }

        static GtkSource.Buffer BufferWith(string text)
        {
            var buffer = new GtkSource.Buffer(new Gtk.TextTagTable());
            buffer.Text = text;
            return buffer;
        }

        static string TextOf(Gtk.TextBuffer buffer)
            => buffer.GetText(buffer.StartIter, buffer.EndIter, true);

        /// <summary>Gtk 4's GetIterAtLine answers through an out-parameter and
        /// returns whether the line existed; every use here is a line the test
        /// just wrote, so the bool is asserted rather than dropped.</summary>
        static Gtk.TextIter LineStart(Gtk.TextBuffer buffer, int line)
        {
            Assert.True(buffer.GetIterAtLine(out var iter, line), $"line {line} should exist");
            return iter;
        }

        static bool PumpUntil(Func<bool> condition, int timeoutMs = 5000)
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (!condition() && clock.ElapsedMilliseconds < timeoutMs)
            {
                if (Gtk.Application.EventsPending())
                    Gtk.Application.RunIteration(false);
                else
                    System.Threading.Thread.Sleep(1);
            }

            return condition();
        }

        // -------------------------------------------------------- source marks

        [Fact]
        public void A_source_mark_is_found_at_the_line_it_was_put_on()
        {
            // Source marks are how a gutter shows breakpoints and errors. The
            // array-returning lookups are the interesting part: both are the
            // shape this binding has repeatedly got wrong.
            Run(() =>
            {
                var buffer = BufferWith("one\ntwo\nthree\nfour");

                var third = LineStart(buffer, 2);
                buffer.CreateSourceMark("breakpoint-1", "breakpoint", third);

                var found = buffer.GetSourceMarksAtLine(2, "breakpoint");

                Assert.Single(found);
                Assert.Equal("breakpoint-1", found[0].Name);
                Assert.Equal("breakpoint", found[0].Category);

                // ...and nowhere else.
                Assert.Empty(buffer.GetSourceMarksAtLine(0, "breakpoint"));
            });
        }

        [Fact]
        public void Marks_of_a_different_category_are_not_returned()
        {
            // The category filter is the whole point of the API: an editor puts
            // breakpoints and errors in the same buffer and asks for one kind.
            Run(() =>
            {
                var buffer = BufferWith("one\ntwo\nthree");

                buffer.CreateSourceMark("b", "breakpoint", LineStart(buffer, 1));
                buffer.CreateSourceMark("e", "error", LineStart(buffer, 1));

                Assert.Single(buffer.GetSourceMarksAtLine(1, "breakpoint"));
                Assert.Single(buffer.GetSourceMarksAtLine(1, "error"));

                // A null category means every category.
                Assert.Equal(2, buffer.GetSourceMarksAtLine(1, null).Length);
            });
        }

        [Fact]
        public void Walking_forward_finds_the_next_mark_and_stops_at_the_last()
        {
            Run(() =>
            {
                var buffer = BufferWith("one\ntwo\nthree\nfour\nfive");

                buffer.CreateSourceMark("a", "breakpoint", LineStart(buffer, 1));
                buffer.CreateSourceMark("b", "breakpoint", LineStart(buffer, 3));

                var iter = buffer.StartIter;

                Assert.True(buffer.ForwardIterToSourceMark(ref iter, "breakpoint"));
                Assert.Equal(1, iter.Line);

                Assert.True(buffer.ForwardIterToSourceMark(ref iter, "breakpoint"));
                Assert.Equal(3, iter.Line);

                // Nothing after the last one, and the iterator is left alone.
                Assert.False(buffer.ForwardIterToSourceMark(ref iter, "breakpoint"));
            });
        }

        [Fact]
        public void Removing_marks_over_a_range_removes_only_those_in_it()
        {
            Run(() =>
            {
                var buffer = BufferWith("one\ntwo\nthree\nfour");

                buffer.CreateSourceMark("a", "breakpoint", LineStart(buffer, 0));
                buffer.CreateSourceMark("b", "breakpoint", LineStart(buffer, 2));

                buffer.RemoveSourceMarks(LineStart(buffer, 2), buffer.EndIter, "breakpoint");

                Assert.Single(buffer.GetSourceMarksAtLine(0, "breakpoint"));
                Assert.Empty(buffer.GetSourceMarksAtLine(2, "breakpoint"));
            });
        }

        // ------------------------------------------------------ style schemes

        [Fact]
        public void The_style_scheme_manager_publishes_schemes_it_can_then_hand_over()
        {
            // The ids come from GtkSourceView, so the test asks it for the list
            // and then asks for each one back rather than hard-coding a name that
            // a future release might drop.
            Run(() =>
            {
                var manager = GtkSource.StyleSchemeManager.Default;
                var ids = manager.SchemeIds;

                Assert.NotNull(ids);
                Assert.NotEmpty(ids);

                foreach (var id in ids.Take(5))
                {
                    var scheme = manager.GetScheme(id);

                    Assert.NotNull(scheme);
                    Assert.Equal(id, scheme.Id);
                    Assert.False(string.IsNullOrEmpty(scheme.Name), $"scheme '{id}' should have a name");
                }
            });
        }

        [Fact]
        public void Asking_for_a_scheme_that_does_not_exist_gives_nothing()
        {
            Run(() => Assert.Null(GtkSource.StyleSchemeManager.Default.GetScheme("no-such-scheme-9f2a")));
        }

        [Fact]
        public void A_buffer_keeps_the_style_scheme_it_is_given()
        {
            Run(() =>
            {
                var manager = GtkSource.StyleSchemeManager.Default;
                var id = manager.SchemeIds.First();
                var scheme = manager.GetScheme(id);

                var buffer = BufferWith("text");
                buffer.StyleScheme = scheme;

                Assert.NotNull(buffer.StyleScheme);
                Assert.Equal(id, buffer.StyleScheme.Id);
            });
        }

        // ---------------------------------------------------------- languages

        [Fact]
        public void A_language_reports_the_details_the_manager_indexed_it_under()
        {
            Run(() =>
            {
                var manager = GtkSource.LanguageManager.Default;
                var language = manager.GetLanguage("c");

                Assert.NotNull(language);
                Assert.Equal("c", language.Id);
                Assert.False(string.IsNullOrEmpty(language.Name));

                // A C source file is what this language is for, so its globs have
                // to mention one.
                Assert.Contains("*.c", language.Globs);
                Assert.Contains("text/x-csrc", language.MimeTypes);
            });
        }

        [Fact]
        public void A_buffer_given_a_language_highlights_with_it()
        {
            Run(() =>
            {
                var language = GtkSource.LanguageManager.Default.GetLanguage("c");

                var buffer = new GtkSource.Buffer(language);

                Assert.NotNull(buffer.Language);
                Assert.Equal("c", buffer.Language.Id);
                Assert.True(buffer.HighlightSyntax, "a buffer with a language highlights by default");
            });
        }

        [Fact]
        public void Context_classes_say_which_part_of_the_syntax_an_iterator_is_in()
        {
            // The feature an editor uses to decide whether the cursor is inside a
            // comment or a string. GtkSourceView computes it from the language
            // definition, so the oracle is the code the test wrote.
            Run(() =>
            {
                var language = GtkSource.LanguageManager.Default.GetLanguage("c");
                var buffer = new GtkSource.Buffer(language);

                buffer.Text = "int x; /* a comment */\n";
                buffer.EnsureHighlight(buffer.StartIter, buffer.EndIter);

                Assert.True(PumpUntil(() =>
                    buffer.IterHasContextClass(buffer.GetIterAtOffset(12), "comment")),
                    "the text inside /* */ should be in the comment context class");

                Assert.False(buffer.IterHasContextClass(buffer.GetIterAtOffset(1), "comment"));

                var classes = buffer.GetContextClassesAtIter(buffer.GetIterAtOffset(12));
                Assert.Contains("comment", classes);
            });
        }

        // ------------------------------------------------- buffer manipulation

        [Fact]
        public void Sorting_lines_reorders_only_the_range_it_was_given()
        {
            // Arithmetic the test can do itself, and the lines outside the range
            // are the control.
            Run(() =>
            {
                var buffer = BufferWith("header\ndelta\nalpha\ncharlie\nfooter\n");

                // The end iterator is exclusive: pointing it at the start of line 3
                // sorts lines 1 and 2 only. Reaching line 3 means pointing at the
                // start of line 4, which is the off-by-one this test was written
                // with and is the reason it now says so.
                buffer.SortLines(LineStart(buffer, 1), LineStart(buffer, 4),
                                 GtkSource.SortFlags.None, 0);

                var lines = TextOf(buffer).Split('\n');

                Assert.Equal("header", lines[0]);
                Assert.Equal(new[] { "alpha", "charlie", "delta" }, lines.Skip(1).Take(3).ToArray());
                Assert.Equal("footer", lines[4]);
            });
        }

        [Fact]
        public void Sorting_can_be_reversed_and_can_ignore_case()
        {
            Run(() =>
            {
                var buffer = BufferWith("b\na\nc\n");

                buffer.SortLines(buffer.StartIter, LineStart(buffer, 3),
                                 GtkSource.SortFlags.ReverseOrder, 0);

                Assert.Equal(new[] { "c", "b", "a" }, TextOf(buffer).Split('\n').Take(3).ToArray());
            });
        }

        [Fact]
        public void Joining_lines_replaces_the_newlines_between_them()
        {
            Run(() =>
            {
                var buffer = BufferWith("one\ntwo\nthree\n");

                buffer.JoinLines(buffer.StartIter, LineStart(buffer, 2));

                var text = TextOf(buffer);

                Assert.DoesNotContain("one\ntwo", text);
                Assert.Contains("one", text);
                Assert.Contains("three", text);
            });
        }

        [Fact]
        public void Changing_case_rewrites_only_the_selected_range()
        {
            Run(() =>
            {
                var buffer = BufferWith("hello world");

                buffer.ChangeCase(GtkSource.ChangeCaseType.Upper,
                                  buffer.StartIter, buffer.GetIterAtOffset(5));

                Assert.Equal("HELLO world", TextOf(buffer));
            });
        }

        // --------------------------------------------------- File load and save

        [Fact]
        public void A_buffer_saved_through_GtkSource_File_lands_on_disk_as_its_text()
        {
            // The oracle is entirely outside the library: the bytes File.ReadAllText
            // gets back are the ones the test can check without asking
            // GtkSourceView anything.
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(), "gtksharp-source-" + Guid.NewGuid().ToString("N") + ".txt");

                try
                {
                    var buffer = BufferWith("saved through GtkSource.File\n");

                    var file = new GtkSource.File { Location = GLib.FileFactory.NewForPath(path) };
                    var saver = new GtkSource.FileSaver(buffer, file);

                    bool done = false, saved = false;
                    // 0 is G_PRIORITY_DEFAULT; the parameter is a plain int here.
                    saver.SaveAsync(0, null, null, (o, result, data) =>
                    {
                        saved = saver.SaveFinish(result);
                        done = true;
                    });

                    Assert.True(PumpUntil(() => done), "the save should complete");
                    Assert.True(saved, "and should report success");

                    Assert.True(System.IO.File.Exists(path), "the file should have been written");

                    // Two newlines, not one. GtkSource.Buffer has
                    // ImplicitTrailingNewline set by default: the buffer's own
                    // text does not include a final newline, and the saver adds
                    // one. Text that already ends in a newline therefore gains a
                    // second. That is GtkSourceView working as designed, and it
                    // is exactly what an editor wants, but it surprised this test
                    // when it was written.
                    Assert.Equal("saved through GtkSource.File\n\n", System.IO.File.ReadAllText(path));
                }
                finally
                {
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
            });
        }

        [Fact]
        public void A_buffer_loaded_through_GtkSource_File_holds_what_the_file_held()
        {
            // The other direction, and the text came from the test rather than
            // from a previous call to the library.
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(), "gtksharp-source-" + Guid.NewGuid().ToString("N") + ".txt");

                try
                {
                    System.IO.File.WriteAllText(path, "loaded from disk\nsecond line\n");

                    var buffer = new GtkSource.Buffer(new Gtk.TextTagTable());
                    var file = new GtkSource.File { Location = GLib.FileFactory.NewForPath(path) };
                    var loader = new GtkSource.FileLoader(buffer, file);

                    bool done = false, loaded = false;
                    loader.LoadAsync(0, null, null, (o, result, data) =>
                    {
                        loaded = loader.LoadFinish(result);
                        done = true;
                    });

                    Assert.True(PumpUntil(() => done), "the load should complete");
                    Assert.True(loaded, "and should report success");

                    // The mirror image of the save: the trailing newline is
                    // implicit, so it is stripped from the buffer's text rather
                    // than being part of it.
                    Assert.Equal("loaded from disk\nsecond line", TextOf(buffer));
                    Assert.False(buffer.Loading, "the buffer should not still be loading");
                }
                finally
                {
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
            });
        }

        [Fact]
        public void The_implicit_trailing_newline_is_on_by_default_and_can_be_turned_off()
        {
            // The setting behind the two round-trip tests above. With it on, a
            // buffer's text never ends in a newline and the saver supplies one;
            // with it off, what you put in is what gets written. Both halves are
            // asserted, because "the file has a trailing newline" means nothing
            // without the case where it does not.
            Run(() =>
            {
                var buffer = BufferWith("no newline here");

                Assert.True(buffer.ImplicitTrailingNewline,
                            "GtkSource.Buffer adds one by default");

                buffer.ImplicitTrailingNewline = false;
                Assert.False(buffer.ImplicitTrailingNewline);
            });
        }

        [Fact]
        public void Turning_the_implicit_newline_off_writes_the_text_unchanged()
        {
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(),
                                        "gtksharp-source-" + Guid.NewGuid().ToString("N") + ".txt");

                try
                {
                    var buffer = new GtkSource.Buffer(new Gtk.TextTagTable())
                    {
                        ImplicitTrailingNewline = false,
                    };
                    buffer.Text = "exactly this";

                    var file = new GtkSource.File { Location = GLib.FileFactory.NewForPath(path) };
                    var saver = new GtkSource.FileSaver(buffer, file);

                    bool done = false;
                    saver.SaveAsync(0, null, null, (o, result, data) =>
                    {
                        saver.SaveFinish(result);
                        done = true;
                    });

                    Assert.True(PumpUntil(() => done), "the save should complete");
                    Assert.Equal("exactly this", System.IO.File.ReadAllText(path));
                }
                finally
                {
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
            });
        }

        // ------------------------------------------------------------- the view

        [Fact]
        public void A_source_view_keeps_the_display_settings_it_is_given()
        {
            Run(() =>
            {
                var view = new GtkSource.SourceView
                {
                    ShowLineNumbers = true,
                    HighlightCurrentLine = true,
                    TabWidth = 3,
                    InsertSpacesInsteadOfTabs = true,
                    AutoIndent = true,
                };

                Assert.True(view.ShowLineNumbers);
                Assert.True(view.HighlightCurrentLine);
                Assert.Equal(3u, view.TabWidth);
                Assert.True(view.InsertSpacesInsteadOfTabs);
                Assert.True(view.AutoIndent);

                // ...and turning one off does not turn the others off.
                view.ShowLineNumbers = false;

                Assert.False(view.ShowLineNumbers);
                Assert.True(view.HighlightCurrentLine);
            });
        }

        [Fact]
        public void A_source_view_shows_the_buffer_it_was_built_around()
        {
            Run(() =>
            {
                var buffer = BufferWith("in the view");
                var view = new GtkSource.SourceView(buffer);

                Assert.Same(buffer, view.Buffer);
                Assert.Equal("in the view", TextOf(view.Buffer));
            });
        }
    }
}

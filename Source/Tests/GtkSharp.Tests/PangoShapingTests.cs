using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pango;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The half of Pango that turns text into glyphs: itemization, shaping, the
    /// break algorithm, bidi and the layout iterator. <c>PangoTests</c> covers
    /// attributes, measurement and font descriptions; nothing had ever called
    /// this half.
    /// </summary>
    /// <remarks>
    /// Text shaping has oracles outside the library, which is why this is worth
    /// testing at all: Unicode says where the word boundaries are, the bidi
    /// algorithm says what embedding level a Hebrew run gets, and arithmetic
    /// says that per-character widths have to add up to the run's width.
    ///
    /// Several functions here were unusable before this file existed. Pango
    /// fills caller-provided arrays whose length is a separate argument or is
    /// implied by the text, and neither the api.xml nor the codegen has a rule
    /// for that shape, so each came out taking or returning a single value --
    /// an <c>int*</c> the callee fills with one entry per character became
    /// <c>out int</c>, and Pango wrote the rest past the end of a four-byte
    /// block. They are hidden in PangoSharp.metadata and rebound over real
    /// arrays; the tests below are what says the rebinding is right.
    /// </remarks>
    public class PangoShapingTests : GtkTestBase
    {
        public PangoShapingTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>A layout on a real Cairo context, which is the only way to get
        /// a Pango context without a display. The Cairo objects are released here
        /// because the Pango context copies what it needs from them; a leaked
        /// Cairo IDisposable takes the test host down on a later GC.</summary>
        private static Layout LayoutOf(string text, string font = "Sans 12")
        {
            using (var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 400, 200))
            using (var cr = new Cairo.Context(surface))
            {
                var layout = Pango.CairoHelper.CreateLayout(cr);
                layout.FontDescription = FontDescription.FromString(font);
                layout.SetText(text);
                return layout;
            }
        }

        private static Pango.Context ContextOf() => LayoutOf("x").Context;

        private static Item[] Itemize(Pango.Context context, string text) =>
            Pango.Global.ItemizeWithBaseDir(context, Direction.Ltr, text, 0,
                                            Encoding.UTF8.GetByteCount(text), null, null);

        /// <summary>Itemizes and shapes text that is all one script, so there is
        /// exactly one item and the glyph string covers the whole string.</summary>
        private static void ShapeOne(string text, out Item item, out GlyphString glyphs)
        {
            var items = Itemize(ContextOf(), text);
            Assert.Single(items);
            item = items[0];
            glyphs = Pango.Global.Shape(text, item.Analysis);
        }

        private static int[] IndexesWhere(LogAttr[] attrs, Func<LogAttr, bool> predicate) =>
            Enumerable.Range(0, attrs.Length).Where(i => predicate(attrs[i])).ToArray();

        // ------------------------------------------------------- break algorithm

        [Fact]
        public void A_log_attr_array_has_one_entry_per_code_point_plus_a_final_one()
        {
            // Two emoji are four UTF-16 units and two code points, so a binding
            // that sized the array by string.Length would ask Pango for five
            // entries and read four. Pango counts what g_utf8_strlen counts.
            Run(() =>
            {
                Assert.Equal(6, Pango.Global.GetLogAttrs("hello", 0, null).Length);

                const string emoji = "\U0001F600\U0001F600";

                Assert.Equal(4, emoji.Length);
                Assert.Equal(3, Pango.Global.GetLogAttrs(emoji, 0, null).Length);
            });
        }

        [Fact]
        public void Word_starts_and_ends_land_on_the_word_boundaries()
        {
            // Unicode UAX #29, not Pango's opinion: a word ends at the position
            // *after* its last character, so "one two three" ends words at 3, 7
            // and 13 -- the last of which is the sentinel entry past the text.
            Run(() =>
            {
                var attrs = Pango.Global.GetLogAttrs("one two three", 0, null);

                Assert.Equal(new[] { 0, 4, 8 }, IndexesWhere(attrs, a => a.IsWordStart));
                Assert.Equal(new[] { 3, 7, 13 }, IndexesWhere(attrs, a => a.IsWordEnd));
                Assert.Equal(new[] { 3, 7, 13 }, IndexesWhere(attrs, a => a.IsWhite));
            });
        }

        [Fact]
        public void A_combining_mark_is_not_a_cursor_position()
        {
            // e + U+0301 is two code points and one grapheme, so the cursor may
            // not sit between them. Written as an escape rather than a literal:
            // a source file can be normalised, and a precomposed U+00E9 would
            // make this pass for the wrong reason.
            Run(() =>
            {
                var attrs = Pango.Global.GetLogAttrs("e\u0301x", 0, null);

                Assert.Equal(4, attrs.Length);
                Assert.Equal(new[] { 0, 2, 3 }, IndexesWhere(attrs, a => a.IsCursorPosition));
                Assert.False(attrs[1].IsCharBreak);

                // Backspace at the end of the grapheme removes the whole thing.
                Assert.True(attrs[0].BackspaceDeletesCharacter);
            });
        }

        [Fact]
        public void A_newline_is_a_mandatory_break_and_a_space_is_not()
        {
            // The break is reported at the position *after* the newline, and the
            // sentinel at the end of the paragraph is mandatory too. A space is
            // a line break opportunity but never a mandatory one.
            Run(() =>
            {
                var broken = Pango.Global.GetLogAttrs("a\nb", 0, null);
                Assert.Equal(new[] { 2, 3 }, IndexesWhere(broken, a => a.IsMandatoryBreak));

                var spaced = Pango.Global.GetLogAttrs("a b", 0, null);
                Assert.Equal(new[] { 3 }, IndexesWhere(spaced, a => a.IsMandatoryBreak));
                Assert.Equal(new[] { 2, 3 }, IndexesWhere(spaced, a => a.IsLineBreak));
            });
        }

        [Fact]
        public void The_layouts_own_log_attrs_agree_with_the_global_break_function()
        {
            // Two independently bound routes to the same answer:
            // pango_layout_get_log_attrs_readonly hands back an array Pango owns,
            // while pango_get_log_attrs fills one this binding allocates. They
            // must agree bit for bit -- which is the check that says the rebound
            // array argument is the right length and the right way round.
            Run(() =>
            {
                const string text = "one two three";
                var layout = LayoutOf(text);

                var fromLayout = layout.LogAttrs;
                var fromGlobal = Pango.Global.GetLogAttrs(text, 0, null);

                Assert.Equal(14, fromGlobal.Length);
                Assert.Equal(fromLayout.Select(a => a.Bitfield), fromGlobal.Select(a => a.Bitfield));
            });
        }

        [Fact]
        public void Tailoring_a_break_in_place_reproduces_get_log_attrs()
        {
            // pango_get_log_attrs is documented as pango_default_break followed
            // by pango_tailor_break per item, so doing it by hand has to land on
            // the same answer. TailorBreak is the one of the four that reads the
            // array as well as writing it, so this also proves it is passed in.
            Run(() =>
            {
                const string text = "one two three";
                var item = Itemize(ContextOf(), text).Single();

                var attrs = Pango.Global.DefaultBreak(text, item.Analysis);
                Pango.Global.TailorBreak(text, item.Analysis, item.Offset, attrs);

                Assert.Equal(Pango.Global.GetLogAttrs(text, 0, null).Select(a => a.Bitfield),
                             attrs.Select(a => a.Bitfield));

                // pango_break is the deprecated name for exactly that pair, and
                // is kept only because it is the fourth member of the family
                // whose array argument had to be rebound; this is what says the
                // rebinding of it is right rather than merely compiling.
#pragma warning disable 618
                Assert.Equal(attrs.Select(a => a.Bitfield),
                             Pango.Global.Break(text, item.Analysis).Select(a => a.Bitfield));
#pragma warning restore 618

                Assert.Throws<ArgumentNullException>(() =>
                    Pango.Global.TailorBreak(text, item.Analysis, 0, null));
            });
        }

        // ------------------------------------------------------------------ bidi

        [Fact]
        public void Embedding_levels_are_even_for_the_latin_run_and_odd_for_the_hebrew_one()
        {
            // The Unicode bidirectional algorithm, which is the oracle here: in
            // an LTR paragraph the Hebrew letters go one level deeper, and in an
            // RTL paragraph it is the Latin ones. The space between them is
            // neutral and takes the paragraph level both times.
            //
            // pango_log2vis_get_embedding_levels returns a g_malloc'd byte per
            // character; codegen bound it as a plain byte, so what came back was
            // the low eight bits of the array's address.
            Run(() =>
            {
                const string text = "abc אבג";

                var direction = Direction.Ltr;
                Assert.Equal(new byte[] { 0, 0, 0, 0, 1, 1, 1 },
                             Pango.Global.Log2visGetEmbeddingLevels(text, ref direction));
                Assert.Equal(Direction.Ltr, direction);

                direction = Direction.Rtl;
                Assert.Equal(new byte[] { 2, 2, 2, 1, 1, 1, 1 },
                             Pango.Global.Log2visGetEmbeddingLevels(text, ref direction));
                Assert.Equal(Direction.Rtl, direction);
            });
        }

        [Fact]
        public void Direction_is_decided_by_the_first_strong_character()
        {
            // Digits are weak, so a string of them has no direction at all --
            // which is why "neutral" exists as an answer separate from LTR.
            Run(() =>
            {
                Assert.Equal(Direction.Ltr, Pango.Global.FindBaseDir("abc"));
                Assert.Equal(Direction.Rtl, Pango.Global.FindBaseDir("אבג"));
                Assert.Equal(Direction.Neutral, Pango.Global.FindBaseDir("123"));
                Assert.Equal(Direction.Ltr, Pango.Global.FindBaseDir("123 abc"));

                Assert.Equal(Direction.Ltr, Pango.Global.UnicharDirection('a'));
                Assert.Equal(Direction.Rtl, Pango.Global.UnicharDirection('א'));
                Assert.Equal(Direction.Neutral, Pango.Global.UnicharDirection('1'));

                Assert.True(Pango.Global.GetMirrorChar('(', out var mirrored));
                Assert.Equal(')', mirrored);
                Assert.False(Pango.Global.GetMirrorChar('a', out _));
            });
        }

        // -------------------------------------------------------------- language

        [Fact]
        public void A_languages_scripts_are_the_ones_its_writing_system_uses()
        {
            // pango_language_get_scripts returns "const PangoScript *" plus a
            // count. The api.xml has nowhere to record the count, so codegen
            // declared the return value as the enum itself and handed back the
            // low 32 bits of the array's address as a script.
            //
            // IncludesScript is bound normally and is therefore the independent
            // check that the array now says what Pango says.
            Run(() =>
            {
                Assert.Equal(new[] { Script.Latin }, Language.FromString("en").GetScripts());
                Assert.Equal(new[] { Script.Cyrillic }, Language.FromString("ru").GetScripts());
                Assert.Equal(new[] { Script.Greek }, Language.FromString("el").GetScripts());
                Assert.Equal(new[] { Script.Hebrew }, Language.FromString("he").GetScripts());

                // Japanese takes three, which is what the count is there for.
                Assert.Equal(new[] { Script.Han, Script.Katakana, Script.Hiragana },
                             Language.FromString("ja").GetScripts());

                foreach (var code in new[] { "en", "ru", "el", "he", "ja" })
                {
                    var language = Language.FromString(code);
                    foreach (var script in new[] { Script.Latin, Script.Cyrillic, Script.Greek,
                                                   Script.Hebrew, Script.Han, Script.Hiragana })
                        Assert.Equal(language.GetScripts().Contains(script),
                                     language.IncludesScript(script));
                }
            });
        }

        [Fact]
        public void A_language_pango_has_no_table_entry_for_includes_every_script()
        {
            // The empty answer means "unknown", not "none", and IncludesScript
            // says yes to everything rather than no -- so a caller filtering
            // fonts by script gets everything through for an unknown language
            // and nothing looks wrong.
            Run(() =>
            {
                var unknown = Language.FromString("zz");

                Assert.Empty(unknown.GetScripts());
                Assert.True(unknown.IncludesScript(Script.Latin));
                Assert.True(unknown.IncludesScript(Script.Han));
            });
        }

        [Fact]
        public void A_language_is_interned_so_the_same_tag_is_the_same_pointer()
        {
            // PangoLanguage values are canonical: this is what lets the itemizer
            // compare them by pointer, and it is how a caller can tell a real
            // language back from a wrapper around something else.
            Run(() =>
            {
                Assert.Equal(Language.FromString("en-gb").Handle, Language.FromString("EN-GB").Handle);
                Assert.NotEqual(Language.FromString("en-gb").Handle, Language.FromString("fr").Handle);

                var fallback = Language.Default;
                Assert.Equal(fallback.Handle, Language.FromString(fallback.ToString()).Handle);
            });
        }

        // ------------------------------------------------------------- itemizing

        [Fact]
        public void An_items_analysis_reports_the_script_language_and_bidi_level()
        {
            // PangoItem.analysis is a struct embedded in the item, and the field
            // generator read the pointer-sized word at its offset as the address
            // of the struct. That word is PangoAnalysis.shape_engine, which is
            // always NULL, so every item reported a zeroed analysis: script
            // Common, no language, level 0. Shaping still worked, because the
            // real struct was passed straight back to Pango -- only a caller
            // *reading* it saw nothing.
            Run(() =>
            {
                const string text = "abc אבג def";
                var items = Itemize(ContextOf(), text);

                Assert.Equal(4, items.Length);

                var latin = items[0];
                Assert.Equal(0, latin.Offset);
                Assert.Equal(4, latin.Length);
                Assert.Equal(Script.Latin, (Script) latin.Analysis.Script);
                Assert.Equal(0, latin.Analysis.Level % 2);
                Assert.NotNull(latin.Analysis.Language);
                Assert.NotNull(latin.Analysis.Font);

                var hebrew = items[1];
                Assert.Equal(4, hebrew.Offset);
                Assert.Equal(6, hebrew.Length);      // three characters, two bytes each
                Assert.Equal(3, hebrew.NumChars);
                Assert.Equal(Script.Hebrew, (Script) hebrew.Analysis.Script);
                Assert.Equal(1, hebrew.Analysis.Level % 2);
                Assert.True(hebrew.Analysis.Language.IncludesScript(Script.Hebrew));

                // The items tile the text with no gap and no overlap.
                Assert.Equal(Encoding.UTF8.GetByteCount(text), items.Sum(i => i.Length));
                Assert.Equal(items.Select(i => i.Offset),
                             items.Select((_, n) => items.Take(n).Sum(i => i.Length)));
            });
        }

        // --------------------------------------------------------------- shaping

        [Fact]
        public void A_shaped_runs_glyph_widths_add_up_to_its_width()
        {
            // pango_glyph_string_get_width is bound independently of the glyphs
            // array, so this is arithmetic against a number Pango computed: a
            // wrong stride or a wrong offset into PangoGlyphInfo cannot survive
            // it. Both array fields were hidden with nothing in their place, so
            // the glyphs a shaping run produced were unreachable from managed
            // code at all.
            Run(() =>
            {
                ShapeOne("abcdef", out _, out var glyphs);

                Assert.Equal(glyphs.NumGlyphs, glyphs.Glyphs.Length);
                Assert.Equal(glyphs.Width, glyphs.Glyphs.Sum(g => g.Geometry.Width));
                Assert.All(glyphs.Glyphs, g => Assert.True(g.Geometry.Width > 0));
            });
        }

        [Fact]
        public void Log_clusters_are_the_byte_offsets_of_the_characters_they_came_from()
        {
            // One entry per glyph, holding the byte index into the shaped text --
            // so a two-byte character makes the sequence skip a number, which is
            // what separates a byte offset from a character index.
            Run(() =>
            {
                ShapeOne("aebc", out _, out var ascii);
                Assert.Equal(new[] { 0, 1, 2, 3 }, ascii.LogClusters);

                ShapeOne("a\u00e9bc", out _, out var accented);
                Assert.Equal(4, Encoding.UTF8.GetByteCount("a\u00e9bc") - 1);
                Assert.Equal(new[] { 0, 1, 3, 4 }, accented.LogClusters);
            });
        }

        [Fact]
        public void There_is_one_logical_width_per_character_and_they_sum_to_the_run()
        {
            // pango_glyph_string_get_logical_widths writes one int per character
            // of the text. Codegen bound it "out int" and returned that one int,
            // so Pango wrote the rest past the end of a four-byte stack slot.
            Run(() =>
            {
                const string text = "a\u00e9bc";
                ShapeOne(text, out _, out var glyphs);

                var widths = glyphs.GetLogicalWidths(text, 0);

                Assert.Equal(4, widths.Length);           // characters, not bytes
                Assert.Equal(glyphs.Width, widths.Sum());
                Assert.All(widths, w => Assert.True(w > 0));
            });
        }

        [Fact]
        public void A_glyph_item_reports_a_logical_width_for_every_character_of_its_item()
        {
            Run(() =>
            {
                const string text = "abcdef";
                ShapeOne(text, out var item, out var glyphs);
                var run = new GlyphItem { Item = item, Glyphs = glyphs };

                var widths = run.GetLogicalWidths(text);

                Assert.Equal(item.NumChars, widths.Length);
                Assert.Equal(glyphs.Width, widths.Sum());
                Assert.Equal(glyphs.GetLogicalWidths(text, 0), widths);
            });
        }

        [Fact]
        public void Letter_spacing_is_inserted_between_clusters_not_around_them()
        {
            // Named as though it padded every letter; it puts the space in the
            // gaps, so n clusters grow the run by n-1 spacings and a one-letter
            // run does not grow at all. Text set with letter spacing therefore
            // measures narrower than "characters times spacing" predicts.
            //
            // The log-attr argument is what decides where a cluster starts, and
            // codegen passed a single PangoLogAttr for the whole array, so Pango
            // read the rest of it off the end of one four-byte block.
            Run(() =>
            {
                int spacing = 4 * (int) Scale.PangoScale;

                foreach (var text in new[] { "ab", "abcd", "abcdef" })
                {
                    ShapeOne(text, out var item, out var glyphs);
                    var run = new GlyphItem { Item = item, Glyphs = glyphs };
                    int before = glyphs.Width;

                    run.LetterSpace(text, Pango.Global.GetLogAttrs(text, 0, null), spacing);

                    Assert.Equal(before + (text.Length - 1) * spacing, glyphs.Width);
                }
            });
        }

        [Fact]
        public void Index_to_x_agrees_with_the_full_form_that_takes_log_attrs()
        {
            // pango_glyph_string_index_to_x_full is the same question with the
            // cluster boundaries supplied, so on text where every character is a
            // cursor position it has to give the plain form's answer. Its attrs
            // argument came out as a single struct, which left Pango reading
            // past the end of it for any index beyond the first cluster.
            Run(() =>
            {
                const string text = "abcdef";
                ShapeOne(text, out var item, out var glyphs);
                var attrs = Pango.Global.GetLogAttrs(text, 0, null);

                var positions = new List<int>();
                for (int index = 0; index <= text.Length; index++)
                {
                    int plain = glyphs.IndexToX(text, item.Analysis, index, false);
                    Assert.Equal(plain, glyphs.IndexToXFull(text, item.Analysis, attrs, index, false));
                    positions.Add(plain);
                }

                Assert.Equal(0, positions.First());
                Assert.Equal(glyphs.Width, positions.Last());
                Assert.Equal(positions.OrderBy(x => x), positions);
            });
        }

        // --------------------------------------------------------- layout iterator

        [Fact]
        public void The_iterator_walks_the_byte_index_of_every_character()
        {
            // Index is a byte offset, so a two-byte character makes it jump --
            // the same distinction the log clusters carry, from the other side.
            Run(() =>
            {
                var layout = LayoutOf("h\u00e9llo");
                var iterator = layout.Iter;

                var indexes = new List<int>();
                do
                    indexes.Add(iterator.Index);
                while (iterator.NextChar());

                Assert.Equal(new[] { 0, 1, 3, 4, 5 }, indexes);
                Assert.Equal(5, layout.CharacterCount);
            });
        }

        [Fact]
        public void Cluster_extents_tile_the_line_they_belong_to()
        {
            // Every cluster's logical width added up is the layout's width, and
            // each starts where the previous one ended. Latin text has one
            // cluster per character, so the count is the character count.
            Run(() =>
            {
                var layout = LayoutOf("abcdef");
                var iterator = layout.Iter;

                int clusters = 0, total = 0, expectedX = 0;
                do
                {
                    iterator.GetClusterExtents(out _, out var logical);
                    Assert.Equal(expectedX, logical.X);
                    Assert.Equal(clusters, iterator.Index);
                    expectedX += logical.Width;
                    total += logical.Width;
                    clusters++;
                } while (iterator.NextCluster());

                layout.GetExtents(out _, out var extents);

                Assert.Equal(6, clusters);
                Assert.Equal(extents.Width, total);
            });
        }

        [Fact]
        public void Runs_partition_the_text_and_their_widths_tile_the_line()
        {
            // Mixed script means more than one run, and the iterator hands back
            // a *null* item once it has passed the last one -- which is the trap
            // here, because the wrapper turns that into a zeroed GlyphItem
            // rather than null, so the loop reads Item and gets nothing.
            Run(() =>
            {
                const string text = "abc אבג def";
                var layout = LayoutOf(text);
                var iterator = layout.Iter;

                var bytes = Encoding.UTF8.GetBytes(text);
                var offsets = new List<int>();
                var lengths = new List<int>();
                int width = 0;
                int trailingNullRuns = 0;

                do
                {
                    var run = iterator.Run;
                    if (run.Item == null)
                    {
                        trailingNullRuns++;
                        continue;
                    }

                    iterator.GetRunExtents(out _, out var logical);
                    Assert.Equal(width, logical.X);
                    width += logical.Width;
                    offsets.Add(run.Item.Offset);
                    lengths.Add(run.Item.Length);

                    // The two logical-width calls take their text differently,
                    // and neither says so: a glyph string is given only the text
                    // it shaped, while a glyph item is given the whole paragraph
                    // and finds its slice through Item.Offset. Passing the
                    // paragraph to the glyph string yields one width per
                    // character of the *paragraph*, silently.
                    var slice = Encoding.UTF8.GetString(bytes, run.Item.Offset, run.Item.Length);
                    var byString = run.Glyphs.GetLogicalWidths(slice, run.Item.Analysis.Level);

                    Assert.Equal(run.Item.NumChars, byString.Length);
                    Assert.Equal(byString, run.GetLogicalWidths(text));
                    Assert.Equal(run.Glyphs.Width, byString.Sum());
                } while (iterator.NextRun());

                layout.GetExtents(out _, out var extents);

                Assert.Equal(1, trailingNullRuns);
                Assert.Equal(4, offsets.Count);
                Assert.Equal(0, offsets.First());
                Assert.Equal(Encoding.UTF8.GetByteCount(text), lengths.Sum());
                Assert.Equal(offsets, offsets.Select((_, n) => lengths.Take(n).Sum()));
                Assert.Equal(extents.Width, width);
            });
        }

        [Fact]
        public void Line_y_ranges_tile_and_the_baselines_march_down_the_layout()
        {
            Run(() =>
            {
                var layout = LayoutOf("one\ntwo\nthree");
                var iterator = layout.Iter;

                int lines = 0, previousBaseline = int.MinValue, previousBottom = 0;
                do
                {
                    iterator.GetLineYrange(out var top, out var bottom);

                    Assert.Equal(previousBottom, top);
                    Assert.True(bottom > top);
                    Assert.InRange(iterator.Baseline, top, bottom);
                    Assert.True(iterator.Baseline > previousBaseline);
                    Assert.Equal(iterator.Baseline, iterator.RunBaseline);

                    previousBaseline = iterator.Baseline;
                    previousBottom = bottom;
                    lines++;
                } while (iterator.NextLine());

                layout.GetExtents(out _, out var extents);

                Assert.Equal(3, lines);
                Assert.Equal(extents.Height, previousBottom);
                Assert.True(iterator.AtLastLine());
            });
        }

        [Fact]
        public void A_characters_extents_sit_inside_its_lines()
        {
            Run(() =>
            {
                var layout = LayoutOf("abcdef");
                var iterator = layout.Iter;

                iterator.GetLineExtents(out _, out var line);
                var first = iterator.CharExtents;

                Assert.Equal(line.X, first.X);
                Assert.Equal(line.Y, first.Y);
                Assert.Equal(line.Height, first.Height);
                Assert.True(first.Width > 0 && first.Width < line.Width);

                Assert.True(iterator.NextChar());
                var second = iterator.CharExtents;
                Assert.Equal(first.X + first.Width, second.X);
            });
        }

        // ------------------------------------------------------- layout geometry

        [Fact]
        public void Xy_to_index_inverts_index_to_pos_for_every_character()
        {
            // The two directions a text cursor moves in: index_to_pos asks
            // where a byte offset is drawn, xy_to_index asks which byte offset
            // a click lands on. Sampling inside a character's own rectangle has
            // to name that character back, and which half decides the trailing
            // flag -- which is what puts the caret on the far side of a letter
            // when the click was nearer its right edge.
            Run(() =>
            {
                const string text = "héllo world";
                var layout = LayoutOf(text);

                var starts = new List<int>();
                var iterator = layout.Iter;
                do
                    starts.Add(iterator.Index);
                while (iterator.NextChar());

                Assert.Equal(11, starts.Count);
                Assert.Equal(new[] { 0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, starts);

                foreach (var index in starts)
                {
                    var rect = layout.IndexToPos(index);
                    Assert.True(rect.Width > 0);

                    Assert.True(layout.XyToIndex(rect.X + rect.Width / 4,
                                                 rect.Y + rect.Height / 2,
                                                 out var hit, out var trailing));
                    Assert.Equal(index, hit);
                    Assert.Equal(0, trailing);

                    Assert.True(layout.XyToIndex(rect.X + 3 * rect.Width / 4,
                                                 rect.Y + rect.Height / 2,
                                                 out hit, out trailing));
                    Assert.Equal(index, hit);
                    Assert.Equal(1, trailing);
                }

                // A point to the left of the first character is still the
                // first character: xy_to_index clamps within the line and
                // reports false only for a y outside the layout.
                Assert.False(layout.XyToIndex(0, -1000, out _, out _));
            });
        }

        [Fact]
        public void Moving_the_cursor_visually_walks_every_position_and_stops_at_the_ends()
        {
            // pango_layout_move_cursor_visually is how an arrow key is
            // implemented, and it reports running off the layout with two
            // different sentinels: -1 at the beginning and G_MAXINT at the
            // end. Neither is a byte offset, and a loop written as
            // "while (index >= 0)" therefore does not terminate going
            // forwards -- it feeds G_MAXINT back in for ever. That is what
            // this test was written as first, and it hung.
            Run(() =>
            {
                var layout = LayoutOf("héllo");
                var visited = new List<int>();

                int index = 0, trailing = 0;
                for (int step = 0; step < 32 && index >= 0 && index != int.MaxValue; step++)
                {
                    visited.Add(index + trailing);
                    layout.MoveCursorVisually(true, index, trailing, 1, out index, out trailing);
                }

                // Five characters in six bytes: every cursor position in
                // logical order, because the text is all left-to-right.
                Assert.Equal(new[] { 0, 1, 3, 4, 5, 6 }, visited);
                Assert.Equal(int.MaxValue, index);

                layout.MoveCursorVisually(true, 0, 0, -1, out var before, out _);
                Assert.Equal(-1, before);
            });
        }

        [Fact]
        public void Doubling_the_font_size_doubles_the_metrics_and_the_line_it_produces()
        {
            // The oracle is proportion, which is a fact about type rather than
            // about the machine: the same family at twice the size is twice as
            // tall. What may *not* be asserted is that the layout's line box
            // equals ascent + descent -- measured here it is 21504 against
            // 19776, because the line box is rounded up to whole pixels while
            // the context's metrics are not, and how much hinting rounds is a
            // property of the backend. Hence a ratio with a tolerance: a whole
            // pixel out of twenty-one at 12pt is under 5%, so 10% is generous
            // and still nowhere near the 1.0 a broken binding would give.
            Run(() =>
            {
                var context = ContextOf();
                var english = Language.FromString("en");

                var small = context.GetMetrics(FontDescription.FromString("Sans 12"), english);
                var large = context.GetMetrics(FontDescription.FromString("Sans 24"), english);

                Assert.True(small.Ascent > 0);
                Assert.True(small.Descent > 0);
                Assert.True(small.ApproximateDigitWidth > 0);

                Assert.InRange((double) large.Ascent / small.Ascent, 1.8, 2.2);
                Assert.InRange((double) large.Descent / small.Descent, 1.8, 2.2);

                var smallLayout = LayoutOf("hello", "Sans 12");
                var largeLayout = LayoutOf("hello", "Sans 24");
                smallLayout.GetExtents(out _, out var smallExtents);
                largeLayout.GetExtents(out _, out var largeExtents);

                Assert.InRange((double) largeExtents.Height / smallExtents.Height, 1.8, 2.2);
                Assert.InRange((double) largeExtents.Width / smallExtents.Width, 1.8, 2.2);
                Assert.True(largeLayout.Baseline > smallLayout.Baseline);

                // The strong cursor at the start of the text is a zero-width
                // rectangle at the origin spanning the whole line.
                smallLayout.GetCursorPos(0, out var strong, out _);
                Assert.Equal(0, strong.Width);
                Assert.Equal(0, strong.X);
                Assert.Equal(smallExtents.Height, strong.Height);
                Assert.InRange(smallLayout.Baseline, 0, smallExtents.Height);
            });
        }

        // ------------------------------------------------------------ script iter

        [Fact]
        public void The_script_iterator_splits_text_at_the_script_boundaries()
        {
            // Ranges are in characters, not bytes, and the Hebrew run is two
            // bytes per character -- so a start of 5 for the second Latin run is
            // the conversion having happened.
            Run(() =>
            {
                var ranges = new List<(int Start, int Length, Script Script)>();
                var iterator = new ScriptIter("abcאבdef");
                do
                {
                    iterator.GetRange(out var start, out var length, out var script);
                    ranges.Add((start, length, script));
                } while (iterator.Next());

                Assert.Equal(new[]
                {
                    (0, 3, Script.Latin),
                    (3, 2, Script.Hebrew),
                    (5, 3, Script.Latin),
                }, ranges);
            });
        }

        // -------------------------------------------------------------- attr list

        /// <summary>The byte range each kind of attribute in a list covers.</summary>
        private static Dictionary<AttrType, (uint Start, uint End)> RangesOf(AttrList list)
        {
            var result = new Dictionary<AttrType, (uint, uint)>();
            var iterator = list.Iterator;
            do
                foreach (var attribute in iterator.Attrs)
                    result[attribute.Type] = (attribute.StartIndex, attribute.EndIndex);
            while (iterator.Next());
            return result;
        }

        private static AttrList TwoAttributes()
        {
            var list = new AttrList();
            list.Insert(new AttrWeight(Weight.Bold) { StartIndex = 0, EndIndex = 4 });
            list.Insert(new AttrStyle(Style.Italic) { StartIndex = 6, EndIndex = 10 });
            return list;
        }

        [Fact]
        public void Splicing_shifts_what_follows_and_inserts_the_other_list_at_the_position()
        {
            // Splice is what an editor calls after pasting: the other list's
            // attributes arrive offset by pos, and anything already at or after
            // pos is pushed along by len.
            Run(() =>
            {
                var list = TwoAttributes();
                var inserted = new AttrList();
                inserted.Insert(new AttrUnderline(Underline.Single) { StartIndex = 0, EndIndex = 2 });

                list.Splice(inserted, 5, 3);

                var ranges = RangesOf(list);
                Assert.Equal((0u, 4u), ranges[AttrType.Weight]);        // before pos: untouched
                Assert.Equal((5u, 7u), ranges[AttrType.Underline]);     // shifted by pos
                Assert.Equal((9u, 13u), ranges[AttrType.Style]);        // pushed along by len
            });
        }

        [Fact]
        public void Update_stretches_the_range_the_edit_falls_inside_and_moves_the_rest()
        {
            // Two characters at index 2 replaced by five: the bold run spanning
            // the edit grows by three, and the italic run after it slides.
            Run(() =>
            {
                var list = TwoAttributes();

                list.Update(2, 2, 5);

                var ranges = RangesOf(list);
                Assert.Equal((0u, 7u), ranges[AttrType.Weight]);
                Assert.Equal((9u, 13u), ranges[AttrType.Style]);
            });
        }

        [Fact]
        public void Filter_takes_the_attributes_it_matched_out_of_the_list()
        {
            // Both halves matter: what comes back and what is left behind. A
            // filter that matches nothing returns null rather than an empty list.
            Run(() =>
            {
                var list = TwoAttributes();

                var removed = list.Filter(a => a.Type == AttrType.Weight);

                Assert.Equal(new[] { AttrType.Weight }, RangesOf(removed).Keys);
                Assert.Equal(new[] { AttrType.Style }, RangesOf(list).Keys);

                Assert.Null(list.Filter(a => a.Type == AttrType.Weight));
            });
        }

        [Fact]
        public void The_attributes_a_filter_callback_was_handed_outlive_the_call()
        {
            // Pango.Attribute destroyed its native pointer from the finalizer
            // whatever it was handed, and a filter callback is handed attributes
            // that a PangoAttrList still owns -- one about to move into the list
            // Filter returns, one staying where it is. Both were freed twice.
            //
            // The second free is what does the damage, and it lands whenever the
            // GC and the main loop next run, so a plain assertion after the call
            // passes on luck. Collecting and then churning 256 attributes of the
            // same shape makes the reuse certain: without the fix the ranges
            // below read back as whatever the churn wrote there.
            Run(() =>
            {
                var list = TwoAttributes();
                var removed = list.Filter(a => a.Type == AttrType.Weight);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var churn = new AttrList();
                for (uint i = 0; i < 256; i++)
                    churn.Insert(new AttrWeight(Weight.Light) { StartIndex = i, EndIndex = i + 1 });

                Assert.Equal((0u, 4u), RangesOf(removed)[AttrType.Weight]);
                Assert.Equal((6u, 10u), RangesOf(list)[AttrType.Style]);

                // pango_attr_iterator_get is the same shape -- the attribute it
                // names belongs to the list -- and NULL is how it says there is
                // none, which used to arrive as a wrapper around address zero.
                var iterator = TwoAttributes().Iterator;
                Assert.Equal(AttrType.Weight, iterator.Get(AttrType.Weight).Type);
                Assert.Null(iterator.Get(AttrType.Underline));

                GC.KeepAlive(churn);
            });
        }

        [Fact]
        public void Two_attributes_are_equal_when_their_values_are_whatever_range_they_cover()
        {
            // pango_attribute_equal compares the *value* and deliberately
            // ignores start_index and end_index -- it is what an attr list uses
            // to decide two runs can be merged. So "bold here" and "bold there"
            // are equal, and code that de-duplicates attributes with it loses
            // every range but the first. Nothing in the name says so.
            Run(() =>
            {
                var here = new AttrWeight(Weight.Bold) { StartIndex = 0, EndIndex = 4 };
                var there = new AttrWeight(Weight.Bold) { StartIndex = 40, EndIndex = 44 };
                var lighter = new AttrWeight(Weight.Light) { StartIndex = 0, EndIndex = 4 };
                var otherKind = new AttrStyle(Style.Italic) { StartIndex = 0, EndIndex = 4 };

                Assert.True(here.Equal(there));
                Assert.False(here.Equal(lighter));
                Assert.False(here.Equal(otherKind));

                // A copy is a separate allocation carrying the same value and
                // the same range -- so equality cannot tell it from the
                // original, and the handle is what says it is not the same one.
                var copy = here.Copy();
                Assert.NotEqual(here.Handle, copy.Handle);
                Assert.True(here.Equal(copy));
                Assert.Equal(here.StartIndex, copy.StartIndex);
                Assert.Equal(here.EndIndex, copy.EndIndex);

                // pango_attr_font_features_new is the only PangoAttribute
                // constructor codegen emits, and its result is transfer-full
                // where the other four uses of the same manual symbol are all
                // borrowed -- so it is bound by hand rather than sharing the
                // borrowing default that keeps a filter callback from freeing
                // the list's attributes.
                var ligatures = AttrFontFeatures.New("liga 1");
                Assert.Equal(AttrType.FontFeatures, ligatures.Type);
                Assert.True(ligatures.Equal(AttrFontFeatures.New("liga 1")));
                Assert.False(ligatures.Equal(AttrFontFeatures.New("liga 0")));
            });
        }

        [Fact]
        public void An_attribute_list_round_trips_through_its_string_form()
        {
            Run(() =>
            {
                var list = TwoAttributes();

                var parsed = AttrList.FromString(list.ToString());

                Assert.True(list.Equal(parsed));
                Assert.Equal(RangesOf(list), RangesOf(parsed));
                Assert.False(list.Equal(new AttrList()));
            });
        }

        // --------------------------------------------------------------- tab array

        [Fact]
        public void A_decimal_tab_puts_the_decimal_point_in_the_same_place_whatever_precedes_it()
        {
            // The point of a decimal tab, asserted where it shows: two numbers
            // of different widths after the same tab stop line their dots up.
            Run(() =>
            {
                int stop = 100 * (int) Scale.PangoScale;
                var tabs = new TabArray(1, false);
                tabs.SetTab(0, TabAlign.Decimal, stop);
                tabs.SetDecimalPoint(0, '.');

                var positions = new List<int>();
                foreach (var text in new[] { "\t1.5", "\t22.25", "\t333.125" })
                {
                    var layout = LayoutOf(text);
                    layout.Tabs = tabs;
                    positions.Add(layout.IndexToPos(text.IndexOf('.')).X);
                }

                Assert.Single(positions.Distinct());

                // The dot is centred on the stop, so its left edge is half its
                // own advance to the left of it -- well under one em.
                Assert.InRange(positions[0], stop - 12 * (int) Scale.PangoScale, stop);
            });
        }

        [Fact]
        public void Tab_stops_report_the_alignment_and_decimal_point_they_were_given()
        {
            // A tab whose decimal point was never set reports U+0000, not '.':
            // reading it as a character and printing it produces a NUL.
            Run(() =>
            {
                var tabs = new TabArray(3, false);
                tabs.SetTab(0, TabAlign.Decimal, 100);
                tabs.SetDecimalPoint(0, ',');
                tabs.SetTab(1, TabAlign.Right, 200);
                tabs.SetTab(2, TabAlign.Center, 300);

                tabs.GetTabs(out var alignments, out var locations);

                Assert.Equal(new[] { TabAlign.Decimal, TabAlign.Right, TabAlign.Center }, alignments);
                Assert.Equal(new[] { 100, 200, 300 }, locations);
                Assert.Equal(',', tabs.GetDecimalPoint(0));
                Assert.Equal('\0', tabs.GetDecimalPoint(1));

                tabs.GetTab(2, out var alignment, out var location);
                Assert.Equal(TabAlign.Center, alignment);
                Assert.Equal(300, location);
            });
        }

        // ----------------------------------------------------------- fonts, coverage

        [Fact]
        public void A_fonts_coverage_says_which_characters_it_can_draw()
        {
            // A noncharacter is in no font on any platform, and a font resolved
            // for Latin text has to have 'a' -- otherwise nothing above would
            // have measured anything.
            //
            // Reading is all that may be asserted of a *font's* coverage. Set is
            // a vfunc, and the fontconfig backend's PangoFcCoverage overrides it
            // with an empty body: the round trip below holds under gvsbuild's
            // win32 backend and silently does nothing on Debian, so which of the
            // two happens is a fact about the host. The round trip belongs on a
            // coverage this test made, which is pango's own class either way.
            Run(() =>
            {
                var font = ContextOf().LoadFont(FontDescription.FromString("Sans 12"));
                var coverage = font.GetCoverage(Language.FromString("en"));

                Assert.Equal(CoverageLevel.Exact, coverage.Get('a'));
                Assert.Equal(CoverageLevel.None, coverage.Get(0x10FFFE));
            });
        }

        [Fact]
        public void Setting_a_coverage_is_visible_to_get_and_a_copy_is_independent()
        {
            // Pango 1.44 reimplemented coverage over hb_set and folded every
            // level other than NONE into EXACT, so asking for APPROXIMATE and
            // reading back EXACT is the answer rather than a marshalling fault.
            Run(() =>
            {
                var coverage = new Coverage();

                Assert.Equal(CoverageLevel.None, coverage.Get('a'));

                coverage.Set('a', CoverageLevel.Exact);
                coverage.Set('b', CoverageLevel.Approximate);

                Assert.Equal(CoverageLevel.Exact, coverage.Get('a'));
                Assert.Equal(CoverageLevel.Exact, coverage.Get('b'));
                Assert.Equal(CoverageLevel.None, coverage.Get('c'));

                // A copy has to be a copy: writing through one must not be
                // visible through the other, which is the one thing a Copy that
                // handed back the same object would fail.
                var copy = coverage.Copy();
                copy.Set('c', CoverageLevel.Exact);
                coverage.Set('a', CoverageLevel.None);

                Assert.Equal(CoverageLevel.Exact, copy.Get('a'));
                Assert.Equal(CoverageLevel.None, coverage.Get('c'));
            });
        }

        [Fact]
        public void The_deprecated_coverage_serialisation_is_inert_rather_than_broken()
        {
            // Pango 1.44 reimplemented coverage on hb_set and turned both halves
            // into no-ops: to_bytes writes NULL and zero, from_bytes returns
            // NULL. That is worth pinning because the unguarded binding turned
            // "nothing to serialise" into an ArgumentNullException out of
            // Marshal.Copy, and because from_bytes's input array had been bound
            // as an out parameter -- the caller could not supply bytes at all.
            Run(() =>
            {
                var font = ContextOf().LoadFont(FontDescription.FromString("Sans 12"));
                var coverage = font.GetCoverage(Language.FromString("en"));

                coverage.ToBytes(out var bytes);

                Assert.Empty(bytes);
                Assert.Null(Coverage.FromBytes(bytes));
                Assert.Null(Coverage.FromBytes(new byte[] { 1, 2, 3 }));
                Assert.Throws<ArgumentNullException>(() => Coverage.FromBytes(null));
            });
        }

        [Fact]
        public void A_font_face_belongs_to_the_family_that_listed_it()
        {
            // pango_font_face_list_sizes takes "int **sizes" -- a pointer the
            // callee fills in. The api.xml records it as an out int, a four-byte
            // slot, and Pango wrote an eight-byte address through it. Scalable
            // faces answer with NULL and zero, so the overrun wrote four zero
            // bytes of stack and nothing looked wrong.
            Run(() =>
            {
                var families = ContextOf().FontMap.Families.Where(f => f.Faces.Length > 0).ToArray();
                Assert.NotEmpty(families);

                // Every family, not the first one: the order pango lists them in
                // is the order the platform enumerated its fonts, so a test that
                // indexes into it is asserting something about the machine.
                foreach (var family in families)
                    foreach (var face in family.Faces)
                    {
                        // pango_font_face_get_family is the inverse of
                        // list_faces, and describe() names the same family, so
                        // both hold for every face there is.
                        Assert.Equal(family.Name, face.Family.Name);
                        Assert.Equal(family.Name, face.Describe().Family);

                        // GetFace is a linear search that stops at the first
                        // match, and face names are *not* unique within a
                        // family -- this machine has one shipping "Thin" twice.
                        // So the contract is the name, not the identity: the
                        // handles are only equal for the first face of a name,
                        // and a test that asserted identity for all of them was
                        // one font install away from failing.
                        var found = family.GetFace(face.FaceName);
                        Assert.Equal(face.FaceName, found.FaceName);
                        Assert.Equal(family.Faces.First(f => f.FaceName == face.FaceName).Handle,
                                     found.Handle);

                        // Empty for a scalable face, ascending Pango units for a
                        // bitmap one; which of the two turns up is a fact about
                        // the fonts installed, so only the ordering is asserted.
                        var sizes = face.ListSizes();
                        Assert.All(sizes, size => Assert.True(size > 0));
                        Assert.Equal(sizes.OrderBy(s => s), sizes);
                    }
            });
        }

        // ---------------------------------------------------------------- matrix

        [Fact]
        public void A_ninety_degree_rotation_sends_the_x_axis_to_the_negative_y_axis()
        {
            // Pango's y axis points down, so a positive rotation carries (1,0)
            // to (0,-1) -- counter-clockwise on screen, the opposite sign from
            // the textbook matrix. The tolerance is there because cos(pi/2) is
            // 6e-17 rather than 0 in double precision on every machine.
            Run(() =>
            {
                var matrix = Pango.Matrix.Identity;
                matrix.Rotate(90);

                double x = 1, y = 0;
                matrix.TransformPoint(ref x, ref y);

                Assert.Equal(0.0, x, 12);
                Assert.Equal(-1.0, y, 12);

                double dx = 2, dy = 0;
                matrix.TransformDistance(ref dx, ref dy);
                Assert.Equal(0.0, dx, 12);
                Assert.Equal(-2.0, dy, 12);
            });
        }

        [Fact]
        public void The_identity_matrix_survives_being_rotated_where_it_stands()
        {
            // Every PangoMatrix operation mutates in place, and Identity used to
            // be a static field: this line rotated the identity for every other
            // caller in the process, permanently, with nothing to show for it.
            Run(() =>
            {
                Pango.Matrix.Identity.Rotate(90);
                Pango.Matrix.Identity.Scale(3, 3);

                var identity = Pango.Matrix.Identity;

                Assert.Equal(1.0, identity.Xx);
                Assert.Equal(0.0, identity.Xy);
                Assert.Equal(0.0, identity.Yx);
                Assert.Equal(1.0, identity.Yy);
            });
        }

        [Fact]
        public void Font_scale_factors_are_the_scale_the_matrix_applies()
        {
            Run(() =>
            {
                var matrix = Pango.Matrix.Identity;
                matrix.Scale(2, 3);

                matrix.GetFontScaleFactors(out var xscale, out var yscale);

                Assert.Equal(2.0, xscale, 12);
                Assert.Equal(3.0, yscale, 12);

                // The single-factor form is the y one, because that is what the
                // height of a font is measured along.
                Assert.Equal(3.0, matrix.FontScaleFactor, 12);
                Assert.Equal(0.0, matrix.SlantRatio, 12);
            });
        }

        // --------------------------------------------------------- serialisation

        [Fact]
        public void A_serialised_layout_reads_back_as_the_layout_it_came_from()
        {
            Run(() =>
            {
                var layout = LayoutOf("hello\nworld");
                layout.Alignment = Alignment.Center;
                layout.Spacing = 512;

                var bytes = layout.Serialize(LayoutSerializeFlags.Context);
                var restored = Layout.Deserialize(layout.Context, bytes, LayoutDeserializeFlags.Context);

                Assert.Equal("hello\nworld", restored.Text);
                Assert.Equal(Alignment.Center, restored.Alignment);
                Assert.Equal(512, restored.Spacing);
                Assert.Equal(2, restored.LineCount);

                layout.GetSize(out var width, out var height);
                restored.GetSize(out var restoredWidth, out var restoredHeight);
                Assert.Equal(width, restoredWidth);
                Assert.Equal(height, restoredHeight);
            });
        }
    }
}

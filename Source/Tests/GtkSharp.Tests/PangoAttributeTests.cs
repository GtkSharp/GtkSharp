using System;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>Pango.AttrList</c> and <c>Pango.AttrIterator</c> — how a run of text
    /// carries formatting.
    /// </summary>
    /// <remarks>
    /// Almost all of this is hand-written: <c>AttrList.cs</c>, <c>Attribute.cs</c>
    /// and one file per attribute type, plus an <c>AttrIterator</c> whose two
    /// interesting members both return arrays out of a <c>GSList</c>. That is the
    /// shape this binding has got wrong repeatedly — an element type the list does
    /// not know, ownership on the way out — and <c>AttrIterator</c> had no mention
    /// in the suite at all.
    ///
    /// Attribute indices are in **bytes**, not characters, which the last test
    /// here pins because it is the difference between formatting the right word
    /// and formatting half a letter.
    /// </remarks>
    public class PangoAttributeTests : GtkTestBase
    {
        public PangoAttributeTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>An attribute list over "0123456789": bold across [0,5) and
        /// red across [3,10).</summary>
        static Pango.AttrList Overlapping()
        {
            var list = new Pango.AttrList();

            var bold = new Pango.AttrWeight(Pango.Weight.Bold) { StartIndex = 0, EndIndex = 5 };
            var red = new Pango.AttrForeground(ushort.MaxValue, 0, 0) { StartIndex = 3, EndIndex = 10 };

            list.Insert(bold);
            list.Insert(red);

            return list;
        }

        // -------------------------------------------------------- the attributes

        [Fact]
        public void An_attribute_keeps_the_range_and_the_value_it_was_given()
        {
            Run(() =>
            {
                var bold = new Pango.AttrWeight(Pango.Weight.Bold) { StartIndex = 2, EndIndex = 7 };

                Assert.Equal(2u, bold.StartIndex);
                Assert.Equal(7u, bold.EndIndex);
                Assert.Equal(Pango.AttrType.Weight, bold.Type);
                Assert.Equal(Pango.Weight.Bold, bold.Weight);
            });
        }

        [Fact]
        public void A_colour_attribute_keeps_all_three_channels()
        {
            Run(() =>
            {
                var green = new Pango.AttrForeground(0, ushort.MaxValue, 0);

                var colour = green.Color;

                Assert.Equal(0, colour.Red);
                Assert.Equal(ushort.MaxValue, colour.Green);
                Assert.Equal(0, colour.Blue);
                Assert.Equal(Pango.AttrType.Foreground, green.Type);
            });
        }

        // ----------------------------------------------------------- the list

        [Fact]
        public void A_list_hands_back_the_attributes_put_into_it()
        {
            Run(() =>
            {
                using var list = Overlapping();

                var types = list.Attributes
                                .Cast<Pango.Attribute>()
                                .Select(a => a.Type)
                                .OrderBy(t => t.ToString())
                                .ToArray();

                Assert.Equal(new[] { Pango.AttrType.Foreground, Pango.AttrType.Weight }, types);
            });
        }

        [Fact]
        public void A_list_round_trips_through_its_string_form()
        {
            // Pango's own serialisation is the oracle: it parses back what it
            // printed, so a wrapper that lost a range or a value shows up as a
            // difference here without the test having to know the format.
            Run(() =>
            {
                using var list = Overlapping();

                var text = list.ToString();
                using var reparsed = Pango.AttrList.FromString(text);

                Assert.NotNull(reparsed);
                Assert.True(list.Equal(reparsed), $"'{text}' should parse back to an equal list");
            });
        }

        [Fact]
        public void A_copied_list_is_equal_to_the_original_and_independent_of_it()
        {
            Run(() =>
            {
                using var list = Overlapping();
                using var copy = list.Copy();

                Assert.True(list.Equal(copy));

                copy.Insert(new Pango.AttrStyle(Pango.Style.Italic) { StartIndex = 0, EndIndex = 2 });

                Assert.False(list.Equal(copy), "the copy should have changed on its own");
            });
        }

        [Fact]
        public void Filtering_removes_exactly_the_attributes_the_predicate_accepts()
        {
            // The filter callback is a managed delegate called from Pango for each
            // attribute, and what it returns decides ownership -- true means Pango
            // hands the attribute over.
            Run(() =>
            {
                using var list = Overlapping();

                var seen = new System.Collections.Generic.List<Pango.AttrType>();

                using var removed = list.Filter(attr =>
                {
                    seen.Add(attr.Type);
                    return attr.Type == Pango.AttrType.Weight;
                });

                Assert.Equal(2, seen.Count);          // asked about both

                var left = list.Attributes.Cast<Pango.Attribute>().Select(a => a.Type).ToArray();

                Assert.Equal(new[] { Pango.AttrType.Foreground }, left);
            });
        }

        // ------------------------------------------------------- the iterator

        [Fact]
        public void An_iterator_walks_the_runs_the_overlaps_divide_the_text_into()
        {
            // Two attributes over [0,5) and [3,10) make three runs: 0-3 bold,
            // 3-5 both, 5-10 red. The boundaries are arithmetic over the ranges
            // the test itself chose.
            Run(() =>
            {
                using var list = Overlapping();
                var iterator = list.Iterator;

                iterator.Range(out int start, out int end);
                Assert.Equal(0, start);
                Assert.Equal(3, end);

                Assert.True(iterator.Next());
                iterator.Range(out start, out end);
                Assert.Equal(3, start);
                Assert.Equal(5, end);

                Assert.True(iterator.Next());
                iterator.Range(out start, out end);
                Assert.Equal(5, start);
                Assert.Equal(10, end);

                // ...and then one more, covering the rest of whatever text this
                // list is applied to. I expected the iterator to stop after the
                // last attribute; it does not, and a caller who assumes it does
                // will read the final run's formatting off the end of the array
                // it was collecting into. G_MAXINT is how Pango spells "to the
                // end", since the list has no idea how long the text is.
                Assert.True(iterator.Next(), "there is a trailing run with no attributes");

                iterator.Range(out start, out end);
                Assert.Equal(10, start);
                Assert.Equal(int.MaxValue, end);
                Assert.Empty(iterator.Attrs);

                Assert.False(iterator.Next(), "and nothing after that one");
            });
        }

        [Fact]
        public void Each_run_reports_the_attributes_that_apply_across_it()
        {
            // AttrIterator.Attrs is hand-written over a GSList whose element type
            // Pango does not declare. Without the element type the list marshals
            // each item as a GObject -- which a PangoAttribute is not -- and hands
            // back null.
            Run(() =>
            {
                using var list = Overlapping();
                var iterator = list.Iterator;

                Assert.Equal(new[] { Pango.AttrType.Weight },
                             iterator.Attrs.Select(a => a.Type).ToArray());

                Assert.True(iterator.Next());

                Assert.Equal(new[] { Pango.AttrType.Foreground, Pango.AttrType.Weight },
                             iterator.Attrs.Select(a => a.Type).OrderBy(t => t.ToString()).ToArray());

                Assert.True(iterator.Next());

                Assert.Equal(new[] { Pango.AttrType.Foreground },
                             iterator.Attrs.Select(a => a.Type).ToArray());
            });
        }

        [Fact]
        public void Asking_a_run_for_one_kind_of_attribute_answers_only_when_it_applies()
        {
            Run(() =>
            {
                using var list = Overlapping();
                var iterator = list.Iterator;

                Assert.NotNull(iterator.Get(Pango.AttrType.Weight));
                Assert.Null(iterator.Get(Pango.AttrType.Foreground));

                iterator.Next();
                iterator.Next();          // the last run: red, no longer bold

                Assert.Null(iterator.Get(Pango.AttrType.Weight));
                Assert.NotNull(iterator.Get(Pango.AttrType.Foreground));
            });
        }

        [Fact]
        public void GetFont_merges_the_font_attributes_and_hands_the_rest_back_separately()
        {
            // The split is the point: weight belongs in the FontDescription,
            // foreground cannot, so it comes out in extra_attrs. That array is the
            // other hand-written GSList walk, and it owns what it hands over.
            Run(() =>
            {
                using var list = Overlapping();
                var iterator = list.Iterator;

                iterator.Next();          // the run where both apply

                iterator.GetFont(out var description, out var language, out var extra);

                Assert.NotNull(description);
                Assert.Equal(Pango.Weight.Bold, description.Weight);

                Assert.Equal(new[] { Pango.AttrType.Foreground },
                             extra.Select(a => a.Type).ToArray());

                // Reading them proves the wrappers point at real attributes rather
                // than at whatever a mis-marshalled list handed over.
                Assert.Equal(ushort.MaxValue, ((Pango.AttrForeground) extra[0]).Color.Red);

                GC.KeepAlive(language);
            });
        }

        [Fact]
        public void A_run_with_no_extra_attributes_gives_an_empty_array_rather_than_null()
        {
            Run(() =>
            {
                using var list = new Pango.AttrList();
                list.Insert(new Pango.AttrWeight(Pango.Weight.Bold) { StartIndex = 0, EndIndex = 5 });

                list.Iterator.GetFont(out var description, out _, out var extra);

                Assert.NotNull(extra);
                Assert.Empty(extra);
                Assert.Equal(Pango.Weight.Bold, description.Weight);
            });
        }

        // ------------------------------------------------ what it does to text

        [Fact]
        public void Attributes_on_a_layout_change_how_the_text_measures()
        {
            // The end of the pipeline: an attribute list is only worth anything if
            // Pango lays the text out differently for it. Bold is wider than
            // regular at the same size, and the comparison is against the same
            // string in the same font, so no absolute measurement is assumed.
            Run(() =>
            {
                using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 400, 50);
                using var cr = new Cairo.Context(surface);
                using var layout = Pango.CairoHelper.CreateLayout(cr);

                layout.FontDescription = Pango.FontDescription.FromString("Sans 20");
                layout.SetText("mmmmmmmmmm");

                layout.GetPixelSize(out int plain, out _);

                var bold = new Pango.AttrList();
                bold.Insert(new Pango.AttrWeight(Pango.Weight.Bold) { StartIndex = 0, EndIndex = 10 });
                layout.Attributes = bold;

                layout.GetPixelSize(out int emboldened, out _);

                Assert.True(emboldened > plain,
                            $"bold ({emboldened}) should be wider than regular ({plain})");
            });
        }

        [Fact]
        public void Attribute_indices_are_byte_offsets_rather_than_character_offsets()
        {
            // The trap. "é" is two bytes in UTF-8, so an attribute meant to cover
            // the first two *characters* of "éa" ends at byte 3, not 2. Getting it
            // wrong formats half a letter, and Pango will not complain.
            Run(() =>
            {
                const string text = "éa";      // e-acute, then 'a'

                Assert.Equal(2, text.Length);                                        // characters
                Assert.Equal(3, System.Text.Encoding.UTF8.GetByteCount(text));       // bytes

                using var list = new Pango.AttrList();
                list.Insert(new Pango.AttrWeight(Pango.Weight.Bold) { StartIndex = 0, EndIndex = 3 });

                var iterator = list.Iterator;
                iterator.Range(out int start, out int end);

                Assert.Equal(0, start);
                Assert.Equal(3, end);      // the byte count, not the character count
            });
        }
    }
}

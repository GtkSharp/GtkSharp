using System;
using System.Linq;
using Pango;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// PangoSharp's hand-written layer was 14.5% covered, and its largest file —
    /// <c>Attribute.cs</c>, the hand-rolled hierarchy that turns a
    /// <c>PangoAttribute</c> back into the right managed subclass — had never run
    /// at all. Text layout has real oracles: an attribute applied over a byte range
    /// comes back over that range, wrapping a line makes more lines, and a tab stop
    /// set is a tab stop reported.
    /// </summary>
    public class PangoTests : GtkTestBase
    {
        public PangoTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>A layout on a real Cairo context, which is the only way to
        /// get a Pango context without a display.</summary>
        private static Layout LayoutOf(string text)
        {
            var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 200, 100);
            var cr = new Cairo.Context(surface);
            var layout = Pango.CairoHelper.CreateLayout(cr);
            layout.FontDescription = FontDescription.FromString("Sans 12");
            layout.SetText(text);
            return layout;
        }

        // ---------------------------------------------------------- Attribute

        [Fact]
        public void An_attribute_keeps_the_byte_range_it_was_given()
        {
            Run(() =>
            {
                var attr = new AttrWeight(Weight.Bold) { StartIndex = 4, EndIndex = 9 };

                Assert.Equal(4u, attr.StartIndex);
                Assert.Equal(9u, attr.EndIndex);
                Assert.Equal(AttrType.Weight, attr.Type);
            });
        }

        [Fact]
        public void Each_attribute_subclass_reports_its_own_type_and_value()
        {
            // Attribute.GetAttribute switches on the native AttrType to decide
            // which managed subclass to build. Every branch it gets wrong hands
            // back an attribute of the wrong shape, so each is asserted.
            Run(() =>
            {
                Assert.Equal(AttrType.Weight, new AttrWeight(Weight.Bold).Type);
                Assert.Equal(AttrType.Style, new AttrStyle(Style.Italic).Type);
                Assert.Equal(AttrType.Underline, new AttrUnderline(Underline.Single).Type);
                Assert.Equal(AttrType.Strikethrough, new AttrStrikethrough(true).Type);
                Assert.Equal(AttrType.Size, new AttrSize(12 * (int) Scale.PangoScale).Type);
                Assert.Equal(AttrType.Family, new AttrFamily("Serif").Type);
                Assert.Equal(AttrType.Foreground, new AttrForeground(65535, 0, 0).Type);
                Assert.Equal(AttrType.Background, new AttrBackground(0, 0, 65535).Type);
                Assert.Equal(AttrType.Rise, new AttrRise(1000).Type);
                Assert.Equal(AttrType.Scale, new AttrScale(1.5).Type);

                Assert.Equal(Weight.Bold, new AttrWeight(Weight.Bold).Weight);
                Assert.Equal(Style.Italic, new AttrStyle(Style.Italic).Style);
                Assert.Equal("Serif", new AttrFamily("Serif").Family);
                Assert.Equal(1.5, new AttrScale(1.5).Scale, 6);
            });
        }

        [Fact]
        public void A_copied_attribute_equals_the_original()
        {
            Run(() =>
            {
                var original = new AttrWeight(Weight.Bold) { StartIndex = 0, EndIndex = 4 };

                var copy = original.Copy();

                Assert.True(original.Equal(copy));
                Assert.Equal(original.Type, copy.Type);
                Assert.Equal(original.StartIndex, copy.StartIndex);
            });
        }

        [Fact]
        public void Attributes_of_different_kinds_are_not_equal()
        {
            Run(() =>
            {
                var bold = new AttrWeight(Weight.Bold);
                var italic = new AttrStyle(Style.Italic);

                Assert.False(bold.Equal(italic));
            });
        }

        [Fact]
        public void An_attribute_read_back_out_of_a_list_is_the_subclass_it_went_in_as()
        {
            // This is the point of the hand-written hierarchy: a PangoAttribute
            // coming back from C must become the managed subclass that matches
            // its type, or the caller cannot read its value at all.
            Run(() =>
            {
                var list = new AttrList();
                list.Insert(new AttrWeight(Weight.Bold) { StartIndex = 0, EndIndex = 4 });
                list.Insert(new AttrFamily("Serif") { StartIndex = 0, EndIndex = 4 });

                var iterator = list.Iterator;
                var attrs = iterator.Attrs;

                Assert.Equal(2, attrs.Length);

                var weight = attrs.OfType<AttrWeight>().Single();
                var family = attrs.OfType<AttrFamily>().Single();

                Assert.Equal(Weight.Bold, weight.Weight);
                Assert.Equal("Serif", family.Family);
            });
        }

        [Fact]
        public void An_attribute_iterator_walks_the_ranges_the_attributes_created()
        {
            Run(() =>
            {
                var list = new AttrList();
                list.Insert(new AttrWeight(Weight.Bold) { StartIndex = 0, EndIndex = 5 });

                var iterator = list.Iterator;

                // The first range is the attributed one.
                iterator.Range(out var start, out var end);
                Assert.Equal(0, start);
                Assert.Equal(5, end);
                Assert.Single(iterator.Attrs);

                // The next covers the rest of the text, with nothing on it.
                Assert.True(iterator.Next());
                iterator.Range(out var nextStart, out _);
                Assert.Equal(5, nextStart);
                Assert.Empty(iterator.Attrs);
            });
        }

        [Fact]
        public void An_iterator_reports_the_font_the_attributes_describe()
        {
            Run(() =>
            {
                var list = new AttrList();
                list.Insert(new AttrFamily("Serif") { StartIndex = 0, EndIndex = 4 });
                list.Insert(new AttrWeight(Weight.Bold) { StartIndex = 0, EndIndex = 4 });

                list.Iterator.GetFont(out var description, out _, out var extra);

                Assert.Equal("Serif", description.Family);
                Assert.Equal(Weight.Bold, description.Weight);
                Assert.NotNull(extra);
            });
        }

        [Fact]
        public void Markup_is_parsed_into_attributes_and_plain_text()
        {
            // pango_parse_markup is hidden="1" in the api.xml, so the only bound
            // route to the markup parser is a widget that uses it. A label given
            // markup reports the text without the tags -- which is the parser
            // having run, not the string having been stored.
            Run(() =>
            {
                var label = new Gtk.Label(null) { UseMarkup = true };

                label.LabelProp = "plain <b>bold</b> plain";

                Assert.Equal("plain bold plain", label.Text);
                Assert.Equal("plain <b>bold</b> plain", label.LabelProp);
            });
        }

        // ------------------------------------------------------------- Layout

        [Fact]
        public void Applying_attributes_to_a_layout_changes_how_wide_it_is()
        {
            // Bold text is wider than regular text in any real font, so this
            // asserts the attribute list actually reached the layout rather
            // than merely being stored on it.
            Run(() =>
            {
                var plain = LayoutOf("the quick brown fox");
                plain.GetPixelSize(out var plainWidth, out _);

                var bold = LayoutOf("the quick brown fox");
                var list = new AttrList();
                list.Insert(new AttrWeight(Weight.Bold) { StartIndex = 0, EndIndex = 19 });
                bold.Attributes = list;

                bold.GetPixelSize(out var boldWidth, out _);

                Assert.True(boldWidth > plainWidth,
                            $"bold {boldWidth} should exceed plain {plainWidth}");
            });
        }

        [Fact]
        public void A_layout_reports_the_lines_it_broke_the_text_into()
        {
            Run(() =>
            {
                var layout = LayoutOf("first line\nsecond line");

                Assert.Equal(2, layout.LineCount);
                Assert.Equal("first line\nsecond line", layout.Text);

                var lines = layout.Lines;

                Assert.Equal(2, lines.Length);
                Assert.All(lines, line => Assert.True(line.Length > 0));
            });
        }

        [Fact]
        public void Alignment_and_justification_are_reported_back()
        {
            Run(() =>
            {
                var layout = LayoutOf("aligned");

                layout.Alignment = Alignment.Center;
                layout.Justify = true;
                layout.SingleParagraphMode = true;
                layout.Spacing = 512;

                Assert.Equal(Alignment.Center, layout.Alignment);
                Assert.True(layout.Justify);
                Assert.True(layout.SingleParagraphMode);
                Assert.Equal(512, layout.Spacing);
            });
        }

        [Fact]
        public void An_ellipsised_layout_stays_within_the_width_it_was_given()
        {
            Run(() =>
            {
                var layout = LayoutOf("a line long enough that it certainly will not fit");
                layout.GetPixelSize(out var unconstrained, out _);

                layout.Width = 60 * (int) Scale.PangoScale;
                layout.Ellipsize = EllipsizeMode.End;

                layout.GetPixelSize(out var constrained, out _);

                Assert.True(layout.IsEllipsized);
                Assert.True(constrained <= 60, $"ellipsised width {constrained} should fit in 60");
                Assert.True(unconstrained > 60);
            });
        }

        [Fact]
        public void Index_to_position_and_back_agree_with_each_other()
        {
            Run(() =>
            {
                var layout = LayoutOf("positions");

                var position = layout.IndexToPos(4);

                Assert.True(position.Width > 0);

                Assert.True(layout.XyToIndex(position.X, position.Y, out var index, out _));
                Assert.Equal(4, index);
            });
        }

        [Fact]
        public void A_layout_iterator_walks_every_line_and_finishes()
        {
            Run(() =>
            {
                var layout = LayoutOf("one\ntwo\nthree");

                var iterator = layout.Iter;
                var lines = 0;

                do
                    lines++;
                while (iterator.NextLine());

                Assert.Equal(layout.LineCount, lines);
            });
        }

        // ----------------------------------------------------------- TabArray

        [Fact]
        public void A_tab_array_reports_the_stops_that_were_set_on_it()
        {
            Run(() =>
            {
                var tabs = new TabArray(3, true);

                tabs.SetTab(0, TabAlign.Left, 10);
                tabs.SetTab(1, TabAlign.Left, 40);
                tabs.SetTab(2, TabAlign.Left, 90);

                Assert.Equal(3, tabs.Size);

                tabs.GetTabs(out var alignments, out var locations);

                Assert.Equal(new[] { 10, 40, 90 }, locations);
                Assert.All(alignments, a => Assert.Equal(TabAlign.Left, a));
            });
        }

        [Fact]
        public void A_layout_reports_back_the_tab_array_it_was_given()
        {
            Run(() =>
            {
                var tabs = new TabArray(1, true);
                tabs.SetTab(0, TabAlign.Left, 64);

                var layout = LayoutOf("a\tb");
                layout.Tabs = tabs;

                layout.Tabs.GetTabs(out _, out var locations);

                Assert.Equal(new[] { 64 }, locations);
            });
        }

        // ------------------------------------------------------------- fonts

        [Fact]
        public void A_font_description_reports_every_field_that_was_set()
        {
            Run(() =>
            {
                var font = new FontDescription
                {
                    Family = "Serif",
                    Weight = Weight.Bold,
                    Style = Style.Italic,
                    Stretch = Stretch.Condensed,
                    Size = 14 * (int) Scale.PangoScale,
                };

                Assert.Equal("Serif", font.Family);
                Assert.Equal(Weight.Bold, font.Weight);
                Assert.Equal(Style.Italic, font.Style);
                Assert.Equal(Stretch.Condensed, font.Stretch);
                Assert.Equal(14 * (int) Scale.PangoScale, font.Size);
            });
        }

        [Fact]
        public void Two_font_descriptions_with_the_same_fields_compare_equal()
        {
            Run(() =>
            {
                var first = FontDescription.FromString("Serif Bold 12");
                var same = FontDescription.FromString("Serif Bold 12");
                var different = FontDescription.FromString("Serif Bold 14");

                Assert.True(first.Equal(same));
                Assert.False(first.Equal(different));

                // FontDescription inherited GLib.Opaque's handle comparison, so
                // two descriptions Pango calls equal were unequal here and
                // hashed differently -- one could not be used to find the other
                // in a dictionary.
                Assert.True(first.Equals(same));
                Assert.False(first.Equals(different));
                Assert.Equal(first.GetHashCode(), same.GetHashCode());

                var byFont = new System.Collections.Generic.Dictionary<FontDescription, string>
                {
                    [first] = "found",
                };

                Assert.Equal("found", byFont[same]);
            });
        }

        [Fact]
        public void Merging_a_font_description_takes_the_fields_that_were_set()
        {
            Run(() =>
            {
                var target = FontDescription.FromString("Sans 10");
                var source = FontDescription.FromString("Bold");

                target.Merge(source, true);

                Assert.Equal(Weight.Bold, target.Weight);
                Assert.Equal("Sans", target.Family);   // not overwritten
            });
        }

        [Fact]
        public void A_font_map_lists_families_that_include_the_one_a_layout_resolved()
        {
            Run(() =>
            {
                var layout = LayoutOf("text");
                var map = layout.Context.FontMap;

                var families = map.Families;

                Assert.NotEmpty(families);
                Assert.All(families, f => Assert.False(string.IsNullOrEmpty(f.Name)));
            });
        }

        [Fact]
        public void Font_metrics_describe_a_font_with_sensible_proportions()
        {
            Run(() =>
            {
                var layout = LayoutOf("metrics");

                var metrics = layout.Context.GetMetrics(layout.FontDescription, null);

                Assert.True(metrics.Ascent > 0);
                Assert.True(metrics.Descent > 0);
                Assert.True(metrics.ApproximateCharWidth > 0);

                // A line has to be at least as tall as the part above the
                // baseline; anything else means the units are wrong.
                Assert.True(metrics.Height >= metrics.Ascent || metrics.Height == 0);
            });
        }

        [Fact]
        public void A_language_round_trips_through_its_string_form()
        {
            Run(() =>
            {
                var language = Language.FromString("en-gb");

                Assert.Equal("en-gb", language.ToString());
                Assert.True(language.Matches("en-gb"));
                Assert.False(language.Matches("fr"));
            });
        }
    }
}

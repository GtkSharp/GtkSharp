using System;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The last two files `Docs/coverage.md` marks "worth testing":
    /// <c>Gtk.ComboBoxText</c>'s protected constructor and
    /// <c>Pango.Analysis</c>'s hand-written accessors.
    /// </summary>
    /// <remarks>
    /// Both are reachable only obliquely, which is why neither had coverage.
    ///
    /// <c>ComboBoxText(bool)</c> is `protected`, so it can only be called by a
    /// subclass — and it does two entirely different things depending on whether
    /// the caller *is* a subclass, which is the branch worth a test.
    ///
    /// <c>Analysis</c> is a struct handed out by Pango's itemiser rather than
    /// constructed, so reaching its accessors means itemising real text.
    /// </remarks>
    public class ComboBoxTextAndAnalysisTests : GtkTestBase
    {
        public ComboBoxTextAndAnalysisTests(GtkFixture fixture) : base(fixture) { }

        // ---------------------------------------------------- Gtk.ComboBoxText

        /// <summary>A subclass, which is the only way to reach the protected ctor.</summary>
        class DerivedCombo : Gtk.ComboBoxText
        {
            public DerivedCombo(bool hasEntry) : base(hasEntry) { }
        }

        [Fact]
        public void A_subclass_gets_a_combo_built_through_CreateNativeObject()
        {
            // The branch that matters. For a subclass the constructor cannot
            // call gtk_combo_box_text_new -- that would create a GtkComboBoxText
            // rather than the derived GType -- so it goes through
            // CreateNativeObject with the three properties set by hand. If those
            // property names or values were wrong the object would still be
            // built, and only its behaviour would say so.
            Run(() =>
            {
                var combo = new DerivedCombo(true);

                Assert.NotEqual(IntPtr.Zero, combo.Handle);
                Assert.True(combo.HasEntry, "has-entry was passed as true");

                // entry-text-column 0 and id-column 1 are the other two the
                // constructor sets, and they are what makes Append/ActiveText
                // work at all.
                Assert.Equal(0, combo.EntryTextColumn);
                Assert.Equal(1, combo.IdColumn);

                // The derived GType is registered, not GtkComboBoxText's.
                Assert.IsType<DerivedCombo>(combo);
            });
        }

        [Fact]
        public void A_subclass_without_an_entry_still_works_as_a_combo()
        {
            Run(() =>
            {
                var combo = new DerivedCombo(false);

                Assert.False(combo.HasEntry);

                combo.AppendText("first");
                combo.AppendText("second");
                combo.Active = 1;

                Assert.Equal("second", combo.ActiveText);
            });
        }

        [Fact]
        public void The_plain_type_takes_the_direct_native_constructor()
        {
            // The other side of the same branch: an exact GtkComboBoxText goes
            // straight to gtk_combo_box_text_new. Both paths have to produce a
            // combo that behaves the same, which is what this asserts rather
            // than which function was called.
            Run(() =>
            {
                var combo = new Gtk.ComboBoxText();

                combo.Append("id-a", "Apple");
                combo.Append("id-b", "Banana");
                combo.Prepend("id-z", "Zeroth");

                combo.Active = 0;
                Assert.Equal("Zeroth", combo.ActiveText);
                Assert.Equal("id-z", combo.ActiveId);

                combo.ActiveId = "id-b";
                Assert.Equal("Banana", combo.ActiveText);

                combo.Remove(0);
                combo.Active = 0;
                Assert.Equal("Apple", combo.ActiveText);

                combo.RemoveAll();
                Assert.Null(combo.ActiveText);
            });
        }

        [Fact]
        public void Inserting_puts_an_entry_where_it_was_asked_for()
        {
            Run(() =>
            {
                var combo = new Gtk.ComboBoxText();

                combo.AppendText("one");
                combo.AppendText("three");
                combo.InsertText(1, "two");

                foreach (var (index, expected) in new[] { (0, "one"), (1, "two"), (2, "three") })
                {
                    combo.Active = index;
                    Assert.Equal(expected, combo.ActiveText);
                }
            });
        }

        // ------------------------------------------------------ Pango.Analysis

        /// <summary>Itemise text, which is the only way to obtain an Analysis.</summary>
        /// <remarks>
        /// This helper is the reason `pango_itemize` got fixed. It returned a
        /// bare <c>GLib.List</c> with no element type, so iterating it read each
        /// <c>PangoItem*</c> as a GObject and took the process down. Its sibling
        /// <c>ItemizeWithBaseDir</c> — same C signature, same return type — had
        /// the metadata and returned <c>Pango.Item[]</c>. Both do now.
        /// </remarks>
        static Pango.Item[] Itemize(string text, Pango.AttrList attrs)
        {
            var context = new Gtk.Label("x").PangoContext;

            return Pango.Global.Itemize(context, text, 0,
                                        System.Text.Encoding.UTF8.GetByteCount(text),
                                        attrs, null);
        }

        [Fact]
        public void Itemize_returns_items_rather_than_an_untyped_list()
        {
            // The regression test for the crash above. An untyped GLib.List
            // cannot be asserted against -- reading one element ends the
            // process -- so what this pins is the *shape* of the return, which
            // is what makes reading it safe.
            Run(() =>
            {
                using var attrs = new Pango.AttrList();

                var items = Itemize("hello world", attrs);

                Assert.NotEmpty(items);
                Assert.All(items, item => Assert.NotNull(item));

                // The runs together have to cover the text they came from.
                Assert.Equal(11, items.Sum(i => i.Length));
                Assert.Equal(0, items[0].Offset);
            });
        }

        [Fact]
        public void Both_itemisers_agree_about_the_same_text()
        {
            // The two are the same C function with one extra argument, and they
            // disagreed for years only because one carried the element-type
            // metadata and the other did not. Asserting they agree is what stops
            // that drifting apart again.
            Run(() =>
            {
                using var attrs = new Pango.AttrList();
                var context = new Gtk.Label("x").PangoContext;
                const string text = "hello world";
                int bytes = System.Text.Encoding.UTF8.GetByteCount(text);

                var plain = Pango.Global.Itemize(context, text, 0, bytes, attrs, null);
                var directed = Pango.Global.ItemizeWithBaseDir(
                    context, Pango.Direction.Ltr, text, 0, bytes, attrs, null);

                Assert.Equal(plain.Length, directed.Length);
                Assert.Equal(plain.Select(i => i.Offset), directed.Select(i => i.Offset));
                Assert.Equal(plain.Select(i => i.Length), directed.Select(i => i.Length));
            });
        }

        [Fact]
        public void An_itemised_run_reports_the_font_and_language_it_was_analysed_with()
        {
            // font and language are marked obsolete in favour of Font and
            // Language, but they are still the hand-written accessors that read
            // the struct's raw IntPtr fields, and a wrong one returns null
            // rather than failing.
            Run(() =>
            {
                using var attrs = new Pango.AttrList();

                var items = Itemize("hello", attrs);

                Assert.NotEmpty(items);

                var analysis = items[0].Analysis;

#pragma warning disable CS0618
                Assert.NotNull(analysis.font);
                Assert.NotNull(analysis.language);
#pragma warning restore CS0618

                // The non-obsolete properties read the same fields, so they have
                // to agree -- which is what says the accessors decode the struct
                // rather than happening to be non-null.
#pragma warning disable CS0618
                Assert.Equal(analysis.Language.ToString(), analysis.language.ToString());
#pragma warning restore CS0618
            });
        }

        [Fact]
        public void The_extra_attributes_of_a_run_are_the_ones_that_applied_to_it()
        {
            // ExtraAttrs walks a GSList of PangoAttribute* by hand. An empty
            // list and a populated one are different code paths through that
            // loop, and the populated one is the only thing that shows the walk
            // is right.
            Run(() =>
            {
                using var attrs = new Pango.AttrList();

                // Which attributes land in ExtraAttrs is not a free choice.
                // Pango folds anything that affects *font selection* -- weight,
                // style, family, size -- into analysis.font, and carries only
                // the rest as extra. So underline and strikethrough are the
                // ones that arrive here; a test written with Bold and Italic
                // gets an empty array and looks like a broken accessor.
                var underline = new Pango.AttrUnderline(Pango.Underline.Single);
                underline.StartIndex = 0;
                underline.EndIndex = 5;
                attrs.Insert(underline);

                var strike = new Pango.AttrStrikethrough(true);
                strike.StartIndex = 0;
                strike.EndIndex = 5;
                attrs.Insert(strike);

                var items = Itemize("hello", attrs);
                Assert.NotEmpty(items);

                var extra = items[0].Analysis.ExtraAttrs;

                Assert.NotNull(extra);
                Assert.Equal(2, extra.Length);
                Assert.All(extra, a => Assert.NotNull(a));

                var kinds = extra.Select(a => a.Type).ToArray();
                Assert.Contains(Pango.AttrType.Underline, kinds);
                Assert.Contains(Pango.AttrType.Strikethrough, kinds);
            });
        }

        [Fact]
        public void A_run_with_no_extra_attributes_reports_an_empty_array()
        {
            // The control, and the empty-GSList branch of the same walk. An
            // implementation that returned null here would fail differently from
            // one that returned a wrong length.
            Run(() =>
            {
                using var attrs = new Pango.AttrList();

                var items = Itemize("hello", attrs);
                Assert.NotEmpty(items);

                var extra = items[0].Analysis.ExtraAttrs;

                Assert.NotNull(extra);
                Assert.Empty(extra);
            });
        }
    }
}

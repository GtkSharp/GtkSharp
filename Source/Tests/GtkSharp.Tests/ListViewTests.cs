using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The half of the Gtk 4 list pipeline that turns a model into rows:
    /// <c>Gtk.Bitset</c>, the three selection models, <c>SignalListItemFactory</c>
    /// and <c>ListView</c>.
    /// </summary>
    /// <remarks>
    /// The model stages — <c>SortListModel</c>, <c>FilterListModel</c>,
    /// <c>StringSorter</c> — are covered by <c>ActionsAndModelsTests</c>. Nothing
    /// covered what happens after them: <c>ListView</c>, <c>MultiSelection</c>,
    /// <c>NoSelection</c>, <c>ListItem</c> and <c>Bitset</c> had no mention in the
    /// suite at all, and <c>ListView</c> is the widget
    /// <c>Docs/getting-started.md</c> tells people to use instead of
    /// <c>TreeView</c>.
    ///
    /// A <c>Bitset</c> is the only thing a selection model will tell you about a
    /// multiple selection, so it is tested first and against set arithmetic the
    /// test does itself.
    /// </remarks>
    public class ListViewTests : GtkTestBase
    {
        public ListViewTests(GtkFixture fixture) : base(fixture) { }

        static Gtk.StringList Strings(params string[] items) => new Gtk.StringList(items);

        static readonly string[] Four = { "alpha", "bravo", "charlie", "delta" };

        /// <summary>The members of a bitset, which is otherwise only readable one
        /// index at a time.</summary>
        static uint[] Members(Gtk.Bitset set)
        {
            var members = new List<uint>();
            for (ulong i = 0; i < set.Size; i++)
                members.Add(set.GetNth((uint) i));
            return members.ToArray();
        }

        // ------------------------------------------------------------- Bitset

        [Fact]
        public void A_bitset_holds_the_values_added_to_it_and_no_others()
        {
            Run(() =>
            {
                using var set = new Gtk.Bitset();

                Assert.True(set.IsEmpty);

                Assert.True(set.Add(4), "adding a new value reports that it was new");
                Assert.False(set.Add(4), "adding it again reports that it was not");
                set.Add(1);
                set.Add(9);

                Assert.False(set.IsEmpty);
                Assert.Equal(3ul, set.Size);
                Assert.Equal(new uint[] { 1, 4, 9 }, Members(set));   // sorted, not insertion order

                Assert.True(set.Contains(4));
                Assert.False(set.Contains(5));
                Assert.Equal(1u, set.Minimum);
                Assert.Equal(9u, set.Maximum);
            });
        }

        [Fact]
        public void Removing_from_a_bitset_reports_whether_anything_went()
        {
            Run(() =>
            {
                using var set = new Gtk.Bitset(0, 5);      // 0..4

                Assert.Equal(5ul, set.Size);
                Assert.True(set.Remove(2));
                Assert.False(set.Remove(2));

                Assert.Equal(new uint[] { 0, 1, 3, 4 }, Members(set));

                set.RemoveRange(3, 2);
                Assert.Equal(new uint[] { 0, 1 }, Members(set));

                set.RemoveAll();
                Assert.True(set.IsEmpty);
            });
        }

        [Fact]
        public void Bitset_union_intersection_and_difference_are_the_set_operations()
        {
            // Arithmetic the test can do itself, which is the point: the oracle
            // is what the words mean, not what Gtk happens to return.
            Run(() =>
            {
                using var left = new Gtk.Bitset(0, 4);     // 0,1,2,3
                using var right = new Gtk.Bitset(2, 4);    // 2,3,4,5

                using var union = left.Copy();
                union.Union(right);
                Assert.Equal(new uint[] { 0, 1, 2, 3, 4, 5 }, Members(union));

                using var intersection = left.Copy();
                intersection.Intersect(right);
                Assert.Equal(new uint[] { 2, 3 }, Members(intersection));

                using var subtracted = left.Copy();
                subtracted.Subtract(right);
                Assert.Equal(new uint[] { 0, 1 }, Members(subtracted));

                // Difference is the symmetric one -- everything in exactly one.
                using var difference = left.Copy();
                difference.Difference(right);
                Assert.Equal(new uint[] { 0, 1, 4, 5 }, Members(difference));
            });
        }

        [Fact]
        public void A_copied_bitset_does_not_change_when_the_original_does()
        {
            Run(() =>
            {
                using var original = new Gtk.Bitset(0, 3);
                using var copy = original.Copy();

                original.Add(10);

                Assert.Equal(4ul, original.Size);
                Assert.Equal(3ul, copy.Size);
                Assert.False(copy.Contains(10));
                Assert.False(original.Equals(copy));
            });
        }

        [Fact]
        public void Shifting_a_bitset_moves_every_value_by_the_same_amount()
        {
            Run(() =>
            {
                using var set = new Gtk.Bitset();
                set.Add(1);
                set.Add(3);

                set.ShiftRight(10);
                Assert.Equal(new uint[] { 11, 13 }, Members(set));

                set.ShiftLeft(10);
                Assert.Equal(new uint[] { 1, 3 }, Members(set));
            });
        }

        // --------------------------------------------------------- StringList

        static string[] Contents(Gtk.StringList list)
        {
            var items = new string[list.NItems];
            for (uint i = 0; i < list.NItems; i++)
                items[i] = list.GetString(i);
            return items;
        }

        [Fact]
        public void Splicing_nothing_out_is_a_pure_insertion()
        {
            // This is the call that used to destroy data. gtk_string_list_splice
            // takes (position, n_removals, additions), but codegen mistook
            // n_removals for the additions array's length -- the parameter
            // vanished from the signature and the array's length was passed in
            // its place. So this call removed two items and the caller had no way
            // to say otherwise, or even to see that it had happened.
            Run(() =>
            {
                var list = Strings(Four);

                list.Splice(1, 0, new[] { "insert1", "insert2" });

                Assert.Equal(
                    new[] { "alpha", "insert1", "insert2", "bravo", "charlie", "delta" },
                    Contents(list));
            });
        }

        [Fact]
        public void Splicing_replaces_exactly_the_rows_it_was_told_to()
        {
            Run(() =>
            {
                var list = Strings(Four);

                list.Splice(1, 2, new[] { "replacement" });

                Assert.Equal(new[] { "alpha", "replacement", "delta" }, Contents(list));
            });
        }

        [Fact]
        public void Splicing_with_no_additions_only_removes()
        {
            Run(() =>
            {
                var list = Strings(Four);

                list.Splice(0, 2, new string[0]);

                Assert.Equal(new[] { "charlie", "delta" }, Contents(list));
            });
        }

        [Fact]
        public void A_splice_reports_one_change_covering_both_halves()
        {
            // A view rebuilds from these arguments, so the removal and the
            // addition counts have to be the real ones rather than a single
            // number standing in for both.
            Run(() =>
            {
                var list = Strings(Four);

                uint position = 0, removed = 0, added = 0;
                int fired = 0;

                list.ItemsChanged += (o, args) =>
                {
                    fired++;
                    position = args.Position;
                    removed = args.Removed;
                    added = args.Added;
                };

                list.Splice(1, 2, new[] { "one", "two", "three" });

                Assert.Equal(1, fired);
                Assert.Equal(1u, position);
                Assert.Equal(2u, removed);
                Assert.Equal(3u, added);
            });
        }

        // --------------------------------------------------- selection models

        [Fact]
        public void A_single_selection_selects_one_row_and_reports_it_three_ways()
        {
            Run(() =>
            {
                var selection = new Gtk.SingleSelection(Strings(Four));

                selection.Selected = 2;

                Assert.Equal(2u, selection.Selected);
                Assert.True(selection.IsSelected(2));
                Assert.False(selection.IsSelected(1));

                // ...and the bitset agrees, holding exactly that one index.
                using var selected = selection.Selection;
                Assert.Equal(new uint[] { 2 }, Members(selected));
            });
        }

        [Fact]
        public void Emptying_a_single_selection_takes_both_of_its_flags()
        {
            // Two independent guards, and knowing only about one is a trap: I
            // expected CanUnselect on its own to be enough, and it is not.
            //
            //   CanUnselect  refuses the unselect outright.
            //   Autoselect   allows it, then immediately picks a row again so the
            //                selection is never empty.
            //
            // Both default to the value that keeps a row selected, which is why a
            // list view always has one highlighted. Each step below asserts the
            // state after it, so the one that finally works is distinguishable
            // from the two that quietly do not.
            Run(() =>
            {
                var selection = new Gtk.SingleSelection(Strings(Four)) { Selected = 1 };

                Assert.False(selection.CanUnselect);
                Assert.True(selection.Autoselect);

                selection.UnselectItem(1);
                Assert.Equal(1u, selection.Selected);        // refused

                selection.CanUnselect = true;
                selection.UnselectItem(1);
                Assert.Equal(1u, selection.Selected);        // allowed, then undone

                selection.Autoselect = false;
                selection.UnselectItem(1);
                Assert.Equal(Gtk.Global.InvalidListPosition, selection.Selected);
            });
        }

        [Fact]
        public void An_empty_single_selection_reports_the_invalid_position()
        {
            // GTK_INVALID_LIST_POSITION is a <constant> in the gir, which
            // GirToGapi does not emit, so until it was hand-written the only way
            // to ask "is anything selected" was to compare against uint.MaxValue
            // and hope. StringList.Find answers with the same value.
            Run(() =>
            {
                var list = Strings(Four);
                var selection = new Gtk.SingleSelection(list)
                {
                    Autoselect = false,
                    CanUnselect = true,
                };

                selection.UnselectAll();

                Assert.Equal(Gtk.Global.InvalidListPosition, selection.Selected);
                Assert.Equal(Gtk.Global.InvalidListPosition, list.Find("not in the list"));
                Assert.Equal(0u, list.Find("alpha"));
            });
        }

        [Fact]
        public void A_multi_selection_holds_as_many_rows_as_it_is_given()
        {
            Run(() =>
            {
                var selection = new Gtk.MultiSelection(Strings(Four));

                selection.SelectItem(0, false);
                selection.SelectItem(2, false);

                using (var selected = selection.Selection)
                    Assert.Equal(new uint[] { 0, 2 }, Members(selected));

                // false means "do not unselect the rest", so this adds to them.
                selection.SelectRange(2, 2, false);

                using (var selected = selection.Selection)
                    Assert.Equal(new uint[] { 0, 2, 3 }, Members(selected));

                selection.SelectAll();
                using (var selected = selection.Selection)
                    Assert.Equal(4ul, selected.Size);

                selection.UnselectAll();
                using (var selected = selection.Selection)
                    Assert.True(selected.IsEmpty);
            });
        }

        [Fact]
        public void Selecting_with_unselect_rest_replaces_the_selection()
        {
            Run(() =>
            {
                var selection = new Gtk.MultiSelection(Strings(Four));

                selection.SelectItem(0, false);
                selection.SelectItem(1, false);
                selection.SelectItem(3, true);      // true: drop the others

                using var selected = selection.Selection;
                Assert.Equal(new uint[] { 3 }, Members(selected));
            });
        }

        [Fact]
        public void A_no_selection_model_never_selects_anything()
        {
            // The wrapper a read-only list uses. It still has to be a selection
            // model, because that is what a view takes.
            Run(() =>
            {
                var selection = new Gtk.NoSelection(Strings(Four));

                Assert.Equal(4u, selection.NItems);

                selection.SelectItem(1, false);
                selection.SelectAll();

                Assert.False(selection.IsSelected(1));
                using var selected = selection.Selection;
                Assert.True(selected.IsEmpty);
            });
        }

        [Fact]
        public void A_selection_model_passes_the_underlying_model_through()
        {
            Run(() =>
            {
                var source = Strings(Four);
                var selection = new Gtk.SingleSelection(source);

                Assert.Equal(4u, selection.NItems);
                Assert.Same(source, selection.Model);

                // And it follows the source rather than snapshotting it.
                source.Append("echo");
                Assert.Equal(5u, selection.NItems);
            });
        }

        [Fact]
        public void Adding_to_the_source_tells_the_selection_model_what_changed()
        {
            // ItemsChanged is how a view knows which rows to rebuild. The
            // arguments are the whole content of the signal, so they are what is
            // asserted rather than the fact that it fired.
            Run(() =>
            {
                var source = Strings(Four);
                var selection = new Gtk.SingleSelection(source);

                uint position = uint.MaxValue, removed = uint.MaxValue, added = uint.MaxValue;
                int fired = 0;

                selection.ItemsChanged += (o, args) =>
                {
                    fired++;
                    position = args.Position;
                    removed = args.Removed;
                    added = args.Added;
                };

                source.Append("echo");

                Assert.Equal(1, fired);
                Assert.Equal(4u, position);
                Assert.Equal(0u, removed);
                Assert.Equal(1u, added);
            });
        }

        // ------------------------------------------------------------ ListView

        [Fact]
        public void A_list_view_keeps_the_model_and_factory_it_was_built_with()
        {
            Run(() =>
            {
                var selection = new Gtk.SingleSelection(Strings(Four));
                var factory = new Gtk.SignalListItemFactory();

                var view = new Gtk.ListView(selection, factory);

                Assert.Same(selection, view.Model);
                Assert.Same(factory, view.Factory);
            });
        }

        [Fact]
        public void A_list_view_builds_a_row_widget_for_each_visible_item()
        {
            // Setup creates the row widget once and Bind fills it in, possibly
            // many times, because the view recycles widgets as it scrolls. This
            // is the whole point of the factory, and it only happens once the
            // view has been laid out -- hence the window.
            Run(() =>
            {
                var selection = new Gtk.SingleSelection(Strings(Four));
                var factory = new Gtk.SignalListItemFactory();

                int setups = 0;
                var bound = new List<string>();

                factory.Setup += (o, args) =>
                {
                    setups++;
                    ((Gtk.ListItem) args.Object).Child = new Gtk.Label("");
                };

                factory.Bind += (o, args) =>
                {
                    var item = (Gtk.ListItem) args.Object;
                    var text = ((Gtk.StringObject) GLib.Object.GetObject(item.Item)).String;

                    ((Gtk.Label) item.Child).Text = text;
                    bound.Add(text);
                };

                var view = new Gtk.ListView(selection, factory);

                using var window = new Gtk.Window { DefaultWidth = 200, DefaultHeight = 300 };
                window.Child = view;
                window.Present();

                Assert.True(PumpUntil(() => bound.Count >= 4),
                            $"the factory should have bound every row; bound {bound.Count}");

                Assert.True(setups >= 4, $"a row widget per item, got {setups}");

                // The rows carry the model's strings, in the model's order.
                Assert.Equal(Four, bound.Take(4).ToArray());

                window.Destroy();
            });
        }

        [Fact]
        public void A_bound_row_knows_its_position_in_the_model()
        {
            Run(() =>
            {
                var selection = new Gtk.SingleSelection(Strings(Four));
                var factory = new Gtk.SignalListItemFactory();

                var positions = new Dictionary<string, uint>();

                factory.Setup += (o, args) => ((Gtk.ListItem) args.Object).Child = new Gtk.Label("");
                factory.Bind += (o, args) =>
                {
                    var item = (Gtk.ListItem) args.Object;
                    var text = ((Gtk.StringObject) GLib.Object.GetObject(item.Item)).String;
                    positions[text] = item.Position;
                };

                using var window = new Gtk.Window { DefaultWidth = 200, DefaultHeight = 300 };
                window.Child = new Gtk.ListView(selection, factory);
                window.Present();

                Assert.True(PumpUntil(() => positions.Count >= 4), "every row should bind");

                Assert.Equal(0u, positions["alpha"]);
                Assert.Equal(3u, positions["delta"]);

                window.Destroy();
            });
        }

        [Fact]
        public void Replacing_a_list_views_model_rebinds_it_to_the_new_one()
        {
            Run(() =>
            {
                var factory = new Gtk.SignalListItemFactory();
                var bound = new List<string>();

                factory.Setup += (o, args) => ((Gtk.ListItem) args.Object).Child = new Gtk.Label("");
                factory.Bind += (o, args) =>
                {
                    var item = (Gtk.ListItem) args.Object;
                    bound.Add(((Gtk.StringObject) GLib.Object.GetObject(item.Item)).String);
                };

                var view = new Gtk.ListView(new Gtk.SingleSelection(Strings("one", "two")), factory);

                using var window = new Gtk.Window { DefaultWidth = 200, DefaultHeight = 300 };
                window.Child = view;
                window.Present();

                Assert.True(PumpUntil(() => bound.Count >= 2), "the first model should bind");

                bound.Clear();
                view.Model = new Gtk.SingleSelection(Strings("three", "four"));

                Assert.True(PumpUntil(() => bound.Count >= 2), "the second model should bind too");
                Assert.Contains("three", bound);
                Assert.DoesNotContain("one", bound);

                window.Destroy();
            });
        }

        [Fact]
        public void A_row_reports_whether_it_is_the_selected_one()
        {
            Run(() =>
            {
                var selection = new Gtk.SingleSelection(Strings(Four)) { Selected = 2 };
                var factory = new Gtk.SignalListItemFactory();

                var selectedRows = new List<string>();

                factory.Setup += (o, args) => ((Gtk.ListItem) args.Object).Child = new Gtk.Label("");
                factory.Bind += (o, args) =>
                {
                    var item = (Gtk.ListItem) args.Object;
                    if (item.Selected)
                        selectedRows.Add(((Gtk.StringObject) GLib.Object.GetObject(item.Item)).String);
                };

                using var window = new Gtk.Window { DefaultWidth = 200, DefaultHeight = 300 };
                window.Child = new Gtk.ListView(selection, factory);
                window.Present();

                Assert.True(PumpUntil(() => selectedRows.Count >= 1), "the selected row should bind");

                Assert.Equal(new[] { "charlie" }, selectedRows.Distinct().ToArray());

                window.Destroy();
            });
        }

        [Fact]
        public void Unbind_runs_when_a_row_widget_is_recycled_away()
        {
            // The half people forget: anything Bind attaches -- a signal handler,
            // a subscription -- has to come off in Unbind, or a recycled widget
            // fires for the row it used to show.
            Run(() =>
            {
                var source = Strings(Four);
                var factory = new Gtk.SignalListItemFactory();

                int binds = 0, unbinds = 0;

                factory.Setup += (o, args) => ((Gtk.ListItem) args.Object).Child = new Gtk.Label("");
                factory.Bind += (o, args) => binds++;
                factory.Unbind += (o, args) => unbinds++;

                using var window = new Gtk.Window { DefaultWidth = 200, DefaultHeight = 300 };
                window.Child = new Gtk.ListView(new Gtk.SingleSelection(source), factory);
                window.Present();

                Assert.True(PumpUntil(() => binds >= 4), "every row should bind");

                // Emptying the model has to release the rows that were showing.
                source.Splice(0, 4, new string[0]);

                Assert.True(PumpUntil(() => unbinds >= 4),
                            $"every bound row should unbind; {unbinds} of {binds} did");

                window.Destroy();
            });
        }

        // ------------------------------------------------------------- helper

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
    }
}

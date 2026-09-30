using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The other end of <c>GtkSelectionModel</c>: a C# class that <em>is</em> one.
    ///
    /// <para><c>Gtk.SelectionModelAdapter</c> writes nine managed function pointers
    /// into a <c>GLib.Object</c> subclass's <c>GtkSelectionModelInterface</c>, and
    /// <c>GLib.ListModelAdapter</c> writes three more into its
    /// <c>GListModelInterface</c> -- both are needed, because
    /// <c>G_DEFINE_INTERFACE (GtkSelectionModel, gtk_selection_model,
    /// G_TYPE_LIST_MODEL)</c> makes the list model a prerequisite of the selection
    /// model. Nothing in the tree had ever implemented either from managed code:
    /// every selection model a program held was a bound concrete type.</para>
    ///
    /// <para>The oracle is a set of positions this test chooses, and the questions
    /// are asked only through C. <c>gtk_selection_model_get_selection</c> is Gtk's
    /// own default implementation and reaches the answer by calling
    /// <c>get_selection_in_range</c> through the vtable;
    /// <c>GtkSelectionFilterModel</c> is a list model written entirely in C whose
    /// contents are decided by asking the selection model which of its items are
    /// selected, and by listening for <c>::selection-changed</c>. Neither answer
    /// exists anywhere in this file's own data structures.</para>
    /// </summary>
    public class SelectionModelImplementorTests : GtkTestBase
    {
        public SelectionModelImplementorTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------ the model

        /// <summary>
        /// A selection model written in C#: eight lettered items and a set of
        /// selected positions, with every interface method logging the arguments
        /// it was handed so the test can assert what Gtk actually asked.
        /// </summary>
        public class LetterModel : GLib.Object, GLib.IListModelImplementor, Gtk.ISelectionModelImplementor
        {
            private readonly List<Gtk.StringObject> _items = new List<Gtk.StringObject>();
            private readonly SortedSet<uint> _selected = new SortedSet<uint>();
            private Gtk.SelectionModelAdapter _asSelection;

            public LetterModel(string letters = "ABCDEFGH")
            {
                foreach (var c in letters)
                    _items.Add(new Gtk.StringObject(c.ToString()));
            }

            public LetterModel(IntPtr raw) : base(raw) { }

            /// <summary>Every call Gtk made, in order, with its arguments.</summary>
            public List<string> Log { get; } = new List<string>();

            /// <summary>Stops the mutators emitting ::selection-changed, so a test
            /// can show what the signal is load-bearing for.</summary>
            public bool Silent { get; set; }

            /// <summary>Makes every mutator refuse, which is what a FALSE return
            /// from one of these vfuncs means.</summary>
            public bool ReadOnly { get; set; }

            public IReadOnlyCollection<uint> Selected => _selected;

            public string TextAt(uint position) => _items[(int) position].String;

            /// <summary>The same object seen through GtkSelectionModel.</summary>
            public Gtk.SelectionModelAdapter AsSelectionModel =>
                _asSelection ?? (_asSelection = new Gtk.SelectionModelAdapter(this));

            public void PresetSelection(params uint[] positions)
            {
                _selected.Clear();
                foreach (var p in positions)
                    _selected.Add(p);
            }

            // -------- GLib.IListModelImplementor

            public GLib.GType ItemType => Gtk.StringObject.GType;

            public uint NItems => (uint) _items.Count;

            public IntPtr GetItem(uint position)
            {
                Log.Add("get-item " + position);
                // g_list_model_get_item is transfer-full: the caller owns a
                // reference, so one has to be taken here.
                return position < _items.Count ? _items[(int) position].OwnedHandle : IntPtr.Zero;
            }

            // -------- Gtk.ISelectionModelImplementor

            public bool IsSelected(uint position)
            {
                Log.Add("is-selected " + position);
                return _selected.Contains(position);
            }

            public Gtk.Bitset GetSelectionInRange(uint position, uint n_items)
            {
                // Gtk spells "the whole selection" as [0, G_MAXUINT), so the window
                // has to be clipped to the model before anything walks it.
                Log.Add("get-selection-in-range " + position + " " +
                        (n_items == uint.MaxValue ? "max" : n_items.ToString()));

                var set = new Gtk.Bitset();
                ulong end = Math.Min((ulong) position + n_items, (ulong) _items.Count);
                for (ulong i = position; i < end; i++)
                    if (_selected.Contains((uint) i))
                        set.Add((uint) i);
                return set;
            }

            public bool SelectItem(uint position, bool unselect_rest)
            {
                Log.Add("select-item " + position + " " + unselect_rest);
                return Change(() =>
                {
                    if (unselect_rest)
                        _selected.Clear();
                    _selected.Add(position);
                });
            }

            public bool UnselectItem(uint position)
            {
                Log.Add("unselect-item " + position);
                return Change(() => _selected.Remove(position));
            }

            public bool SelectRange(uint position, uint n_items, bool unselect_rest)
            {
                Log.Add("select-range " + position + " " + n_items + " " + unselect_rest);
                return Change(() =>
                {
                    if (unselect_rest)
                        _selected.Clear();
                    for (uint i = position; i < position + n_items; i++)
                        _selected.Add(i);
                });
            }

            public bool UnselectRange(uint position, uint n_items)
            {
                Log.Add("unselect-range " + position + " " + n_items);
                return Change(() =>
                {
                    for (uint i = position; i < position + n_items; i++)
                        _selected.Remove(i);
                });
            }

            public bool SelectAll()
            {
                Log.Add("select-all");
                return Change(() =>
                {
                    for (uint i = 0; i < _items.Count; i++)
                        _selected.Add(i);
                });
            }

            public bool UnselectAll()
            {
                Log.Add("unselect-all");
                return Change(() => _selected.Clear());
            }

            public bool SetSelection(Gtk.Bitset selected, Gtk.Bitset mask)
            {
                Log.Add("set-selection selected=" + Describe(selected) + " mask=" + Describe(mask));
                return Change(() =>
                {
                    for (uint i = 0; i < _items.Count; i++)
                    {
                        if (!mask.Contains(i))
                            continue;
                        if (selected.Contains(i))
                            _selected.Add(i);
                        else
                            _selected.Remove(i);
                    }
                });
            }

            /// <summary>Applies a mutation and tells Gtk about the span that moved.</summary>
            private bool Change(Action mutate)
            {
                if (ReadOnly)
                    return false;

                var before = new SortedSet<uint>(_selected);
                mutate();

                var moved = new SortedSet<uint>(before);
                moved.SymmetricExceptWith(_selected);
                if (moved.Count > 0 && !Silent)
                    AsSelectionModel.EmitSelectionChanged(moved.Min, moved.Max - moved.Min + 1);

                return true;
            }
        }

        /// <summary>
        /// The same two interfaces, declared the other way round: GtkSelectionModel
        /// first and its GListModel prerequisite second. Only the order differs, so
        /// anything this model does differently is a defect in how the interfaces
        /// are registered rather than in either model.
        /// </summary>
        public sealed class SelectionFirstModel : GLib.Object, Gtk.ISelectionModelImplementor, GLib.IListModelImplementor
        {
            private readonly List<Gtk.StringObject> _items = new List<Gtk.StringObject>();
            private readonly SortedSet<uint> _selected = new SortedSet<uint>();

            public SelectionFirstModel()
            {
                foreach (var c in "ABCDEFGH")
                    _items.Add(new Gtk.StringObject(c.ToString()));
                _selected.Add(1);
                _selected.Add(3);
                _selected.Add(4);
            }

            public SelectionFirstModel(IntPtr raw) : base(raw) { }

            public GLib.GType ItemType => Gtk.StringObject.GType;
            public uint NItems => (uint) _items.Count;
            public IntPtr GetItem(uint position) =>
                position < _items.Count ? _items[(int) position].OwnedHandle : IntPtr.Zero;

            public bool IsSelected(uint position) => _selected.Contains(position);

            public Gtk.Bitset GetSelectionInRange(uint position, uint n_items)
            {
                var set = new Gtk.Bitset();
                ulong end = Math.Min((ulong) position + n_items, (ulong) _items.Count);
                for (ulong i = position; i < end; i++)
                    if (_selected.Contains((uint) i))
                        set.Add((uint) i);
                return set;
            }

            public bool SelectItem(uint position, bool unselect_rest) { _selected.Add(position); return true; }
            public bool UnselectItem(uint position) { _selected.Remove(position); return true; }
            public bool SelectRange(uint position, uint n_items, bool unselect_rest) => true;
            public bool UnselectRange(uint position, uint n_items) => true;
            public bool SelectAll() => true;
            public bool UnselectAll() { _selected.Clear(); return true; }
            public bool SetSelection(Gtk.Bitset selected, Gtk.Bitset mask) => true;
        }

        // ------------------------------------------------------ helpers

        private static string Describe(Gtk.Bitset set)
        {
            return set == null ? "(null)" : "{" + string.Join(",", Members(set)) + "}";
        }

        /// <summary>Every position a bitset holds, read out through Gtk.</summary>
        private static uint[] Members(Gtk.Bitset set)
        {
            var result = new List<uint>();
            for (ulong i = 0; i < set.Size; i++)
                result.Add(set.GetNth((uint) i));
            return result.ToArray();
        }

        private static string[] Texts(GLib.IListModel model)
        {
            var result = new List<string>();
            for (uint i = 0; i < model.NItems; i++)
                result.Add(((Gtk.StringObject) model.GetObject(i)).String);
            return result.ToArray();
        }

        private static LetterModel Sample()
        {
            var model = new LetterModel();
            model.PresetSelection(1, 3, 4);
            return model;
        }

        // ------------------------------------------------ the interface is there

        [Fact]
        public void A_managed_implementor_is_a_selection_model_and_a_list_model_to_GType()
        {
            Run(() =>
            {
                var model = Sample();

                // GtkSelectionModel has GListModel as a GInterface prerequisite, so
                // neither of these can be true unless both interfaces were added,
                // and added in an order GLib accepts.
                Assert.True(GLib.ListModelAdapter.GType.IsInstance(model.Handle));
                Assert.True(Gtk.SelectionModelAdapter.GType.IsInstance(model.Handle));

                // The control: a GLib.Object that implements neither interface, so
                // an IsInstance that answered yes to everything would be caught.
                var plain = new Gtk.StringObject("x");
                Assert.False(Gtk.SelectionModelAdapter.GType.IsInstance(plain.Handle));
                Assert.False(GLib.ListModelAdapter.GType.IsInstance(plain.Handle));
            });
        }

        [Fact]
        public void The_order_the_interfaces_are_declared_in_does_not_change_the_result()
        {
            Run(() =>
            {
                // Same members, opposite declaration order. GLib refuses to add an
                // interface before its prerequisite, so registering them in
                // reflection order is only correct by luck.
                var model = new SelectionFirstModel();

                Assert.True(GLib.ListModelAdapter.GType.IsInstance(model.Handle));
                Assert.True(Gtk.SelectionModelAdapter.GType.IsInstance(model.Handle));

                var selection = new Gtk.SelectionModelAdapter(model);
                Assert.True(selection.IsSelected(3));
                Assert.False(selection.IsSelected(2));
                Assert.Equal(new uint[] { 1, 3, 4 }, Members(selection.Selection));
            });
        }

        // ------------------------------------------------ Gtk asking the model

        [Fact]
        public void Gtk_answers_is_selected_out_of_the_managed_set()
        {
            Run(() =>
            {
                var model = Sample();
                var selection = new Gtk.SelectionModelAdapter(model);

                // gtk_selection_model_is_selected is C and reaches the answer only
                // through the vtable, so the positive and the negative are both
                // round trips.
                Assert.Equal(new[] { false, true, false, true, true, false, false, false },
                    Enumerable.Range(0, 8).Select(i => selection.IsSelected((uint) i)).ToArray());

                Assert.Equal(8, model.Log.Count(entry => entry.StartsWith("is-selected")));
                Assert.Equal("is-selected 7", model.Log.Last());
            });
        }

        [Fact]
        public void The_whole_selection_is_asked_for_through_both_managed_vtables()
        {
            Run(() =>
            {
                var model = Sample();
                var selection = new Gtk.SelectionModelAdapter(model);

                var whole = selection.Selection;

                Assert.Equal(new uint[] { 1, 3, 4 }, Members(whole));
                Assert.Equal(3ul, whole.Size);
                Assert.Equal(1u, whole.Minimum);
                Assert.Equal(4u, whole.Maximum);

                // The expectation here was originally "0 max", and it was wrong
                // rather than the library: gtk_selection_model_get_selection is
                //   get_selection_in_range (model, 0, g_list_model_get_n_items (model))
                // so the window it asks for is the managed model's own item count,
                // fetched through the GListModel vtable first. One call, one window,
                // and the count in it is this model's eight items.
                Assert.Equal(new[] { "get-selection-in-range 0 8" }, model.Log.ToArray());
            });
        }

        [Fact]
        public void An_empty_selection_comes_back_as_an_empty_bitset()
        {
            Run(() =>
            {
                // The control for the test above: identical path, nothing selected.
                var model = new LetterModel();
                var selection = new Gtk.SelectionModelAdapter(model);

                var whole = selection.Selection;

                Assert.True(whole.IsEmpty);
                Assert.Equal(0ul, whole.Size);
                Assert.Empty(Members(whole));
            });
        }

        [Fact]
        public void A_range_query_is_clipped_to_the_window_that_was_asked_for()
        {
            Run(() =>
            {
                var model = Sample();
                var selection = new Gtk.SelectionModelAdapter(model);

                // [2,5) holds 3 and 4 but not 1; [5,8) holds none of them.
                Assert.Equal(new uint[] { 3, 4 }, Members(selection.GetSelectionInRange(2, 3)));
                Assert.Empty(Members(selection.GetSelectionInRange(5, 3)));
                Assert.Equal(new uint[] { 1 }, Members(selection.GetSelectionInRange(0, 2)));

                Assert.Equal(new[]
                {
                    "get-selection-in-range 2 3",
                    "get-selection-in-range 5 3",
                    "get-selection-in-range 0 2"
                }, model.Log.ToArray());
            });
        }

        // ------------------------------------------------ Gtk changing the model

        [Fact]
        public void The_mutators_reach_the_implementor_with_the_arguments_C_was_given()
        {
            Run(() =>
            {
                var model = new LetterModel();
                var selection = new Gtk.SelectionModelAdapter(model);

                Assert.True(selection.SelectRange(2, 3, false));
                Assert.Equal(new uint[] { 2, 3, 4 }, model.Selected.ToArray());

                Assert.True(selection.UnselectItem(3));
                Assert.Equal(new uint[] { 2, 4 }, model.Selected.ToArray());

                Assert.True(selection.SelectItem(6, true));
                Assert.Equal(new uint[] { 6 }, model.Selected.ToArray());

                Assert.True(selection.SelectAll());
                Assert.Equal(8, model.Selected.Count);

                Assert.True(selection.UnselectRange(0, 6));
                Assert.Equal(new uint[] { 6, 7 }, model.Selected.ToArray());

                Assert.True(selection.UnselectAll());
                Assert.Empty(model.Selected);

                // The booleans are the part a wrong marshalling would lose: a
                // gboolean is four bytes and the delegates declare `bool`.
                Assert.Equal(new[]
                {
                    "select-range 2 3 False",
                    "unselect-item 3",
                    "select-item 6 True",
                    "select-all",
                    "unselect-range 0 6",
                    "unselect-all"
                }, model.Log.ToArray());
            });
        }

        [Fact]
        public void A_model_that_refuses_reports_it_back_through_C()
        {
            Run(() =>
            {
                // The control for the test above. FALSE out of one of these vfuncs
                // is how a selection model says it does not support the operation,
                // and it has to survive the trip back through gboolean.
                var model = Sample();
                model.ReadOnly = true;
                var selection = new Gtk.SelectionModelAdapter(model);

                Assert.False(selection.SelectAll());
                Assert.False(selection.UnselectAll());
                Assert.False(selection.SelectItem(0, false));
                Assert.False(selection.UnselectRange(1, 4));

                Assert.Equal(new uint[] { 1, 3, 4 }, model.Selected.ToArray());
            });
        }

        [Fact]
        public void Set_selection_hands_the_implementor_both_bitsets_intact()
        {
            Run(() =>
            {
                var model = Sample();
                var selection = new Gtk.SelectionModelAdapter(model);

                var selected = new Gtk.Bitset();
                selected.Add(0);
                selected.Add(1);
                selected.Add(2);

                var mask = new Gtk.Bitset();
                mask.Add(1);
                mask.Add(2);
                mask.Add(5);

                Assert.True(selection.SetSelection(selected, mask));

                // gtk_selection_model_set_selection is defined as
                //   new = (old & ~mask) | (selected & mask)
                // which for old={1,3,4} is ({3,4}) | ({1,2}) = {1,2,3,4}.
                Assert.Equal(new uint[] { 1, 2, 3, 4 }, model.Selected.ToArray());
                Assert.Equal(new uint[] { 1, 2, 3, 4 }, Members(selection.Selection));

                // Both arguments crossed the boundary as themselves, not as the
                // same pointer twice and not as a copy of one of them.
                Assert.Equal("set-selection selected={0,1,2} mask={1,2,5}", model.Log[0]);
            });
        }

        // ------------------------------------------------ the signal

        [Fact]
        public void Selection_changed_names_the_span_that_moved()
        {
            Run(() =>
            {
                var model = new LetterModel();
                var selection = new Gtk.SelectionModelAdapter(model);

                var spans = new List<(uint Position, uint NItems)>();
                selection.SelectionChanged += (o, args) => spans.Add((args.Position, args.NItems));

                // The signal is declared on GtkSelectionModel, so connecting to it
                // at all requires the interface to be on this instance's type.
                selection.SelectItem(4, false);
                selection.SelectRange(1, 2, false);
                selection.UnselectAll();

                Assert.Equal(new[] { (4u, 1u), (1u, 2u), (1u, 4u) }, spans.ToArray());
            });
        }

        [Fact]
        public void A_change_that_moves_nothing_emits_nothing()
        {
            Run(() =>
            {
                // The control: selecting what is already selected is a no-op, and a
                // model that emitted anyway would be indistinguishable here from one
                // that emits for a real change.
                var model = Sample();
                var selection = new Gtk.SelectionModelAdapter(model);

                int emissions = 0;
                selection.SelectionChanged += (o, args) => emissions++;

                selection.SelectItem(3, false);
                selection.UnselectItem(7);
                Assert.Equal(0, emissions);

                selection.SelectItem(7, false);
                Assert.Equal(1, emissions);
            });
        }

        // ------------------------------------------------ a C consumer

        [Fact]
        public void A_selection_filter_model_lists_exactly_the_selected_items()
        {
            Run(() =>
            {
                var model = Sample();
                var selection = new Gtk.SelectionModelAdapter(model);

                // GtkSelectionFilterModel is written entirely in C. It knows which
                // items to publish only by asking the selection model, and it reads
                // the items themselves through GListModel, so both managed vtables
                // are on the path to this answer.
                var filter = new Gtk.SelectionFilterModel(selection);

                Assert.Equal(3u, filter.NItems);
                Assert.Equal(new[] { "B", "D", "E" }, Texts(filter));

                // The item type came back as GObject, and that is Gtk rather than a
                // defect: gtk_selection_filter_model_get_item_type returns
                // G_TYPE_OBJECT unconditionally and does not forward the wrapped
                // model's. The managed model's own answer is the specific one, and
                // the objects the filter hands back really are GtkStringObjects.
                Assert.Equal(GLib.GType.Object, filter.ItemType);
                Assert.Equal(Gtk.StringObject.GType,
                    ((GLib.IListModel) (Gtk.ISelectionModel) selection).ItemType);
                Assert.All(Enumerable.Range(0, 3),
                    i => Assert.IsType<Gtk.StringObject>(filter.GetObject((uint) i)));
            });
        }

        [Fact]
        public void A_selection_filter_model_over_an_empty_selection_is_empty()
        {
            Run(() =>
            {
                // The control: same eight items, nothing selected.
                var model = new LetterModel();
                var filter = new Gtk.SelectionFilterModel(new Gtk.SelectionModelAdapter(model));

                Assert.Equal(0u, filter.NItems);
                Assert.Empty(Texts(filter));
            });
        }

        [Fact]
        public void The_filter_model_follows_the_selection_changed_signal()
        {
            Run(() =>
            {
                var model = Sample();
                var selection = new Gtk.SelectionModelAdapter(model);
                var filter = new Gtk.SelectionFilterModel(selection);

                Assert.Equal(new[] { "B", "D", "E" }, Texts(filter));

                selection.SelectItem(6, false);
                Assert.Equal(new[] { "B", "D", "E", "G" }, Texts(filter));

                selection.UnselectRange(3, 2);
                Assert.Equal(new[] { "B", "G" }, Texts(filter));

                selection.SelectAll();
                Assert.Equal(new[] { "A", "B", "C", "D", "E", "F", "G", "H" }, Texts(filter));
            });
        }

        [Fact]
        public void Without_the_signal_the_filter_model_never_learns()
        {
            Run(() =>
            {
                // The control for the test above, and the proof that it is the
                // signal doing the work rather than the filter re-reading the model:
                // the same mutation with emission suppressed leaves C holding the
                // old answer while the managed set has already moved.
                var model = Sample();
                model.Silent = true;
                var selection = new Gtk.SelectionModelAdapter(model);
                var filter = new Gtk.SelectionFilterModel(selection);

                Assert.Equal(new[] { "B", "D", "E" }, Texts(filter));

                selection.SelectItem(6, false);

                Assert.Equal(new uint[] { 1, 3, 4, 6 }, model.Selected.ToArray());
                Assert.Equal(new[] { "B", "D", "E" }, Texts(filter));
            });
        }

        [Fact]
        public void The_model_Gtk_hands_back_is_the_managed_object_that_went_in()
        {
            Run(() =>
            {
                var model = Sample();
                var filter = new Gtk.SelectionFilterModel(new Gtk.SelectionModelAdapter(model));

                // Out of C and back into managed: gtk_selection_filter_model_get_model
                // returns a bare GObject*, and SelectionModelAdapter.GetObject has to
                // recognise it as an implementor rather than wrapping the handle in a
                // second adapter that knows nothing.
                var back = filter.Model;

                Assert.Same(model, ((Gtk.SelectionModelAdapter) back).Implementor);
                Assert.True(back.IsSelected(4));
                Assert.False(back.IsSelected(5));
            });
        }

        [Fact]
        public void Gtks_own_selection_model_can_be_built_on_a_managed_list_model()
        {
            Run(() =>
            {
                // The mirror image of the rest of this file: the selection is Gtk's
                // and the items are managed. GtkSingleSelection keeps its own bitset
                // but has to fetch every item through the managed GListModel vtable,
                // which is also where the transfer-full reference in GetItem is paid
                // for -- a missing one leaves SelectedItem pointing at a dead object.
                var model = new LetterModel();
                var single = new Gtk.SingleSelection(new GLib.ListModelAdapter(model));

                Assert.Equal(8u, single.NItems);

                single.SelectItem(3, true);
                Assert.Equal(3u, single.Selected);
                Assert.Equal(new uint[] { 3 }, Members(single.Selection));
                Assert.Equal("D", ((Gtk.StringObject) single.GetObject(3)).String);
                Assert.True(single.IsSelected(3));
                Assert.False(single.IsSelected(4));

                // And the managed model never saw a selection call: this selection
                // belongs to Gtk, not to the model underneath it.
                Assert.DoesNotContain(model.Log, entry => entry.StartsWith("is-selected"));
                Assert.Contains(model.Log, entry => entry.StartsWith("get-item"));
            });
        }

        // ------------------------------------------------ the adapter as a list model

        [Fact]
        public void A_selection_model_adapter_is_also_a_list_model()
        {
            Run(() =>
            {
                var model = Sample();
                var selection = new Gtk.SelectionModelAdapter(model);

                // GtkSelectionModel's prerequisite is invisible to the generated
                // interface, so Gtk.ISelectionModel carries GLib.IListModel by hand.
                var asList = (GLib.IListModel) (Gtk.ISelectionModel) selection;

                Assert.Equal(8u, asList.NItems);
                Assert.Equal(Gtk.StringObject.GType, asList.ItemType);
                Assert.Equal("C", ((Gtk.StringObject) asList.GetObject(2)).String);
                Assert.Equal(new[] { "A", "B", "C", "D", "E", "F", "G", "H" }, Texts(asList));

                // Reading an item out is a call into the managed GetItem, once per
                // position, and only through g_list_model_get_item.
                Assert.Equal(9, model.Log.Count(entry => entry.StartsWith("get-item")));
            });
        }

        [Fact]
        public void The_items_changed_signal_of_a_managed_selection_model_reaches_managed_code()
        {
            Run(() =>
            {
                var model = Sample();
                var selection = new Gtk.SelectionModelAdapter(model);
                var asList = (GLib.IListModel) (Gtk.ISelectionModel) selection;

                var changes = new List<(uint Position, uint Removed, uint Added)>();
                asList.ItemsChanged += (o, args) => changes.Add((args.Position, args.Removed, args.Added));

                asList.EmitItemsChanged(3, 2, 1);
                asList.EmitItemsChanged(0, 0, 4);

                Assert.Equal(new[] { (3u, 2u, 1u), (0u, 0u, 4u) }, changes.ToArray());
            });
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The direction nothing had ever taken: a C# class that <em>is</em> a
    /// <c>GtkTreeModel</c>. <c>Gtk.TreeModelAdapter</c> installs fifteen managed
    /// function pointers into the <c>GtkTreeModelIface</c> vtable of a
    /// <c>GLib.Object</c> subclass, so every question Gtk asks of the model --
    /// where the first row is, what the third child of the second row is, what a
    /// row's path is, what value sits in column one -- is answered by code in this
    /// repository running under a C stack frame.
    ///
    /// <para><c>NodeStore</c> is the only implementor in the tree and it is tested
    /// through its own <c>ITreeNode</c> API; the adapter itself, the vtable it
    /// writes, and <c>Gtk.TreeEnumerator</c> had no coverage at all.</para>
    ///
    /// <para>The oracle is a tree the test declares and can walk itself: five files
    /// and folders whose depth-first order, path strings, sibling order, child
    /// counts and parent links are all facts about the literal below rather than
    /// about anything the binding reports. Gtk's own walkers -- gtk_tree_model_foreach,
    /// GtkTreeModelFilter, GtkTreeView's expander logic -- are then pointed at it,
    /// and their answers have to agree.</para>
    /// </summary>
    public class TreeModelImplementorTests : GtkTestBase
    {
        public TreeModelImplementorTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------ the model

        /// <summary>One row of <see cref="FileTreeModel"/>.</summary>
        public sealed class Entry
        {
            public Entry(string name, int size, bool folder)
            {
                Name = name;
                Size = size;
                IsFolder = folder;
            }

            public string Name { get; }
            public int Size { get; }
            public bool IsFolder { get; }
            public Entry Parent { get; internal set; }
            public int Index { get; internal set; }
            public List<Entry> Children { get; } = new List<Entry>();

            public Entry Add(Entry child)
            {
                child.Parent = this;
                child.Index = Children.Count;
                Children.Add(child);
                return child;
            }
        }

        /// <summary>
        /// A GtkTreeModel written in C#. Iterators carry the model's stamp and a
        /// one-based index into a registry that never shrinks, which is what makes
        /// <c>ItersPersist</c> an honest claim.
        /// </summary>
        public sealed class FileTreeModel : GLib.Object, ITreeModelImplementor
        {
            private readonly List<Entry> _roots = new List<Entry>();
            private readonly List<Entry> _registry = new List<Entry>();
            private readonly int _stamp;
            private readonly bool _listOnly;

            public FileTreeModel(bool listOnly = false)
            {
                _listOnly = listOnly;
                // Any non-zero value will do; it only has to differ from the zero
                // stamp of Gtk.TreeIter.Zero so a stale iter is detectable.
                _stamp = 0x5EED;
            }

            public int RefNodeCalls { get; private set; }

            public Entry AddRoot(Entry entry)
            {
                entry.Index = _roots.Count;
                _roots.Add(entry);
                return entry;
            }

            /// <summary>Registers every entry reachable from the roots. Called once
            /// the tree is fully built, so that ids are stable afterwards.</summary>
            public void Freeze()
            {
                _registry.Clear();
                foreach (var root in _roots)
                    Register(root);
            }

            private void Register(Entry entry)
            {
                _registry.Add(entry);
                foreach (var child in entry.Children)
                    Register(child);
            }

            private TreeIter IterFor(Entry entry)
            {
                int id = _registry.IndexOf(entry);
                if (id < 0)
                    throw new InvalidOperationException("entry is not in the registry; call Freeze()");
                return new TreeIter { Stamp = _stamp, UserData = (IntPtr) (id + 1) };
            }

            /// <summary>The entry an iter names, or null for the invisible root.</summary>
            private Entry EntryFor(TreeIter iter)
            {
                if (iter.UserData == IntPtr.Zero)
                    return null;
                if (iter.Stamp != _stamp)
                    throw new InvalidOperationException("iter belongs to another model");
                return _registry[iter.UserData.ToInt32() - 1];
            }

            private IList<Entry> SiblingsOf(Entry entry)
            {
                return entry.Parent == null ? (IList<Entry>) _roots : entry.Parent.Children;
            }

            // -------- ITreeModelImplementor

            public TreeModelFlags Flags
            {
                get
                {
                    var flags = TreeModelFlags.ItersPersist;
                    if (_listOnly)
                        flags |= TreeModelFlags.ListOnly;
                    return flags;
                }
            }

            public int NColumns => 3;

            public GLib.GType GetColumnType(int index_)
            {
                switch (index_)
                {
                    case 0: return GLib.GType.String;
                    case 1: return GLib.GType.Int;
                    case 2: return GLib.GType.Boolean;
                    default: throw new ArgumentOutOfRangeException(nameof(index_));
                }
            }

            public bool GetIter(out TreeIter iter, TreePath path)
            {
                iter = TreeIter.Zero;
                if (path == null || path.Depth == 0)
                    return false;

                IList<Entry> level = _roots;
                Entry entry = null;
                foreach (int index in path.Indices)
                {
                    if (index < 0 || index >= level.Count)
                        return false;
                    entry = level[index];
                    level = entry.Children;
                }

                iter = IterFor(entry);
                return true;
            }

            public TreePath GetPath(TreeIter iter)
            {
                var path = new TreePath();
                for (var entry = EntryFor(iter); entry != null; entry = entry.Parent)
                    path.PrependIndex(entry.Index);
                return path;
            }

            public void GetValue(TreeIter iter, int column, ref GLib.Value value)
            {
                var entry = EntryFor(iter);
                value.Init(GetColumnType(column));
                switch (column)
                {
                    case 0: value.Val = entry.Name; break;
                    case 1: value.Val = entry.Size; break;
                    case 2: value.Val = entry.IsFolder; break;
                }
            }

            public bool IterNext(ref TreeIter iter)
            {
                var entry = EntryFor(iter);
                var siblings = SiblingsOf(entry);
                if (entry.Index + 1 >= siblings.Count)
                    return false;
                iter = IterFor(siblings[entry.Index + 1]);
                return true;
            }

            public bool IterPrevious(ref TreeIter iter)
            {
                var entry = EntryFor(iter);
                if (entry.Index == 0)
                    return false;
                iter = IterFor(SiblingsOf(entry)[entry.Index - 1]);
                return true;
            }

            public bool IterChildren(out TreeIter iter, TreeIter parent)
            {
                return IterNthChild(out iter, parent, 0);
            }

            public bool IterHasChild(TreeIter iter) => IterNChildren(iter) > 0;

            public int IterNChildren(TreeIter iter)
            {
                var entry = EntryFor(iter);
                return entry == null ? _roots.Count : entry.Children.Count;
            }

            public bool IterNthChild(out TreeIter iter, TreeIter parent, int n)
            {
                iter = TreeIter.Zero;
                var entry = EntryFor(parent);
                var level = entry == null ? (IList<Entry>) _roots : entry.Children;
                if (n < 0 || n >= level.Count)
                    return false;
                iter = IterFor(level[n]);
                return true;
            }

            public bool IterParent(out TreeIter iter, TreeIter child)
            {
                iter = TreeIter.Zero;
                var parent = EntryFor(child).Parent;
                if (parent == null)
                    return false;
                iter = IterFor(parent);
                return true;
            }

            public void RefNode(TreeIter iter) => RefNodeCalls++;

            public void UnrefNode(TreeIter iter) { }
        }

        /// <summary>
        /// docs/            0
        ///   readme.md      0:0
        ///   notes/         0:1
        ///     todo.txt     0:1:0
        /// src/             1     (a folder with nothing in it)
        /// LICENSE          2
        /// </summary>
        private static FileTreeModel SampleTree()
        {
            var model = new FileTreeModel();

            var docs = model.AddRoot(new Entry("docs", 0, true));
            docs.Add(new Entry("readme.md", 12, false));
            var notes = docs.Add(new Entry("notes", 0, true));
            notes.Add(new Entry("todo.txt", 34, false));

            model.AddRoot(new Entry("src", 0, true));
            model.AddRoot(new Entry("LICENSE", 56, false));

            model.Freeze();
            return model;
        }

        /// <summary>The whole tree in the order a depth-first walk must produce.</summary>
        private static readonly string[] PreOrderPaths =
            { "0", "0:0", "0:1", "0:1:0", "1", "2" };

        private static readonly string[] PreOrderNames =
            { "docs", "readme.md", "notes", "todo.txt", "src", "LICENSE" };

        private static List<string> Walk(ITreeModel model, Func<TreePath, TreeIter, bool> stop = null)
        {
            var seen = new List<string>();
            model.Foreach((m, path, iter) =>
            {
                seen.Add(path + "=" + m.GetValue(iter, 0));
                return stop != null && stop(path, iter);
            });
            return seen;
        }

        private static string[] Expected(IEnumerable<int> rows)
        {
            return rows.Select(i => PreOrderPaths[i] + "=" + PreOrderNames[i]).ToArray();
        }

        // ------------------------------------------------ Gtk walking the model

        [Fact]
        public void Gtk_walks_a_managed_model_depth_first()
        {
            Run(() =>
            {
                var model = new TreeModelAdapter(SampleTree());

                // gtk_tree_model_foreach is C: it reaches every row here only by
                // calling GetIterFirst, IterChildren, IterHasChild, IterNext and
                // GetPath on the managed implementor.
                Assert.Equal(Expected(new[] { 0, 1, 2, 3, 4, 5 }), Walk(model));
            });
        }

        [Fact]
        public void An_empty_managed_model_is_walked_and_nothing_is_visited()
        {
            Run(() =>
            {
                // The control for the walk above: identical code path, no rows.
                var empty = new FileTreeModel();
                empty.Freeze();
                var model = new TreeModelAdapter(empty);

                Assert.Empty(Walk(model));
                Assert.False(model.GetIterFirst(out _));
                Assert.Equal(0, model.IterNChildren());
            });
        }

        [Fact]
        public void A_foreach_that_returns_true_stops_the_walk_where_it_said()
        {
            Run(() =>
            {
                var model = new TreeModelAdapter(SampleTree());

                // Stopping at "notes" must leave todo.txt, src and LICENSE unseen,
                // even though todo.txt is the very next row a walk would reach.
                var seen = Walk(model, (path, iter) => (string) model.GetValue(iter, 0) == "notes");

                Assert.Equal(Expected(new[] { 0, 1, 2 }), seen);
            });
        }

        [Fact]
        public void A_path_string_and_an_iterator_convert_into_each_other()
        {
            Run(() =>
            {
                var model = new TreeModelAdapter(SampleTree());

                Assert.True(model.GetIterFromString(out var iter, "0:1:0"));
                Assert.Equal("todo.txt", model.GetValue(iter, 0));
                Assert.Equal(34, model.GetValue(iter, 1));
                Assert.False((bool) model.GetValue(iter, 2));

                // Back the other way, through the model's own GetPath.
                Assert.Equal("0:1:0", model.GetStringFromIter(iter));
                Assert.Equal(new[] { 0, 1, 0 }, model.GetPath(iter).Indices);

                // A path that names no row: the third root's second child.
                Assert.False(model.GetIterFromString(out _, "2:1"));
                Assert.False(model.GetIter(out _, new TreePath("9")));
            });
        }

        [Fact]
        public void Child_counts_and_the_has_child_flag_come_from_the_model()
        {
            Run(() =>
            {
                var model = new TreeModelAdapter(SampleTree());

                Assert.Equal(3, model.IterNChildren());

                Assert.True(model.GetIterFromString(out var docs, "0"));
                Assert.Equal(2, model.IterNChildren(docs));
                Assert.True(model.IterHasChild(docs));

                // Two controls: an empty folder and a file. Both must answer no,
                // and for different reasons in the model's own code.
                Assert.True(model.GetIterFromString(out var src, "1"));
                Assert.Equal(0, model.IterNChildren(src));
                Assert.False(model.IterHasChild(src));

                Assert.True(model.GetIterFromString(out var license, "2"));
                Assert.False(model.IterHasChild(license));

                // The parentless overload is the hand-written one: it passes NULL
                // rather than a zeroed iter, so it asks about the roots.
                Assert.True(model.IterChildren(out var firstRoot));
                Assert.Equal("docs", model.GetValue(firstRoot, 0));
            });
        }

        [Fact]
        public void Stepping_forward_and_back_through_siblings_gives_one_list_reversed()
        {
            Run(() =>
            {
                var model = new TreeModelAdapter(SampleTree());

                var forward = new List<string>();
                Assert.True(model.IterChildren(out var iter));
                do
                {
                    forward.Add((string) model.GetValue(iter, 0));
                } while (model.IterNext(ref iter));

                Assert.Equal(new[] { "docs", "src", "LICENSE" }, forward);

                // iter now sits on the last row and IterNext has refused to move,
                // so walking back has to retrace exactly the same three rows.
                var backward = new List<string>();
                do
                {
                    backward.Add((string) model.GetValue(iter, 0));
                } while (model.IterPrevious(ref iter));

                Assert.Equal(forward.AsEnumerable().Reverse(), backward);
            });
        }

        [Fact]
        public void The_nth_child_is_the_row_the_nth_step_of_IterNext_reaches()
        {
            Run(() =>
            {
                var model = new TreeModelAdapter(SampleTree());
                Assert.True(model.GetIterFromString(out var docs, "0"));

                // Two independent routes into "docs": index it, and walk it.
                var indexed = new List<string>();
                for (int n = 0; model.IterNthChild(out var child, docs, n); n++)
                    indexed.Add((string) model.GetValue(child, 0));

                var walked = new List<string>();
                Assert.True(model.IterChildren(out var iter, docs));
                do
                {
                    walked.Add((string) model.GetValue(iter, 0));
                } while (model.IterNext(ref iter));

                Assert.Equal(new[] { "readme.md", "notes" }, indexed);
                Assert.Equal(indexed, walked);

                // The parentless nth-child overload indexes the roots instead.
                Assert.True(model.IterNthChild(out var thirdRoot, 2));
                Assert.Equal("LICENSE", model.GetValue(thirdRoot, 0));
                Assert.False(model.IterNthChild(out _, 3));
            });
        }

        [Fact]
        public void A_row_walks_back_up_to_the_root_and_the_root_has_no_parent()
        {
            Run(() =>
            {
                var model = new TreeModelAdapter(SampleTree());
                Assert.True(model.GetIterFromString(out var iter, "0:1:0"));

                var upwards = new List<string>();
                while (model.IterParent(out var parent, iter))
                {
                    upwards.Add((string) model.GetValue(parent, 0));
                    iter = parent;
                }

                Assert.Equal(new[] { "notes", "docs" }, upwards);
                // The loop ended because "docs" is a root, not because it ran out
                // of rows: the model still answers about it.
                Assert.Equal("docs", model.GetValue(iter, 0));
            });
        }

        [Fact]
        public void The_column_types_and_the_model_flags_are_the_managed_ones()
        {
            Run(() =>
            {
                var tree = new TreeModelAdapter(SampleTree());

                Assert.Equal(3, tree.NColumns);
                Assert.Equal(GLib.GType.String, tree.GetColumnType(0));
                Assert.Equal(GLib.GType.Int, tree.GetColumnType(1));
                Assert.Equal(GLib.GType.Boolean, tree.GetColumnType(2));

                // Flags go out through gtk_tree_model_get_flags and come back from
                // the managed getter, so the ListOnly bit separates the two models.
                Assert.Equal(TreeModelFlags.ItersPersist, tree.Flags);

                var flat = new FileTreeModel(listOnly: true);
                flat.AddRoot(new Entry("only", 1, false));
                flat.Freeze();

                Assert.Equal(TreeModelFlags.ItersPersist | TreeModelFlags.ListOnly,
                             new TreeModelAdapter(flat).Flags);
            });
        }

        // ------------------------------------- Gtk's own consumers of the model

        [Fact]
        public void A_filter_over_a_managed_model_keeps_only_the_rows_it_passes()
        {
            Run(() =>
            {
                var model = new TreeModelAdapter(SampleTree());

                // GtkTreeModelFilter is C code that has to build a parallel tree by
                // interrogating the managed model. Hiding the files leaves docs and
                // src, and "notes" survives only because its own parent survived.
                var filtered = (TreeModelFilter) model.FilterNew(null);
                filtered.VisibleFunc = (m, iter) => (bool) m.GetValue(iter, 2);

                Assert.Equal(new[] { "0=docs", "0:0=notes", "1=src" }, Walk(filtered));

                // The control: a predicate that admits nothing must leave nothing,
                // including the folders that were visible a moment ago.
                var nothing = (TreeModelFilter) model.FilterNew(null);
                nothing.VisibleFunc = (m, iter) => false;
                Assert.Empty(Walk(nothing));
            });
        }

        [Fact]
        public void A_tree_view_expands_exactly_the_rows_the_model_says_have_children()
        {
            Run(() =>
            {
                var model = SampleTree();
                var view = new TreeView { Model = new TreeModelAdapter(model) };
                view.AppendColumn("Name", new CellRendererText(), "text", 0);

                view.ExpandAll();

                var open = new List<string>();
                view.MapExpandedRows((v, path) => open.Add(path.ToString()));

                // GtkTreeView decided this on its own, by asking the managed model
                // which rows have children: docs and notes do, src is an empty
                // folder and LICENSE is a file, so neither of those can open.
                Assert.Equal(new[] { "0", "0:1" }, open);

                view.CollapseAll();
                open.Clear();
                view.MapExpandedRows((v, path) => open.Add(path.ToString()));
                Assert.Empty(open);

                // Reaching those rows means Gtk took references to them through
                // the interface, which is the vfunc pair a model may ignore but
                // must still be called on.
                Assert.True(model.RefNodeCalls > 0);
            });
        }

        // -------------------------------------------------- rows-reordered

        [Fact]
        public void A_reorder_reaches_a_handler_connected_to_the_adapter()
        {
            Run(() =>
            {
                var model = new TreeModelAdapter(SampleTree());
                Assert.True(model.GetIterFromString(out var docs, "0"));

                TreePath seenPath = null;
                int[] seenOrder = null;
                RowsReorderedHandler handler = (o, args) =>
                {
                    seenPath = args.Path;
                    seenOrder = args.NewChildOrder;
                };
                model.RowsReordered += handler;

                // Swap readme.md and notes: new_order[newpos] = oldpos.
                model.EmitRowsReordered(new TreePath("0"), docs, new[] { 1, 0 });

                // Before the sender cast in TreeModelAdapter was fixed, this line
                // was never reached: the callback cast the emitter to
                // TreeModelFilter, got null, threw, and the handler for that
                // called Environment.Exit. The whole test host went down here.
                Assert.NotNull(seenOrder);
                Assert.Equal(new[] { 1, 0 }, seenOrder);
                Assert.Equal("0", seenPath.ToString());

                // The control: a disconnected handler must not see the next one.
                model.RowsReordered -= handler;
                seenOrder = null;
                model.EmitRowsReordered(new TreePath("0"), docs, new[] { 0, 1 });
                Assert.Null(seenOrder);
            });
        }

        [Fact]
        public void The_with_length_reorder_carries_the_whole_permutation()
        {
            Run(() =>
            {
                var tree = new FileTreeModel();
                var root = tree.AddRoot(new Entry("root", 0, true));
                root.Add(new Entry("a", 1, false));
                root.Add(new Entry("b", 2, false));
                root.Add(new Entry("c", 3, false));
                tree.Freeze();

                var model = new TreeModelAdapter(tree);
                Assert.True(model.GetIterFromString(out var iter, "0"));

                int[] seen = null;
                model.RowsReordered += (o, args) => seen = args.NewChildOrder;

                // gtk_tree_model_rows_reordered_with_length takes the array and its
                // length as two arguments. Bound as a scalar `out int`, it could
                // not express a permutation at all: it passed the address of one
                // uninitialised stack slot and told GTK to read three integers
                // from it.
                model.RowsReorderedWithLength(new TreePath("0"), iter, new[] { 2, 0, 1 });

                Assert.Equal(new[] { 2, 0, 1 }, seen);
            });
        }

        // ------------------------------------------------ Gtk.TreeEnumerator

        private static ListStore Store(params string[] names)
        {
            var store = new ListStore(typeof(string), typeof(int));
            foreach (var name in names)
                store.AppendValues(name, name.Length);
            return store;
        }

        private static List<string> Rows(IEnumerable source)
        {
            var rows = new List<string>();
            foreach (object[] row in source)
                rows.Add(row[0] + ":" + row[1]);
            return rows;
        }

        [Fact]
        public void Enumerating_a_list_store_yields_every_column_of_every_row_in_order()
        {
            Run(() =>
            {
                // TreeEnumerator is what `foreach` over a ListStore runs. It reads
                // NColumns and walks with GetIterFirst/IterNext, so the row arrays
                // have to match what was appended, in the order it was appended.
                Assert.Equal(new[] { "alpha:5", "bee:3", "gamma:5" },
                             Rows(Store("alpha", "bee", "gamma")));

                // The control: nothing to walk, so MoveNext must fail immediately
                // rather than hand back a row built from an invalid iter.
                Assert.Empty(Rows(Store()));
            });
        }

        [Fact]
        public void An_enumerator_refuses_to_be_read_before_it_is_started()
        {
            Run(() =>
            {
                var enumerator = Store("alpha", "bee").GetEnumerator();

                Assert.Throws<InvalidOperationException>(() => enumerator.Current);

                Assert.True(enumerator.MoveNext());
                Assert.Equal("alpha", ((object[]) enumerator.Current)[0]);

                // Reset puts it back before the first row, so Current is again a
                // question the enumerator cannot answer.
                enumerator.Reset();
                Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            });
        }

        [Fact]
        public void Changing_the_store_mid_walk_invalidates_the_enumerator()
        {
            Run(() =>
            {
                var store = Store("alpha", "bee");
                var enumerator = store.GetEnumerator();

                Assert.True(enumerator.MoveNext());

                // The enumerator listens for row-inserted, row-deleted, row-changed
                // and rows-reordered on the model, which is the only way it can know.
                store.AppendValues("gamma", 5);

                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());

                // Reset is the documented way out, and the walk then sees the row
                // that was added.
                enumerator.Reset();
                var names = new List<string>();
                while (enumerator.MoveNext())
                    names.Add((string) ((object[]) enumerator.Current)[0]);

                Assert.Equal(new[] { "alpha", "bee", "gamma" }, names);
            });
        }

        [Fact]
        public void Writing_a_cell_is_a_change_the_enumerator_notices_too()
        {
            Run(() =>
            {
                var store = Store("alpha", "bee");
                Assert.True(store.GetIterFirst(out var first));

                var untouched = store.GetEnumerator();
                Assert.True(untouched.MoveNext());

                // A row-changed emission counts: the row the enumerator is standing
                // on may be the one that moved out from under it.
                store.SetValue(first, 0, "rewritten");

                Assert.Throws<InvalidOperationException>(() => untouched.MoveNext());

                // A fresh enumerator over the same store is unaffected and reads
                // the new value, so the exception was about the walk and not about
                // the store having been damaged.
                Assert.Equal(new[] { "rewritten:5", "bee:3" }, Rows(store));
            });
        }
    }
}

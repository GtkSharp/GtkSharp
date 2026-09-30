using System;
using System.Collections;
using System.Linq;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>NodeStore</c> is 384 hand-written lines that nothing had ever reached. It
    /// is not a binding of anything: it is a managed <c>GtkTreeModel</c>
    /// implementation, so every column read, iterator step and change notification
    /// is code in this repository that Gtk calls back into. That makes it both the
    /// largest untested file and the one where a defect is most likely to be ours
    /// rather than inherited.
    /// </summary>
    public class NodeStoreTests : GtkTestBase
    {
        public NodeStoreTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>A node whose two columns come from properties rather than a
        /// column-type array, which is the whole point of NodeStore.</summary>
        [TreeNode(ListOnly = true)]
        public class Person : TreeNode
        {
            private string name;
            private int age;

            public Person(string name, int age)
            {
                this.name = name;
                this.age = age;
            }

            [TreeNodeValue(Column = 0)]
            public string Name
            {
                get => name;
                set { name = value; OnChanged(); }
            }

            [TreeNodeValue(Column = 1)]
            public int Age
            {
                get => age;
                set { age = value; OnChanged(); }
            }
        }

        /// <summary>A node that may hold children, for the tree cases.</summary>
        [TreeNode]
        public class Folder : TreeNode
        {
            public Folder(string label) => Label = label;

            [TreeNodeValue(Column = 0)]
            public string Label { get; set; }
        }

        private static NodeStore PeopleStore(params (string name, int age)[] people)
        {
            var store = new NodeStore(typeof(Person));
            foreach (var (name, age) in people)
                store.AddNode(new Person(name, age));
            return store;
        }

        [Fact]
        public void A_node_store_derives_its_columns_from_the_node_type()
        {
            Run(() =>
            {
                var store = PeopleStore();

                // The column types come from the [TreeNodeValue] properties, so
                // this is asserting that the attribute scan found both of them
                // and mapped their CLR types to the right GTypes.
                Assert.Equal(2, ModelOf(store).NColumns);
                Assert.Equal(GLib.GType.String, ModelOf(store).GetColumnType(0));
                Assert.Equal(GLib.GType.Int, ModelOf(store).GetColumnType(1));
            });
        }

        [Fact]
        public void A_node_store_reads_column_values_off_the_node_properties()
        {
            Run(() =>
            {
                var store = PeopleStore(("Ada", 36), ("Grace", 45));
                var model = ModelOf(store);

                Assert.True(model.GetIterFirst(out var iter));
                Assert.Equal("Ada", model.GetValue(iter, 0));
                Assert.Equal(36, model.GetValue(iter, 1));

                Assert.True(model.IterNext(ref iter));
                Assert.Equal("Grace", model.GetValue(iter, 0));
                Assert.Equal(45, model.GetValue(iter, 1));

                Assert.False(model.IterNext(ref iter));
            });
        }

        [Fact]
        public void Changing_a_property_changes_what_the_model_reports()
        {
            // The value is read through the property on each access rather than
            // copied in at AddNode time -- which is what distinguishes NodeStore
            // from ListStore and the reason it exists.
            Run(() =>
            {
                var ada = new Person("Ada", 36);
                var store = new NodeStore(typeof(Person));
                store.AddNode(ada);

                var model = ModelOf(store);
                model.GetIterFirst(out var iter);

                ada.Age = 37;

                Assert.Equal(37, model.GetValue(iter, 1));
            });
        }

        [Fact]
        public void Changing_a_property_raises_RowChanged_for_that_row()
        {
            Run(() =>
            {
                var store = PeopleStore(("Ada", 36), ("Grace", 45));
                var grace = store.Cast<Person>().Single(p => p.Name == "Grace");

                string changed = null;
                ModelOf(store).RowChanged += (o, args) => changed = args.Path.ToString();

                grace.Age = 46;

                Assert.Equal("1", changed);
            });
        }

        [Fact]
        public void Adding_a_node_raises_RowInserted_with_its_path()
        {
            Run(() =>
            {
                var store = PeopleStore(("Ada", 36));

                string inserted = null;
                ModelOf(store).RowInserted += (o, args) => inserted = args.Path.ToString();

                store.AddNode(new Person("Grace", 45));

                Assert.Equal("1", inserted);
            });
        }

        [Fact]
        public void Adding_at_a_position_puts_the_node_there()
        {
            Run(() =>
            {
                var store = PeopleStore(("first", 1), ("third", 3));

                store.AddNode(new Person("second", 2), 1);

                Assert.Equal(new[] { "first", "second", "third" }, Names(store));
            });
        }

        [Fact]
        public void Removing_a_node_takes_it_out_and_raises_RowDeleted()
        {
            Run(() =>
            {
                var store = PeopleStore(("keep", 1), ("drop", 2));
                var doomed = store.Cast<Person>().Single(p => p.Name == "drop");

                string deleted = null;
                ModelOf(store).RowDeleted += (o, args) => deleted = args.Path.ToString();

                store.RemoveNode(doomed);

                Assert.Equal("1", deleted);
                Assert.Equal(new[] { "keep" }, Names(store));
            });
        }

        [Fact]
        public void Clearing_a_store_empties_it()
        {
            Run(() =>
            {
                var store = PeopleStore(("a", 1), ("b", 2));

                store.Clear();

                Assert.Empty(Names(store));
                Assert.False(ModelOf(store).GetIterFirst(out _));
            });
        }

        [Fact]
        public void A_node_can_be_found_again_from_the_path_that_names_it()
        {
            Run(() =>
            {
                var store = PeopleStore(("a", 1), ("b", 2), ("c", 3));

                var node = store.GetNode(new TreePath("1"));

                Assert.Equal("b", ((Person) node).Name);
            });
        }

        [Fact]
        public void A_store_enumerates_the_nodes_that_were_added_to_it()
        {
            Run(() =>
            {
                var store = PeopleStore(("a", 1), ("b", 2));

                var seen = store.Cast<Person>().Select(p => p.Name).ToArray();

                Assert.Equal(new[] { "a", "b" }, seen);
            });
        }

        // ------------------------------------------------------------ tree shape

        [Fact]
        public void A_node_with_children_presents_them_as_a_subtree()
        {
            Run(() =>
            {
                var store = new NodeStore(typeof(Folder));

                var root = new Folder("root");
                root.AddChild(new Folder("child one"));
                root.AddChild(new Folder("child two"));
                store.AddNode(root);

                var model = ModelOf(store);

                Assert.True(model.GetIterFirst(out var rootIter));
                Assert.True(model.IterHasChild(rootIter));
                Assert.Equal(2, model.IterNChildren(rootIter));

                Assert.True(model.IterChildren(out var child, rootIter));
                Assert.Equal("child one", model.GetValue(child, 0));

                Assert.True(model.IterNext(ref child));
                Assert.Equal("child two", model.GetValue(child, 0));
            });
        }

        [Fact]
        public void A_child_added_after_the_node_is_in_the_store_is_reported()
        {
            Run(() =>
            {
                var store = new NodeStore(typeof(Folder));
                var root = new Folder("root");
                store.AddNode(root);

                string inserted = null;
                ModelOf(store).RowInserted += (o, args) => inserted = args.Path.ToString();

                root.AddChild(new Folder("late arrival"));

                Assert.Equal("0:0", inserted);
                Assert.Equal(1, ModelOf(store).IterNChildren(IterFor(store, "0")));
            });
        }

        [Fact]
        public void Removing_a_child_is_reported_and_leaves_the_parent()
        {
            Run(() =>
            {
                var store = new NodeStore(typeof(Folder));
                var root = new Folder("root");
                var doomed = new Folder("doomed");
                root.AddChild(doomed);
                root.AddChild(new Folder("kept"));
                store.AddNode(root);

                string deleted = null;
                ModelOf(store).RowDeleted += (o, args) => deleted = args.Path.ToString();

                root.RemoveChild(doomed);

                Assert.Equal("0:0", deleted);
                Assert.Equal(1, ModelOf(store).IterNChildren(IterFor(store, "0")));
            });
        }

        [Fact]
        public void A_grandchild_path_records_its_depth_and_walks_back_to_its_parent()
        {
            Run(() =>
            {
                var store = new NodeStore(typeof(Folder));

                var root = new Folder("root");
                var child = new Folder("child");
                var grandchild = new Folder("grandchild");
                child.AddChild(grandchild);
                root.AddChild(child);
                store.AddNode(root);

                var model = ModelOf(store);
                var iter = IterFor(store, "0:0:0");

                Assert.Equal("grandchild", model.GetValue(iter, 0));

                Assert.True(model.IterParent(out var parent, iter));
                Assert.Equal("child", model.GetValue(parent, 0));

                Assert.Equal(3, model.GetPath(iter).Depth);
            });
        }

        [Fact]
        public void A_node_knows_its_parent_and_its_children_by_index()
        {
            Run(() =>
            {
                var root = new Folder("root");
                var first = new Folder("first");
                var second = new Folder("second");

                root.AddChild(first);
                root.AddChild(second);

                Assert.Equal(2, root.ChildCount);
                Assert.Same(root, first.Parent);
                Assert.Same(second, root[1]);
                Assert.Equal(1, root.IndexOf(second));
                Assert.Equal(-1, root.IndexOf(new Folder("stranger")));
            });
        }

        [Fact]
        public void A_list_only_node_type_reports_the_list_only_model_flag()
        {
            // ListOnly lets Gtk skip the tree-walking paths, so getting it wrong
            // is a performance and correctness difference the model must declare
            // rather than a cosmetic one.
            Run(() =>
            {
                var list = ModelOf(new NodeStore(typeof(Person)));
                var tree = ModelOf(new NodeStore(typeof(Folder)));

                Assert.True(list.Flags.HasFlag(TreeModelFlags.ListOnly));
                Assert.False(tree.Flags.HasFlag(TreeModelFlags.ListOnly));
            });
        }

        [Fact]
        public void A_node_store_can_back_a_real_tree_view()
        {
            // The point of implementing GtkTreeModel is that Gtk accepts it, so
            // the last assertion is that Gtk does: hand the store to a TreeView
            // and read a value back through the view's own model reference.
            Run(() =>
            {
                var store = PeopleStore(("Ada", 36));

                var view = new NodeView();
                view.NodeStore = store;

                Assert.NotNull(view.Model);
                Assert.True(view.Model.GetIterFirst(out var iter));
                Assert.Equal("Ada", view.Model.GetValue(iter, 0));
            });
        }

        /// <summary>NodeStore does not implement ITreeModel and its adapter is
        /// internal, so the only public way to reach the model it implements is
        /// to hand it to a NodeView. Each store gets one view, kept for the
        /// lifetime of the test so signal subscriptions stay on one model.</summary>
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<NodeStore, NodeView>
            views = new System.Runtime.CompilerServices.ConditionalWeakTable<NodeStore, NodeView>();

        private static ITreeModel ModelOf(NodeStore store)
            => views.GetValue(store, s => new NodeView(s)).Model;

        private static TreeIter IterFor(NodeStore store, string path)
        {
            Assert.True(ModelOf(store).GetIter(out var iter, new TreePath(path)),
                        $"the store should have a row at {path}");
            return iter;
        }

        private static string[] Names(NodeStore store)
        {
            var model = ModelOf(store);
            var names = new System.Collections.Generic.List<string>();

            if (model.GetIterFirst(out var iter))
                do
                    names.Add((string) model.GetValue(iter, 0));
                while (model.IterNext(ref iter));

            return names.ToArray();
        }
    }
}

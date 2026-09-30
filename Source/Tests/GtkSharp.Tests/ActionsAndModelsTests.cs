using System;
using System.Linq;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The two stacks a Gtk 4 application is built on and a Gtk 3 one was not:
    /// <c>GAction</c> and <c>GMenu</c> in place of <c>GtkAction</c> and
    /// <c>GtkUIManager</c>, and the <c>GListModel</c> filter/sort chain in place of
    /// <c>GtkTreeModelFilter</c> and <c>GtkTreeModelSort</c>. Both were introduced
    /// wholesale by the port, so neither has any history of working.
    /// </summary>
    public class ActionsAndModelsTests : GtkTestBase
    {
        public ActionsAndModelsTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------- actions

        [Fact]
        public void Activating_an_action_reaches_its_handler()
        {
            Run(() =>
            {
                var action = new GLib.SimpleAction("save", null);
                var activations = 0;
                action.Activated += (o, args) => activations++;

                action.Activate(null);

                Assert.Equal(1, activations);
                Assert.Equal("save", action.Name);
                Assert.True(action.Enabled);
            });
        }

        [Fact]
        public void A_disabled_action_does_not_reach_its_handler()
        {
            Run(() =>
            {
                var action = new GLib.SimpleAction("save", null);
                var activations = 0;
                action.Activated += (o, args) => activations++;

                action.Enabled = false;

                action.Activate(null);

                Assert.Equal(0, activations);
                Assert.False(action.Enabled);
            });
        }

        [Fact]
        public void An_action_with_a_parameter_receives_the_variant_it_was_given()
        {
            Run(() =>
            {
                var action = new GLib.SimpleAction("open", GLib.VariantType.String);
                string received = null;
                action.Activated += (o, args) => received = (string) args.Parameter;

                action.Activate(new GLib.Variant("/tmp/file.txt"));

                Assert.Equal("/tmp/file.txt", received);
                Assert.Equal("s", action.ParameterType.ToString());
            });
        }

        [Fact]
        public void A_stateful_action_with_no_handler_applies_the_state_itself()
        {
            Run(() =>
            {
                var action = new GLib.SimpleAction("toggle", null, new GLib.Variant(false));

                Assert.False((bool) action.State);

                action.ChangeState(new GLib.Variant(true));

                Assert.True((bool) action.State);
            });
        }

        [Fact]
        public void Handling_StateChanged_means_the_handler_owns_the_transition()
        {
            // StateChanged is the change-state signal, not a notification after
            // the fact. GSimpleAction's default handler is what applies the new
            // state, and connecting replaces it -- so a handler that only looks
            // at the value silently leaves the action on its old state. It is
            // named like an observer and behaves like a veto.
            Run(() =>
            {
                var action = new GLib.SimpleAction("toggle", null, new GLib.Variant(false));
                bool? proposed = null;
                action.StateChanged += (o, args) => proposed = (bool) args.Value;

                action.ChangeState(new GLib.Variant(true));

                Assert.True(proposed);
                Assert.False((bool) action.State);

                // Applying it is the handler's job -- through the property,
                // since g_simple_action_set_state is bound as the setter.
                action.State = new GLib.Variant(true);

                Assert.True((bool) action.State);
            });
        }

        [Fact]
        public void An_action_group_reports_the_actions_added_to_it_and_dispatches_by_name()
        {
            Run(() =>
            {
                var group = new GLib.SimpleActionGroup();
                var saves = 0;

                var save = new GLib.SimpleAction("save", null);
                save.Activated += (o, args) => saves++;
                group.AddAction(save);

                Assert.True(group.HasAction("save"));
                Assert.Contains("save", group.ListActions());

                group.ActivateAction("save", null);

                Assert.Equal(1, saves);
            });
        }

        [Fact]
        public void Removing_an_action_takes_it_out_of_the_group()
        {
            Run(() =>
            {
                var group = new GLib.SimpleActionGroup();
                group.AddAction(new GLib.SimpleAction("doomed", null));
                group.AddAction(new GLib.SimpleAction("kept", null));

                group.RemoveAction("doomed");

                Assert.False(group.HasAction("doomed"));
                Assert.True(group.HasAction("kept"));
            });
        }

        [Fact]
        public void An_action_group_reports_whether_an_action_is_enabled_and_its_state()
        {
            Run(() =>
            {
                var group = new GLib.SimpleActionGroup();
                var toggle = new GLib.SimpleAction("toggle", null, new GLib.Variant(true));
                group.AddAction(toggle);

                Assert.True(group.GetActionEnabled("toggle"));
                Assert.True((bool) group.GetActionState("toggle"));

                group.ChangeActionState("toggle", new GLib.Variant(false));

                Assert.False((bool) group.GetActionState("toggle"));
            });
        }

        [Fact]
        public void A_widget_resolves_an_action_group_inserted_on_an_ancestor()
        {
            // This is how "win.save" reaches a button: the group is inserted
            // under a prefix on an ancestor and looked up by walking the tree.
            Run(() =>
            {
                var window = new Window();
                var box = new Box(Orientation.Vertical, 0);
                var button = new Button();
                box.Append(button);
                window.Child = box;

                var group = new GLib.SimpleActionGroup();
                var saves = 0;
                var save = new GLib.SimpleAction("save", null);
                save.Activated += (o, args) => saves++;
                group.AddAction(save);

                window.InsertActionGroup("win", group);

                button.ActionName = "win.save";
                Assert.True(button.ActivateActionVariant("win.save", null));

                Assert.Equal(1, saves);
            });
        }

        // --------------------------------------------------------------- menus

        [Fact]
        public void A_menu_reports_the_items_appended_to_it()
        {
            Run(() =>
            {
                var menu = new GLib.Menu();

                menu.Append("Save", "win.save");
                menu.Append("Quit", "app.quit");

                Assert.Equal(2, (int) menu.NItems);
                Assert.Equal("Save", ItemAttribute(menu, 0, "label"));
                Assert.Equal("win.save", ItemAttribute(menu, 0, "action"));
                Assert.Equal("Quit", ItemAttribute(menu, 1, "label"));
            });
        }

        [Fact]
        public void Inserting_and_removing_menu_items_changes_what_the_menu_holds()
        {
            Run(() =>
            {
                var menu = new GLib.Menu();
                menu.Append("first", "app.first");
                menu.Append("third", "app.third");

                menu.Insert(1, "second", "app.second");

                Assert.Equal(3, (int) menu.NItems);
                Assert.Equal("second", ItemAttribute(menu, 1, "label"));

                menu.Remove(1);

                Assert.Equal(2, (int) menu.NItems);
                Assert.Equal("third", ItemAttribute(menu, 1, "label"));
            });
        }

        [Fact]
        public void A_submenu_and_a_section_nest_inside_the_parent_menu()
        {
            Run(() =>
            {
                var file = new GLib.Menu();
                file.Append("Open", "win.open");

                var menu = new GLib.Menu();
                menu.AppendSubmenu("File", file);

                var section = new GLib.Menu();
                section.Append("About", "app.about");
                menu.AppendSection(null, section);

                Assert.Equal(2, (int) menu.NItems);
                Assert.Equal("File", ItemAttribute(menu, 0, "label"));

                // The submenu is reachable as a link rather than an attribute.
                var link = menu.GetItemLink(0, "submenu");
                Assert.NotNull(link);
                Assert.Equal(1, (int) link.NItems);
            });
        }

        [Fact]
        public void Changing_a_menu_raises_items_changed_with_the_position_and_counts()
        {
            Run(() =>
            {
                var menu = new GLib.Menu();
                menu.Append("first", "app.first");

                // GMenuModel's items-changed carries signed counts here; GListModel's
                // carries unsigned ones. They are separate signals with separate
                // args classes, which is what the port had to separate them into.
                int position = 0, removed = 0, added = 0;
                menu.MenuItemsChanged += (o, args) =>
                {
                    position = args.Position;
                    removed = args.Removed;
                    added = args.Added;
                };

                menu.Append("second", "app.second");

                Assert.Equal(1, position);
                Assert.Equal(0, removed);
                Assert.Equal(1, added);
            });
        }

        [Fact]
        public void A_menu_item_carries_the_attributes_set_on_it()
        {
            Run(() =>
            {
                var item = new GLib.MenuItem("Label", "app.action");
                item.SetAttributeValue("accel", new GLib.Variant("<Control>s"));

                var menu = new GLib.Menu();
                menu.AppendItem(item);

                Assert.Equal("Label", ItemAttribute(menu, 0, "label"));
                Assert.Equal("<Control>s", ItemAttribute(menu, 0, "accel"));
            });
        }

        // ------------------------------------------------- GListModel pipeline

        [Fact]
        public void A_string_list_holds_the_strings_it_was_built_from()
        {
            // gtk_string_list_new takes "const char* const*" -- a NULL-terminated
            // char**. GirToGapi's type normaliser collapsed that spelling to
            // "const char*", dropping a level of indirection, so this bound as a
            // single string and handed GTK the bytes of that string to read as an
            // array of pointers. 67 parameters and return values across seven
            // assemblies were spelled that way.
            Run(() =>
            {
                var list = new StringList(new[] { "alpha", "bravo", "charlie" });

                Assert.Equal(3u, list.NItems);
                Assert.Equal("alpha", list.GetString(0));
                Assert.Equal("bravo", list.GetString(1));
                Assert.Equal("charlie", list.GetString(2));

                // An empty array must give an empty list rather than one entry.
                Assert.Equal(0u, new StringList(new string[0]).NItems);
            });
        }

        [Fact]
        public void Appending_and_removing_change_the_string_list_and_raise_items_changed()
        {
            Run(() =>
            {
                var list = new StringList(new[] { "one" });

                uint added = 0, removed = 0;
                list.ItemsChanged += (o, args) =>
                {
                    added += args.Added;
                    removed += args.Removed;
                };

                list.Append("two");
                Assert.Equal(2u, list.NItems);

                list.Remove(0);
                Assert.Equal(1u, list.NItems);
                Assert.Equal("two", list.GetString(0));

                Assert.Equal(1u, added);
                Assert.Equal(1u, removed);
            });
        }

        [Fact]
        public void A_filtered_list_model_shows_only_what_the_filter_accepts()
        {
            // The Gtk 4 replacement for TreeModelFilter, and unlike it this one
            // is a GListModel, so the oracle is NItems and GetString.
            Run(() =>
            {
                var source = new StringList(new[] { "keep one", "drop", "keep two" });

                var filter = new CustomFilter(
                    item => ((StringObject) GLib.Object.GetObject(item)).String.StartsWith("keep"));

                var filtered = new FilterListModel(source, filter);

                Assert.Equal(2u, filtered.NItems);
                Assert.Equal("keep one", ((StringObject) Item(filtered, 0)).String);
                Assert.Equal("keep two", ((StringObject) Item(filtered, 1)).String);
            });
        }

        [Fact]
        public void Changing_the_filter_changes_what_the_model_shows()
        {
            Run(() =>
            {
                var source = new StringList(new[] { "a", "bb", "ccc" });

                var filter = new CustomFilter(
                    item => ((StringObject) GLib.Object.GetObject(item)).String.Length >= 2);

                var filtered = new FilterListModel(source, filter);
                Assert.Equal(2u, filtered.NItems);

                filter.FilterFunc = item => ((StringObject) GLib.Object.GetObject(item)).String.Length >= 3;

                Assert.Equal(1u, filtered.NItems);
            });
        }

        [Fact]
        public void A_sorted_list_model_presents_the_items_in_order()
        {
            Run(() =>
            {
                var source = new StringList(new[] { "charlie", "alpha", "bravo" });

                // CustomSorter's constructor is protected -- gtk_custom_sorter_new
                // is hidden in the api.xml -- so sorting goes the way Gtk 4
                // intends, through an expression naming the property to sort on.
                var sorted = new SortListModel(source, StringSorterOnValue());

                Assert.Equal(3u, sorted.NItems);
                Assert.Equal(new[] { "alpha", "bravo", "charlie" }, Strings(sorted));
            });
        }

        [Fact]
        public void A_selection_model_tracks_which_item_is_selected()
        {
            Run(() =>
            {
                var list = new StringList(new[] { "alpha", "bravo", "charlie" });
                var selection = new SingleSelection(list);

                selection.Selected = 1;

                Assert.Equal(1u, selection.Selected);
                Assert.Equal("bravo", ((StringObject) GLib.Object.GetObject(selection.SelectedItem)).String);
                Assert.True(selection.IsSelected(1));
                Assert.False(selection.IsSelected(0));
            });
        }

        [Fact]
        public void Selecting_raises_selection_changed_with_the_range_that_moved()
        {
            Run(() =>
            {
                var selection = new SingleSelection(new StringList(new[] { "a", "b", "c" }));
                selection.Selected = 0;

                var changes = 0;
                selection.SelectionChanged += (o, args) => changes++;

                selection.Selected = 2;

                Assert.True(changes > 0, "moving the selection should be reported");
                Assert.Equal(2u, selection.Selected);
            });
        }

        [Fact]
        public void A_filter_over_a_sorted_model_composes_as_a_pipeline()
        {
            // Chaining is the point of the GListModel design: each stage is a
            // model in its own right, so they stack.
            Run(() =>
            {
                var source = new StringList(new[] { "delta", "alpha", "charlie", "bravo" });

                var sorted = new SortListModel(source, StringSorterOnValue());

                var filter = new CustomFilter(
                    item => ((StringObject) GLib.Object.GetObject(item)).String.Length == 5);
                var filtered = new FilterListModel(sorted, filter);

                // alpha, bravo, delta -- charlie is seven letters.
                Assert.Equal(3u, filtered.NItems);
                Assert.Equal("alpha", ((StringObject) Item(filtered, 0)).String);
                Assert.Equal("delta", ((StringObject) Item(filtered, 2)).String);
            });
        }

        /// <summary>Sorts StringObjects by their string, the Gtk 4 way.</summary>
        private static StringSorter StringSorterOnValue()
            => new StringSorter(new PropertyExpression(StringObject.GType, null, "string"));

        private static string[] Strings(GLib.IListModel model)
            => Enumerable.Range(0, (int) model.NItems)
                         .Select(i => ((StringObject) model.GetObject((uint) i)).String)
                         .ToArray();

        /// <summary>The concrete list models return an IntPtr from GetObject;
        /// the GLib.IListModel interface returns a wrapper, so go through it.</summary>
        private static GLib.Object Item(GLib.IListModel model, uint position)
            => model.GetObject(position);

        private static string ItemAttribute(GLib.Menu menu, int index, string attribute)
        {
            var value = menu.GetItemAttributeValue(index, attribute, GLib.VariantType.String);
            return value == null ? null : (string) value;
        }
    }
}

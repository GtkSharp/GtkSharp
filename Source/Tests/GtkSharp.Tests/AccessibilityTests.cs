using System;
using System.Linq;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// GtkAccessible, which replaced ATK in Gtk 4 and had no coverage at all.
    /// </summary>
    /// <remarks>
    /// The oracle throughout is Gtk's own accessibility test API -
    /// gtk_test_accessible_has_role/state/property/relation - so what is
    /// asserted is what Gtk's AT context ended up holding, not what this binding
    /// remembers. Where a fact has a source outside Gtk it is used instead: an
    /// ARIA role is what a screen reader must be told a widget is, and a
    /// widget's accessible bounds have to agree with the size Gtk laid it out
    /// at.
    /// </remarks>
    public class AccessibilityTests : GtkTestBase
    {
        public AccessibilityTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------ helpers

        private static void Settle(Gtk.Window window)
        {
            window.Present();
            for (int i = 0; i < 400 && Gtk.Application.EventsPending(); i++)
                Gtk.Application.RunIteration(false);
        }

        private static bool Pump(Func<bool> until, int milliseconds = 4000)
        {
            var deadline = System.Diagnostics.Stopwatch.StartNew();
            while (!until() && deadline.ElapsedMilliseconds < milliseconds)
                Gtk.Application.RunIteration(false);
            return until();
        }

        /// <summary>Builds the value a state wants, sets it, and unsets it afterwards.</summary>
        /// <remarks>
        /// GLib.Value is a mutable struct, so it cannot be a `using` variable -
        /// assigning Val to one is a compile error. Every test that sets an
        /// attribute goes through one of these rather than repeating the
        /// try/finally.
        /// </remarks>
        private static void Update(IAccessible accessible, AccessibleState state, object content)
        {
            GLib.Value value = state.InitValue();
            try
            {
                value.Val = content;
                accessible.UpdateState(state, value);
            }
            finally { value.Dispose(); }
        }

        /// <summary>The parallel-array form: one value per attribute, all in one call.</summary>
        private static GLib.Value[] Values<T>(Func<T, GLib.Value> init, T[] attributes, object[] contents)
        {
            var values = new GLib.Value[attributes.Length];
            for (int i = 0; i < attributes.Length; i++)
            {
                values[i] = init(attributes[i]);
                values[i].Val = contents[i];
            }
            return values;
        }

        private static void Dispose(GLib.Value[] values)
        {
            for (int i = 0; i < values.Length; i++)
                values[i].Dispose();
        }

        private static Widget Make(string kind)
        {
            switch (kind)
            {
                case "button": return new Button();
                case "label": return new Label("x");
                case "entry": return new Entry();
                case "search": return new SearchEntry();
                case "check": return new CheckButton();
                case "toggle": return new ToggleButton();
                case "switch": return new Switch();
                case "scale": return new Scale(Orientation.Horizontal, new Adjustment(0, 0, 100, 1, 10, 0));
                case "progress": return new ProgressBar();
                case "spin": return new SpinButton(0, 100, 1);
                case "link": return new LinkButton("http://example.invalid/");
                case "image": return new Image();
                case "listbox": return new ListBox();
                case "columnview": return new ColumnView(null);
                case "gridview": return new GridView(null, null);
                case "separator": return new Separator(Orientation.Horizontal);
                case "box": return new Box(Orientation.Horizontal, 0);
                default: throw new ArgumentException("unknown widget kind " + kind, nameof(kind));
            }
        }

        // ------------------------------------------------------------ roles

        // Two independent readings of the same fact: the property, and Gtk's own
        // has_role. Both go through the GtkAccessibleRole enum, so a member
        // inserted into the middle of it - it has grown four times since 4.0 -
        // shifts every answer here at once, and the ARIA names are the fixed
        // point that catches it.
        [Theory]
        [InlineData("button", AccessibleRole.Button)]
        [InlineData("label", AccessibleRole.Label)]
        [InlineData("entry", AccessibleRole.TextBox)]
        [InlineData("search", AccessibleRole.SearchBox)]
        [InlineData("check", AccessibleRole.Checkbox)]
        [InlineData("toggle", AccessibleRole.ToggleButton)]
        [InlineData("switch", AccessibleRole.Switch)]
        [InlineData("scale", AccessibleRole.Slider)]
        [InlineData("progress", AccessibleRole.ProgressBar)]
        [InlineData("spin", AccessibleRole.SpinButton)]
        [InlineData("link", AccessibleRole.Link)]
        [InlineData("image", AccessibleRole.Img)]
        [InlineData("listbox", AccessibleRole.List)]
        [InlineData("columnview", AccessibleRole.TreeGrid)]
        [InlineData("gridview", AccessibleRole.Grid)]
        [InlineData("separator", AccessibleRole.Separator)]
        [InlineData("box", AccessibleRole.Generic)]
        public void A_widget_reports_the_role_its_class_declares(string kind, AccessibleRole expected)
        {
            Run(() =>
            {
                var widget = Make(kind);

                Assert.Equal(expected, widget.AccessibleRole);
                Assert.True(Gtk.Test.AccessibleHasRole(widget, expected),
                    kind + " should be " + expected + " but Gtk says " + widget.AccessibleRole);
                Assert.False(Gtk.Test.AccessibleHasRole(widget, AccessibleRole.Alert));
            });
        }

        [Fact]
        public void A_widget_can_be_given_a_role_other_than_its_class_role()
        {
            Run(() =>
            {
                var label = new Label("careful");
                Assert.Equal(AccessibleRole.Label, label.AccessibleRole);

                label.AccessibleRole = AccessibleRole.Alert;

                Assert.Equal(AccessibleRole.Alert, label.AccessibleRole);
                Assert.True(Gtk.Test.AccessibleHasRole(label, AccessibleRole.Alert));
                Assert.False(Gtk.Test.AccessibleHasRole(label, AccessibleRole.Label));
            });
        }

        // GTK_ACCESSIBLE_ROLE_WIDGET is the abstract "some widget" role, and
        // assigning it is not an error and not a change: the widget goes on
        // reporting what its class declared. So a caller that meant to clear a
        // role by assigning the base one silently keeps the old one.
        [Fact]
        public void Assigning_the_abstract_widget_role_leaves_the_class_role_in_place()
        {
            Run(() =>
            {
                var label = new Label("x");
                label.AccessibleRole = AccessibleRole.Widget;

                Assert.Equal(AccessibleRole.Label, label.AccessibleRole);
                Assert.True(Gtk.Test.AccessibleHasRole(label, AccessibleRole.Label));
            });
        }

        [Fact]
        public void The_role_named_in_a_ui_file_is_the_role_the_widget_reports()
        {
            Run(() =>
            {
                var builder = new Builder();
                builder.AddFromString(
                    "<interface>" +
                    "  <object class=\"GtkLabel\" id=\"warning\">" +
                    "    <property name=\"accessible-role\">alert</property>" +
                    "    <property name=\"label\">disk full</property>" +
                    "  </object>" +
                    "</interface>");

                var label = (Label)builder.GetObject("warning");

                Assert.Equal("disk full", label.Text);
                Assert.Equal(AccessibleRole.Alert, label.AccessibleRole);
                Assert.True(Gtk.Test.AccessibleHasRole(label, AccessibleRole.Alert));
            });
        }

        // ------------------------------------------------------------ AT context

        // The trap: a widget and its AT context do not answer the same way. The
        // context holds only a role that was assigned; a role that comes from
        // the widget class is applied when the context is realized, which for a
        // widget that was never shown never happens. So GetAtContext().Role is
        // not a shortcut for GetAccessibleRole().
        [SkippableFact]
        public void An_unrealized_context_reports_the_generic_role_until_one_is_assigned()
        {
            Skip.IfNot(TestEnvironment.GtkAtLeast(4, 22),
                       TestEnvironment.NeedsGtk(4, 22, "GtkATContext:realized"));

            Run(() =>
            {
                var label = new Label("x");
                Assert.Equal(AccessibleRole.Label, label.AccessibleRole);
                Assert.False(((ATContext)label.AtContext).Realized);
                Assert.Equal(AccessibleRole.Widget, label.AtContext.AccessibleRole);

                label.AccessibleRole = AccessibleRole.Alert;
                Assert.Equal(AccessibleRole.Alert, label.AtContext.AccessibleRole);
            });
        }

        [Fact]
        public void An_accessible_and_its_context_point_at_each_other()
        {
            Run(() =>
            {
                var button = new Button("ok");

                var context = button.AtContext;
                Assert.NotNull(context);
                // One native object is one managed wrapper, both ways round.
                Assert.Same(context, button.AtContext);
                Assert.Same(button, context.Accessible);
            });
        }

        [SkippableFact]
        public void A_context_built_by_hand_carries_the_role_it_was_built_with()
        {
            Skip.IfNot(TestEnvironment.GtkAtLeast(4, 22),
                       TestEnvironment.NeedsGtk(4, 22, "GtkATContext:realized"));

            Run(() =>
            {
                var button = new Button("ok");
                var context = new ATContext(AccessibleRole.Alert, button, Gdk.Display.Default);

                Assert.Equal(AccessibleRole.Alert, context.AccessibleRole);
                Assert.Same(button, context.Accessible);
                // Realizing is the AT backend's job, and nothing here is one.
                Assert.False(context.Realized);
                // Made beside the widget's own context, not in place of it.
                Assert.NotSame(context, button.AtContext);
            });
        }

        // ------------------------------------------------------------ the tree

        [Fact]
        public void The_accessible_tree_follows_the_widget_tree()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Horizontal, 0);
                var button = new Button("a");
                var label = new Label("b");
                box.Append(button);
                box.Append(label);

                Assert.Same(box, button.AccessibleParent);
                Assert.Same(button, box.FirstAccessibleChild);
                Assert.Same(label, button.NextAccessibleSibling);
                Assert.Null(label.NextAccessibleSibling);
            });
        }

        // A GtkSpinButton's first accessible child is the GtkText inside it -
        // a widget the application never made and cannot reach through the
        // child list. It is also the regression test for the registry
        // bootstrap: GtkText is the one type in this assembly whose managed name
        // the mangler cannot guess, so before Widget's static constructor
        // populated the registry this came back as a bare Gtk.Widget.
        [Fact]
        public void A_composite_widget_exposes_its_parts_rather_than_its_children()
        {
            Run(() =>
            {
                var spin = new SpinButton(0, 100, 1);

                var text = spin.FirstAccessibleChild;
                Assert.IsType<Gtk.TextWidget>(text);
                Assert.Same(spin, text.AccessibleParent);
                // The parts continue past it: the up/down buttons are siblings.
                Assert.IsType<Gtk.Button>(text.NextAccessibleSibling);
            });
        }

        // set_accessible_parent moves a widget in the accessible tree without
        // moving it in the widget tree - and only in one direction. The parent
        // it names reports the children it really has, so the new child is
        // reachable upwards and invisible downwards.
        [Fact]
        public void An_accessible_parent_can_be_assigned_and_cleared()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Horizontal, 0);
                var real = new Button("real");
                box.Append(real);

                var adopted = new Label("adopted");
                adopted.SetAccessibleParent(box, null);

                Assert.Same(box, adopted.AccessibleParent);
                Assert.Same(real, box.FirstAccessibleChild);

                adopted.SetAccessibleParent(null, null);
                Assert.Null(adopted.AccessibleParent);
            });
        }

        // The sibling has to be given to set_accessible_parent. Calling
        // update_next_accessible_sibling on a widget whose accessible parent was
        // never set does nothing at all, and says nothing about it.
        [Fact]
        public void The_sibling_given_when_the_parent_is_set_is_the_one_reported()
        {
            Run(() =>
            {
                var parent = new Label("p");
                var first = new Label("1");
                var second = new Label("2");

                second.SetAccessibleParent(parent, null);
                first.SetAccessibleParent(parent, second);

                Assert.Same(second, first.NextAccessibleSibling);
                Assert.Null(second.NextAccessibleSibling);

                var stray = new Label("stray");
                stray.UpdateNextAccessibleSibling(second);
                Assert.Null(stray.NextAccessibleSibling);
            });
        }

        // ------------------------------------------------------------ platform state

        [Fact]
        public void Focusability_is_a_platform_state_and_follows_the_widget()
        {
            Run(() =>
            {
                var button = new Button("ok");
                var label = new Label("x");

                Assert.True(button.GetPlatformState(AccessiblePlatformState.Focusable));
                Assert.False(label.GetPlatformState(AccessiblePlatformState.Focusable));
                Assert.False(button.GetPlatformState(AccessiblePlatformState.Focused));

                label.Focusable = true;
                Assert.True(label.GetPlatformState(AccessiblePlatformState.Focusable));
            });
        }

        // Bounds exist only once something has laid the widget out, and the
        // oracle is geometry this test arranged: two buttons stacked in a
        // spacing-free vertical box are the same width and the second starts
        // exactly where the first ends. Nothing here compares against an
        // absolute size - a window's outer size includes whatever decoration
        // the platform draws, which is not the same number on Windows and
        // under a plain X server, and gtk_accessible_get_bounds reports the
        // widget's border box rather than gtk_widget_get_width's content box.
        [Fact]
        public void Bounds_appear_once_the_widget_is_laid_out_and_describe_where_it_is()
        {
            Run(() =>
            {
                var top = new Button("top");
                var bottom = new Button("bottom");
                Assert.False(top.GetBounds(out int x0, out int y0, out int w0, out int h0));
                Assert.Equal(0, w0 + h0 + x0 + y0);

                var box = new Box(Orientation.Vertical, 0);
                box.Append(top);
                box.Append(bottom);

                var window = new Gtk.Window { Child = box, DefaultWidth = 220, DefaultHeight = 160 };
                try
                {
                    Settle(window);
                    Assert.True(Pump(() => top.Width > 0), "the buttons were never allocated");

                    Assert.True(top.GetBounds(out int tx, out int ty, out int tw, out int th));
                    Assert.True(bottom.GetBounds(out int bx, out int by, out int bw, out int bh));

                    Assert.True(tw > 0 && th > 0, $"empty bounds {tw}x{th}");
                    Assert.Equal(tw, bw);
                    Assert.Equal(th, bh);
                    Assert.Equal(tx, bx);
                    Assert.Equal(ty + th, by);
                }
                finally
                {
                    window.Destroy();
                }
            });
        }

        [SkippableFact]
        public void The_id_in_a_ui_file_is_the_accessible_id()
        {
            Skip.IfNot(TestEnvironment.GtkAtLeast(4, 22),
                       TestEnvironment.NeedsGtk(4, 22, "GtkATContext:realized"));

            Run(() =>
            {
                var builder = new Builder();
                builder.AddFromString("<interface><object class=\"GtkButton\" id=\"save-button\"/></interface>");

                Assert.Equal("save-button", ((Button)builder.GetObject("save-button")).AccessibleId);
                // A widget nothing named has no id at all - not the empty string
                // an id-shaped getter suggests.
                Assert.Null(new Button().AccessibleId);
            });
        }

        // ------------------------------------------------------------ states

        [Fact]
        public void A_state_set_through_the_binding_is_the_state_Gtk_reports()
        {
            Run(() =>
            {
                var button = new Button("ok");
                Assert.False(Gtk.Test.AccessibleHasState(button, AccessibleState.Busy));

                Update(button, AccessibleState.Busy, true);

                Assert.True(Gtk.Test.AccessibleHasState(button, AccessibleState.Busy));

                button.ResetState(AccessibleState.Busy);
                Assert.False(Gtk.Test.AccessibleHasState(button, AccessibleState.Busy));
            });
        }

        // gtk_accessible_update_state_value takes two parallel arrays behind one
        // count. Nothing in the api.xml can say so, and the generated binding
        // passed an uninitialised stack slot as the list of states, so this -
        // setting two at once and clearing one of them - could not be done at
        // all before the hand-written rebinding.
        [Fact]
        public void Several_states_can_be_set_in_one_call_and_cleared_one_at_a_time()
        {
            Run(() =>
            {
                var button = new Button("ok");

                var states = new[] { AccessibleState.Busy, AccessibleState.Disabled };
                var values = Values(AccessibleExtensions.InitValue, states, new object[] { true, true });
                try { button.UpdateState(states, values); } finally { Dispose(values); }

                Assert.True(Gtk.Test.AccessibleHasState(button, AccessibleState.Busy));
                Assert.True(Gtk.Test.AccessibleHasState(button, AccessibleState.Disabled));

                button.ResetState(AccessibleState.Busy);
                Assert.False(Gtk.Test.AccessibleHasState(button, AccessibleState.Busy));
                Assert.True(Gtk.Test.AccessibleHasState(button, AccessibleState.Disabled));
            });
        }

        // gtk_accessible_state_init_value exists, the documentation says, "mostly
        // for language bindings" - and this binding could not call it, because
        // the gir hangs it off the enum and gapi enums carry no methods. Which
        // type each state wants is not guessable: checked and pressed are
        // tristates, invalid is its own enum, and expanded is an int rather than
        // the boolean its name suggests.
        [Theory]
        [InlineData(AccessibleState.Busy, "gboolean")]
        [InlineData(AccessibleState.Disabled, "gboolean")]
        [InlineData(AccessibleState.Hidden, "gboolean")]
        [InlineData(AccessibleState.Checked, "GtkAccessibleTristate")]
        [InlineData(AccessibleState.Pressed, "GtkAccessibleTristate")]
        [InlineData(AccessibleState.Invalid, "GtkAccessibleInvalidState")]
        [InlineData(AccessibleState.Expanded, "gint")]
        public void A_state_value_is_initialised_to_the_type_that_state_takes(AccessibleState state, string type)
        {
            Run(() =>
            {
                GLib.Value value = state.InitValue();
                try { Assert.Equal(type, value.ValueType.ToString()); } finally { value.Dispose(); }
            });
        }

        [Fact]
        public void A_tristate_state_carries_its_third_value_through_the_binding()
        {
            Run(() =>
            {
                var check = new CheckButton();

                GLib.Value value = AccessibleState.Checked.InitValue();
                try
                {
                    value.Val = AccessibleTristate.Mixed;
                    // The enum survives the round trip through the GValue, which
                    // is what tells a tristate apart from the boolean it is not.
                    Assert.Equal(AccessibleTristate.Mixed, value.Val);
                    check.UpdateState(AccessibleState.Checked, value);
                }
                finally { value.Dispose(); }

                Assert.True(Gtk.Test.AccessibleHasState(check, AccessibleState.Checked));
            });
        }

        // Gtk 4.22 added GTK_ACCESSIBLE_STATE_VISITED without a case in
        // gtk_accessible_state_init_value, so it hands back an untyped GValue -
        // and "v.Val = true" on one of those throws rather than doing nothing.
        // Pinned because the obvious reading, that InitValue always describes
        // the state, is what produces the surprise.
        [Fact]
        public void The_visited_state_has_no_initial_value_and_must_be_built_by_hand()
        {
            Run(() =>
            {
                // Not disposed: g_value_unset on a value that was never
                // initialised is itself an assertion failure.
                GLib.Value uninitialised = AccessibleState.Visited.InitValue();
                Assert.Equal(GLib.GType.Invalid.Val, uninitialised.ValueType.Val);

                var link = new LinkButton("http://example.invalid/");
                GLib.Value boolean = new GLib.Value(true);
                try { link.UpdateState(AccessibleState.Visited, boolean); } finally { boolean.Dispose(); }

                Assert.True(Gtk.Test.AccessibleHasState(link, AccessibleState.Visited));
            });
        }

        // has_state answers "is this attribute present", not "is it true". A
        // check button publishes checked=false from the moment it is built, so
        // the reading that a present state is a set state is wrong for exactly
        // the states an application cares about.
        [Fact]
        public void A_present_state_is_not_a_true_state()
        {
            Run(() =>
            {
                var check = new CheckButton();
                Assert.False(check.Active);
                Assert.True(Gtk.Test.AccessibleHasState(check, AccessibleState.Checked));

                var button = new Button("ok");
                Assert.True(button.Sensitive);
                Assert.False(Gtk.Test.AccessibleHasState(button, AccessibleState.Disabled));
            });
        }

        // Gtk maintains part of the accessible description itself, so an
        // application that never touches the accessibility API still produces
        // some of it. What it does not do is publish the button's own label as
        // the accessible label - that is computed when an AT asks.
        [Fact]
        public void Gtk_keeps_some_accessible_attributes_in_step_with_the_widget()
        {
            Run(() =>
            {
                var button = new Button("press me");
                button.Sensitive = false;
                Assert.True(Gtk.Test.AccessibleHasState(button, AccessibleState.Disabled));
                Assert.False(Gtk.Test.AccessibleHasProperty(button, AccessibleProperty.Label));

                var toggle = new ToggleButton { Active = true };
                Assert.True(Gtk.Test.AccessibleHasState(toggle, AccessibleState.Pressed));

                var expander = new Expander("more") { Expanded = true };
                Assert.True(Gtk.Test.AccessibleHasState(expander, AccessibleState.Expanded));

                var entry = new Entry { PlaceholderText = "name" };
                Assert.True(Gtk.Test.AccessibleHasProperty(entry, AccessibleProperty.Placeholder));

                var scale = new Scale(Orientation.Horizontal, new Adjustment(5, 0, 100, 1, 10, 0));
                Assert.True(Gtk.Test.AccessibleHasProperty(scale, AccessibleProperty.ValueNow));
                Assert.True(Gtk.Test.AccessibleHasProperty(scale, AccessibleProperty.ValueMax));
            });
        }

        // ------------------------------------------------------------ properties

        [Fact]
        public void A_property_set_through_the_binding_is_the_property_Gtk_reports()
        {
            Run(() =>
            {
                var button = new Button();
                Assert.False(Gtk.Test.AccessibleHasProperty(button, AccessibleProperty.Label));

                GLib.Value value = AccessibleProperty.Label.InitValue();
                try
                {
                    Assert.Equal("gchararray", value.ValueType.ToString());
                    value.Val = "close the window";
                    button.UpdateProperty(AccessibleProperty.Label, value);
                }
                finally { value.Dispose(); }

                Assert.True(Gtk.Test.AccessibleHasProperty(button, AccessibleProperty.Label));

                button.ResetProperty(AccessibleProperty.Label);
                Assert.False(Gtk.Test.AccessibleHasProperty(button, AccessibleProperty.Label));
            });
        }

        // Three properties of three different value types in one call: if the
        // two arrays ever slipped against each other, the string would be
        // collected for the int property and none of these would be set.
        [Fact]
        public void Properties_of_different_types_set_together_all_arrive()
        {
            Run(() =>
            {
                var button = new Button();

                var properties = new[] { AccessibleProperty.Label, AccessibleProperty.Description, AccessibleProperty.Level };
                var values = Values(AccessibleExtensions.InitValue, properties,
                                    new object[] { "chapter", "the first one", 3 });
                try { button.UpdateProperty(properties, values); } finally { Dispose(values); }

                Assert.True(Gtk.Test.AccessibleHasProperty(button, AccessibleProperty.Label));
                Assert.True(Gtk.Test.AccessibleHasProperty(button, AccessibleProperty.Description));
                Assert.True(Gtk.Test.AccessibleHasProperty(button, AccessibleProperty.Level));
            });
        }

        // Gtk type-checks the GValue and refuses rather than reinterpreting it.
        // Worth pinning because the failure is silent - no exception, no return
        // value - so the only thing that distinguishes it from success is asking
        // afterwards.
        [Fact]
        public void A_value_of_the_wrong_type_leaves_the_property_unset()
        {
            Run(() =>
            {
                var button = new Button();

                GLib.Value wrong = new GLib.Value(42);
                try { button.UpdateProperty(AccessibleProperty.Label, wrong); } finally { wrong.Dispose(); }

                Assert.False(Gtk.Test.AccessibleHasProperty(button, AccessibleProperty.Label));
            });
        }

        // Gtk reads one value per attribute named, so a short values array is a
        // read past the end of it rather than an error. The binding refuses
        // instead.
        [Fact]
        public void The_attribute_and_value_arrays_must_be_the_same_length()
        {
            Run(() =>
            {
                var button = new Button();

                var values = Values(AccessibleExtensions.InitValue, new[] { AccessibleState.Busy }, new object[] { true });
                try
                {
                    Assert.Throws<ArgumentException>(() =>
                        button.UpdateState(new[] { AccessibleState.Busy, AccessibleState.Disabled }, values));
                }
                finally { Dispose(values); }

                Assert.False(Gtk.Test.AccessibleHasState(button, AccessibleState.Busy));
            });
        }

        // ------------------------------------------------------------ relations

        [Fact]
        public void One_widget_can_be_declared_the_label_of_another()
        {
            Run(() =>
            {
                var caption = new Label("File name");
                var entry = new Entry();
                Assert.False(Gtk.Test.AccessibleHasRelation(entry, AccessibleRelation.LabelledBy));

                entry.UpdateRelation(AccessibleRelation.LabelledBy, caption);
                Assert.True(Gtk.Test.AccessibleHasRelation(entry, AccessibleRelation.LabelledBy));

                entry.UpdateRelation(AccessibleRelation.DescribedBy, caption, new Label("must be unique"));
                Assert.True(Gtk.Test.AccessibleHasRelation(entry, AccessibleRelation.DescribedBy));

                entry.ResetRelation(AccessibleRelation.LabelledBy);
                Assert.False(Gtk.Test.AccessibleHasRelation(entry, AccessibleRelation.LabelledBy));
                Assert.True(Gtk.Test.AccessibleHasRelation(entry, AccessibleRelation.DescribedBy));
            });
        }

        // active-descendant is the one reference relation that points at a
        // single accessible; every other one - error-message included, which
        // reads like a single thing - is a list. Nothing in the names says so,
        // and passing the wrong shape sets nothing.
        [Fact]
        public void Active_descendant_names_one_accessible_where_the_others_name_a_list()
        {
            Run(() =>
            {
                var list = new ListBox();
                var row = new ListBoxRow();

                list.UpdateRelation(AccessibleRelation.ActiveDescendant, row);
                Assert.True(Gtk.Test.AccessibleHasRelation(list, AccessibleRelation.ActiveDescendant));

                Assert.Throws<ArgumentException>(() =>
                    list.UpdateRelation(AccessibleRelation.ActiveDescendant, row, new ListBoxRow()));

                var entry = new Entry();
                entry.UpdateRelation(AccessibleRelation.ErrorMessage, new Label("not a number"));
                Assert.True(Gtk.Test.AccessibleHasRelation(entry, AccessibleRelation.ErrorMessage));
            });
        }

        [Fact]
        public void A_relation_can_carry_a_number_rather_than_a_widget()
        {
            Run(() =>
            {
                var row = new ListBoxRow();

                var relations = new[] { AccessibleRelation.RowIndex, AccessibleRelation.RowCount };
                var values = Values(AccessibleExtensions.InitValue, relations, new object[] { 4, 10 });
                try
                {
                    Assert.Equal("gint", values[0].ValueType.ToString());
                    row.UpdateRelation(relations, values);
                }
                finally { Dispose(values); }

                Assert.True(Gtk.Test.AccessibleHasRelation(row, AccessibleRelation.RowIndex));
                Assert.True(Gtk.Test.AccessibleHasRelation(row, AccessibleRelation.RowCount));
                Assert.False(Gtk.Test.AccessibleHasRelation(row, AccessibleRelation.ColIndex));
            });
        }

        // A label with a mnemonic labels its target: Gtk sets the relation from
        // gtk_label_set_mnemonic_widget, on the *target*, not on the label.
        [Fact]
        public void A_mnemonic_label_labels_its_target()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Horizontal, 0);
                var caption = new Label("_Name") { UseUnderline = true };
                var entry = new Entry();
                box.Append(caption);
                box.Append(entry);

                Assert.False(Gtk.Test.AccessibleHasRelation(entry, AccessibleRelation.LabelledBy));
                caption.MnemonicWidget = entry;

                Assert.True(Gtk.Test.AccessibleHasRelation(entry, AccessibleRelation.LabelledBy));
                Assert.False(Gtk.Test.AccessibleHasRelation(caption, AccessibleRelation.LabelledBy));
            });
        }

        [Fact]
        public void A_ui_file_can_declare_states_properties_and_relations()
        {
            Run(() =>
            {
                var builder = new Builder();
                builder.AddFromString(
                    "<interface>" +
                    "  <object class=\"GtkLabel\" id=\"caption\"><property name=\"label\">Name</property></object>" +
                    "  <object class=\"GtkEntry\" id=\"field\">" +
                    "    <accessibility>" +
                    "      <property name=\"description\">the account name</property>" +
                    "      <state name=\"busy\">true</state>" +
                    "      <relation name=\"labelled-by\">caption</relation>" +
                    "    </accessibility>" +
                    "  </object>" +
                    "</interface>");

                var field = (Entry)builder.GetObject("field");

                Assert.True(Gtk.Test.AccessibleHasProperty(field, AccessibleProperty.Description));
                Assert.True(Gtk.Test.AccessibleHasState(field, AccessibleState.Busy));
                Assert.True(Gtk.Test.AccessibleHasRelation(field, AccessibleRelation.LabelledBy));
                Assert.False(Gtk.Test.AccessibleHasProperty(field, AccessibleProperty.Label));
            });
        }

        // ------------------------------------------------------------ AccessibleList

        [Fact]
        public void An_accessible_list_gives_back_what_it_was_built_from()
        {
            Run(() =>
            {
                var first = new Button("one");
                var second = new Label("two");

                using (var list = new AccessibleList(new IAccessible[] { first, second }))
                {
                    var objects = list.Objects.Cast<object>().ToList();

                    Assert.Equal(2, objects.Count);
                    Assert.Same(first, objects[0]);
                    Assert.Same(second, objects[1]);
                }

                // Order is the order it was given, which is the order a screen
                // reader would announce the labels in.
                using (var reversed = new AccessibleList(new IAccessible[] { second, first }))
                    Assert.Same(second, reversed.Objects.Cast<object>().First());
            });
        }

        // ------------------------------------------------------------ what cannot be reached

        // GtkAccessibleText and GtkAccessibleRange are pure vfunc tables: Gtk
        // exports no C function that calls one, so codegen - which builds an
        // adapter by pairing each vfunc with the function that invokes it -
        // drops every one of them and emits an empty Implementor interface. A
        // managed widget therefore cannot say what its text is, and no managed
        // caller can read another widget's. The consumer half is bound, so the
        // widgets do implement the interfaces; there is simply nothing on them.
        //
        // This fails the day the generator learns to emit a vfunc with no target
        // method, which is the point of pinning it.
        [Fact]
        public void The_accessible_text_and_range_interfaces_cannot_be_implemented_from_managed_code()
        {
            Run(() =>
            {
                Assert.IsAssignableFrom<IAccessibleText>(new Label("x"));
                Assert.IsAssignableFrom<IAccessibleText>(new TextView());
                Assert.IsAssignableFrom<IAccessibleRange>(
                    new Scale(Orientation.Horizontal, new Adjustment(0, 0, 100, 1, 10, 0)));

                // GLib.IWrapper contributes Handle; anything beyond that would be
                // a vfunc a managed type could supply.
                Assert.Empty(typeof(IAccessibleTextImplementor).GetMembers());
                Assert.Empty(typeof(IAccessibleRangeImplementor).GetMembers());

                // Only the notifications survive, because those are real C
                // functions rather than vfuncs.
                Assert.NotEmpty(typeof(IAccessibleText).GetMethod("UpdateContents").GetParameters());
            });
        }
    }
}

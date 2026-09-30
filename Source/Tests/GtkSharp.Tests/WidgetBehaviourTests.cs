using System;
using System.Linq;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// What a Gtk 4 application actually does: measure and lay out widgets, attach
    /// event controllers, style with CSS, and walk the widget tree. Gtk 4 replaced
    /// nearly all of this — <c>size_allocate</c> became <c>measure</c>, signals
    /// became controllers, <c>GtkContainer</c> disappeared — so these are the paths
    /// most likely to have been ported wrongly and least likely to be reached by a
    /// property round-trip.
    /// </summary>
    public class WidgetBehaviourTests : GtkTestBase
    {
        public WidgetBehaviourTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>Lets Gtk settle the layout without showing anything.</summary>
        private static void Settle()
        {
            var guard = 0;
            while (Gtk.Application.EventsPending() && guard++ < 1000)
                Gtk.Application.RunIteration(false);
        }

        // ------------------------------------------------------------ measuring

        [Fact]
        public void A_label_measures_wider_for_longer_text()
        {
            // Measure replaced GetPreferredWidth in Gtk 4 and takes an
            // orientation plus a for-size, which is where a port goes wrong.
            Run(() =>
            {
                var shortLabel = new Label("hi");
                var longLabel = new Label("a considerably longer piece of text");

                shortLabel.Measure(Orientation.Horizontal, -1,
                                   out var shortMin, out var shortNat, out _, out _);
                longLabel.Measure(Orientation.Horizontal, -1,
                                  out var longMin, out var longNat, out _, out _);

                Assert.True(shortMin > 0, "a label with text must want some width");
                Assert.True(longMin > shortMin,
                            $"longer text should measure wider: {longMin} vs {shortMin}");
                Assert.True(longNat >= longMin, "natural size cannot be below the minimum");
            });
        }

        [Fact]
        public void A_wrapping_label_measured_for_a_narrow_width_wants_more_height()
        {
            // The for-size argument is the whole point of Measure: height
            // depends on the width the widget is given.
            Run(() =>
            {
                var label = new Label("the quick brown fox jumps over the lazy dog") { Wrap = true };

                label.Measure(Orientation.Vertical, -1, out var tallForAny, out _, out _, out _);
                label.Measure(Orientation.Vertical, 40, out var tallForNarrow, out _, out _, out _);

                Assert.True(tallForNarrow > tallForAny,
                            $"wrapping into 40px should need more height: {tallForNarrow} vs {tallForAny}");
            });
        }

        [Fact]
        public void A_size_request_raises_the_measured_minimum()
        {
            Run(() =>
            {
                var button = new Button();

                button.Measure(Orientation.Horizontal, -1, out var natural, out _, out _, out _);

                button.WidthRequest = natural + 200;

                button.Measure(Orientation.Horizontal, -1, out var requested, out _, out _, out _);

                Assert.Equal(natural + 200, requested);
                Assert.Equal(natural + 200, button.WidthRequest);
            });
        }

        [Fact]
        public void A_box_measures_the_sum_of_its_children_plus_the_spacing()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Vertical, 7);
                var first = new Label("one");
                var second = new Label("two");
                box.Append(first);
                box.Append(second);

                first.Measure(Orientation.Vertical, -1, out var firstHeight, out _, out _, out _);
                second.Measure(Orientation.Vertical, -1, out var secondHeight, out _, out _, out _);
                box.Measure(Orientation.Vertical, -1, out var boxHeight, out _, out _, out _);

                Assert.Equal(firstHeight + secondHeight + 7, boxHeight);
            });
        }

        // ------------------------------------------------------- the widget tree

        [Fact]
        public void A_box_reports_its_children_in_the_order_they_were_appended()
        {
            // GtkContainer is gone: children are a linked list on Widget now, so
            // walking it is the only way to enumerate them.
            Run(() =>
            {
                var box = new Box(Orientation.Vertical, 0);
                var names = new[] { "first", "second", "third" };

                foreach (var name in names)
                    box.Append(new Label(name));

                Assert.Equal(names, ChildLabels(box));
                Assert.Equal("first", ((Label) box.FirstChild).Text);
                Assert.Equal("third", ((Label) box.LastChild).Text);
            });
        }

        [Fact]
        public void Prepending_and_inserting_place_children_where_their_names_say()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Vertical, 0);

                var second = new Label("second");
                box.Append(second);
                box.Prepend(new Label("first"));
                box.InsertChildAfter(new Label("third"), second);

                Assert.Equal(new[] { "first", "second", "third" }, ChildLabels(box));
            });
        }

        [Fact]
        public void Removing_a_child_unparents_it_and_leaves_the_others()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Vertical, 0);
                var doomed = new Label("doomed");
                box.Append(new Label("kept"));
                box.Append(doomed);

                box.Remove(doomed);

                Assert.Equal(new[] { "kept" }, ChildLabels(box));
                Assert.Null(doomed.Parent);
            });
        }

        [Fact]
        public void Reordering_a_child_moves_it_without_reparenting()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Vertical, 0);
                var moving = new Label("moving");
                box.Append(moving);
                box.Append(new Label("other"));

                box.ReorderChildAfter(moving, box.LastChild);

                Assert.Equal(new[] { "other", "moving" }, ChildLabels(box));
                Assert.Same(box, moving.Parent);
            });
        }

        [Fact]
        public void Siblings_link_to_each_other_in_both_directions()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Vertical, 0);
                var first = new Label("first");
                var second = new Label("second");
                box.Append(first);
                box.Append(second);

                Assert.Same(second, first.NextSibling);
                Assert.Same(first, second.PrevSibling);
                Assert.Null(first.PrevSibling);
                Assert.Null(second.NextSibling);
            });
        }

        [Fact]
        public void A_nested_widget_finds_the_ancestor_of_a_given_type()
        {
            Run(() =>
            {
                var window = new Window();
                var outer = new Box(Orientation.Vertical, 0);
                var inner = new Box(Orientation.Horizontal, 0);
                var leaf = new Label("leaf");

                inner.Append(leaf);
                outer.Append(inner);
                window.Child = outer;

                Assert.Same(inner, leaf.GetAncestor(Box.GType));
                Assert.Same(window, leaf.GetAncestor(Window.GType));
                Assert.Null(leaf.GetAncestor(Button.GType));
                Assert.True(leaf.IsAncestor(window));
            });
        }

        // ---------------------------------------------------- event controllers

        [Fact]
        public void Activate_does_not_reach_a_click_handler_but_emitting_clicked_does()
        {
            // Gtk 4 routes presses through a gesture on the button rather than
            // through the widget's own signal, so Widget.Activate does not reach
            // a Clicked handler on an unrealised button. This is the behaviour
            // that made the sample button-press test pass while pressing
            // nothing, so it is worth pinning in both directions rather than
            // left as folklore.
            Run(() =>
            {
                var button = new Button { Label = "press me" };
                var clicks = 0;
                button.Clicked += (o, e) => clicks++;

                button.Activate();
                Settle();

                Assert.Equal(0, clicks);

                GLib.Signal.Emit(button, "clicked");

                Assert.Equal(1, clicks);
            });
        }

        [Fact]
        public void A_controller_added_to_a_widget_is_listed_on_it_and_can_be_removed()
        {
            Run(() =>
            {
                var widget = new Box(Orientation.Vertical, 0);
                var gesture = new GestureClick();

                widget.AddController(gesture);

                Assert.Same(widget, gesture.Widget);

                widget.RemoveController(gesture);

                Assert.Null(gesture.Widget);
            });
        }

        [Fact]
        public void A_key_controller_reports_the_key_that_was_delivered_to_it()
        {
            Run(() =>
            {
                var entry = new Entry();
                var controller = new EventControllerKey();
                entry.AddController(controller);

                uint seen = 0;
                Gdk.ModifierType state = Gdk.ModifierType.None;
                controller.KeyPressed += (o, args) =>
                {
                    seen = args.Keyval;
                    state = args.State;
                    args.RetVal = false;
                };

                // No display to synthesise events through, so the signal is
                // emitted directly -- which still proves the argument
                // marshalling, which is the part that can be wrong.
                //
                // It proves nothing about *delivery*, and that gap has bitten:
                // this test passed while a controller attached exactly like the
                // one above never fired for a real keystroke, because emitting
                // on the controller skips propagation entirely. The two tests
                // below pin the propagation side of it.
                GLib.Signal.Emit(controller, "key-pressed",
                                 (uint) Gdk.Key.a, 38u, Gdk.ModifierType.ControlMask);

                Assert.Equal((uint) Gdk.Key.a, seen);
                Assert.Equal(Gdk.ModifierType.ControlMask, state);
            });
        }

        [Fact]
        public void An_entry_delegates_keys_to_an_internal_text_child()
        {
            // Half of why a key controller on an Entry never fires: the Entry is
            // not what the key event targets. It is a shell around a GtkText,
            // and that child is what takes the focus -- so a controller on the
            // Entry is already one widget outwards from the target, and the
            // default Bubble phase only reaches it if the GtkText declines the
            // key. It does not: it inserts the character and returns true.
            Run(() =>
            {
                var entry = new Entry();

                var inner = entry.FirstChild;

                Assert.NotNull(inner);
                // GtkText is bound as Gtk.TextWidget; see GtkSharp.metadata.
                Assert.IsType<TextWidget>(inner);
                Assert.True(inner.CanFocus, "the GtkText child is what takes the focus");
            });
        }

        [Fact]
        public void A_controller_propagates_in_the_bubble_phase_unless_told_otherwise()
        {
            // The other half. Bubble runs from the event's target outwards, so
            // any widget in between that handles the event ends the emission
            // before an ancestor's controller is reached. Capture runs root to
            // target instead, which is the fix for the case above.
            Run(() =>
            {
                var controller = new EventControllerKey();

                Assert.Equal(PropagationPhase.Bubble, controller.PropagationPhase);

                controller.PropagationPhase = PropagationPhase.Capture;

                Assert.Equal(PropagationPhase.Capture, controller.PropagationPhase);

                // Attaching must not reset it -- the order the two are written in
                // is not something a caller should have to think about.
                new Entry().AddController(controller);

                Assert.Equal(PropagationPhase.Capture, controller.PropagationPhase);
            });
        }

        [Fact]
        public void A_shortcut_controller_holds_the_shortcuts_added_to_it()
        {
            Run(() =>
            {
                var controller = new ShortcutController();
                var shortcut = new Shortcut(new ShortcutTrigger("<Control>s"),
                                            new ShortcutAction("action(win.save)"));

                controller.AddShortcut(shortcut);

                Assert.Equal(1u, controller.NItems);
                Assert.NotNull(controller.GetObject(0));
            });
        }

        [Fact]
        public void A_control_s_trigger_prints_back_as_the_string_it_was_parsed_from()
        {
            Run(() =>
            {
                var trigger = new ShortcutTrigger("<Control>s");

                Assert.NotNull(trigger);
                Assert.Equal("<Control>s", trigger.ToString());
            });
        }

        // ------------------------------------------------------------------ CSS

        [Fact]
        public void A_css_class_added_to_a_widget_is_reported_back()
        {
            Run(() =>
            {
                var button = new Button();

                Assert.DoesNotContain("suggested-action", button.CssClasses);

                button.AddCssClass("suggested-action");

                Assert.Contains("suggested-action", button.CssClasses);
                Assert.True(button.HasCssClass("suggested-action"));

                button.RemoveCssClass("suggested-action");

                Assert.False(button.HasCssClass("suggested-action"));
            });
        }

        [Fact]
        public void Css_classes_can_be_replaced_wholesale()
        {
            Run(() =>
            {
                var button = new Button();

                button.CssClasses = new[] { "one", "two" };

                Assert.Contains("one", button.CssClasses);
                Assert.Contains("two", button.CssClasses);
            });
        }

        [Fact]
        public void A_widget_reports_the_css_name_of_its_type()
        {
            Run(() =>
            {
                Assert.Equal("button", Widget.GetCssName(Button.GType));
                Assert.Equal("label", Widget.GetCssName(Label.GType));
                Assert.Equal("window", Widget.GetCssName(Window.GType));
            });
        }

        [Fact]
        public void A_css_provider_parses_valid_css_and_reports_errors_in_bad_css()
        {
            // The parsing-error signal is the only way to learn that a stylesheet
            // was rejected: LoadFromData does not throw.
            Run(() =>
            {
                var good = new CssProvider();
                var goodErrors = 0;
                good.ParsingError += (o, args) => goodErrors++;
                good.LoadFromString("button { color: red; }");

                Assert.Equal(0, goodErrors);

                // Handling this signal used to take the process down. Its GError
                // argument has no managed counterpart, and GLib.Value.Val threw
                // "Unknown type GError" from inside the signal marshaller, where
                // nothing can catch it -- so the one signal that tells an
                // application its stylesheet is broken was unusable.
                var bad = new CssProvider();
                var badErrors = 0;
                Gtk.CssSection section = null;
                bad.ParsingError += (o, args) =>
                {
                    badErrors++;
                    section = args.Section;
                };

                bad.LoadFromString("button { this-is-not-a-property: 3; }");

                Assert.True(badErrors > 0, "a bogus property should be reported");
                Assert.NotNull(section);
                Assert.True(section.StartLocation.Lines == 0,
                            "the error is on the first line of the stylesheet");
            });
        }

        // ------------------------------------------------------------ visibility

        [Fact]
        public void Visibility_and_sensitivity_round_trip_and_compose()
        {
            Run(() =>
            {
                var window = new Window();
                var child = new Label("child");
                window.Child = child;

                Assert.True(child.Visible);
                Assert.True(child.Sensitive);

                child.Visible = false;
                Assert.False(child.Visible);

                child.Visible = true;

                // IsSensitive folds in the parent's state; Sensitive does not.
                window.Sensitive = false;

                Assert.True(child.Sensitive);
                Assert.False(child.IsSensitive);
            });
        }

        [Fact]
        public void Expansion_and_alignment_round_trip()
        {
            Run(() =>
            {
                var label = new Label("aligned")
                {
                    Hexpand = true,
                    Vexpand = true,
                    Halign = Align.End,
                    Valign = Align.Start,
                    MarginStart = 4,
                    MarginEnd = 8,
                };

                Assert.True(label.Hexpand);
                Assert.True(label.Vexpand);
                Assert.Equal(Align.End, label.Halign);
                Assert.Equal(Align.Start, label.Valign);
                Assert.Equal(4, label.MarginStart);
                Assert.Equal(8, label.MarginEnd);
            });
        }

        [Fact]
        public void A_widget_reports_the_direction_it_was_given()
        {
            Run(() =>
            {
                var label = new Label("directional");

                label.Direction = TextDirection.Rtl;
                Assert.Equal(TextDirection.Rtl, label.Direction);

                label.Direction = TextDirection.Ltr;
                Assert.Equal(TextDirection.Ltr, label.Direction);
            });
        }

        // ------------------------------------------------------ layout managers

        [Fact]
        public void A_grid_places_children_at_the_coordinates_it_was_given()
        {
            Run(() =>
            {
                var grid = new Grid();
                var topLeft = new Label("0,0");
                var bottomRight = new Label("1,1");

                grid.Attach(topLeft, 0, 0, 1, 1);
                grid.Attach(bottomRight, 1, 1, 1, 1);

                Assert.Same(topLeft, grid.GetChildAt(0, 0));
                Assert.Same(bottomRight, grid.GetChildAt(1, 1));
                Assert.Null(grid.GetChildAt(5, 5));
            });
        }

        [Fact]
        public void A_grid_reports_the_position_and_span_of_a_child()
        {
            Run(() =>
            {
                var grid = new Grid();
                var wide = new Label("spanning");

                grid.Attach(wide, 1, 2, 3, 1);

                grid.QueryChild(wide, out var column, out var row, out var width, out var height);

                Assert.Equal(1, column);
                Assert.Equal(2, row);
                Assert.Equal(3, width);
                Assert.Equal(1, height);
            });
        }

        [Fact]
        public void A_widget_reports_the_layout_manager_its_type_uses()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Vertical, 0);

                Assert.NotNull(box.LayoutManager);
                Assert.IsType<BoxLayout>(box.LayoutManager);
                Assert.Equal(Orientation.Vertical, ((BoxLayout) box.LayoutManager).Orientation);
            });
        }

        private static string[] ChildLabels(Widget parent)
        {
            var labels = new System.Collections.Generic.List<string>();

            for (var child = parent.FirstChild; child != null; child = child.NextSibling)
                labels.Add(((Label) child).Text);

            return labels.ToArray();
        }
    }
}

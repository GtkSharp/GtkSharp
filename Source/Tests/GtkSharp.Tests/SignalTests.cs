using System;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The signal and property machinery: the part of the binding every other
    /// part depends on.
    /// </summary>
    /// <remarks>
    /// A break here does not look like a break. Handlers simply stop arriving,
    /// or arrive with the wrong argument — which is exactly what happened with
    /// GtkWidget::destroy (a signal Gtk 4 removed, whose handlers therefore
    /// never ran) and with GLib.ItemsChangedArgs (whose position was unboxed as
    /// the wrong type and aborted the process).
    /// </remarks>
    public class SignalTests : GtkTestBase
    {
        public SignalTests(GtkFixture fixture) : base(fixture) { }

        [Fact]
        public void A_connected_handler_runs_and_a_disconnected_one_does_not()
        {
            Run(() =>
            {
                var button = new Button("press");
                int calls = 0;
                System.EventHandler handler = (o, e) => calls++;

                button.Clicked += handler;
                GLib.Signal.Emit(button, "clicked");
                Assert.Equal(1, calls);

                button.Clicked -= handler;
                GLib.Signal.Emit(button, "clicked");
                Assert.Equal(1, calls);
            });
        }

        [Fact]
        public void The_sender_is_the_object_that_raised_the_signal()
        {
            Run(() =>
            {
                var button = new Button("press");
                object sender = null;

                button.Clicked += (o, e) => sender = o;
                GLib.Signal.Emit(button, "clicked");

                Assert.Same(button, sender);
            });
        }

        [Fact]
        public void Notify_reports_the_property_that_changed()
        {
            Run(() =>
            {
                var entry = new Entry();
                string changed = null;

                entry.AddNotification("text", (o, args) => changed = args.Property);
                entry.Text = "typed";

                Assert.Equal("text", changed);
            });
        }

        [Fact]
        public void Signal_arguments_carry_their_values()
        {
            // items-changed reports where the change happened and how big it
            // was. This is also the signal whose args class was unboxing a
            // guint as int and aborting the process, so it is worth pinning
            // precisely rather than loosely.
            Run(() =>
            {
                var store = new GLib.ListStore((GLib.GType) typeof(ListModelTests.Row));
                uint? position = null, removed = null, added = null;

                store.ItemsChanged += (o, args) => {
                    position = args.Position;
                    removed = args.Removed;
                    added = args.Added;
                };

                store.Append(new ListModelTests.Row { Name = "first" }.Handle);

                Assert.Equal(0u, position);
                Assert.Equal(0u, removed);
                Assert.Equal(1u, added);
            });
        }

        [Fact]
        public void An_enum_property_round_trips_through_the_marshaller()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Horizontal, 0);

                box.Orientation = Orientation.Vertical;

                Assert.Equal(Orientation.Vertical, box.Orientation);
            });
        }

        [Fact]
        public void A_flags_value_keeps_its_combination()
        {
            Run(() =>
            {
                var target = new DropTarget(GLib.GType.String, Gdk.DragAction.Copy | Gdk.DragAction.Move);

                Assert.True(target.Actions.HasFlag(Gdk.DragAction.Copy));
                Assert.True(target.Actions.HasFlag(Gdk.DragAction.Move));
                Assert.False(target.Actions.HasFlag(Gdk.DragAction.Ask));
            });
        }

        [Fact]
        public void An_interface_implemented_by_a_widget_dispatches_to_it()
        {
            // Box implements GtkOrientable. Reaching it through the interface
            // goes via the generated adapter rather than the class, which is a
            // separate code path.
            Run(() =>
            {
                IOrientable orientable = new Box(Orientation.Horizontal, 0);

                orientable.Orientation = Orientation.Vertical;

                Assert.Equal(Orientation.Vertical, orientable.Orientation);
            });
        }

        [Fact]
        public void An_overridden_virtual_method_is_called_by_Gtk()
        {
            // Managed subclasses register their own GType and patch the class
            // struct. If that breaks, the override is simply never reached and
            // the base implementation runs instead -- silently.
            Run(() =>
            {
                var widget = new MeasuringWidget();

                widget.Measure(Orientation.Horizontal, -1, out int minimum, out int natural, out _, out _);

                Assert.True(widget.MeasureCalled, "OnMeasure should have been invoked by Gtk");
                Assert.Equal(123, minimum);
                Assert.Equal(123, natural);
            });
        }

        private class MeasuringWidget : Widget
        {
            public bool MeasureCalled { get; private set; }

            protected override void OnMeasure(Orientation orientation, int forSize,
                                              out int minimum, out int natural,
                                              out int minimumBaseline, out int naturalBaseline)
            {
                MeasureCalled = true;
                minimum = natural = 123;
                minimumBaseline = naturalBaseline = -1;
            }
        }
    }
}

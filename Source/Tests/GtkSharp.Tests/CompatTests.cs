using System;
// Cairo, deliberately: A_widget_subclass_can_still_use_a_type_named_Color needs an unqualified
// type called Color in scope inside a Widget subclass, which is the exact shape that broke.
using Cairo;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The Gtk 3 compatibility surface in <c>Source/Libs/*/Compat/</c>: the types and members
    /// Gtk 4 removed, re-provided over what replaced them.
    /// </summary>
    /// <remarks>
    /// These exist because a shim that compiles proves nothing. Every one of them is built on
    /// native entry points that may or may not still exist, and <c>FuncLoader.LoadFunction</c>
    /// returns <c>default(T)</c> for a missing export - so a wrong assumption surfaces as a
    /// NullReferenceException at the call site, naming nothing, or as an override that is simply
    /// never reached. Each test below therefore CALLS the thing, through Gtk, and asserts on what
    /// Gtk did with it.
    /// </remarks>
    public class CompatTests : GtkTestBase
    {
        public CompatTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------ helpers

        /// <summary>Runs the main loop until <paramref name="until"/> holds, or time runs out.</summary>
        /// <remarks>
        /// A condition rather than a fixed iteration count: realizing and rendering a window goes
        /// through the display server, and how many iterations that takes is not ours to know.
        /// </remarks>
        private static bool Pump(Func<bool> until, int milliseconds = 4000)
        {
            var deadline = System.Diagnostics.Stopwatch.StartNew();
            while (!until() && deadline.ElapsedMilliseconds < milliseconds)
                Gtk.Application.RunIteration(false);
            return until();
        }

        // ------------------------------------------------------------ Gdk.Color

        /// <summary>
        /// GdkColor's channels are 16-bit, and a shim storing 8-bit ones would still compile and
        /// still look like a colour.
        /// </summary>
        /// <remarks>
        /// This is a real bug from the Xamarin.Forms backend, recorded in its ColorExtensions:
        /// treating the channels as bytes made every converted colour saturate to pure
        /// red/green/blue/white, because the consumer divides by 65535.
        /// </remarks>
        [Fact]
        public void Gdk_colour_keeps_sixteen_bit_channels()
        {
            var white = new Gdk.Color((byte)0xFF, (byte)0xFF, (byte)0xFF);

            Assert.Equal(ushort.MaxValue, white.Red);
            Assert.Equal(ushort.MaxValue, white.Green);
            Assert.Equal(ushort.MaxValue, white.Blue);

            // 0x80 replicated, not shifted: 0x8080, so that mid-grey stays mid-grey after the
            // round trip rather than drifting a channel low.
            var grey = new Gdk.Color((byte)0x80, (byte)0x80, (byte)0x80);
            Assert.Equal(0x8080, grey.Red);
        }

        [Fact]
        public void Gdk_colour_round_trips_through_rgba()
        {
            var original = new Gdk.Color((byte)12, (byte)34, (byte)56);
            var back = Gdk.Color.FromRGBA(original.ToRGBA());

            Assert.Equal(original, back);
        }

        [Fact]
        public void Gdk_colour_parses_what_rgba_parses()
        {
            var parsed = new Gdk.Color();

            Assert.True(Gdk.Color.Parse("#0C2238", ref parsed));
            Assert.Equal(new Gdk.Color((byte)0x0C, (byte)0x22, (byte)0x38), parsed);
            Assert.False(Gdk.Color.Parse("not a colour", ref parsed));
        }

        // ------------------------------------------------------------ Container

        /// <summary>
        /// A container reports a size that covers its children at their offsets, and Gtk asks it
        /// for that size.
        /// </summary>
        /// <remarks>
        /// Asked through the PARENT, never by calling OnMeasure: Gtk only measures a widget it
        /// has a reason to, and a direct call would pass whether or not the vfunc is wired up.
        /// </remarks>
        [Fact]
        public void Container_measures_to_cover_its_children()
        {
            Run(() =>
            {
                var container = new Container();
                var child = new Label("x");
                child.SetSizeRequest(40, 20);

                container.Put(child, 30, 10);

                int minimum, natural, ignored;
                container.Measure(Orientation.Horizontal, -1, out minimum, out natural, out ignored, out ignored);
                Assert.Equal(70, natural);

                container.Measure(Orientation.Vertical, -1, out minimum, out natural, out ignored, out ignored);
                Assert.Equal(30, natural);
            });
        }

        [Fact]
        public void Container_add_remove_and_children_track_parenting()
        {
            Run(() =>
            {
                var container = new Container();
                var first = new Label("1");
                var second = new Label("2");

                container.Add(first);
                container.Add(second);

                Assert.Equal(new Widget[] { first, second }, container.Children);
                Assert.Same(container, first.Parent);

                container.Remove(first);

                Assert.Equal(new Widget[] { second }, container.Children);
                Assert.Null(first.Parent);
            });
        }

        /// <summary>Adding the same child twice is a no-op, not a second entry.</summary>
        /// <remarks>
        /// Gtk 4 aborts if a widget that already has a parent is parented again, so this guard is
        /// the difference between a warning and a dead process.
        /// </remarks>
        [Fact]
        public void Container_ignores_a_child_it_already_has()
        {
            Run(() =>
            {
                var container = new Container();
                var child = new Label("x");

                container.Add(child);
                container.Add(child);

                Assert.Single(container.Children);
            });
        }

        [Fact]
        public void Container_allocates_children_at_their_offsets()
        {
            Run(() =>
            {
                var window = new Window();
                var container = new Container();
                var child = new Label("x");
                child.SetSizeRequest(40, 20);

                container.Put(child, 30, 10);
                window.Child = container;
                window.SetDefaultSize(200, 100);
                window.Present();

                Assert.True(Pump(() => child.Width > 0), "the child was never allocated");

                // Width and Height come from the allocation Gtk gave the child, so a non-zero
                // size is proof the container's size_allocate ran and reached it.
                Assert.Equal(40, child.Width);
                Assert.Equal(20, child.Height);

                window.Destroy();
            });
        }

        // ------------------------------------------------------------ EventBox

        private class DrawingEventBox : EventBox
        {
            public int Draws;

            protected override bool OnDrawn(Cairo.Context cr)
            {
                Draws++;

                cr.SetSourceRGBA(1, 0, 0, 1);
                cr.Rectangle(0, 0, 10, 10);
                cr.Fill();

                return false;
            }
        }

        /// <summary>
        /// The Gtk 3 draw vfunc is reached through Gtk 4's snapshot.
        /// </summary>
        /// <remarks>
        /// The one that could silently not work: a widget with a GtkLayoutManager has its MEASURE
        /// vfunc answered by the manager instead of itself, and it was not obvious that snapshot
        /// escapes the same fate. It does - but only a widget Gtk actually renders proves it,
        /// hence the realized window.
        /// </remarks>
        [Fact]
        public void EventBox_draw_override_is_reached_through_snapshot()
        {
            Run(() =>
            {
                var window = new Window();
                var box = new DrawingEventBox();
                box.SetSizeRequest(50, 50);

                window.Child = box;
                window.Present();

                Assert.True(Pump(() => box.Draws > 0),
                    "OnDrawn was never reached: snapshot did not call it.");

                window.Destroy();
            });
        }

        // ------------------------------------------------------------ input events

        /// <summary>
        /// The Gtk 3 button-press-event fires from a Gtk 4 GtkGestureClick, carrying the button,
        /// the coordinates and the repeat count.
        /// </summary>
        [Fact]
        public void Button_press_event_fires_from_a_click_gesture()
        {
            Run(() =>
            {
                var box = new EventBox();
                Gdk.EventButton received = null;

                box.ButtonPressEvent += (o, args) => received = args.Event;

                // Emitting on the gesture the SHIM attached, found by walking the widget's
                // controllers. Two things make this the only way:
                //
                //   - Gtk 4 removed every way for an application to synthesize input. There is no
                //     public GdkEvent constructor and no gtk_widget_event, so driving the
                //     controller's signal directly is what is left.
                //   - Attaching a second GestureClick and emitting on THAT proves nothing: the
                //     shim listens to its own. (This test failed exactly that way first.)
                //
                // Xamarin.Forms' GtkTestHost.PressButton needs the same recipe.
                var gesture = ControllerOf<GestureClick>(box);
                Assert.NotNull(gesture);
                GLib.Signal.Emit(gesture, "pressed", 2, 12.0, 34.0);

                Assert.NotNull(received);
                Assert.Equal(Gdk.EventType.ButtonPress, received.Type);
                Assert.Equal(12.0, received.X);
                Assert.Equal(34.0, received.Y);
                Assert.Equal(2, received.NPress);
            });
        }

        /// <summary>
        /// Nothing is attached to a widget nobody listens to.
        /// </summary>
        /// <remarks>
        /// Every wrapper in the process inherits these events, so a controller created eagerly
        /// would cost one per widget for a feature almost nothing uses.
        /// </remarks>
        [Fact]
        public void No_controller_is_attached_until_something_subscribes()
        {
            Run(() =>
            {
                var box = new EventBox();
                Assert.Equal(0, CountControllers(box));

                ButtonPressEventHandler handler = (o, args) => { };
                box.ButtonPressEvent += handler;

                Assert.Equal(1, CountControllers(box));
            });
        }

        /// <summary>The first controller of the given kind attached to a widget, or null.</summary>
        private static T ControllerOf<T>(Widget widget) where T : EventController
        {
            var controllers = widget.ObserveControllers();

            for (uint i = 0; i < controllers.NItems; i++)
            {
                if (controllers.GetObject(i) is T match)
                    return match;
            }

            return null;
        }

        private static int CountControllers(Widget widget)
        {
            var controllers = widget.ObserveControllers();
            return (int)controllers.NItems;
        }

        // ------------------------------------------------------------ Widget members

        [Fact]
        public void Destroy_unparents_a_child_and_is_idempotent()
        {
            Run(() =>
            {
                var container = new Container();
                var child = new Label("x");
                container.Add(child);

                child.Destroy();
                Assert.Null(child.Parent);

                // Twice: Gtk aborts on a double destroy, and a teardown path that runs from both
                // an explicit call and Dispose will do exactly this.
                child.Destroy();
            });
        }

        [Fact]
        public void Get_preferred_width_and_height_report_what_measure_reports()
        {
            Run(() =>
            {
                var label = new Label("x");
                label.SetSizeRequest(64, 48);

                int minimumWidth, naturalWidth, minimumHeight, naturalHeight;
                label.GetPreferredWidth(out minimumWidth, out naturalWidth);
                label.GetPreferredHeight(out minimumHeight, out naturalHeight);

                Assert.Equal(64, minimumWidth);
                Assert.Equal(48, minimumHeight);
            });
        }

        // ------------------------------------------------------------ name collisions

        private class ColourNamingWidget : Box
        {
            public ColourNamingWidget() : base(Orientation.Vertical, 0) { }

            /// <summary>
            /// Uses the identifier <c>Color</c> as a TYPE inside a Widget subclass.
            /// </summary>
            /// <remarks>
            /// The regression this guards: gtk_widget_get_color, bound under its own name, puts
            /// an instance property "Color" on Gtk.Widget - and an inherited member beats a type
            /// of the same name in C# simple-name resolution, so this method stops compiling for
            /// every subclass in every consumer. GtkSharp.metadata renames it to StyleColor.
            /// If this file stops compiling, that rename was lost.
            /// </remarks>
            public Color Opaque(Color source)
            {
                return new Color(source.R, source.G, source.B, 1.0);
            }
        }

        [Fact]
        public void A_widget_subclass_can_still_use_a_type_named_Color()
        {
            Run(() =>
            {
                var widget = new ColourNamingWidget();
                var opaque = widget.Opaque(new Cairo.Color(0.5, 0.25, 0.125, 0.0));

                Assert.Equal(1.0, opaque.A);

                // And the renamed property still reaches the native getter.
                Assert.True(widget.StyleColor.Alpha >= 0);
            });
        }
    }
}

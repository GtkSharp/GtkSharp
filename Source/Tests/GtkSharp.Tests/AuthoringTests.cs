using System;
using System.Reflection;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Writing new Gtk types in C# rather than merely calling the ones Gtk
    /// ships: subclassing a widget, declaring properties and an activation
    /// signal, overriding virtual methods, and writing a layout manager.
    /// </summary>
    /// <remarks>
    /// This is the half of the binding that runs <em>C into managed code</em>.
    /// A managed subclass registers its own GType, patches vfunc pointers into
    /// the class struct, installs GParamSpecs whose get/set land back in C#, and
    /// hands GObject a GCHandle to itself so the wrapper survives construction.
    /// None of that is exercised by calling a binding: it only runs when Gtk
    /// calls back. When it breaks, an override is simply never reached and the
    /// base implementation runs instead -- or, as with the two defects this file
    /// found, the type cannot be constructed at all and the exception names
    /// nothing.
    ///
    /// Two rules the tests below lean on:
    ///
    ///   - Gtk only measures, allocates and snapshots widgets it has a reason
    ///     to. An override is proved to be reached by asking a *parent* for the
    ///     answer, never by calling the override directly.
    ///   - Every managed GObject subclass that native code may hand back needs
    ///     an <c>(IntPtr)</c> constructor. Gtk.Builder is the usual way to find
    ///     out, and it finds out at run time.
    /// </remarks>
    public class AuthoringTests : GtkTestBase
    {
        public AuthoringTests(GtkFixture fixture) : base(fixture) { }

        // --------------------------------------------------------- the subjects

        /// <summary>A widget that wants a fixed 40px in both directions.</summary>
        private class FixedSizeWidget : Widget
        {
            protected override void OnMeasure(Orientation orientation, int forSize,
                                              out int minimum, out int natural,
                                              out int minimumBaseline, out int naturalBaseline)
            {
                minimum = natural = 40;
                minimumBaseline = naturalBaseline = -1;
            }
        }

        /// <summary>
        /// A Label that adds a margin of its own on top of whatever GtkLabel
        /// measured. Label, unlike Box, has no layout manager, so its measure
        /// vfunc is the one Gtk actually calls -- see
        /// <see cref="A_layout_manager_answers_instead_of_the_measure_override_on_its_widget"/>.
        /// </summary>
        private class PaddedLabel : Label
        {
            public const int Padding = 40;

            public PaddedLabel(string text) : base(text) { }

            protected override void OnMeasure(Orientation orientation, int forSize,
                                              out int minimum, out int natural,
                                              out int minimumBaseline, out int naturalBaseline)
            {
                // Chaining up runs class_abi.BaseOverride, which walks to the
                // parent class's vtable slot. If that lookup is wrong the call
                // either throws or silently re-enters this method.
                base.OnMeasure(orientation, forSize, out minimum, out natural,
                               out minimumBaseline, out naturalBaseline);
                minimum += Padding;
                natural += Padding;
            }
        }

        /// <summary>Three levels of managed subclass, the last adding nothing.</summary>
        private class DeeperLabel : PaddedLabel
        {
            public DeeperLabel(string text) : base(text) { }
        }

        /// <summary>A Box subclass that overrides nothing at all.</summary>
        private class InertBox : Box
        {
            public InertBox(Orientation orientation, int spacing) : base(orientation, spacing) { }
        }

        /// <summary>A Label subclass that overrides nothing at all.</summary>
        private class InertLabel : Label
        {
            public InertLabel(string text) : base(text) { }
        }

        /// <summary>A Box subclass whose measure override Gtk 4 never reaches.</summary>
        private class MeasuringBox : Box
        {
            public const int Wanted = 88;

            public int Measures;

            public MeasuringBox(Orientation orientation, int spacing) : base(orientation, spacing) { }

            protected override void OnMeasure(Orientation orientation, int forSize,
                                              out int minimum, out int natural,
                                              out int minimumBaseline, out int naturalBaseline)
            {
                Measures++;
                minimum = natural = Wanted;
                minimumBaseline = naturalBaseline = -1;
            }
        }

        private class HeightForWidthWidget : Widget
        {
            protected override SizeRequestMode OnGetRequestMode()
            {
                return SizeRequestMode.HeightForWidth;
            }
        }

        /// <summary>Records every lifecycle vfunc Gtk calls on it.</summary>
        private class RecordingWidget : Widget
        {
            public int Roots, Unroots, Realizes, Allocations, Snapshots;
            public int AllocatedWidth, AllocatedHeight, AllocatedBaseline;

            /// <summary>The rectangle OnSnapshot paints, and the oracle for the node tree.</summary>
            public const int PaintWidth = 11;
            public const int PaintHeight = 13;

            protected override void OnMeasure(Orientation orientation, int forSize,
                                              out int minimum, out int natural,
                                              out int minimumBaseline, out int naturalBaseline)
            {
                minimum = natural = 40;
                minimumBaseline = naturalBaseline = -1;
            }

            protected override void OnRoot() { Roots++; base.OnRoot(); }
            protected override void OnUnroot() { Unroots++; base.OnUnroot(); }
            protected override void OnRealized() { Realizes++; base.OnRealized(); }

            protected override void OnSizeAllocate(int width, int height, int baseline)
            {
                Allocations++;
                AllocatedWidth = width;
                AllocatedHeight = height;
                AllocatedBaseline = baseline;
            }

            protected override void OnSnapshot(Snapshot snapshot)
            {
                Snapshots++;
                var bounds = Graphene.Rect.Alloc();
                bounds.Init(0, 0, PaintWidth, PaintHeight);
                snapshot.AppendColor(new Gdk.RGBA { Red = 1f, Alpha = 1f }, bounds);
            }
        }

        /// <summary>Overriding OnActivate declares an activation signal on this type's GType.</summary>
        private class ActivatingWidget : Widget
        {
            public int Activations;

            protected override void OnActivate() { Activations++; }
        }

        /// <summary>The same widget without the override, so without the signal.</summary>
        private class InertWidget : Widget { }

        /// <summary>A plain GObject subclass carrying [GLib.Property] declarations.</summary>
        [GLib.TypeName(AuthoredThingTypeName)]
        private class AuthoredThing : GLib.Object
        {
            private int number;
            private string text;
            private bool flag;
            private double amount;
            private Widget widget;
            private int counter;

            public AuthoredThing() { }

            // Required of any managed GObject that native code may hand back --
            // Gtk.Builder constructs the native object first and only then looks
            // for a managed wrapper to put around it.
            public AuthoredThing(IntPtr raw) : base(raw) { }

            [GLib.Property("number")]
            public int Number { get { return number; } set { number = value; } }

            [GLib.Property("text")]
            public string Text { get { return text; } set { text = value; } }

            [GLib.Property("flag")]
            public bool Flag { get { return flag; } set { flag = value; } }

            [GLib.Property("amount")]
            public double Amount { get { return amount; } set { amount = value; } }

            [GLib.Property("widget")]
            public Widget Widget { get { return widget; } set { widget = value; } }

            /// <summary>No setter, so the GParamSpec is installed read-only.</summary>
            [GLib.Property("constant")]
            public int Constant { get { return 5; } }

            /// <summary>A setter that announces itself, which the plain ones above do not.</summary>
            [GLib.Property("counter")]
            public int Counter
            {
                get { return counter; }
                set { counter = value; Notify("counter"); }
            }
        }

        private const string AuthoredThingTypeName = "GtkSharpTestsAuthoredThing";

        /// <summary>
        /// A layout manager that stacks children down and to the right in fixed
        /// steps, so where each one lands is arithmetic the test does itself.
        /// </summary>
        private class StaircaseLayout : LayoutManager
        {
            public const int Step = 10;
            public const int ChildWidth = 30;
            public const int ChildHeight = 40;
            public const int Wanted = 99;

            public int Measures, Allocations;
            public Widget MeasuredWidget;
            public int AllocatedWidth, AllocatedHeight;

            /// <summary>When set, children are allocated with no transform at all.</summary>
            public bool UseNullTransform;

            protected override void OnMeasure(Widget widget, Orientation orientation, int forSize,
                                              out int minimum, out int natural,
                                              out int minimumBaseline, out int naturalBaseline)
            {
                Measures++;
                MeasuredWidget = widget;
                minimum = natural = Wanted;
                minimumBaseline = naturalBaseline = -1;
            }

            protected override void OnAllocate(Widget widget, int width, int height, int baseline)
            {
                Allocations++;
                AllocatedWidth = width;
                AllocatedHeight = height;

                var index = 0;
                for (var child = widget.FirstChild; child != null; child = child.NextSibling, index++)
                {
                    Gsk.Transform transform = null;
                    if (!UseNullTransform)
                    {
                        var offset = new Graphene.Point();
                        offset.Init(Step * index, 2 * Step * index);
                        transform = new Gsk.Transform().Translate(offset);
                    }

                    child.Allocate(ChildWidth, ChildHeight, -1, transform);
                }
            }
        }

        // ------------------------------------------------------ GType registration

        [Fact]
        public void A_managed_subclass_gets_a_gtype_of_its_own_below_the_type_it_derives_from()
        {
            Run(() =>
            {
                var gtype = (GLib.GType) typeof(FixedSizeWidget);

                Assert.NotEqual(Widget.GType, gtype);
                Assert.Equal(Widget.GType, gtype.GetBaseType());

                var instance = new FixedSizeWidget();

                // Both must say yes: the instance is of the new type, and the
                // new type still is a GtkWidget as far as C is concerned.
                Assert.True(gtype.IsInstance(instance.Handle));
                Assert.True(Widget.GType.IsInstance(instance.Handle));
            });
        }

        [Fact]
        public void All_instances_of_a_subclass_share_the_one_gtype_it_registered()
        {
            // Registration happens once, at first use of the managed type. If it
            // ran per instance, GObject would accumulate a type per widget and
            // nothing would compare equal.
            Run(() =>
            {
                var first = new InertBox(Orientation.Vertical, 0);
                var second = new InertBox(Orientation.Vertical, 0);

                var gtype = (GLib.GType) typeof(InertBox);

                Assert.True(gtype.IsInstance(first.Handle));
                Assert.True(gtype.IsInstance(second.Handle));

                // The name GObject knows it by must resolve back to the same
                // GType, which is what makes the registration visible to C.
                Assert.Equal(gtype, GLib.GType.FromName(gtype.ToString()));
            });
        }

        [Fact]
        public void A_TypeName_attribute_is_the_name_the_gtype_is_registered_under()
        {
            Run(() =>
            {
                var named = (GLib.GType) typeof(AuthoredThing);
                Assert.Equal(AuthoredThingTypeName, named.ToString());
                Assert.Equal(named, GLib.GType.FromName(AuthoredThingTypeName));

                // Without the attribute the name is generated, and is required
                // only to be unique -- so this pins the prefix, not the number.
                var unnamed = ((GLib.GType) typeof(InertWidget)).ToString();
                Assert.StartsWith("__gtksharp_", unnamed);
                Assert.Contains("InertWidget", unnamed);
            });
        }

        [Fact]
        public void Each_level_of_a_managed_hierarchy_registers_its_own_gtype()
        {
            Run(() =>
            {
                var padded = (GLib.GType) typeof(PaddedLabel);
                var deeper = (GLib.GType) typeof(DeeperLabel);

                Assert.NotEqual(padded, deeper);
                Assert.Equal(padded, deeper.GetBaseType());
                Assert.Equal(Label.GType, padded.GetBaseType());

                var instance = new DeeperLabel("three levels down");

                Assert.True(deeper.IsInstance(instance.Handle));
                Assert.True(padded.IsInstance(instance.Handle));
                Assert.True(Label.GType.IsInstance(instance.Handle));
            });
        }

        [Fact]
        public void Gtk_hands_back_the_very_managed_instance_it_was_given()
        {
            // The wrapper identity map is what makes an override reachable at
            // all: when Gtk calls a vfunc it passes a GObject*, and the callback
            // has to find *this* object rather than manufacture a new wrapper
            // whose fields are all zero.
            Run(() =>
            {
                var box = new Box(Orientation.Vertical, 0);
                var child = new RecordingWidget();
                box.Append(child);

                Assert.Same(child, box.FirstChild);
                Assert.Same(box, child.Parent);
            });
        }

        // ------------------------------------------------------------ properties

        [Fact]
        public void A_declared_property_is_readable_and_writable_through_the_gobject_property_system()
        {
            // Reading goes GObject -> GetPropertyCallback -> the C# getter;
            // writing goes the other way through SetPropertyCallback. Neither
            // path is reached by using the C# property on its own.
            Run(() =>
            {
                var thing = new AuthoredThing();

                thing.Number = 42;
                thing.Text = "written in C#";
                thing.Flag = true;
                thing.Amount = 2.5;

                Assert.Equal(42, thing.GetProperty("number").Val);
                Assert.Equal("written in C#", thing.GetProperty("text").Val);
                Assert.Equal(true, thing.GetProperty("flag").Val);
                Assert.Equal(2.5, thing.GetProperty("amount").Val);

                thing.SetProperty("number", new GLib.Value(7));
                thing.SetProperty("text", new GLib.Value("written through GObject"));
                thing.SetProperty("flag", new GLib.Value(false));
                thing.SetProperty("amount", new GLib.Value(-0.25));

                Assert.Equal(7, thing.Number);
                Assert.Equal("written through GObject", thing.Text);
                Assert.False(thing.Flag);
                Assert.Equal(-0.25, thing.Amount);
            });
        }

        [Fact]
        public void A_property_typed_as_a_widget_hands_back_the_same_managed_object()
        {
            // An object-typed property crosses as a GObject*, so what comes back
            // is only the same object if the identity map is consulted. A fresh
            // wrapper would compare equal on Handle and not be Same.
            Run(() =>
            {
                var thing = new AuthoredThing();
                var label = new Label("held");

                thing.Widget = label;

                Assert.Same(label, thing.GetProperty("widget").Val);
            });
        }

        [Fact]
        public void Assigning_a_declared_property_in_C_sharp_notifies_nobody()
        {
            // The trap: a [GLib.Property] setter is an ordinary C# setter. Only
            // a write that goes *through* GObject emits notify, so a binding or
            // an expression watching the property never sees an assignment made
            // in C#. The property has to call Notify itself -- which the next
            // test shows working.
            Run(() =>
            {
                var thing = new AuthoredThing();
                var notifications = 0;
                string named = null;

                thing.AddNotification("number", (o, args) => { notifications++; named = args.Property; });

                thing.Number = 3;
                Assert.Equal(0, notifications);
                Assert.Equal(3, thing.Number);

                thing.SetProperty("number", new GLib.Value(9));
                Assert.Equal(1, notifications);
                Assert.Equal("number", named);
            });
        }

        [Fact]
        public void A_setter_that_calls_Notify_reaches_a_notify_handler()
        {
            Run(() =>
            {
                var thing = new AuthoredThing();
                var seen = 0;
                string named = null;

                thing.AddNotification("counter", (o, args) => { seen++; named = args.Property; });

                thing.Counter = 1;
                thing.Counter = 2;

                Assert.Equal(2, seen);
                Assert.Equal("counter", named);
                Assert.Equal(2, thing.Counter);
            });
        }

        [Fact]
        public void A_property_with_no_setter_is_installed_read_only()
        {
            // ParamSpec is built from CanRead/CanWrite on the PropertyInfo, so a
            // getter-only C# property produces a read-only GParamSpec. GObject
            // refuses the write with a critical rather than an exception, so the
            // value staying put is the only observable difference.
            Run(() =>
            {
                var thing = new AuthoredThing();

                Assert.Equal(5, thing.GetProperty("constant").Val);

                thing.SetProperty("constant", new GLib.Value(99));

                Assert.Equal(5, thing.Constant);
                Assert.Equal(5, thing.GetProperty("constant").Val);
            });
        }

        [Fact]
        public void Gtk_Builder_constructs_a_managed_type_by_name_and_sets_its_declared_properties()
        {
            // The whole authoring stack at once: the GType has to be registered
            // under the name in the XML, GObject has to construct it, the
            // properties have to arrive at the C# setters, and the wrapper has
            // to be paired with the native object afterwards -- which is why
            // SetPropertyCallback defers writes it cannot deliver yet.
            Run(() =>
            {
                // Guard, not decoration: without an (IntPtr) constructor
                // ObjectManager.CreateObject raises MissingIntPtrCtorException
                // from inside GObject's constructor callback -- a managed
                // exception thrown across a native frame. Windows unwinds it
                // back to AddFromString; Linux cannot, and the test host dies
                // mid-run under a "Passed!" line. So the requirement is asserted
                // here rather than demonstrated by breaking it.
                Assert.NotNull(typeof(AuthoredThing).GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, new[] { typeof(IntPtr) }, null));

                // Force registration: Builder looks the name up in GObject's
                // type system, which has never heard of a type nothing touched.
                var gtype = (GLib.GType) typeof(AuthoredThing);
                gtype.GetClassPtr();

                var builder = new Builder();
                builder.AddFromString(
                    "<interface>" +
                    "  <object class=\"" + AuthoredThingTypeName + "\" id=\"thing\">" +
                    "    <property name=\"number\">17</property>" +
                    "    <property name=\"text\">from the markup</property>" +
                    "  </object>" +
                    "</interface>");

                var built = builder.GetObject("thing");

                var thing = Assert.IsType<AuthoredThing>(built);
                Assert.Equal(17, thing.Number);
                Assert.Equal("from the markup", thing.Text);
            });
        }

        // ------------------------------------------------------- declaring a signal

        [Fact]
        public void Overriding_OnActivate_declares_an_activation_signal_that_Activate_emits()
        {
            // This is the one path by which managed code creates a brand new
            // GObject signal: overriding OnActivate makes the class initializer
            // call g_signal_newv and register the result as the type's
            // activation signal. Before the Gtk 4 port was finished it wrote
            // the id into a GtkWidgetClass field Gtk 4 no longer has, and the
            // subclass could not be constructed at all.
            Run(() =>
            {
                var widget = new ActivatingWidget();

                Assert.True(widget.Activate(), "Activate should report that the class has an activation signal");
                Assert.Equal(1, widget.Activations);

                // ... and the signal really is a signal: it can be emitted by
                // name, which is a different route into the same closure.
                GLib.Signal.Emit(widget, "activate_signal");
                Assert.Equal(2, widget.Activations);
            });
        }

        [Fact]
        public void A_handler_connected_to_the_declared_signal_runs_beside_the_override()
        {
            Run(() =>
            {
                var widget = new ActivatingWidget();
                var handled = 0;
                object sender = null;

                widget.AddSignalHandler("activate_signal",
                                        new System.EventHandler((o, e) => { handled++; sender = o; }));

                widget.Activate();

                Assert.Equal(1, widget.Activations);
                Assert.Equal(1, handled);
                Assert.Same(widget, sender);
            });
        }

        [Fact]
        public void A_widget_that_does_not_override_OnActivate_has_no_such_signal()
        {
            // The signal belongs to the subclass that asked for it, not to
            // Gtk.Widget: a sibling subclass has neither the signal nor an
            // activation.
            Run(() =>
            {
                var inert = new InertWidget();

                Assert.False(inert.Activate());
                Assert.Throws<ArgumentException>(() => GLib.Signal.Emit(inert, "activate_signal"));
            });
        }

        [Fact]
        public void Activating_one_instance_leaves_its_siblings_alone()
        {
            // The signal is registered on the GType, which the two instances
            // share; the closure still has to dispatch to the instance it was
            // emitted on.
            Run(() =>
            {
                var first = new ActivatingWidget();
                var second = new ActivatingWidget();

                first.Activate();
                first.Activate();
                second.Activate();

                Assert.Equal(2, first.Activations);
                Assert.Equal(1, second.Activations);
            });
        }

        // ------------------------------------------------- overriding virtual methods

        [Fact]
        public void An_override_of_OnMeasure_is_what_the_parent_adds_up()
        {
            // Measuring the subclass directly would prove nothing -- C# would
            // dispatch to the override whether or not Gtk knew about it. Asking
            // the *box* means the number can only have come through the class
            // struct.
            Run(() =>
            {
                var box = new Box(Orientation.Vertical, 7);
                box.Append(new FixedSizeWidget());
                box.Append(new FixedSizeWidget());
                box.Append(new FixedSizeWidget());

                box.Measure(Orientation.Vertical, -1, out var minimum, out var natural, out _, out _);

                // three children of 40, two gaps of 7
                Assert.Equal(3 * 40 + 2 * 7, minimum);
                Assert.Equal(3 * 40 + 2 * 7, natural);
            });
        }

        [Fact]
        public void Chaining_to_base_from_an_override_reaches_the_implementation_below_it()
        {
            // base.OnMeasure does not mean "the C# method I inherited" -- there
            // isn't one, GtkLabel's measure is C. It goes through
            // class_abi.BaseOverride, which reads the vtable slot of the parent
            // GType, so the number added to is gtk_label_measure's own answer.
            Run(() =>
            {
                const string text = "a line of text to measure";

                var plain = new Label(text);
                plain.Measure(Orientation.Horizontal, -1, out var plainWidth, out _, out _, out _);
                Assert.True(plainWidth > 0, "a label with text must want some width");

                var padded = new PaddedLabel(text);
                padded.Measure(Orientation.Horizontal, -1, out var paddedWidth, out _, out _, out _);

                Assert.Equal(plainWidth + PaddedLabel.Padding, paddedWidth);
            });
        }

        [Fact]
        public void An_override_two_levels_up_still_runs_for_the_deeper_subclass()
        {
            // DeeperLabel adds nothing, so its behaviour is PaddedLabel's --
            // which is only true if the vfunc pointer PaddedLabel installed is
            // inherited rather than reset when DeeperLabel registers its own
            // class.
            Run(() =>
            {
                const string text = "a line of text to measure";

                var padded = new PaddedLabel(text);
                padded.Measure(Orientation.Horizontal, -1, out var paddedWidth, out _, out _, out _);

                var deeper = new DeeperLabel(text);
                deeper.Measure(Orientation.Horizontal, -1, out var deeperWidth, out _, out _, out _);

                Assert.Equal(paddedWidth, deeperWidth);
            });
        }

        [Fact]
        public void A_layout_manager_answers_instead_of_the_measure_override_on_its_widget()
        {
            // Found by an override that did nothing, and it is not a binding
            // defect: Gtk 4 asks a widget's layout manager to measure it and
            // only falls back to the class's measure vfunc when there is none.
            // GtkBox always has a GtkBoxLayout, so overriding OnMeasure on a Box
            // subclass is silently inert -- the override is installed in the
            // class struct (this test proves it, by clearing the layout manager
            // and watching the same override take effect) and simply never
            // consulted. A Box subclass that wants to measure differently has to
            // replace the layout manager, not override the vfunc.
            Run(() =>
            {
                var reference = new Box(Orientation.Horizontal, 0);
                reference.Append(new Label("x"));
                reference.Measure(Orientation.Horizontal, -1, out var boxWidth, out _, out _, out _);

                var subclass = new MeasuringBox(Orientation.Horizontal, 0);
                subclass.Append(new Label("x"));

                Assert.IsType<BoxLayout>(subclass.LayoutManager);

                subclass.Measure(Orientation.Horizontal, -1, out var withLayout, out _, out _, out _);

                Assert.Equal(boxWidth, withLayout);
                Assert.Equal(0, subclass.Measures);

                subclass.LayoutManager = null;
                subclass.Measure(Orientation.Horizontal, -1, out var withoutLayout, out _, out _, out _);

                Assert.Equal(MeasuringBox.Wanted, withoutLayout);
                Assert.Equal(1, subclass.Measures);
            });
        }

        [Fact]
        public void A_subclass_that_overrides_nothing_measures_exactly_like_its_base()
        {
            // The other half of the same guard: registering a GType and patching
            // the class struct must not disturb the slots the subclass did not
            // ask about. A Label, so that the answer really does come from a
            // measure vfunc rather than from a layout manager.
            Run(() =>
            {
                const string text = "some text worth measuring";

                var plain = new Label(text);
                plain.Measure(Orientation.Horizontal, -1, out var plainWidth, out _, out _, out _);

                var inert = new InertLabel(text);
                inert.Measure(Orientation.Horizontal, -1, out var inertWidth, out _, out _, out _);

                Assert.True(plainWidth > 0);
                Assert.Equal(plainWidth, inertWidth);
            });
        }

        [Fact]
        public void An_override_of_OnGetRequestMode_is_what_the_widget_reports()
        {
            // A vfunc with a return value rather than out-parameters, so the
            // marshalling back into C is a different code path.
            Run(() =>
            {
                Assert.Equal(SizeRequestMode.ConstantSize, new InertWidget().RequestMode);
                Assert.Equal(SizeRequestMode.HeightForWidth, new HeightForWidthWidget().RequestMode);
            });
        }

        [Fact]
        public void OnSizeAllocate_receives_the_size_the_parent_decided_on()
        {
            // A bare Gtk.Widget subclass has no layout manager, so its
            // size_allocate vfunc is the one Gtk uses. On a Box subclass this
            // override would never run, for the same reason OnMeasure does not.
            Run(() =>
            {
                var box = new Box(Orientation.Horizontal, 0);
                var child = new RecordingWidget();
                box.Append(child);

                // Gtk refuses to allocate a widget it has not measured.
                box.Measure(Orientation.Horizontal, -1, out _, out _, out _, out _);
                box.Measure(Orientation.Vertical, -1, out _, out _, out _, out _);
                box.Allocate(200, 100, -1, null);

                Assert.Equal(1, child.Allocations);
                // The child asked for 40 wide, and a horizontal box gives it no
                // more; the full height is its to fill.
                Assert.Equal(40, child.AllocatedWidth);
                Assert.Equal(100, child.AllocatedHeight);
                Assert.Equal(40, child.Width);
                Assert.Equal(100, child.Height);
            });
        }

        [Fact]
        public void OnRoot_and_OnUnroot_bracket_the_widget_s_time_inside_a_window()
        {
            Run(() =>
            {
                var window = new Window();
                var child = new RecordingWidget();

                Assert.Null(child.Root);

                window.Child = child;
                Assert.Equal(1, child.Roots);
                Assert.Equal(0, child.Unroots);
                Assert.Same(window, child.Root);

                window.Child = null;
                Assert.Equal(1, child.Roots);
                Assert.Equal(1, child.Unroots);
                Assert.Null(child.Root);

                window.Destroy();
            });
        }

        [Fact]
        public void An_unmapped_widget_is_never_snapshotted()
        {
            // The trap this guards: writing an OnSnapshot override, asking its
            // parent to snapshot it, getting an empty tree back, and concluding
            // the override is not being reached. Gtk skips a widget that is not
            // mapped, so a snapshot test that does not show its window asserts
            // nothing at all.
            Run(() =>
            {
                var box = new Box(Orientation.Horizontal, 0);
                var child = new RecordingWidget();
                box.Append(child);

                Assert.False(child.IsMapped);

                var snapshot = new Snapshot();
                box.SnapshotChild(child, snapshot);

                Assert.Equal(0, child.Snapshots);
                Assert.Null(snapshot.ToNode());
            });
        }

        [Fact]
        public void A_presented_window_realizes_and_snapshots_the_managed_widget_inside_it()
        {
            Run(() =>
            {
                var window = new Window();
                var box = new Box(Orientation.Horizontal, 0);
                var child = new RecordingWidget();
                box.Append(child);
                window.Child = box;

                window.Present();
                Pump();

                Assert.True(child.IsRealized, "presenting the window should realize its children");
                Assert.Equal(1, child.Realizes);
                Assert.True(child.Snapshots > 0, "a mapped widget must be asked to draw itself");

                window.Destroy();
            });
        }

        [Fact]
        public void The_node_an_OnSnapshot_override_appends_is_what_the_parent_collects()
        {
            // The oracle is the rectangle the override chose, which nothing else
            // in the process knows about: if the node came from Gtk's default
            // implementation instead it would carry the widget's own size.
            Run(() =>
            {
                var window = new Window();
                var box = new Box(Orientation.Horizontal, 0);
                var child = new RecordingWidget();
                box.Append(child);
                window.Child = box;

                window.Present();
                Pump();

                var snapshot = new Snapshot();
                box.SnapshotChild(child, snapshot);
                var node = snapshot.ToNode();

                Assert.NotNull(node);
                Assert.Equal(RecordingWidget.PaintWidth, node.Bounds.Width, 3);
                Assert.Equal(RecordingWidget.PaintHeight, node.Bounds.Height, 3);

                window.Destroy();
            });
        }

        // -------------------------------------------------- a custom layout manager

        [Fact]
        public void A_custom_layout_manager_decides_what_the_widget_measures()
        {
            // A GtkWidget with no measure vfunc of its own defers to its layout
            // manager, so this proves Gtk reached the managed OnMeasure through
            // two levels of indirection -- and that the widget handed to it is
            // the managed wrapper, not a fresh one.
            Run(() =>
            {
                var box = new Box(Orientation.Horizontal, 0);
                box.Append(new Label("ignored"));
                var layout = new StaircaseLayout();
                box.LayoutManager = layout;

                box.Measure(Orientation.Horizontal, -1, out var width, out _, out _, out _);
                box.Measure(Orientation.Vertical, -1, out var height, out _, out _, out _);

                Assert.Equal(StaircaseLayout.Wanted, width);
                Assert.Equal(StaircaseLayout.Wanted, height);
                Assert.Equal(2, layout.Measures);
                Assert.Same(box, layout.MeasuredWidget);
            });
        }

        [Fact]
        public void A_custom_layout_manager_puts_children_where_its_transforms_say()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Horizontal, 0);
                var first = new Label("first");
                var second = new Label("second");
                var third = new Label("third");
                box.Append(first);
                box.Append(second);
                box.Append(third);

                var layout = new StaircaseLayout();
                box.LayoutManager = layout;

                box.Measure(Orientation.Horizontal, -1, out _, out _, out _, out _);
                box.Measure(Orientation.Vertical, -1, out _, out _, out _, out _);
                box.Allocate(300, 200, -1, null);

                Assert.Equal(1, layout.Allocations);
                Assert.Equal(300, layout.AllocatedWidth);
                Assert.Equal(200, layout.AllocatedHeight);

                var index = 0;
                foreach (var child in new[] { first, second, third })
                {
                    Assert.True(child.ComputeBounds(box, out var bounds));
                    Assert.Equal(StaircaseLayout.Step * index, bounds.X, 3);
                    Assert.Equal(2 * StaircaseLayout.Step * index, bounds.Y, 3);
                    Assert.Equal(StaircaseLayout.ChildWidth, bounds.Width, 3);
                    Assert.Equal(StaircaseLayout.ChildHeight, bounds.Height, 3);
                    index++;
                }
            });
        }

        [Fact]
        public void Allocating_a_child_with_no_transform_leaves_it_at_the_origin()
        {
            // A null GskTransform is the identity, and it is what a layout
            // manager passes for a child that needs no offset. Widget.Allocate
            // used to take the ownership of the transform away before checking
            // whether there was one, so this threw NullReferenceException from
            // inside the binding -- with no native call made and nothing in the
            // message naming the argument.
            Run(() =>
            {
                var box = new Box(Orientation.Horizontal, 0);
                var child = new Label("at the origin");
                box.Append(child);

                var layout = new StaircaseLayout { UseNullTransform = true };
                box.LayoutManager = layout;

                box.Measure(Orientation.Horizontal, -1, out _, out _, out _, out _);
                box.Measure(Orientation.Vertical, -1, out _, out _, out _, out _);
                box.Allocate(300, 200, -1, null);

                Assert.True(child.ComputeBounds(box, out var bounds));
                Assert.Equal(0, bounds.X, 3);
                Assert.Equal(0, bounds.Y, 3);
                Assert.Equal(StaircaseLayout.ChildWidth, bounds.Width, 3);
                Assert.Equal(StaircaseLayout.ChildHeight, bounds.Height, 3);
            });
        }

        // ---------------------------------------------------------- [Gtk.Template]

        /// <summary>
        /// The one composite-template type in the tree lives in the samples, and
        /// is internal there, so it is reached the way the section tests reach
        /// everything else.
        /// </summary>
        private static Type TemplateType()
        {
            return typeof(Samples.SectionAttribute).Assembly.GetType("Samples.CompositeWidget");
        }

        private static object ChildField(Type type, object instance, string name)
        {
            var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            return field.GetValue(instance);
        }

        [Fact]
        public void A_template_widget_binds_its_child_fields_from_the_markup()
        {
            // [Template] installs the .ui on the class and [Child] binds fields
            // to ids in it; the oracle is the markup, which sets those labels
            // and nothing in C# repeats them.
            Run(() =>
            {
                var type = TemplateType();
                Assert.NotNull(type);

                var widget = (Widget) Activator.CreateInstance(type);

                var first = Assert.IsType<Button>(ChildField(type, widget, "btn1"));
                var second = Assert.IsType<Button>(ChildField(type, widget, "btn2"));
                var entry = Assert.IsType<Entry>(ChildField(type, widget, "entry"));

                Assert.Equal("Instance handler", first.Label);
                Assert.Equal("Static handler", second.Label);

                // The .ui puts the two buttons inside a box and the entry after
                // it, so the widget tree has to agree with the file.
                var row = Assert.IsType<Box>(widget.FirstChild);
                Assert.Same(first, row.FirstChild);
                Assert.Same(second, row.LastChild);
                Assert.Same(entry, widget.LastChild);
            });
        }

        [Fact]
        public void Each_template_instance_gets_children_of_its_own()
        {
            // The template is installed once on the class; the children are
            // built per instance. Sharing them would make two windows show the
            // same widget, which is the kind of thing that looks like a
            // repainting bug rather than a binding one.
            Run(() =>
            {
                var type = TemplateType();

                var first = (Widget) Activator.CreateInstance(type);
                var second = (Widget) Activator.CreateInstance(type);

                var firstEntry = Assert.IsType<Entry>(ChildField(type, first, "entry"));
                var secondEntry = Assert.IsType<Entry>(ChildField(type, second, "entry"));

                Assert.NotSame(firstEntry, secondEntry);

                firstEntry.Text = "only this one";
                Assert.Equal("", secondEntry.Text);
            });
        }

        // -------------------------------------------------------------- helpers

        /// <summary>Lets Gtk get as far as drawing, without waiting on anything.</summary>
        private static void Pump()
        {
            for (var i = 0; i < 500 && Application.EventsPending(); i++)
                Application.RunIteration(false);
        }
    }
}

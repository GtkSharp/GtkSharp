using System;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.Object</c> and <c>GLib.Value</c> — the wrapper lifecycle and the
    /// boxing layer that every property, signal argument and list element in the
    /// library passes through. The earlier <c>MarshallingTests</c> covered the
    /// common conversions; these reach the wrapper identity map, notifications,
    /// per-object data, and the <c>Value</c> cases the numeric round-trips do not:
    /// boxed opaques, GTypes, enums as flags, and a managed object carried through
    /// unmanaged code.
    /// </summary>
    public class ObjectAndValueTests : GtkTestBase
    {
        public ObjectAndValueTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------ wrapper identity

        [Fact]
        public void The_same_native_object_always_comes_back_as_the_same_wrapper()
        {
            // Two wrappers over one native object would each try to own it, and
            // the second to be finalised would unref something already freed.
            // This is the invariant the whole Objects map exists to hold.
            Run(() =>
            {
                var label = new Gtk.Label("identity");

                var again = GLib.Object.GetObject(label.Handle);
                var third = GLib.Object.TryGetObject(label.Handle);

                Assert.Same(label, again);
                Assert.Same(label, third);
            });
        }

        [Fact]
        public void TryGetObject_returns_null_for_an_address_no_wrapper_holds()
        {
            // TryGetObject differs from GetObject exactly here: it does not
            // manufacture a wrapper, so it can be asked without side effects.
            Run(() =>
            {
                Assert.Null(GLib.Object.TryGetObject(IntPtr.Zero));
            });
        }

        [Fact]
        public void GetObject_of_the_null_pointer_is_null_rather_than_a_dead_wrapper()
        {
            Run(() =>
            {
                Assert.Null(GLib.Object.GetObject(IntPtr.Zero));
                Assert.Null(GLib.Object.GetObject(IntPtr.Zero, true));
            });
        }

        [Fact]
        public void An_object_reports_the_native_type_it_actually_is()
        {
            Run(() =>
            {
                var button = new Gtk.Button();

                Assert.Equal(Gtk.Button.GType, button.NativeType);
                Assert.NotEqual(Gtk.Label.GType, button.NativeType);

                // And the managed type resolves back from the GType.
                Assert.Equal(typeof(Gtk.Button), (Type) button.NativeType);
            });
        }

        [Fact]
        public void A_widget_retrieved_through_a_container_is_the_wrapper_that_was_added()
        {
            Run(() =>
            {
                var box = new Gtk.Box(Gtk.Orientation.Vertical, 0);
                var child = new Gtk.Label("child");

                box.Append(child);

                Assert.Same(child, box.FirstChild);
                Assert.Same(box, child.Parent);
            });
        }

        // ----------------------------------------------------------- properties

        [Fact]
        public void GetProperty_and_SetProperty_agree_with_the_typed_accessor()
        {
            Run(() =>
            {
                var window = new Gtk.Window();

                window.SetProperty("title", new GLib.Value("by property"));

                Assert.Equal("by property", window.Title);
                Assert.Equal("by property", (string) window.GetProperty("title"));

                window.Title = "by accessor";

                Assert.Equal("by accessor", (string) window.GetProperty("title"));
            });
        }

        [Fact]
        public void A_boolean_and_an_integer_property_round_trip_through_Value()
        {
            Run(() =>
            {
                var window = new Gtk.Window();

                window.SetProperty("resizable", new GLib.Value(false));
                window.SetProperty("default-width", new GLib.Value(321));

                Assert.False((bool) window.GetProperty("resizable"));
                Assert.Equal(321, (int) window.GetProperty("default-width"));
                Assert.False(window.Resizable);
                Assert.Equal(321, window.DefaultWidth);
            });
        }

        [Fact]
        public void An_enum_property_round_trips_through_Value()
        {
            Run(() =>
            {
                var box = new Gtk.Box(Gtk.Orientation.Vertical, 0);

                box.SetProperty("orientation", new GLib.Value(Gtk.Orientation.Horizontal));

                Assert.Equal(Gtk.Orientation.Horizontal, box.Orientation);
                Assert.Equal(Gtk.Orientation.Horizontal,
                             (Gtk.Orientation) (Enum) box.GetProperty("orientation"));
            });
        }

        [Fact]
        public void An_object_valued_property_hands_back_the_same_wrapper()
        {
            Run(() =>
            {
                var child = new Gtk.Label("held");
                var window = new Gtk.Window();

                window.SetProperty("child", new GLib.Value(child));

                Assert.Same(child, (GLib.Object) window.GetProperty("child"));
                Assert.Same(child, window.Child);
            });
        }

        // -------------------------------------------------------- notifications

        [Fact]
        public void A_notification_fires_when_the_named_property_changes()
        {
            Run(() =>
            {
                var window = new Gtk.Window();
                var notifications = 0;
                string seen = null;

                void OnTitle(object o, GLib.NotifyArgs args)
                {
                    notifications++;
                    seen = ((Gtk.Window) o).Title;
                }

                window.AddNotification("title", OnTitle);
                try
                {
                    window.Title = "changed";

                    Assert.Equal(1, notifications);
                    Assert.Equal("changed", seen);
                }
                finally
                {
                    window.RemoveNotification("title", OnTitle);
                }
            });
        }

        [Fact]
        public void A_removed_notification_stops_firing()
        {
            Run(() =>
            {
                var window = new Gtk.Window();
                var notifications = 0;

                void OnTitle(object o, GLib.NotifyArgs args) => notifications++;

                window.AddNotification("title", OnTitle);
                window.Title = "first";
                Assert.Equal(1, notifications);

                window.RemoveNotification("title", OnTitle);
                window.Title = "second";

                Assert.Equal(1, notifications);
            });
        }

        [Fact]
        public void A_notification_on_one_property_does_not_fire_for_another()
        {
            Run(() =>
            {
                var window = new Gtk.Window();
                var notifications = 0;

                void OnTitle(object o, GLib.NotifyArgs args) => notifications++;

                window.AddNotification("title", OnTitle);
                try
                {
                    window.Resizable = false;

                    Assert.Equal(0, notifications);

                    window.Title = "now it should";

                    Assert.Equal(1, notifications);
                }
                finally
                {
                    window.RemoveNotification("title", OnTitle);
                }
            });
        }

        [Fact]
        public void An_unnamed_notification_hears_every_property()
        {
            Run(() =>
            {
                var window = new Gtk.Window();
                var seen = new System.Collections.Generic.List<string>();

                void OnAny(object o, GLib.NotifyArgs args) => seen.Add(args.Property);

                window.AddNotification(OnAny);
                try
                {
                    window.Title = "a title";
                    window.Resizable = false;

                    Assert.Contains("title", seen);
                    Assert.Contains("resizable", seen);
                }
                finally
                {
                    window.RemoveNotification(OnAny);
                }
            });
        }

        // ---------------------------------------------------------------- data

        [Fact]
        public void Arbitrary_managed_data_can_be_hung_off_an_object_and_read_back()
        {
            // Object.Data is a managed side-table keyed by the wrapper, which is
            // how a caller attaches state to a widget it did not write.
            Run(() =>
            {
                var label = new Gtk.Label("carrier");

                label.Data["note"] = "remembered";
                label.Data[42] = new[] { 1, 2, 3 };

                Assert.Equal("remembered", label.Data["note"]);
                Assert.Equal(new[] { 1, 2, 3 }, (int[]) label.Data[42]);
                Assert.Null(label.Data["never set"]);
            });
        }

        [Fact]
        public void Data_is_per_object_rather_than_shared()
        {
            Run(() =>
            {
                var first = new Gtk.Label("first");
                var second = new Gtk.Label("second");

                first.Data["key"] = "first value";
                second.Data["key"] = "second value";

                Assert.Equal("first value", first.Data["key"]);
                Assert.Equal("second value", second.Data["key"]);
            });
        }

        // --------------------------------------------------------- GLib.Value

        [Fact]
        public void A_boxed_opaque_survives_a_round_trip_through_a_Value()
        {
            Run(() =>
            {
                // Value(Opaque, string) resolves the type by name through
                // GType.FromName, which asks GObject -- so it only finds a type
                // the native library has already registered. Reading the
                // binding's GType property is what forces that registration;
                // without it the name resolves to nothing, g_value_init does
                // nothing, and the value reads back as null.
                var gtype = Pango.FontDescription.GType;
                Assert.NotEqual(GLib.GType.Invalid, gtype);

                var original = Pango.FontDescription.FromString("Serif Bold 12");

                var value = new GLib.Value(original, "PangoFontDescription");
                var read = (Pango.FontDescription) (GLib.Opaque) value;

                Assert.NotNull(read);
                Assert.Equal("Serif", read.Family);
                Assert.Equal(Pango.Weight.Bold, read.Weight);
            });
        }

        [Fact]
        public void The_GType_constructor_makes_an_empty_value_of_that_type_not_one_holding_it()
        {
            // A trap worth naming: new Value(GType.String) reads as "a value
            // holding GType.String" and means "an empty value whose type is
            // string". There is an explicit (GType) cast on Value, which makes
            // the pair look symmetric, but it calls g_value_get_gtype -- so
            // applying it here trips a GLib assertion rather than returning the
            // GType that went in. Nothing constructs a value that *holds* a
            // GType, and the cast is only for values that arrived from C.
            Run(() =>
            {
                var value = new GLib.Value(GLib.GType.String);

                Assert.Null(value.Val);
            });
        }

        [Fact]
        public void An_IntPtr_survives_a_round_trip_through_a_Value()
        {
            Run(() =>
            {
                var pointer = new IntPtr(0x1234);

                Assert.Equal(pointer, (IntPtr) new GLib.Value(pointer));
            });
        }

        [Fact]
        public void A_managed_object_that_is_not_a_GObject_survives_a_Value()
        {
            // ManagedValue boxes an arbitrary CLR object into a GValue so it can
            // be carried through native code and come back the same instance.
            Run(() =>
            {
                var carried = new System.Text.StringBuilder("state");

                var value = new GLib.Value(carried);

                Assert.Same(carried, value.Val);
            });
        }

        [Fact]
        public void A_value_array_keeps_what_was_appended_to_it()
        {
            Run(() =>
            {
                var array = new GLib.ValueArray(2u);
                array.Append(new GLib.Value("first"));
                array.Append(new GLib.Value(2));

                Assert.Equal(2, array.Count);

                // The indexer is typed object and hands back a boxed GLib.Value,
                // so it has to be unboxed before Value's explicit operators are
                // in play -- (string) array[0] is a reference cast that throws.
                Assert.Equal("first", (string) (GLib.Value) array[0]);
                Assert.Equal(2, (int) (GLib.Value) array[1]);
            });
        }

        [Fact]
        public void A_value_array_survives_a_round_trip_through_a_Value()
        {
            Run(() =>
            {
                var array = new GLib.ValueArray(1u);
                array.Append(new GLib.Value("carried"));

                var read = (GLib.ValueArray) new GLib.Value(array);

                Assert.Equal(1, read.Count);
                Assert.Equal("carried", (string) (GLib.Value) read[0]);
            });
        }

        [Fact]
        public void Disposing_a_Value_twice_does_not_throw()
        {
            // Every generated property getter disposes the Value it read, and
            // some paths dispose again on the way out.
            Run(() =>
            {
                var value = new GLib.Value("disposable");

                value.Dispose();
                value.Dispose();
            });
        }

        [Fact]
        public void The_empty_Value_reads_as_nothing_rather_than_crashing()
        {
            Run(() =>
            {
                var empty = GLib.Value.Empty;

                Assert.Null(empty.Val);
            });
        }

        // ----------------------------------------------------------- GLib.GType

        [Fact]
        public void A_GType_resolves_to_the_managed_type_that_registered_it()
        {
            Run(() =>
            {
                Assert.Equal(typeof(Gtk.Window), (Type) Gtk.Window.GType);
                Assert.Equal(typeof(Gtk.Label), (Type) Gtk.Label.GType);
                Assert.Equal(typeof(Pango.FontDescription), (Type) Pango.FontDescription.GType);
            });
        }

        [Fact]
        public void A_managed_type_resolves_to_the_GType_it_registered()
        {
            Run(() =>
            {
                Assert.Equal(Gtk.Window.GType, (GLib.GType) typeof(Gtk.Window));
                Assert.Equal(GLib.GType.String, (GLib.GType) typeof(string));
                Assert.Equal(GLib.GType.Int, (GLib.GType) typeof(int));
                Assert.Equal(GLib.GType.Boolean, (GLib.GType) typeof(bool));
            });
        }

        [Fact]
        public void GTypes_compare_by_the_type_they_name()
        {
            Run(() =>
            {
                Assert.True(Gtk.Window.GType == (GLib.GType) typeof(Gtk.Window));
                Assert.True(Gtk.Window.GType != Gtk.Label.GType);
                Assert.Equal(Gtk.Window.GType.GetHashCode(),
                             ((GLib.GType) typeof(Gtk.Window)).GetHashCode());
            });
        }

        [Fact]
        public void A_GType_reports_its_native_name()
        {
            Run(() =>
            {
                Assert.Equal("GtkWindow", Gtk.Window.GType.ToString());
                Assert.Equal("gchararray", GLib.GType.String.ToString());
            });
        }

        [Fact]
        public void A_number_that_lands_on_the_raw_pointer_constructor_is_refused()
        {
            // Since .NET 7 IntPtr is nint and int converts to it implicitly, so
            // "new ValueArray (2)" -- which reads as the preallocation count and
            // is what anyone would write -- binds to ValueArray (IntPtr) and
            // dereferences address 2. That was an access violation that killed
            // the process rather than anything a caller could catch.
            //
            // The wrapper libraries are LangVersion 9, where the conversion does
            // not exist, so this only bites consumers -- which is to say
            // everyone using the NuGet package.
            Run(() =>
            {
                Assert.Throws<ArgumentException>(() => new GLib.ValueArray(new IntPtr(2)));
                Assert.Throws<ArgumentException>(() => new GLib.Date(new IntPtr(2)));
                Assert.Throws<ArgumentException>(() => new GLib.DateTime(new IntPtr(2)));

                // The numeric overloads, reached with a suffix, still work.
                Assert.Equal(0, new GLib.ValueArray(2u).Count);
            });
        }
    }
}

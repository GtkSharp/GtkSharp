using System;
using System.Collections;
using System.Text;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.Value</c> and <c>GLib.Marshaller</c> sit under every other binding
    /// in the library: a property read, a signal argument and a list element all
    /// pass through them. A defect here is not local — it shows up somewhere else
    /// entirely, as the <c>guint</c>-unboxed-as-<c>int</c> abort during the Gtk 4
    /// port did.
    /// </summary>
    public class MarshallingTests : GtkTestBase
    {
        public MarshallingTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------ GLib.Value

        [Fact]
        public void Every_numeric_width_survives_a_round_trip_through_a_Value()
        {
            Run(() =>
            {
                Assert.Equal(-42, (int) new GLib.Value(-42));
                Assert.Equal(4000000000u, (uint) new GLib.Value(4000000000u));
                Assert.Equal(long.MinValue, (long) new GLib.Value(long.MinValue));
                Assert.Equal(ulong.MaxValue, (ulong) new GLib.Value(ulong.MaxValue));
                Assert.Equal((byte) 200, (byte) new GLib.Value((byte) 200));
                Assert.Equal((sbyte) -100, (sbyte) new GLib.Value((sbyte) -100));
                Assert.Equal(1.5f, (float) new GLib.Value(1.5f), 5);
                Assert.Equal(1.0 / 3, (double) new GLib.Value(1.0 / 3), 12);
            });
        }

        [Fact]
        public void A_uint_above_int_MaxValue_does_not_come_back_negative()
        {
            // The GListModel items-changed abort during the port was exactly
            // this: a guint read as an int. At 4 000 000 000 the difference is
            // a wrong sign rather than a wrong-looking number.
            Run(() =>
            {
                var value = new GLib.Value(4000000000u);

                Assert.Equal(4000000000u, (uint) value);

                // Val boxes it back as the CLR type the GType stands for, and
                // it must be uint rather than int for the same reason.
                Assert.IsType<uint>(value.Val);
                Assert.Equal(4000000000u, (uint) value.Val);
            });
        }

        [Fact]
        public void A_boolean_and_a_string_round_trip()
        {
            Run(() =>
            {
                Assert.True((bool) new GLib.Value(true));
                Assert.False((bool) new GLib.Value(false));
                Assert.Equal("text", (string) new GLib.Value("text"));
                Assert.Equal("ünïcode ✓", (string) new GLib.Value("ünïcode ✓"));
                Assert.Null((string) new GLib.Value((string) null));
            });
        }

        [Fact]
        public void A_string_array_round_trips_as_a_strv()
        {
            Run(() =>
            {
                var value = new GLib.Value(new[] { "a", "b", "c" });

                Assert.Equal(new[] { "a", "b", "c" }, (string[]) value);
            });
        }

        [Fact]
        public void Val_boxes_a_Value_back_as_the_CLR_type_its_GType_stands_for()
        {
            Run(() =>
            {
                Assert.IsType<int>(new GLib.Value(1).Val);
                Assert.IsType<string>(new GLib.Value("s").Val);
                Assert.IsType<bool>(new GLib.Value(true).Val);
                Assert.IsType<double>(new GLib.Value(1.0).Val);
                Assert.IsType<long>(new GLib.Value(1L).Val);
                Assert.IsType<float>(new GLib.Value(1.0f).Val);
            });
        }

        [Fact]
        public void An_enum_round_trips_and_keeps_its_own_type()
        {
            Run(() =>
            {
                var value = new GLib.Value(Gtk.Orientation.Vertical);

                Assert.Equal(Gtk.Orientation.Vertical, (Gtk.Orientation) (Enum) value);
            });
        }

        [Fact]
        public void A_GObject_round_trips_as_the_same_wrapper()
        {
            Run(() =>
            {
                var label = new Gtk.Label("held in a Value");

                var value = new GLib.Value(label);

                Assert.Same(label, (GLib.Object) value);
            });
        }

        [Fact]
        public void A_Value_carries_a_live_property_in_both_directions()
        {
            // This is the path every generated property accessor takes, so it
            // is worth asserting against the widget's own accessor. Note that
            // the Value(object, name) constructor only initialises a Value of
            // the property's *type* -- it does not read the property, which is
            // what GetProperty is for.
            Run(() =>
            {
                var label = new Gtk.Label("initial");

                Assert.Equal("initial", (string) label.GetProperty("label"));

                label.SetProperty("label", new GLib.Value("assigned"));

                Assert.Equal("assigned", label.Text);
                Assert.Equal("assigned", (string) label.GetProperty("label"));
            });
        }

        [Fact]
        public void An_uninitialised_property_Value_takes_the_type_but_not_the_contents()
        {
            Run(() =>
            {
                var label = new Gtk.Label("initial");

                var empty = new GLib.Value(label, "label");

                // The type is right, so it can be handed to g_object_get as an
                // out parameter -- but nothing has been read into it yet.
                Assert.Null((string) empty);
            });
        }

        [Fact]
        public void The_untyped_Value_constructor_picks_the_GType_from_the_object()
        {
            Run(() =>
            {
                Assert.Equal(7, new GLib.Value((object) 7).Val);
                Assert.Equal("s", new GLib.Value((object) "s").Val);
                Assert.Equal(true, new GLib.Value((object) true).Val);
                Assert.Equal(2.5, new GLib.Value((object) 2.5).Val);
            });
        }

        [Fact]
        public void A_Variant_survives_being_carried_in_a_Value()
        {
            Run(() =>
            {
                var variant = new GLib.Variant("carried");

                var read = (GLib.Variant) new GLib.Value(variant);

                Assert.Equal("carried", (string) read);
            });
        }

        // ------------------------------------------------------- GLib.Marshaller

        [Fact]
        public void A_string_survives_a_trip_through_unmanaged_memory()
        {
            Run(() =>
            {
                const string text = "round trip — ünïcode ✓ 日本語";

                var native = GLib.Marshaller.StringToPtrGStrdup(text);
                try
                {
                    Assert.Equal(text, GLib.Marshaller.Utf8PtrToString(native));
                }
                finally
                {
                    GLib.Marshaller.Free(native);
                }
            });
        }

        [Fact]
        public void A_null_string_marshals_to_a_null_pointer_and_back()
        {
            Run(() =>
            {
                Assert.Equal(IntPtr.Zero, GLib.Marshaller.StringToPtrGStrdup(null));
                Assert.Null(GLib.Marshaller.Utf8PtrToString(IntPtr.Zero));
            });
        }

        [Fact]
        public void An_empty_string_stays_empty_rather_than_becoming_null()
        {
            Run(() =>
            {
                var native = GLib.Marshaller.StringToPtrGStrdup(string.Empty);
                try
                {
                    Assert.NotEqual(IntPtr.Zero, native);
                    Assert.Equal(string.Empty, GLib.Marshaller.Utf8PtrToString(native));
                }
                finally
                {
                    GLib.Marshaller.Free(native);
                }
            });
        }

        [Fact]
        public void A_string_array_survives_a_trip_through_a_null_terminated_strv()
        {
            Run(() =>
            {
                var strings = new[] { "one", "two", "three" };

                var native = GLib.Marshaller.StringArrayToStrvPtr(strings);

                // owned: true frees the strv on the way back out.
                Assert.Equal(strings, GLib.Marshaller.NullTermPtrToStringArray(native, true));
            });
        }

        [Fact]
        public void A_byte_array_survives_a_trip_through_unmanaged_memory()
        {
            Run(() =>
            {
                var bytes = new byte[] { 0, 1, 127, 128, 255 };

                var native = GLib.Marshaller.ArrayToArrayPtr(bytes);

                Assert.Equal(bytes, GLib.Marshaller.ArrayPtrToArray<byte>(native, bytes.Length, true));
            });
        }

        [Fact]
        public void A_unichar_converts_to_a_char_and_back()
        {
            Run(() =>
            {
                Assert.Equal('A', GLib.Marshaller.GUnicharToChar('A'));
                Assert.Equal((uint) 'A', GLib.Marshaller.CharToGUnichar('A'));

                // 0x00E9 is 'é' — two bytes in UTF-8, one char in UTF-16.
                Assert.Equal('é', GLib.Marshaller.GUnicharToChar(0x00E9));
                Assert.Equal("é", GLib.Marshaller.GUnicharToString(0x00E9));
            });
        }

        [Fact]
        public void A_managed_DateTime_survives_a_trip_through_time_t()
        {
            Run(() =>
            {
                // time_t has one-second resolution, so the input has none finer.
                var when = new DateTime(2015, 4, 2, 11, 22, 33, DateTimeKind.Utc);

                var native = GLib.Marshaller.DateTimeTotime_t(when);

                Assert.Equal(when, GLib.Marshaller.time_tToDateTime(native));
            });
        }

        // ------------------------------------------------- GLib.List and PtrArray

        [Fact]
        public void A_GLib_List_of_strings_keeps_what_was_appended_in_order()
        {
            Run(() =>
            {
                using var list = new GLib.List(typeof(string));
                list.Append("first");
                list.Append("second");

                Assert.Equal(2, list.Count);
                Assert.Equal("first", list[0]);
                Assert.Equal("second", list[1]);
            });
        }

        [Fact]
        public void Prepending_to_a_GLib_List_puts_the_item_at_the_front()
        {
            Run(() =>
            {
                using var list = new GLib.List(typeof(string));
                list.Append("second");
                list.Prepend(GLib.Marshaller.StringToPtrGStrdup("first"));

                Assert.Equal("first", list[0]);
                Assert.Equal("second", list[1]);
            });
        }

        [Fact]
        public void A_GLib_List_enumerates_the_same_items_the_indexer_returns()
        {
            Run(() =>
            {
                using var list = new GLib.List(typeof(string));
                list.Append("a");
                list.Append("b");
                list.Append("c");

                var seen = new System.Collections.Generic.List<string>();
                foreach (string item in list)
                    seen.Add(item);

                Assert.Equal(new[] { "a", "b", "c" }, seen);
            });
        }

        [Fact]
        public void Emptying_a_list_it_owns_clears_it_and_one_it_does_not_own_is_left_alone()
        {
            // Empty only frees the native list when the wrapper owns it. That
            // is deliberate -- a borrowed list belongs to whatever handed it
            // over -- but it means the same call does nothing on the
            // constructor most callers reach for.
            Run(() =>
            {
                using var borrowed = new GLib.List(typeof(string));
                borrowed.Append("a");
                borrowed.Append("b");

                borrowed.Empty();

                Assert.Equal(2, borrowed.Count);

                using var owned = new GLib.List(IntPtr.Zero, typeof(string), true, true);
                owned.Append("a");
                owned.Append("b");

                owned.Empty();

                Assert.Equal(0, owned.Count);
            });
        }

        [Fact]
        public void A_GLib_List_copies_itself_into_an_array()
        {
            Run(() =>
            {
                using var list = new GLib.List(typeof(string));
                list.Append("x");
                list.Append("y");

                var target = new object[4];
                list.CopyTo(target, 1);

                Assert.Null(target[0]);
                Assert.Equal("x", target[1]);
                Assert.Equal("y", target[2]);
            });
        }

        [Fact]
        public void A_PtrArray_reports_what_was_added_to_it()
        {
            Run(() =>
            {
                using var array = new GLib.PtrArray(typeof(string), true, true);

                array.Add(GLib.Marshaller.StringToPtrGStrdup("alpha"));
                array.Add(GLib.Marshaller.StringToPtrGStrdup("beta"));

                Assert.Equal(2, array.Count);
                Assert.Equal("alpha", array[0]);
                Assert.Equal("beta", array[1]);
            });
        }

        [Fact]
        public void Removing_from_a_PtrArray_shortens_it()
        {
            Run(() =>
            {
                using var array = new GLib.PtrArray(typeof(string), true, true);

                var doomed = GLib.Marshaller.StringToPtrGStrdup("doomed");
                array.Add(GLib.Marshaller.StringToPtrGStrdup("kept"));
                array.Add(doomed);

                array.Remove(doomed);

                Assert.Equal(1, array.Count);
                Assert.Equal("kept", array[0]);
            });
        }

        // ------------------------------------------------------------ GLib.Bytes

        [Fact]
        public void A_Bytes_keeps_its_payload_and_length()
        {
            Run(() =>
            {
                var payload = Encoding.UTF8.GetBytes("bytes payload");

                var bytes = new GLib.Bytes(payload);

                Assert.Equal((ulong) payload.Length, bytes.Size);
                Assert.Equal(payload, bytes.Data);
            });
        }

        [Fact]
        public void Two_Bytes_with_the_same_payload_compare_equal()
        {
            Run(() =>
            {
                var first = new GLib.Bytes(new byte[] { 1, 2, 3 });
                var same = new GLib.Bytes(new byte[] { 1, 2, 3 });
                var different = new GLib.Bytes(new byte[] { 1, 2, 4 });

                Assert.Equal(0, first.CompareTo(same));
                Assert.NotEqual(0, first.CompareTo(different));
            });
        }
    }
}

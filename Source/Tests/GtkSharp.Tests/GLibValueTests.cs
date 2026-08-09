using System;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.Value</c> — the box every property read and write in the binding
    /// passes through.
    /// </summary>
    /// <remarks>
    /// 806 hand-written lines, roughly twenty constructors against roughly twenty
    /// explicit conversions, and `Docs/coverage.md` names it the first place
    /// worth more tests. A wrong conversion here is *silent*: you get a default
    /// or a truncated value rather than an error, and it surfaces as a property
    /// that will not take the number you gave it.
    ///
    /// The oracle is a round trip at the **edges** of each type. Storing 1 and
    /// reading 1 back proves almost nothing — a <c>long</c> kept in a 32-bit slot
    /// survives that and loses <c>long.MaxValue</c>. Every numeric test below
    /// therefore uses the extremes, which is where a wrong GType shows.
    ///
    /// <c>ObjectAndValueTests</c> covers the boxed, IntPtr, ValueArray and
    /// managed-object paths; this covers the scalars, the strings and the
    /// string arrays.
    /// </remarks>
    public class GLibValueTests : GtkTestBase
    {
        public GLibValueTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------- integers

        [Fact]
        public void An_int_survives_both_extremes()
        {
            Run(() =>
            {
                Assert.Equal(int.MaxValue, (int) new GLib.Value(int.MaxValue));
                Assert.Equal(int.MinValue, (int) new GLib.Value(int.MinValue));
                Assert.Equal(0, (int) new GLib.Value(0));
                Assert.Equal(-1, (int) new GLib.Value(-1));
            });
        }

        [Fact]
        public void A_uint_survives_the_range_an_int_cannot_hold()
        {
            // Above int.MaxValue is exactly where an unsigned value stored in a
            // signed slot comes back negative.
            Run(() =>
            {
                Assert.Equal(uint.MaxValue, (uint) new GLib.Value(uint.MaxValue));
                Assert.Equal(3_000_000_000u, (uint) new GLib.Value(3_000_000_000u));
                Assert.Equal(0u, (uint) new GLib.Value(0u));
            });
        }

        [Fact]
        public void A_long_survives_values_that_do_not_fit_in_32_bits()
        {
            // The truncation test. A long kept in a G_TYPE_INT would pass any
            // small-number round trip and fail this one.
            Run(() =>
            {
                Assert.Equal(long.MaxValue, (long) new GLib.Value(long.MaxValue));
                Assert.Equal(long.MinValue, (long) new GLib.Value(long.MinValue));
                Assert.Equal(1L << 40, (long) new GLib.Value(1L << 40));
            });
        }

        [Fact]
        public void A_ulong_survives_the_top_of_its_range()
        {
            Run(() =>
            {
                Assert.Equal(ulong.MaxValue, (ulong) new GLib.Value(ulong.MaxValue));
                Assert.Equal(1UL << 63, (ulong) new GLib.Value(1UL << 63));
            });
        }

        [Fact]
        public void The_small_integer_types_keep_their_edges()
        {
            Run(() =>
            {
                Assert.Equal(byte.MaxValue, (byte) new GLib.Value(byte.MaxValue));
                Assert.Equal((byte) 0, (byte) new GLib.Value((byte) 0));

                Assert.Equal(sbyte.MaxValue, (sbyte) new GLib.Value(sbyte.MaxValue));
                Assert.Equal(sbyte.MinValue, (sbyte) new GLib.Value(sbyte.MinValue));

                // ushort is stored in a G_TYPE_UINT, GLib having no 16-bit type,
                // so the narrowing happens on the way back out.
                Assert.Equal(ushort.MaxValue, (ushort) new GLib.Value(ushort.MaxValue));
                Assert.Equal((ushort) 0, (ushort) new GLib.Value((ushort) 0));
            });
        }

        // ------------------------------------------------------- floating point

        [Fact]
        public void A_double_keeps_precision_a_float_would_lose()
        {
            // 0.1 is not representable in binary, so a double that had been
            // through a float comes back visibly different.
            Run(() =>
            {
                Assert.Equal(0.1, (double) new GLib.Value(0.1));
                Assert.Equal(double.MaxValue, (double) new GLib.Value(double.MaxValue));
                Assert.Equal(-0.0, (double) new GLib.Value(-0.0));

                double precise = 1.0 / 3.0;
                Assert.Equal(precise, (double) new GLib.Value(precise));
            });
        }

        [Fact]
        public void A_float_round_trips_as_a_float()
        {
            Run(() =>
            {
                Assert.Equal(0.5f, (float) new GLib.Value(0.5f));
                Assert.Equal(float.MaxValue, (float) new GLib.Value(float.MaxValue));
                Assert.Equal(float.MinValue, (float) new GLib.Value(float.MinValue));
            });
        }

        [Fact]
        public void The_special_floating_point_values_survive()
        {
            // NaN and the infinities are the values a naive conversion through a
            // string or an int destroys.
            Run(() =>
            {
                Assert.True(double.IsNaN((double) new GLib.Value(double.NaN)));
                Assert.True(double.IsPositiveInfinity((double) new GLib.Value(double.PositiveInfinity)));
                Assert.True(double.IsNegativeInfinity((double) new GLib.Value(double.NegativeInfinity)));
            });
        }

        // -------------------------------------------------------------- others

        [Fact]
        public void A_bool_round_trips_both_ways()
        {
            Run(() =>
            {
                Assert.True((bool) new GLib.Value(true));
                Assert.False((bool) new GLib.Value(false));
            });
        }

        [Fact]
        public void A_string_round_trips_including_the_awkward_ones()
        {
            Run(() =>
            {
                Assert.Equal("ordinary", (string) new GLib.Value("ordinary"));
                Assert.Equal("", (string) new GLib.Value(""));
                Assert.Null((string) new GLib.Value((string) null));

                // The strings are marshalled as UTF-8; anything treating them as
                // ANSI comes back mangled rather than failing.
                Assert.Equal("café 日本語 🎉", (string) new GLib.Value("café 日本語 🎉"));

                // Embedded newlines and quotes are not special to a GValue.
                Assert.Equal("a\nb\"c", (string) new GLib.Value("a\nb\"c"));
            });
        }

        [Fact]
        public void A_string_array_round_trips_with_every_element_intact()
        {
            // A boxed GStrv: the constructor writes a NULL-terminated array of
            // g_strdup'd pointers and the conversion walks it back. Reading every
            // element matters, because a wrapper that returned only the first
            // would satisfy a one-element test.
            Run(() =>
            {
                var original = new[] { "alpha", "bravo", "charlie" };

                var read = (string[]) new GLib.Value(original);

                Assert.Equal(original, read);
            });
        }

        [Fact]
        public void An_empty_string_array_is_empty_rather_than_null()
        {
            // The NULL terminator is the only thing distinguishing an empty array
            // from an absent one, and they mean different things.
            Run(() =>
            {
                var read = (string[]) new GLib.Value(new string[0]);

                Assert.NotNull(read);
                Assert.Empty(read);
            });
        }

        [Fact]
        public void A_null_string_array_comes_back_as_null()
        {
            Run(() => Assert.Null((string[]) new GLib.Value((string[]) null)));
        }

        [Fact]
        public void A_string_array_keeps_non_ascii_elements()
        {
            Run(() =>
            {
                var original = new[] { "café", "日本語", "" };

                Assert.Equal(original, (string[]) new GLib.Value(original));
            });
        }

        [Fact]
        public void A_value_that_holds_a_GType_reads_back_as_that_type()
        {
            // Not `new GLib.Value(someGType)` -- that constructor makes an *empty
            // value of* that type, which ObjectAndValueTests pins separately, and
            // reading a GType out of it gives null. The first draft of this test
            // got that wrong.
            //
            // There is in fact no way to construct a GType-valued GLib.Value from
            // C#: the explicit conversion exists in one direction only, and the
            // constructor overload it would need is taken. So the value has to
            // come from something that already holds one -- GListModel's
            // item-type is the obvious candidate, and reading it is the path a
            // caller would really use.
            Run(() =>
            {
                var store = new GLib.ListStore((GLib.GType) typeof(ListModelTests.Row));

                var held = (GLib.GType) store.GetProperty("item-type");

                Assert.Equal((GLib.GType) typeof(ListModelTests.Row), held);
                Assert.NotEqual(GLib.GType.Invalid, held);
            });
        }

        [Fact]
        public void An_enum_and_a_flags_value_keep_what_they_were_given()
        {
            Run(() =>
            {
                var orientation = new GLib.Value(Gtk.Orientation.Vertical);
                Assert.Equal(Gtk.Orientation.Vertical, (Gtk.Orientation) (Enum) orientation);

                // Flags are a combination, and the combination is the point.
                var flags = new GLib.Value(Gtk.DialogFlags.Modal | Gtk.DialogFlags.DestroyWithParent);
                var read = (Gtk.DialogFlags) (Enum) flags;

                Assert.True(read.HasFlag(Gtk.DialogFlags.Modal));
                Assert.True(read.HasFlag(Gtk.DialogFlags.DestroyWithParent));
            });
        }

        [Fact]
        public void A_variant_survives_a_value()
        {
            Run(() =>
            {
                var variant = new GLib.Variant("carried in a value");

                var read = (GLib.Variant) new GLib.Value(variant);

                Assert.NotNull(read);
                Assert.Equal("carried in a value", (string) read);
            });
        }

        // ------------------------------------------------ what the value is for

        [Fact]
        public void Every_scalar_kind_survives_a_real_property_round_trip()
        {
            // The reason GLib.Value exists. A conversion that works standalone
            // but disagrees with what GObject stores would show up here and
            // nowhere else.
            Run(() =>
            {
                var label = new Gtk.Label("start");

                label.SetProperty("label", new GLib.Value("through a property"));
                Assert.Equal("through a property", (string) label.GetProperty("label"));

                label.SetProperty("xalign", new GLib.Value(0.25f));
                Assert.Equal(0.25f, (float) label.GetProperty("xalign"));

                label.SetProperty("selectable", new GLib.Value(true));
                Assert.True((bool) label.GetProperty("selectable"));

                label.SetProperty("width-chars", new GLib.Value(17));
                Assert.Equal(17, (int) label.GetProperty("width-chars"));
            });
        }

        [Fact]
        public void A_value_reports_the_type_it_was_built_as()
        {
            // The GType is what decides which g_value_get_* the conversion may
            // use, so a constructor that picked the wrong one is visible here
            // before any read goes wrong.
            Run(() =>
            {
                Assert.Equal(GLib.GType.Int, new GLib.Value(1).ValueType);
                Assert.Equal(GLib.GType.UInt, new GLib.Value(1u).ValueType);
                Assert.Equal(GLib.GType.Int64, new GLib.Value(1L).ValueType);
                Assert.Equal(GLib.GType.UInt64, new GLib.Value(1UL).ValueType);
                Assert.Equal(GLib.GType.Double, new GLib.Value(1.0).ValueType);
                Assert.Equal(GLib.GType.Float, new GLib.Value(1.0f).ValueType);
                Assert.Equal(GLib.GType.Boolean, new GLib.Value(true).ValueType);
                Assert.Equal(GLib.GType.String, new GLib.Value("s").ValueType);
                Assert.Equal(GLib.GType.UChar, new GLib.Value((byte) 1).ValueType);
                Assert.Equal(GLib.GType.Char, new GLib.Value((sbyte) 1).ValueType);
            });
        }

        [Fact]
        public void The_untyped_reader_hands_back_the_value_that_went_in()
        {
            // Val boxes whatever the GType says is inside, and it is how the
            // signal marshaller reads arguments, so the boxed type has to match
            // what was stored.
            Run(() =>
            {
                Assert.Equal(42, new GLib.Value(42).Val);
                Assert.Equal("text", new GLib.Value("text").Val);
                Assert.Equal(true, new GLib.Value(true).Val);
                Assert.Equal(1.5, new GLib.Value(1.5).Val);
                Assert.Equal(long.MaxValue, new GLib.Value(long.MaxValue).Val);
            });
        }
    }
}

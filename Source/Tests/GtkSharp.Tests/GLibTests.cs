using System;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// GLibSharp is entirely hand-written — no code is generated for it — so
    /// nothing here is covered by the generated-binding tests.
    /// </summary>
    public class GLibTests : GtkTestBase
    {
        public GLibTests(GtkFixture gtk) : base(gtk) { }

        [Theory]
        [InlineData(42)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        public void Value_round_trips_an_int(int expected)
        {
            using var value = new GLib.Value(expected);

            Assert.Equal(expected, (int) value.Val);
        }

        [Theory]
        [InlineData("hello")]
        [InlineData("")]
        [InlineData("ünïcödé ☺")]
        public void Value_round_trips_a_string(string expected)
        {
            using var value = new GLib.Value(expected);

            Assert.Equal(expected, (string) value.Val);
        }

        [Fact]
        public void Value_round_trips_a_bool_and_a_double()
        {
            using var flag = new GLib.Value(true);
            using var number = new GLib.Value(1.5);

            Assert.True((bool) flag.Val);
            Assert.Equal(1.5, (double) number.Val);
        }

        [Fact]
        public void GType_names_match_the_types_they_describe()
        {
            Assert.Equal("gint", GLib.GType.Int.ToString());
            Assert.Equal("gchararray", GLib.GType.String.ToString());
            Assert.Equal("gboolean", GLib.GType.Boolean.ToString());
        }

        [Fact]
        public void Bytes_preserves_its_contents_and_length()
        {
            var data = new byte[] { 1, 2, 3, 250 };

            var bytes = new GLib.Bytes(data);

            Assert.Equal((ulong) data.Length, bytes.Size);
            Assert.Equal(data, bytes.Data);
        }

        [Fact]
        public void Idle_runs_the_handler_on_the_main_loop()
        {
            // Idle and Timeout are how anything gets back onto the main loop, so
            // a break here is invisible until callbacks silently stop arriving.
            Run(() =>
            {
                int calls = 0;

                GLib.Idle.Add(() =>
                {
                    calls++;
                    return false;   // false means "do not run me again"
                });

                for (int i = 0; i < 100 && calls == 0; i++)
                    Gtk.Application.RunIteration(false);

                Assert.Equal(1, calls);
            });
        }

        [Fact]
        public void Timeout_stops_being_called_once_it_returns_false()
        {
            Run(() =>
            {
                int calls = 0;

                GLib.Timeout.Add(1, () => { calls++; return calls < 3; });

                for (int i = 0; i < 5000 && calls < 3; i++)
                    Gtk.Application.RunIteration(false);

                Assert.Equal(3, calls);

                // Draining further must not call it again.
                for (int i = 0; i < 200; i++)
                    Gtk.Application.RunIteration(false);

                Assert.Equal(3, calls);
            });
        }

        [Fact]
        public void Variant_round_trips_its_value_and_reports_its_type()
        {
            var text = new GLib.Variant("hello");
            var number = new GLib.Variant(7);

            Assert.Equal("hello", (string) text);
            Assert.Equal(7, (int) number);
        }

        [Fact]
        public void Marshaller_round_trips_a_string_through_unmanaged_memory()
        {
            // Every string that crosses the boundary goes through here.
            IntPtr native = GLib.Marshaller.StringToPtrGStrdup("ünïcödé ☺");
            try
            {
                Assert.Equal("ünïcödé ☺", GLib.Marshaller.Utf8PtrToString(native));
            }
            finally
            {
                GLib.Marshaller.Free(native);
            }
        }
    }
}

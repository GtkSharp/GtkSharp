using System;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.TimeVal</c> and <c>GLib.DateTime</c>.
    /// </summary>
    /// <remarks>
    /// <c>TimeVal</c> had no coverage at all, and it is the last of the three
    /// hand-written structs the layout audit could only check by field *count*.
    /// The count matches — two members — but <c>GTimeVal</c> is two
    /// <c>glong</c>s, and <c>glong</c> is 64 bits on Linux and macOS and **32 on
    /// Windows**, while the binding declares both as <c>IntPtr</c>.
    ///
    /// So the struct is 16 bytes everywhere and the C one is 16 on Linux and 8 on
    /// Windows. Where C writes 8 bytes into a struct C# reads 16 from, the
    /// seconds and the microseconds end up in one 64-bit field.
    ///
    /// The microseconds have to be non-zero for that to show: with a whole number
    /// of seconds the upper half is zero and the wrong layout reads correctly by
    /// luck, which is exactly the kind of test that would have missed this.
    /// </remarks>
    public class GLibTimeTests : GtkTestBase
    {
        public GLibTimeTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------- TimeVal

        [Fact]
        public void A_parsed_time_keeps_its_seconds_when_the_microseconds_are_zero()
        {
            // The easy case, and the one that hides a wrong layout: with zero
            // microseconds the second half of the C struct is zero, so reading
            // the two fields as one still gives the right number.
            Run(() =>
            {
                Assert.True(GLib.TimeVal.FromIso8601("1970-01-01T00:00:01Z", out var time));

                Assert.Equal(1, time.TvSec);
                Assert.Equal(0, time.TvUsec);
            });
        }

        [Fact]
        public void A_parsed_time_keeps_its_seconds_when_the_microseconds_are_not()
        {
            // The case that shows it. Half a second past the epoch is
            // tv_sec = 1, tv_usec = 500000; if the two are read as one 64-bit
            // field the seconds come back as roughly 2.2 x 10^15.
            Run(() =>
            {
                Assert.True(GLib.TimeVal.FromIso8601("1970-01-01T00:00:01.500000Z", out var time));

                Assert.Equal(1, time.TvSec);
                Assert.Equal(500_000, time.TvUsec);
            });
        }

        [Fact]
        public void A_time_round_trips_through_its_ISO_8601_form()
        {
            // Passing the struct the other way. GLib formats what it reads, so a
            // wrong layout shows up as the wrong instant rather than as an error.
            Run(() =>
            {
                Assert.True(GLib.TimeVal.FromIso8601("2001-02-03T04:05:06Z", out var time));

                var text = time.ToIso8601();

                Assert.NotNull(text);
                Assert.Contains("2001-02-03", text);
                Assert.Contains("04:05:06", text);
            });
        }

        [Fact]
        public void Adding_microseconds_carries_into_the_seconds()
        {
            // Arithmetic the test does itself, and it touches both members: 1.5s
            // plus 600ms is 2.1s, which can only be right if the carry landed in
            // the seconds field rather than overflowing a shared one.
            Run(() =>
            {
                Assert.True(GLib.TimeVal.FromIso8601("1970-01-01T00:00:01.500000Z", out var time));

                time.Add(600_000);

                Assert.Equal(2, time.TvSec);
                Assert.Equal(100_000, time.TvUsec);
            });
        }

        [Fact]
        public void Nonsense_does_not_parse()
        {
            Run(() =>
            {
                Assert.False(GLib.TimeVal.FromIso8601("not a date", out _));
                Assert.False(GLib.TimeVal.FromIso8601("", out _));
            });
        }

        // ------------------------------------------------------------ DateTime

        [Fact]
        public void A_date_time_reports_the_parts_it_was_built_from()
        {
            Run(() =>
            {
                using var utc = GLib.TimeZone.NewUtc();
                using var when = new GLib.DateTime(utc, 2001, 2, 3, 4, 5, 6.0);

                Assert.Equal(2001, when.Year);
                Assert.Equal(2, when.Month);
                Assert.Equal(3, when.DayOfMonth);
                Assert.Equal(4, when.Hour);
                Assert.Equal(5, when.Minute);
                Assert.Equal(6.0, when.Seconds, 3);
            });
        }

        [Fact]
        public void A_unix_time_round_trips()
        {
            // The oracle is outside GLib: the test computes the epoch seconds
            // itself and asks GLib to agree.
            Run(() =>
            {
                const long epochSeconds = 981_173_106;   // 2001-02-03T04:05:06Z

                using var when = new GLib.DateTime(epochSeconds);

                Assert.Equal(epochSeconds, when.ToUnix());

                using var utc = when.ToUtc();
                Assert.Equal(2001, utc.Year);
                Assert.Equal(3, utc.DayOfMonth);
            });
        }

        [Fact]
        public void Adding_an_interval_moves_the_date_by_that_much()
        {
            Run(() =>
            {
                using var utc = GLib.TimeZone.NewUtc();
                using var start = new GLib.DateTime(utc, 2001, 1, 31, 0, 0, 0.0);

                using var later = start.AddDays(1);
                Assert.Equal(2, later.Month);
                Assert.Equal(1, later.DayOfMonth);

                using var muchLater = start.AddMonths(1);
                Assert.Equal(2, muchLater.Month);

                // A leap year is arithmetic the test can check itself.
                using var leap = new GLib.DateTime(utc, 2000, 2, 28, 0, 0, 0.0);
                using var nextDay = leap.AddDays(1);
                Assert.Equal(29, nextDay.DayOfMonth);
            });
        }

        [Fact]
        public void A_difference_between_two_times_is_the_interval_between_them()
        {
            Run(() =>
            {
                using var utc = GLib.TimeZone.NewUtc();
                using var start = new GLib.DateTime(utc, 2001, 1, 1, 0, 0, 0.0);
                using var end = new GLib.DateTime(utc, 2001, 1, 2, 0, 0, 0.0);

                // GTimeSpan is microseconds.
                Assert.Equal(24L * 60 * 60 * 1_000_000, end.Difference(start));
                Assert.Equal(-(24L * 60 * 60 * 1_000_000), start.Difference(end));
            });
        }

        [Fact]
        public void Formatting_produces_the_fields_that_were_asked_for()
        {
            Run(() =>
            {
                using var utc = GLib.TimeZone.NewUtc();
                using var when = new GLib.DateTime(utc, 2001, 2, 3, 4, 5, 6.0);

                Assert.Equal("2001-02-03", when.Format("%Y-%m-%d"));
                Assert.Equal("04:05:06", when.Format("%H:%M:%S"));
            });
        }
    }
}

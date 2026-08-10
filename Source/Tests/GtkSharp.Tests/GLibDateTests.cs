using System;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.Date</c> — the calendar, checked against the one in the BCL.
    /// </summary>
    /// <remarks>
    /// Calendar arithmetic is the rare corner of this binding with a real oracle
    /// sitting in the framework: <c>System.DateTime</c> implements the same
    /// proleptic Gregorian calendar, was written by somebody else, and can be
    /// asked the same questions. So these tests do not assert that GLib returns
    /// what GLib returned last time — they assert the two calendars agree.
    ///
    /// That matters more than usual here because every accessor on this class
    /// crosses into C through a hand-written wrapper, and a wrong one returns a
    /// plausible number rather than failing.
    /// </remarks>
    public class GLibDateTests : GtkTestBase
    {
        public GLibDateTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>A spread of dates: leap days, century boundaries, month ends.</summary>
        public static TheoryData<int, int, int> Dates => new TheoryData<int, int, int>
        {
            { 1970, 1, 1 },
            { 2000, 2, 29 },     // leap year, divisible by 400
            { 1900, 3, 1 },      // *not* a leap year, divisible by 100
            { 2001, 2, 3 },
            { 2024, 12, 31 },
            { 1999, 12, 31 },
            { 2100, 2, 28 },     // not a leap year either
            { 1583, 1, 1 },      // GDate's lower bound is year 1, but the
        };                       // Julian day mapping is worth checking early

        static GLib.Date DateOf(int year, int month, int day)
            => new GLib.Date((byte)day, month, (ushort)year);

        /// <summary>GLib numbers Monday 1 … Sunday 7; the BCL numbers Sunday 0.</summary>
        static int GLibWeekday(System.DateTime when)
            => ((int)when.DayOfWeek + 6) % 7 + 1;

        /// <summary>g_date_get_julian counts days with 1 January year 1 as day 1.</summary>
        static uint GLibJulian(System.DateTime when)
            => (uint)(when - new System.DateTime(1, 1, 1)).Days + 1;

        // ------------------------------------------------------------ the parts

        [Theory]
        [MemberData(nameof(Dates))]
        public void The_parts_of_a_date_match_the_calendar_in_the_BCL(int year, int month, int day)
        {
            Run(() =>
            {
                var oracle = new System.DateTime(year, month, day);
                using var date = DateOf(year, month, day);

                Assert.True(date.Valid(), $"{year}-{month}-{day} should be a valid date");

                Assert.Equal(day, date.Day);
                Assert.Equal(month, date.Month);
                Assert.Equal(year, date.Year);

                Assert.Equal(oracle.DayOfYear, (int)date.DayOfYear);
                Assert.Equal(GLibWeekday(oracle), date.Weekday);
                Assert.Equal(GLibJulian(oracle), date.Julian);
            });
        }

        [Theory]
        [MemberData(nameof(Dates))]
        public void A_date_built_from_its_julian_day_is_the_same_date(int year, int month, int day)
        {
            // The other constructor, and the check that Julian above is not
            // simply wrong in a self-consistent way.
            Run(() =>
            {
                var oracle = new System.DateTime(year, month, day);

                using var date = new GLib.Date(GLibJulian(oracle));

                Assert.Equal(day, date.Day);
                Assert.Equal(month, date.Month);
                Assert.Equal(year, date.Year);
            });
        }

        // ------------------------------------------------------------ arithmetic

        [Theory]
        [InlineData(2000, 2, 28, 1)]      // into a leap day
        [InlineData(1999, 12, 31, 1)]     // across a year
        [InlineData(2001, 1, 1, 365)]
        [InlineData(2024, 3, 15, 1000)]
        public void Adding_days_agrees_with_the_BCL(int year, int month, int day, int days)
        {
            Run(() =>
            {
                var oracle = new System.DateTime(year, month, day).AddDays(days);
                using var date = DateOf(year, month, day);

                date.AddDays((uint)days);

                Assert.Equal(oracle.Year, date.Year);
                Assert.Equal(oracle.Month, date.Month);
                Assert.Equal(oracle.Day, date.Day);
            });
        }

        [Theory]
        [InlineData(2001, 1, 31, 1)]      // clamps to the end of February
        [InlineData(2000, 1, 31, 1)]      // ... and to the 29th in a leap year
        [InlineData(2001, 3, 31, 13)]
        [InlineData(1999, 8, 15, 60)]
        public void Adding_months_clamps_the_day_the_same_way_the_BCL_does(
            int year, int month, int day, int months)
        {
            // The interesting case: 31 January plus a month has no obvious
            // answer, and both calendars have to pick the same one.
            Run(() =>
            {
                var oracle = new System.DateTime(year, month, day).AddMonths(months);
                using var date = DateOf(year, month, day);

                date.AddMonths((uint)months);

                Assert.Equal(oracle.Year, date.Year);
                Assert.Equal(oracle.Month, date.Month);
                Assert.Equal(oracle.Day, date.Day);
            });
        }

        [Theory]
        [InlineData(2000, 2, 29, 1)]      // a leap day plus a year is not a leap day
        [InlineData(2001, 6, 15, 10)]
        public void Adding_years_agrees_with_the_BCL(int year, int month, int day, int years)
        {
            Run(() =>
            {
                var oracle = new System.DateTime(year, month, day).AddYears(years);
                using var date = DateOf(year, month, day);

                date.AddYears((uint)years);

                Assert.Equal(oracle.Year, date.Year);
                Assert.Equal(oracle.Month, date.Month);
                Assert.Equal(oracle.Day, date.Day);
            });
        }

        [Fact]
        public void Subtracting_undoes_adding()
        {
            Run(() =>
            {
                using var date = DateOf(2001, 3, 15);

                date.AddDays(400);
                date.SubtractDays(400);
                Assert.Equal(GLibJulian(new System.DateTime(2001, 3, 15)), date.Julian);

                date.AddMonths(7);
                date.SubtractMonths(7);
                Assert.Equal(3, date.Month);

                date.AddYears(5);
                date.SubtractYears(5);
                Assert.Equal(2001, date.Year);
            });
        }

        [Fact]
        public void The_days_between_two_dates_is_their_difference()
        {
            Run(() =>
            {
                using var start = DateOf(2001, 1, 1);
                using var end = DateOf(2001, 12, 31);

                var oracle = (new System.DateTime(2001, 12, 31) - new System.DateTime(2001, 1, 1)).Days;

                // g_date_days_between (a, b) is b - a.
                Assert.Equal(oracle, start.DaysBetween(end));
                Assert.Equal(-oracle, end.DaysBetween(start));

                using var same = DateOf(2001, 1, 1);
                Assert.Equal(0, start.DaysBetween(same));
            });
        }

        // ------------------------------------------------------------- ordering

        [Fact]
        public void Comparing_orders_dates_the_way_the_calendar_does()
        {
            Run(() =>
            {
                using var earlier = DateOf(2001, 2, 3);
                using var later = DateOf(2001, 2, 4);
                using var same = DateOf(2001, 2, 3);

                Assert.True(earlier.Compare(later) < 0);
                Assert.True(later.Compare(earlier) > 0);
                Assert.Equal(0, earlier.Compare(same));
            });
        }

        [Fact]
        public void Ordering_a_pair_swaps_them_only_when_they_are_the_wrong_way_round()
        {
            Run(() =>
            {
                using var a = DateOf(2001, 6, 1);
                using var b = DateOf(2001, 1, 1);

                a.Order(b);                       // a becomes the earlier of the two

                Assert.Equal(1, a.Month);
                Assert.Equal(6, b.Month);

                a.Order(b);                       // already ordered: nothing moves
                Assert.Equal(1, a.Month);
                Assert.Equal(6, b.Month);
            });
        }

        [Fact]
        public void Clamping_moves_a_date_inside_the_range_and_leaves_it_alone_inside()
        {
            Run(() =>
            {
                using var min = DateOf(2001, 1, 1);
                using var max = DateOf(2001, 12, 31);

                using var before = DateOf(1999, 5, 5);
                before.Clamp(min, max);
                Assert.Equal(2001, before.Year);
                Assert.Equal(1, before.Month);
                Assert.Equal(1, before.Day);

                using var after = DateOf(2005, 5, 5);
                after.Clamp(min, max);
                Assert.Equal(31, after.Day);
                Assert.Equal(12, after.Month);

                using var inside = DateOf(2001, 6, 15);
                inside.Clamp(min, max);
                Assert.Equal(15, inside.Day);
                Assert.Equal(6, inside.Month);
            });
        }

        // ------------------------------------------------------- month position

        [Theory]
        [InlineData(2001, 2, 1, true, false)]
        [InlineData(2001, 2, 28, false, true)]     // February in a common year
        [InlineData(2000, 2, 28, false, false)]    // ... but not in a leap year
        [InlineData(2000, 2, 29, false, true)]
        [InlineData(2001, 6, 15, false, false)]
        public void First_and_last_of_the_month_are_reported_from_the_calendar(
            int year, int month, int day, bool first, bool last)
        {
            Run(() =>
            {
                using var date = DateOf(year, month, day);

                Assert.Equal(first, date.IsFirstOfMonth);
                Assert.Equal(last, date.IsLastOfMonth);

                // The oracle for "last": the BCL's own month length.
                Assert.Equal(day == System.DateTime.DaysInMonth(year, month), date.IsLastOfMonth);
            });
        }

        // -------------------------------------------------------------- statics

        [Theory]
        [InlineData(2000)]
        [InlineData(1900)]
        [InlineData(2001)]
        [InlineData(2024)]
        [InlineData(2100)]
        public void Leap_years_and_month_lengths_agree_with_the_BCL(int year)
        {
            Run(() =>
            {
                Assert.Equal(System.DateTime.IsLeapYear(year), GLib.Date.IsLeapYear((ushort)year));

                for (int month = 1; month <= 12; month++)
                    Assert.Equal(System.DateTime.DaysInMonth(year, month),
                                 GLib.Date.GetDaysInMonth(month, (ushort)year));
            });
        }

        [Fact]
        public void The_validity_checks_reject_what_is_out_of_range()
        {
            Run(() =>
            {
                Assert.True(GLib.Date.ValidDay(1));
                Assert.True(GLib.Date.ValidDay(31));
                Assert.False(GLib.Date.ValidDay(0));
                Assert.False(GLib.Date.ValidDay(32));

                Assert.True(GLib.Date.ValidMonth(1));
                Assert.True(GLib.Date.ValidMonth(12));
                Assert.False(GLib.Date.ValidMonth(0));
                Assert.False(GLib.Date.ValidMonth(13));

                Assert.True(GLib.Date.ValidYear(2001));
                Assert.False(GLib.Date.ValidYear(0));

                Assert.True(GLib.Date.ValidWeekday(1));
                Assert.True(GLib.Date.ValidWeekday(7));
                Assert.False(GLib.Date.ValidWeekday(0));
                Assert.False(GLib.Date.ValidWeekday(8));

                Assert.True(GLib.Date.ValidJulian(1));
                Assert.False(GLib.Date.ValidJulian(0));

                // The one that needs the calendar rather than a range check.
                Assert.True(GLib.Date.ValidDmy(29, 2, 2000));
                Assert.False(GLib.Date.ValidDmy(29, 2, 2001));
                Assert.False(GLib.Date.ValidDmy(31, 4, 2001));
            });
        }

        [Theory]
        [InlineData(2001)]
        [InlineData(2000)]
        [InlineData(2024)]
        public void The_week_counts_of_a_year_are_the_weeks_a_year_can_hold(int year)
        {
            // Not a BCL oracle, but arithmetic the test can do: a year spans 52
            // or 53 whole weeks plus a remainder, so the count of weeks starting
            // on a given day is 52 or 53.
            Run(() =>
            {
                var monday = GLib.Date.GetMondayWeeksInYear((ushort)year);
                var sunday = GLib.Date.GetSundayWeeksInYear((ushort)year);

                Assert.InRange(monday, (byte)52, (byte)53);
                Assert.InRange(sunday, (byte)52, (byte)53);
            });
        }

        [Theory]
        [InlineData(2001, 2, 3)]
        [InlineData(2021, 1, 1)]      // belongs to the *previous* ISO year
        [InlineData(2024, 12, 30)]    // belongs to the *next* ISO year
        public void The_ISO_week_number_matches_the_BCL_ISO_calendar(int year, int month, int day)
        {
            // System.Globalization.ISOWeek is the same ISO 8601 rule, written by
            // somebody else. The two dates that cross a year boundary are the
            // ones where a naive implementation and a correct one part company.
            Run(() =>
            {
                var oracle = System.Globalization.ISOWeek.GetWeekOfYear(
                    new System.DateTime(year, month, day));

                using var date = DateOf(year, month, day);

                Assert.Equal((uint)oracle, date.Iso8601WeekOfYear);
            });
        }

        // ---------------------------------------------------------- set from...

        [Fact]
        public void Setting_a_date_from_a_unix_time_gives_that_day()
        {
            Run(() =>
            {
                using var date = new GLib.Date();

                // 2001-02-03T04:05:06Z. The local timezone can move this by a
                // day, so the assertion is that it lands within one.
                date.TimeT = 981_173_106;

                Assert.True(date.Valid());

                var distance = Math.Abs((long)date.Julian - GLibJulian(new System.DateTime(2001, 2, 3)));
                Assert.True(distance <= 1, $"expected 2001-02-03 give or take a timezone, got {date.Year}-{date.Month}-{date.Day}");
            });
        }

        [Fact]
        public void Setting_a_date_from_a_TimeVal_gives_that_day()
        {
            // The other side of the GTimeVal layout fix: Date.TimeVal is one of
            // the four places the struct crosses into C, and a wrong layout here
            // lands on a date centuries away rather than failing.
            Run(() =>
            {
                Assert.True(GLib.TimeVal.FromIso8601("2001-02-03T12:00:00Z", out var noon));

                using var date = new GLib.Date();
                date.TimeVal = noon;

                Assert.True(date.Valid());
                Assert.Equal(2001, date.Year);
                Assert.Equal(2, date.Month);
                Assert.Equal(3, date.Day);
            });
        }

        [Fact]
        public void Parsing_a_date_string_gives_that_date()
        {
            Run(() =>
            {
                using var date = new GLib.Date();

                // g_date_set_parse is locale-dependent for ambiguous forms;
                // ISO 8601 is unambiguous in every locale.
                date.Parse = "2001-02-03";

                Assert.True(date.Valid());
                Assert.Equal(2001, date.Year);
                Assert.Equal(2, date.Month);
                Assert.Equal(3, date.Day);
            });
        }

        [Fact]
        public void A_cleared_date_is_not_valid()
        {
            Run(() =>
            {
                using var date = DateOf(2001, 2, 3);
                Assert.True(date.Valid());

                date.Clear(1);

                Assert.False(date.Valid());
            });
        }

        // ------------------------------------------------------------- strftime

        [Fact]
        public void Formatting_a_date_returns_the_formatted_text()
        {
            // The C parameter is the output buffer, and the binding took it as a
            // C# string: the buffer was copied in, written over by GLib, freed,
            // and the call returned the length of text nobody could read. The
            // overload that allocates the buffer returns it.
            Run(() =>
            {
                using var date = DateOf(2001, 2, 3);

                Assert.Equal("2001-02-03", GLib.Date.Strftime("%Y-%m-%d", date));
                Assert.Equal("2001", GLib.Date.Strftime("%Y", date));
            });
        }

        [Fact]
        public void Formatting_survives_a_result_longer_than_the_first_buffer()
        {
            // g_date_strftime returns 0 when the buffer was too small, which is
            // indistinguishable from an empty result, so the wrapper grows and
            // retries. A format that repeats the year 200 times is 800 bytes,
            // past the first buffer.
            Run(() =>
            {
                using var date = DateOf(2001, 2, 3);

                var text = GLib.Date.Strftime(string.Concat(System.Linq.Enumerable.Repeat("%Y", 200)), date);

                Assert.NotNull(text);
                Assert.Equal(800, text.Length);
                Assert.StartsWith("20012001", text);
            });
        }

        [Fact]
        public void An_empty_format_produces_an_empty_string_rather_than_growing_forever()
        {
            // The case the retry loop has to tell apart from "buffer too small":
            // both report zero bytes written.
            Run(() =>
            {
                using var date = DateOf(2001, 2, 3);

                Assert.Equal(string.Empty, GLib.Date.Strftime("", date));
            });
        }
    }
}

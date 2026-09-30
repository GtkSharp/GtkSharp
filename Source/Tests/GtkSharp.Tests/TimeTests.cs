using System;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.Date</c> and <c>GLib.DateTime</c> together are ~700 hand-written
    /// lines that nothing reached. Calendar arithmetic is worth testing properly
    /// because the answers are known independently of the library: a leap year has
    /// a 29 February, adding twelve months returns the same day, and a timestamp
    /// converted to Unix time and back is the timestamp.
    /// </summary>
    public class TimeTests : GtkTestBase
    {
        public TimeTests(GtkFixture fixture) : base(fixture) { }

        // ----------------------------------------------------------- GLib.Date

        private static GLib.Date DateOf(int year, int month, int day)
        {
            var date = new GLib.Date();
            date.SetDmy((byte) day, month, (ushort) year);
            return date;
        }

        [Fact]
        public void A_date_reports_back_the_day_month_and_year_it_was_set_to()
        {
            Run(() =>
            {
                var date = DateOf(2024, 2, 29);

                Assert.True(date.Valid());
                Assert.Equal(29, date.Day);
                Assert.Equal(2, date.Month);
                Assert.Equal(2024, date.Year);
            });
        }

        [Fact]
        public void Adding_days_crosses_a_month_boundary_correctly()
        {
            Run(() =>
            {
                var date = DateOf(2024, 1, 30);

                date.AddDays(3);

                Assert.Equal(2, date.Month);
                Assert.Equal(2, date.Day);
            });
        }

        [Fact]
        public void Adding_and_subtracting_the_same_span_returns_the_original_day()
        {
            Run(() =>
            {
                var date = DateOf(2023, 7, 15);
                var julian = date.Julian;

                date.AddMonths(7);
                Assert.NotEqual(julian, date.Julian);

                date.SubtractMonths(7);

                Assert.Equal(julian, date.Julian);
            });
        }

        [Fact]
        public void Adding_a_year_to_29_February_lands_on_28_February()
        {
            // 2025 has no 29th, and GLib clamps rather than overflowing into March.
            Run(() =>
            {
                var date = DateOf(2024, 2, 29);

                date.AddYears(1);

                Assert.Equal(2025, date.Year);
                Assert.Equal(2, date.Month);
                Assert.Equal(28, date.Day);
            });
        }

        [Fact]
        public void Days_between_two_dates_counts_the_leap_day()
        {
            Run(() =>
            {
                var start = DateOf(2024, 2, 28);
                var end = DateOf(2024, 3, 1);

                // The receiver is date1 and the argument date2, and the C
                // function returns date2 - date1 -- so this reads backwards
                // from what the name suggests. 28 Feb -> 29 Feb -> 1 Mar.
                Assert.Equal(2, start.DaysBetween(end));
                Assert.Equal(-2, end.DaysBetween(start));

                // No leap day in 2023, so the same calendar span is one shorter.
                Assert.Equal(1, DateOf(2023, 2, 28).DaysBetween(DateOf(2023, 3, 1)));
            });
        }

        [Fact]
        public void Comparing_dates_orders_them()
        {
            Run(() =>
            {
                var earlier = DateOf(2020, 5, 1);
                var later = DateOf(2020, 5, 2);

                Assert.True(later.Compare(earlier) > 0);
                Assert.True(earlier.Compare(later) < 0);
                Assert.Equal(0, earlier.Compare(DateOf(2020, 5, 1)));
            });
        }

        [Fact]
        public void A_date_knows_whether_it_starts_or_ends_its_month()
        {
            Run(() =>
            {
                Assert.True(DateOf(2023, 6, 1).IsFirstOfMonth);
                Assert.False(DateOf(2023, 6, 2).IsFirstOfMonth);

                Assert.True(DateOf(2023, 6, 30).IsLastOfMonth);
                Assert.False(DateOf(2023, 6, 29).IsLastOfMonth);
            });
        }

        [Fact]
        public void A_date_reports_its_weekday_and_day_of_year()
        {
            Run(() =>
            {
                // 1 January 2024 was a Monday; GLib numbers Monday as 1.
                var date = DateOf(2024, 1, 1);

                Assert.Equal(1, date.Weekday);
                Assert.Equal(1u, date.DayOfYear);

                Assert.Equal(60u, DateOf(2024, 2, 29).DayOfYear);   // leap year
                Assert.Equal(59u, DateOf(2023, 2, 28).DayOfYear);
            });
        }

        [Fact]
        public void Days_in_month_accounts_for_leap_years()
        {
            Run(() =>
            {
                Assert.Equal(29, GLib.Date.GetDaysInMonth(2, 2024));
                Assert.Equal(28, GLib.Date.GetDaysInMonth(2, 2023));
                Assert.Equal(31, GLib.Date.GetDaysInMonth(1, 2023));
                Assert.Equal(30, GLib.Date.GetDaysInMonth(4, 2023));

                Assert.True(GLib.Date.IsLeapYear(2024));
                Assert.False(GLib.Date.IsLeapYear(2100));   // century, not a leap year
            });
        }

        [Fact]
        public void Clamping_pulls_a_date_inside_the_allowed_range()
        {
            Run(() =>
            {
                var date = DateOf(2020, 1, 1);

                date.Clamp(DateOf(2022, 1, 1), DateOf(2023, 1, 1));

                Assert.Equal(2022, date.Year);
            });
        }

        [Fact]
        public void Ordering_two_dates_swaps_them_when_they_are_the_wrong_way_round()
        {
            Run(() =>
            {
                var first = DateOf(2024, 12, 31);
                var second = DateOf(2024, 1, 1);

                first.Order(second);

                // Order makes the receiver the earlier of the two.
                Assert.Equal(1, first.Month);
                Assert.Equal(12, second.Month);
            });
        }

        // ------------------------------------------------------- GLib.DateTime

        [Fact]
        public void A_utc_datetime_reports_the_fields_it_was_built_from()
        {
            Run(() =>
            {
                var when = new GLib.DateTime(new GLib.TimeZone("UTC"), 2024, 3, 17, 13, 45, 30.5);

                Assert.Equal(2024, when.Year);
                Assert.Equal(3, when.Month);
                Assert.Equal(17, when.DayOfMonth);
                Assert.Equal(13, when.Hour);
                Assert.Equal(45, when.Minute);
                Assert.Equal(30, when.Second);
                Assert.Equal(30.5, when.Seconds, 3);
            });
        }

        [Fact]
        public void GetYmd_agrees_with_the_individual_properties()
        {
            Run(() =>
            {
                var when = new GLib.DateTime(new GLib.TimeZone("UTC"), 2019, 11, 5, 0, 0, 0);

                when.GetYmd(out var year, out var month, out var day);

                Assert.Equal(when.Year, year);
                Assert.Equal(when.Month, month);
                Assert.Equal(when.DayOfMonth, day);
            });
        }

        [Fact]
        public void A_timestamp_survives_a_trip_through_unix_time()
        {
            Run(() =>
            {
                var when = new GLib.DateTime(new GLib.TimeZone("UTC"), 2021, 6, 30, 12, 0, 0);

                var back = GLib.DateTime.NewFromUnixUtc(when.ToUnix());

                Assert.Equal(when.ToUnix(), back.ToUnix());
                Assert.Equal(2021, back.Year);
                Assert.Equal(6, back.Month);
                Assert.Equal(30, back.DayOfMonth);
                Assert.Equal(12, back.Hour);
            });
        }

        [Fact]
        public void Adding_a_span_and_taking_the_difference_gives_the_span_back()
        {
            Run(() =>
            {
                var start = new GLib.DateTime(new GLib.TimeZone("UTC"), 2022, 1, 1, 0, 0, 0);

                var later = start.AddHours(30);

                // GTimeSpan is microseconds.
                Assert.Equal(30L * 3600 * 1000000, later.Difference(start));
                Assert.Equal(2, later.DayOfMonth);
                Assert.Equal(6, later.Hour);
            });
        }

        [Fact]
        public void Each_Add_helper_moves_the_field_it_names()
        {
            Run(() =>
            {
                var start = new GLib.DateTime(new GLib.TimeZone("UTC"), 2022, 5, 10, 8, 0, 0);

                Assert.Equal(11, start.AddDays(1).DayOfMonth);
                Assert.Equal(17, start.AddWeeks(1).DayOfMonth);
                Assert.Equal(6, start.AddMonths(1).Month);
                Assert.Equal(2023, start.AddYears(1).Year);
                Assert.Equal(9, start.AddHours(1).Hour);
                Assert.Equal(30, start.AddMinutes(30).Minute);
                Assert.Equal(45, start.AddSeconds(45).Second);
            });
        }

        [Fact]
        public void AddFull_moves_every_field_at_once()
        {
            Run(() =>
            {
                var start = new GLib.DateTime(new GLib.TimeZone("UTC"), 2020, 1, 1, 0, 0, 0);

                var moved = start.AddFull(1, 2, 3, 4, 5, 6);

                Assert.Equal(2021, moved.Year);
                Assert.Equal(3, moved.Month);
                Assert.Equal(4, moved.DayOfMonth);
                Assert.Equal(4, moved.Hour);
                Assert.Equal(5, moved.Minute);
                Assert.Equal(6, moved.Second);
            });
        }

        [Fact]
        public void Formatting_produces_the_fields_that_were_set()
        {
            // Format goes out to strftime and back through UTF-8 marshalling,
            // which is the part that can break rather than the calendar maths.
            Run(() =>
            {
                var when = new GLib.DateTime(new GLib.TimeZone("UTC"), 2024, 12, 25, 18, 30, 0);

                Assert.Equal("2024-12-25 18:30:00", when.Format("%Y-%m-%d %H:%M:%S"));
            });
        }

        [Fact]
        public void A_datetime_in_a_fixed_offset_zone_reports_that_offset()
        {
            Run(() =>
            {
                var when = new GLib.DateTime(new GLib.TimeZone("+02:00"), 2024, 6, 1, 12, 0, 0);

                // GTimeSpan again: two hours in microseconds.
                Assert.Equal(2L * 3600 * 1000000, when.UtcOffset);

                var utc = when.ToUtc();

                Assert.Equal(10, utc.Hour);
                Assert.Equal(when.ToUnix(), utc.ToUnix());
            });
        }

        [Fact]
        public void The_week_of_year_follows_the_iso_rule_at_a_year_boundary()
        {
            Run(() =>
            {
                // 1 January 2021 was a Friday, so ISO puts it in week 53 of 2020.
                var when = new GLib.DateTime(new GLib.TimeZone("UTC"), 2021, 1, 1, 0, 0, 0);

                Assert.Equal(53, when.WeekOfYear);
                Assert.Equal(2020, when.WeekNumberingYear);
            });
        }

        [Fact]
        public void The_current_time_agrees_with_the_system_clock()
        {
            Run(() =>
            {
                var glib = GLib.DateTime.NewNowUtc().ToUnix();
                var system = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                Assert.InRange(Math.Abs(glib - system), 0, 5);
            });
        }

        [Fact]
        public void A_unix_timestamp_builds_the_utc_date_it_stands_for()
        {
            Run(() =>
            {
                // 1 000 000 000 is 2001-09-09 01:46:40 UTC, a fixed fact about
                // the epoch rather than anything the library decides.
                var when = GLib.DateTime.NewFromUnixUtc(1000000000L);

                Assert.Equal(2001, when.Year);
                Assert.Equal(9, when.Month);
                Assert.Equal(9, when.DayOfMonth);
                Assert.Equal(1, when.Hour);
                Assert.Equal(46, when.Minute);
                Assert.Equal(40, when.Second);
                Assert.Equal(1000000000L, when.ToUnix());
            });
        }
    }
}

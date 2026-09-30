// GLib.DateTime <-> System.DateTime
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace GLib {

	public partial class DateTime {

		/// <summary>This instant as a <see cref="System.DateTime"/>, in local time.</summary>
		/// <remarks>
		/// <para>Needed because Gtk 4 exchanges dates as GDateTime where Gtk 3 used plain integers
		/// or a struct tm - GtkCalendar's date property is the case that forces it - and there is
		/// no implicit path between the two.</para>
		/// <para>Built from the broken-down fields rather than from the Unix timestamp, because
		/// GDateTime carries a time zone and its Unix time is UTC: going through the timestamp
		/// would silently shift a date across midnight for anyone east or west of Greenwich, which
		/// is exactly the bug a date picker cannot afford.</para>
		/// <para>Sub-second precision is dropped. GDateTime is microsecond-resolution and
		/// System.DateTime is 100ns, so the two do not divide evenly; nothing that exchanges
		/// calendar dates cares.</para>
		/// </remarks>
		public System.DateTime ToSystemDateTime()
		{
			return new System.DateTime(Year, Month, DayOfMonth, Hour, Minute, Second,
				System.DateTimeKind.Local);
		}

		/// <summary>The GLib equivalent of a <see cref="System.DateTime"/>, in local time.</summary>
		/// <remarks>See <see cref="ToSystemDateTime"/> for why this goes through the fields.</remarks>
		public static DateTime FromSystemDateTime(System.DateTime value)
		{
			return new DateTime(value.Year, value.Month, value.Day,
				value.Hour, value.Minute, value.Second);
		}
	}
}

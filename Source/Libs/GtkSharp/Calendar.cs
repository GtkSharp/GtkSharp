// Gtk.Calendar.cs - Gtk Calendar class customizations
//
// Author:
//	 Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) 2003 Ximian, Inc. (http://www.ximian.com)
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

namespace Gtk {

	using System;

	public partial class Calendar {

		public DateTime GetDate ()
		{
			uint year, month, day;
			GetDate (out year, out month, out day);
			DateTime result;
			try {
				result = new DateTime ((int) year, (int) month + 1, (int) day);
			} catch (ArgumentOutOfRangeException) {
				// Kluge to workaround GtkCalendar being in an invalid state
				// when raising month_changed signals, like in bug #78524.
				result = new DateTime ((int) year, (int) month + 1, DateTime.DaysInMonth ((int) year, (int) month + 1));
			}
			return result;
		}


		// This defines a Date property for Calendar
		// Note that the setter causes CalendarChange events to be fired
		public DateTime Date
		{
			get {
				return this.GetDate();
			}
			set {
				uint month= (uint) value.Month-1;
				uint year= (uint) value.Year;
				uint day = (uint) value.Day;

				SelectMonth(month,year);
				SelectDay(day);
			}
		}
	}
}

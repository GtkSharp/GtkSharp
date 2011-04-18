//  Authors:  Aaron Bockover <abockover@novell.com>
// 
//  Copyright 2007-2010 Novell, Inc.
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

	public partial class Global {

		public static bool ShowUri (string uri)
		{
			return ShowUri (null, uri);
		}

		public static bool ShowUri (Gdk.Screen screen, string uri)
		{
			return ShowUri (screen, uri, Gtk.Global.CurrentEventTime);
		}
	}
}


// ClientBase.cs -
//
// Author: Rachel Hestilow  <hestilow@nullenvoid.com>
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


namespace GConf
{
	using System;
	using System.Runtime.InteropServices;
	
	public abstract class ClientBase
	{
		public abstract object Get (string key);
		internal abstract void SetValue (string key, Value val);

		public void Set (string key, object val)
		{
			SetValue (key, new Value (val));
		}

		[DllImport("gconf-2")]
		static extern bool gconf_is_initialized ();
	
		[DllImport("gconf-2")]
		static extern bool gconf_init (int argc, IntPtr argv, out IntPtr err);
	
		internal void Initialize ()
		{
			if (!gconf_is_initialized ())
			{
				IntPtr err;
				gconf_init (0, IntPtr.Zero, out err);
				if (err != IntPtr.Zero)
					throw new GLib.GException (err);
			}
		}
	}
}


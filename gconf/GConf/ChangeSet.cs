// ChangeSet.cs -
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
	
	public class ChangeSet : ClientBase, IDisposable
	{
		IntPtr Raw = IntPtr.Zero;
	
		[DllImport("gconf-2")]
		static extern IntPtr gconf_change_set_new ();

		public ChangeSet ()
		{
			Initialize ();
			Raw = gconf_change_set_new ();
		}

		internal ChangeSet (IntPtr raw) : base ()
		{
			Initialize ();
			Raw = raw;
		}

		~ChangeSet ()
		{
			Dispose ();
		}

		[DllImport("gconf-2")]
		static extern void gconf_change_set_unref (IntPtr cs);

		public void Dispose ()
		{
			if (Raw != IntPtr.Zero)
			{
				gconf_change_set_unref (Raw);
				Raw = IntPtr.Zero;
			}
		}

		internal IntPtr Handle {
			get {
				return Raw;
			}
		}

		[DllImport("gconf-2")]
		static extern void gconf_change_set_set (IntPtr cs, string key, IntPtr val);
		
		internal override void SetValue (string key, Value val)
		{
			gconf_change_set_set (Raw, key, val.Handle);
		}

		[DllImport("gconf-2")]
		static extern bool gconf_change_set_check_value (IntPtr cs, string key, out IntPtr val);

		public override object Get (string key)
		{
			IntPtr raw_val;
			if (gconf_change_set_check_value (Raw, key, out raw_val) && raw_val != IntPtr.Zero) {
				Value val = new Value (raw_val);
				val.Managed = false;
				return val.Get ();
			} else {
				throw new NoSuchKeyException ();
			}
		}
	}
}


// Engine.cs - configureation engine wrapper
//
// Author: Mike Kestner  <mkestner@novell.com>
//
// Copyright (c) 2004  Novell, Inc.
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
	
	public class Engine : IDisposable
	{
		IntPtr handle = IntPtr.Zero;
	
		[DllImport("gconf-2")]
		static extern bool gconf_is_initialized ();
	
		[DllImport("gconf-2")]
		static extern bool gconf_init (int argc, IntPtr argv, out IntPtr err);
	
		static Engine ()
		{
			if (!gconf_is_initialized ()) {
				IntPtr err;
				gconf_init (0, IntPtr.Zero, out err);
				if (err != IntPtr.Zero)
					throw new GLib.GException (err);
			}
		}

		private Engine (IntPtr handle)
		{
			this.handle = handle;
		}

		~Engine ()
		{
			Dispose ();
		}

		[DllImport("gconf-2")]
		static extern void gconf_engine_unref (IntPtr cs);

		public void Dispose ()
		{
			if (handle != IntPtr.Zero) {
				gconf_engine_unref (handle);
				handle = IntPtr.Zero;
			}
		}

		[DllImport("gconf-2")]
		static extern IntPtr gconf_engine_get_default ();

		static Engine default_engine;
		public static Engine Default {
			get {
				if (default_engine == null)
					default_engine = new Engine (gconf_engine_get_default ());

				return default_engine;
			}
		}
				
		[DllImport("gconf-2")]
		static extern bool gconf_engine_commit_change_set (IntPtr handle, IntPtr cs, bool remove, out IntPtr err);
		
		public void CommitChangeSet (ChangeSet cs, bool remove_committed)
		{
			IntPtr err_handle;
			if (!gconf_engine_commit_change_set (handle, cs.Handle, remove_committed, out err_handle))
				throw new GLib.GException (err_handle);
		}

		[DllImport("gconf-2")]
		static extern IntPtr gconf_engine_reverse_change_set (IntPtr handle, IntPtr cs, out IntPtr err);
		
		public ChangeSet ReverseChangeSet (ChangeSet cs)
		{
			IntPtr err_handle;
			ChangeSet result = new ChangeSet (gconf_engine_reverse_change_set (handle, cs.Handle, out err_handle));
			if (err_handle != IntPtr.Zero)
				throw new GLib.GException (err_handle);
			return result;
		}
	}
}


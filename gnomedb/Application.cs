// GnomeDb.Application.cs - libgnomedb initialization and event loop
//
// Author: Rodrigo Moya <rodrigo@ximian.com>
//
// Copyright (c) 2002 Rodrigo Moya
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


using System;
using System.Runtime.InteropServices;

namespace GnomeDb
{
	/// <summary>
	///   GnomeDb Application class
	/// </summary>
	///
	/// <remarks>
	///   Provides the initialization and event loop iteration related
	///   methods for the libgnomedb library.
	/// </remarks>

	public class Application
	{
		private const string VERSION = "0.10";

		[DllImport("gnomedb-2")]
		static extern void gnome_db_init (string app_id, string version, int nargs, IntPtr args);

		public static void Init ()
		{
			gnome_db_init ("GnomeDb#", VERSION, 0, new IntPtr(0));
		}

		public static void Init (string app_id, string version)
		{
			gnome_db_init (app_id, version, 0, new IntPtr(0));
		}

		[DllImport("gnomedb-2")]
		static extern void gnome_db_init (string app_id, string version, ref int nargs, ref String [] args);
		
		public static void Init (ref string [] args)
		{
			int argc = args.Length;
			gnome_db_init ("GnomeDb#", VERSION, ref argc, ref args);
		}

		public static void Init (string app_id, string version, ref string [] args)
		{
			int argc = args.Length;
			gnome_db_init (app_id, version, ref argc, ref args);
		}
		
		[DllImport("gnomedb-2")]
		static extern void gnome_db_main_run (IntPtr init_func, IntPtr user_data);

		public static void Run ()
		{
			gnome_db_main_run (IntPtr.Zero, IntPtr.Zero);
		}

		[DllImport("gnomedb-2")]
		static extern void gnome_db_main_quit ();

		public static void Quit ()
		{
			gnome_db_main_quit ();
		}
	}
}

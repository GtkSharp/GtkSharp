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

	public class Application
	{
		private const string VERSION = "0.10";

		[DllImport("gnomedb-2")]
		static extern void gnome_db_init (IntPtr app_id, IntPtr version, int nargs, IntPtr args);

		public static void Init ()
		{
			Init ("GnomeDb#", VERSION);
		}

		public static void Init (string app_id, string version)
		{
			IntPtr native_appid = GLib.Marshaller.StringToPtrGStrdup (app_id);
			IntPtr native_version = GLib.Marshaller.StringToPtrGStrdup (version);
			gnome_db_init (native_appid, native_version, 0, IntPtr.Zero);
			GLib.Marshaller.Free (native_appid);
			GLib.Marshaller.Free (native_version);
		}

		[DllImport("gnomedb-2")]
		static extern void gnome_db_init (IntPtr app_id, IntPtr version, ref int argc, ref IntPtr argv);
		
		public static void Init (ref string [] args)
		{
			Init ("GnomeDb#", VERSION, ref args);
		}

		public static void Init (string app_id, string version, ref string[] args)
		{
			IntPtr native_appid = GLib.Marshaller.StringToPtrGStrdup (app_id);
			IntPtr native_version = GLib.Marshaller.StringToPtrGStrdup (version);
			GLib.Argv argv = new GLib.Argv (args);
			IntPtr arg_ptr = argv.Handle;
			int argc = args.Length;
			gnome_db_init (native_appid, native_version, ref argc, ref arg_ptr);
			GLib.Marshaller.Free (native_appid);
			GLib.Marshaller.Free (native_version);
			if (arg_ptr != argv.Handle)
				throw new Exception ("Init returned new argv handle.");
			if (argc <= 1)
				args = new string [0];
			else
				args = argv.GetArgs (argc);
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

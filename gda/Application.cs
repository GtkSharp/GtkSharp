// Gda.Application.cs - libgda initialization and event loop
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

namespace Gda
{
	public class Application
	{
		private const string VERSION = "0.10";

		[DllImport("gda-2")]
		static extern void gda_init (IntPtr app_id, IntPtr version, int nargs, IntPtr args);

		[DllImport("gda-2")]
		static extern void gda_init (IntPtr app_id, IntPtr version, ref int argc, ref IntPtr argv);
		
		public static void Init ()
		{
			Init ("Gda#", VERSION);
		}

		public static void Init (string app_id, string version)
		{
			IntPtr native_appid = GLib.Marshaller.StringToPtrGStrdup (app_id);
			IntPtr native_version = GLib.Marshaller.StringToPtrGStrdup (version);
			gda_init (native_appid, native_version, 0, IntPtr.Zero);
			GLib.Marshaller.Free (native_appid);
			GLib.Marshaller.Free (native_version);
		}

		public static void Init (ref string[] args)
		{
			Init ("Gda#", VERSION, ref args);
		}

		public static void Init (string app_id, string version, ref string[] args)
		{
			IntPtr native_appid = GLib.Marshaller.StringToPtrGStrdup (app_id);
			IntPtr native_version = GLib.Marshaller.StringToPtrGStrdup (version);
			GLib.Argv argv = new GLib.Argv (args);
			IntPtr arg_ptr = argv.Handle;
			int argc = args.Length;
			gda_init (native_appid, native_version, ref argc, ref arg_ptr);
			GLib.Marshaller.Free (native_appid);
			GLib.Marshaller.Free (native_version);
			if (arg_ptr != argv.Handle)
				throw new Exception ("Init returned new argv handle.");
			if (argc <= 1)
				args = new string [0];
			else
				args = argv.GetArgs (argc);
		}

		[DllImport("gda-2")]
		static extern void gda_main_run (IntPtr init_func, IntPtr user_data);

		public static void Run ()
		{
			gda_main_run (IntPtr.Zero, IntPtr.Zero);
		}

		[DllImport("gda-2")]
		static extern void gda_main_quit ();

		public static void Quit ()
		{
			gda_main_quit ();
		}
	}
}

//
// GnomeDb.Application.cs - libgnomedb initialization and event loop
//
// Author: Rodrigo Moya <rodrigo@ximian.com>
//
// (c) 2002 Rodrigo Moya
//

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

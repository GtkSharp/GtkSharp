//
// Gda.Application.cs - libgda initialization and event loop
//
// Author: Rodrigo Moya <rodrigo@ximian.com>
//
// (c) 2002 Rodrigo Moya
//

using System;
using System.Runtime.InteropServices;

namespace Gda
{
	/// <summary>
	///   GDA Application class
	/// </summary>
	///
	/// <remarks>
	///   Provides the initialization and event loop iteration related
	///   methods for the GDA data access library.
	/// </remarks>

	public class Application
	{
		private const string VERSION = "0.10";

		[DllImport("gda-2")]
		static extern void gda_init (string app_id, string version, int nargs, string[] args);

		public static void Init ()
		{
			gda_init ("Gda#", VERSION, 0, new string[0]);
		}

		public static void Init (string app_id, string version)
		{
			gda_init (app_id, version, 0, new string[0]);
		}

		public static void Init (string[] args)
		{
			gda_init ("Gda#", VERSION, args.Length, args);
		}

		public static void Init (string app_id, string version, string[] args)
		{
			gda_init (app_id, version, args.Length, args);
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

// GTK.Application.cs - GTK Main Event Loop class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	/// <summary>
	///	Application Class
	/// </summary>
	///
	/// <remarks>
	///	Provides the initialization and event loop iteration related
	///	methods for the GTK widget library.  Since GTK is an event 
	///	driven toolkit, Applications register callbacks against various
	///	events to handle user input. These callbacks are invoked from
	///	the main event loop when events are detected.
	/// </remarks>

	public class Application {

		[DllImport("gtk-1.3")]
		static extern void gtk_init (ref int argc, ref String[] argv);

		/// <summary>
		///	Init Method
		/// </summary>
		/// 
		/// <remarks>
		///	Initializes GTK resources.
		/// </remarks>

		public static void Init (ref string[] args)
		{
			int argc = args.Length;
			gtk_init (ref argc, ref args);
		}

		/// <summary>
		///	Run Method
		/// </summary>
		/// 
		/// <remarks>
		///	Begins the event loop iteration.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_main ();

		public static void Run ()
		{
			gtk_main ();
		}


		/// <summary>
		///	Quit Method
		/// </summary>
		/// 
		/// <remarks>
		///	Terminates the event loop iteration.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_main_quit ();

		public static void Quit ()
		{
			gtk_main_quit ();
		}
			
	}
}

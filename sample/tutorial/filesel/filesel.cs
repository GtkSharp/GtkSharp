// filesel.cs - Gtk# Tutorial example
//
// Author: Johannes Roith <johannes@jroith.de>
//
// (c) 2002 Johannes Roith

namespace GtkSharpTutorial {


	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;


	public class filesel
	{

		static FileSelection filew;

		/* Get the selected filename and print it to the console */

		static void file_ok_sel_event( object obj, EventArgs args )
		{
		    Console.WriteLine("{0}\n",filew.Filename);
		}

		static void delete_event (object obj, DeleteEventArgs args)
		{
			Application.Quit();
		}

		static void cancel_event (object obj, EventArgs args)
		{
			Application.Quit();
		}

		public static void Main(string[] args)
		{
 
			Application.Init ();

    
			/* Create a new file selection widget */
			filew = new FileSelection("File selection");
    
			filew.DeleteEvent += new DeleteEventHandler (delete_event);

			/* Connect the ok_button to file_ok_sel function */
			filew.OkButton.Clicked +=new EventHandler (file_ok_sel_event);
   
			/* Connect the cancel_button to destroy the widget */

			filew.CancelButton.Clicked +=new EventHandler (cancel_event);


			/* Lets set the filename, as if this were a save dialog, and we are giving
			 a default filename */

			filew.Filename = "penguin.png";
    
			filew.Show();

			Application.Run();

		}

	}
}
// table.cs - Gtk# Tutorial example
//
// Author: Johannes Roith <johannes@jroith.de>
//
// (c) 2002 Johannes Roith

namespace GtkSharpTutorial {


	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;


	public class table
	{

		/* Our new improved callback.  The data passed to this function
		 * is printed to stdout. */

		static void callback( object obj, EventArgs args)
		{
			Button mybutton = (Button) obj;
			Console.WriteLine("Hello again - {0} was pressed", (string) mybutton.Label);
			// Have to figure out, how to recieve button name 
		}

		/* another event */
		static void delete_event (object obj, DeleteEventArgs args)
		{
		Application.Quit();
		}

		static void exit_event (object obj, EventArgs args)
		{
			Application.Quit();
		}

		public static void Main(string[] args)
		{



			Application.Init ();


			/* Create a new window */
			Window window = new Window ("Table");


			/* Set a handler for delete_event that immediately
			 * exits GTK. */
			window.DeleteEvent += new DeleteEventHandler (delete_event);

			/* Sets the border width of the window. */
			window.BorderWidth= 20;

			/* Create a 2x2 table */
			Table table = new Table (2, 2, true);

			/* Put the table in the main window */
			window.Add(table);

			/* Create first button */
			Button button = new Button("button 1");

			/* When the button is clicked, we call the "callback" function
			 * with a pointer to "button 1" as its argument */
			button.Clicked += new EventHandler (callback);


			/* Insert button 1 into the upper left quadrant of the table */
			table.Attach(button, 0, 1, 0, 1);

			button.Show();

			/* Create second button */

			Button button2 = new Button("button 2");

			/* When the button is clicked, we call the "callback" function
			 * with a pointer to "button 2" as its argument */
 
			button2.Clicked += new EventHandler (callback);

			/* Insert button 2 into the upper right quadrant of the table */
			table.Attach(button2, 1, 2, 0, 1);

			button2.Show();

			/* Create "Quit" button */
			Button quitbutton = new Button("Quit");

			/* When the button is clicked, we call the "delete_event" function
			 * and the program exits */
			quitbutton.Clicked += new EventHandler (exit_event);

			/* Insert the quit button into the both 
			 * lower quadrants of the table */
			table.Attach(quitbutton, 0, 2, 1, 2);

			quitbutton.Show();

			table.Show();
			window.ShowAll();

			Application.Run();
 
		}
	}
}
// helloworld2.cs - Gtk# Tutorial example
//
// Author: Johannes Roith <johannes@jroith.de>
//
// (c) 2002 Johannes Roith

namespace GtkSharpTutorial {


	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;


	public class helloworld2
	{

		/* Our new improved callback.  The data is extracted from obj and 
		 * is printed to stdout. */

		static void callback( object obj, EventArgs args)
		{
			Button mybutton = (Button) obj;
			Console.WriteLine("Hello again - {0} was pressed", (string) mybutton.Label);
			 // Have to figure out, how to recieve button name 
		}

		/* Exit event */
		static void delete_event (object obj, DeleteEventArgs args)
		{
			Application.Quit();
		}

		public static void Main(string[] args)
		{


			/* This is called in all GTK applications. Arguments are parsed
			 * from the command line and are returned to the application. */
			Application.Init ();

			/* Create a new window */
			Window window = new Window ("helloworld");

			/* This is a new call, which just resets the title of our
			 * new window to "Hello Buttons!" */
			window.Title ="Hello Buttons!";

			/* Here we just set a handler for delete_event that immediately
			 * exits GTK. */
			window.DeleteEvent += new DeleteEventHandler (delete_event);

			/* Sets the border width of the window. */
			window.BorderWidth = 10;

			/* We create a box to pack widgets into.  This is described in detail
			 * in the "packing" section. The box is not really visible, it
			 * is just used as a tool to arrange widgets. */
			HBox box1 = new HBox (false, 0);
			
			/* Put the box into the main window. */
			window.Add (box1);

			/* Creates a new button with the label "Button 1". */
			ToggleButton button1 = new ToggleButton ("Button 1");
    
			/* Now when the button is clicked, we call the "callback" event
			 * with a pointer to "button 1" as its argument */
			button1.Clicked += new EventHandler (callback);

			/* Instead of gtk_container_add, we pack this button into the invisible
			 * box, which has been packed into the window. */
			box1.PackStart (button1, true, true, 0);
 
			/* Always remember this step, this tells GTK that our preparation for
			 * this button is complete, and it can now be displayed. */
			button1.Show();

			/* Do these same steps again to create a second button */
			Button button2 = new Button ("Button 2");
   
			/* Call the same callback function with a different argument,
			 * passing a pointer to "button 2" instead. */
			button2.Clicked += new EventHandler (callback);

			box1.PackStart (button2, true, true, 0);
 
			/* The order in which we show the buttons is not really important, but I
			 * recommend showing the window last, so it all pops up at once. */

			window.ShowAll ();
 
			/* Rest in  Application.Run and wait for the fun to begin! */
			Application.Run();


			}
	}
}
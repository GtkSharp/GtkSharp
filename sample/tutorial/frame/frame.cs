// frame.cs - Gtk# Tutorial example
//
// Author: Johannes Roith <johannes@jroith.de>
//
// (c) 2002 Johannes Roith

namespace GtkSharpTutorial {


	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;


	public class frame
	{

		static void delete_event (object obj, DeleteEventArgs args)
		{
		Application.Quit();
		}

		public static void Main( string[] args)
		{

			/* Initialise GTK */
			Application.Init();
    
			/* Create a new window */
			Window window = new Window ("Frame Example");
  
			/* Here we connect the "destroy" event to a signal handler */ 
			window.DeleteEvent += new DeleteEventHandler (delete_event);

			window.SetSizeRequest(300, 300);
			/* Sets the border width of the window. */
			window.BorderWidth= 10;

			/* Create a Frame */
			Frame frame = new Frame("MyFrame");
			window.Add(frame);

			/* Set the frame's label */
			frame.Label = "GTK Frame Widget";

			/* Align the label at the right of the frame */

			frame.SetLabelAlign((float)1.0,(float)0.0);

			/* Set the style of the frame */
			frame.ShadowType = (ShadowType) 4;

			frame.Show();
  
			/* Display the window & all widgets*/
			window.ShowAll();
    
			/* Enter the event loop */
			Application.Run();
    
		}

	}

}
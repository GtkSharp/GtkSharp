// aspectframe.cs - Gtk# Tutorial example
//
// Author: Johannes Roith <johannes@jroith.de>
//
// (c) 2002 Johannes Roith

namespace GtkSharpTutorial {

	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;


	public class aspectframe
	{

		static void delete_event (object obj, DeleteEventArgs args)
		{
			Application.Quit();
		}

		public static void  Main(string[] args)
		{

			Application.Init ();

			/* Create new window */
			Window window = new Window ("Aspect Frame");
			window.BorderWidth = 10;

			window.DeleteEvent += new DeleteEventHandler (delete_event);

			/* Create an aspect_frame and add it to our toplevel window */
   
			AspectFrame aspect_frame = new AspectFrame("2x1", (float)0.5,(float)0.5, 2, false);
   
			window.Add(aspect_frame);
			aspect_frame.Show();
   
			/* Now add a child widget to the aspect frame */
  
			DrawingArea  drawing_area = new DrawingArea();
   
			/* Ask for a 200x200 window, but the AspectFrame will give us a 200x100
			 * window since we are forcing a 2x1 aspect ratio */
			drawing_area.SetSizeRequest (200, 200);
			aspect_frame.Add(drawing_area);
			drawing_area.Show();

			window.ShowAll();
   
			Application.Run();
		}
	}

}
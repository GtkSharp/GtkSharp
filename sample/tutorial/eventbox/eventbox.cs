// eventbox.cs - Gtk# Tutorial example
//
// Author: Alejandro Sánchez Acosta <raciel@es.gnu.org>
// 	   Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//
// (c) 2002 Alejandro Sánchez Acosta
// 	    Cesar Octavio Lopez Nataren

namespace GtkSharpTutorial {

	using Gtk;
	using GtkSharp;
	using Gdk;
	using GdkSharp;
	using System;
	using System.Drawing;

	public class eventbox
	{

		static void delete_event (object obj, DeleteEventArgs args)
		{
			Application.Quit();
		}

		static void exitbutton_event (object obj, ButtonPressEventArgs args)
		{
			Application.Quit();
		}

		public static void Main (string[] args)
		{
			Gtk.Window window;
			Gdk.CursorType cursortype;
			EventBox eventbox;
			Label label;

			Application.Init();

			window = new Gtk.Window ("Eventbox");
			window.DeleteEvent += new DeleteEventHandler (delete_event);

			window.BorderWidth = 10;

			eventbox = new EventBox ();
			window.Add (eventbox);
			eventbox.Show();

			label = new Label ("Click here to quit, quit, quit, quit");
			eventbox.Add(label);
			label.Show();

			label.SetSizeRequest(110, 20);

			/* eventbox.Events = GDK_BUTTON_PRESS_MASK; */ //Add this feature

			eventbox.ButtonPressEvent += new ButtonPressEventHandler (exitbutton_event);

			eventbox.Realize();

			eventbox.GdkWindow.Cursor = Cursor.New(CursorType.Hand1);
	
			window.Show();
			
			Application.Run();
		}
	}
}

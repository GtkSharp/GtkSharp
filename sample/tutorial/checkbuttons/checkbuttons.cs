// checkbuttons.cs - GTK# Tutorial example
//
// Authors: Alejandro Sanchez Acosta <raciel@es.gnu.org>
//          Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//
// (C) 2002 Alejandro Sanchez Acosta <raciel@es.gnu.org>
//          Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>

namespace GtkSharpTutorial {


        using Gtk;
        using GtkSharp;
        using System;
        using System.Drawing;


	public class checkbuttons
	{
		static void delete_event(object obj, DeleteEventArgs args)
		{
			Application.Quit();
		}

		static void clickedCallback(object obj, EventArgs args)
		{
			if (((CheckButton) obj).Active)
				Console.WriteLine ("CheckButton clicked, I'm activating");
			else
				Console.WriteLine ("CheckButton clicked, I'm desactivating");
		}


		public static void Main(string[] args)
		{
			Application.Init();
			
			HBox hbox = new HBox(false, 0);
			hbox.BorderWidth = 2;
			
			CheckButton cb1 = new CheckButton ("CheckButton 1");
			cb1.Clicked += new EventHandler (clickedCallback);

			CheckButton cb2 = new CheckButton ("CheckButton 2");
			cb2.Clicked += new EventHandler (clickedCallback);

			hbox.PackStart(cb1, false, false, 3);
			hbox.PackStart(cb2, false, false, 3);

			Window window = new Window ("Check buttons");
			window.BorderWidth = 10;
			window.DeleteEvent  += new DeleteEventHandler (delete_event);

			window.Add(hbox);
			window.ShowAll();
			Application.Run();
		}
	}
}

// Menus.cs : Menu testing sample app
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// <c> 2002 Mike Kestner

namespace GtkSharp.Samples {

	using System;
	using Gtk;

	public class MenuApp {

		public static void Main (string[] args)
		{
			Application.Init();
			Window win = new Window ("Menu Sample App");
			win.DeleteEvent += new DeleteEventHandler (delete_cb);
			win.DefaultWidth = 200;
			win.DefaultHeight = 150;

			VBox box = new VBox (false, 2);

			MenuBar mb = new MenuBar ();
			Menu file_menu = new Menu ();
			MenuItem exit_item = new MenuItem("Exit");
			exit_item.Activated += new EventHandler (exit_cb);
			file_menu.Append (exit_item);
			MenuItem file_item = new MenuItem("File");
			file_item.Submenu = file_menu;
			mb.Append (file_item);
			box.PackStart(mb, false, false, 0);

			Button btn = new Button ("Yep, that's a menu");
			box.PackStart(btn, true, true, 0);
			
			win.Add (box);
			win.ShowAll ();

			Application.Run ();
		}

		static void delete_cb (object o, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}

		static void exit_cb (object o, EventArgs args)
		{
			Application.Quit ();
		}
	}
}


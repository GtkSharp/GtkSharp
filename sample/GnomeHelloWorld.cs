// GnomeHelloWorld.cs - Basic Gnome/Gnome.UI sample app 
//
// Author: Rachel Hestilow <hestilow@ximian.com>
//
// (c) 2002 Rachel Hestilow

namespace GtkSamples {

	using Gtk;
	using Gdk;
	using GtkSharp;
	using Gnome;
	using System;
	using System.IO;
	using System.Drawing;
	using System.Runtime.InteropServices;

	public struct DemoEntry {
		public string program;
		public string desc;
		public string icon;

		public DemoEntry (string program, string desc, string icon)
		{
			this.program = program;
			this.desc = desc;
			this.icon = icon;
		}
	}
	
	public class GnomeHelloWorld  {
		DemoEntry[] entries;

		public GnomeHelloWorld () {
			entries = new DemoEntry [3];
			entries[0] = new DemoEntry ("button.exe", "Button", "gnome-ccdialog.png"); 
			entries[1] = new DemoEntry ("menu.exe", "Menu", "gnome-gmenu.png"); 
			entries[2] = new DemoEntry ("gtk-hello-world.exe", "Gtk# Hello World", "gnome-mdi.png"); 
		}
		
		string BaseName (string filename)
		{
			int ind = filename.LastIndexOf (Path.DirectorySeparatorChar);
			if (ind != -1)
				return filename.Substring (ind);
			else
				return filename;
		}

		IconList CreateList ()
		{
			IconList icons = new IconList (64, new Gtk.Adjustment (IntPtr.Zero), 0);

			foreach (DemoEntry entry in entries)
			{
				icons.Append ("pixmaps" + Path.DirectorySeparatorChar + entry.icon,
							     entry.desc);
			}

			icons.IconSelected += new EventHandler (icon_selected_cb);

			return icons;
		}

		Gtk.Widget CreateMenus ()
		{
			MenuBar bar = new MenuBar ();
			
			Menu file_menu = new Menu ();
			MenuItem file_menu_item = new MenuItem ("_File");
			file_menu_item.Submenu = file_menu;
			
			MenuItem file_exit = new MenuItem ("E_xit");
			file_exit.Activated += new EventHandler (exit_cb);
			file_menu.Append (file_exit);
			bar.Append (file_menu_item);

			Menu help_menu = new Menu ();
			MenuItem help_menu_item = new MenuItem ("_Help");
			help_menu_item.Submenu = help_menu;
			
			MenuItem file_help = new MenuItem ("_About");
			file_help.Activated += new EventHandler (about_cb);
			help_menu.Append (file_help);
			bar.Append (help_menu_item);

			return bar;
		}

		public Gtk.Window CreateWindow ()
		{
			Gtk.Window win = new Gtk.Window ("Gnome# Hello World");
			win.DeleteEvent += new EventHandler (Window_Delete);

			VBox vbox = new VBox (false, 0);
			vbox.PackStart (CreateMenus (), false, false, 0);
			vbox.PackStart (new Label ("The following demos are available.\nTo run a demo, double click on its icon."), false, false, 4);
			vbox.PackStart (CreateList (), true, true, 4);

			win.DefaultSize = new Size (250, 130);
			win.Add (vbox);

			return win;
		}
	
		public static int Main (string[] args)
		{
			Application.Init ();
			
			GnomeHelloWorld hello = new GnomeHelloWorld ();
			Window win = hello.CreateWindow ();
			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void Window_Delete (object obj, EventArgs args)
		{
			SignalArgs sa = (SignalArgs) args;
			Application.Quit ();
			sa.RetVal = true;
		}
		
		static void exit_cb (object o, EventArgs args)
		{
			Application.Quit ();
		}
		
		static void about_cb (object o, EventArgs args)
		{
			Pixbuf logo = new Pixbuf ("pixmaps" + Path.DirectorySeparatorChar + "gtk-sharp-logo.png");
			String[] authors = new string[] {
				"Rachel Hestilow (hestilow@ximian.com)"
			};
			string[] documentors = new string[] {};

			About about = new About ("Gnome# Hello World", "0.0.1",
			                         "Copyright (C) 2002 Rachel Hestilow",
											 "A test application for the GNOME .NET bindings",
											 authors, documentors, "", logo);
			about.Show ();
		}

		// Wonder what the .NET func to do this is...and if it
		// is implemented in mono yet.
		[DllImport("glib-2.0")]
		static extern bool g_spawn_command_line_async (string command, IntPtr err);
		
		void icon_selected_cb (object obj, EventArgs args)
		{
			SignalArgs sa = (SignalArgs) args;
			int idx = (int) sa.Args[0];
			Event ev = (Event) sa.Args[1];

			if (ev.IsValid && ev.Type == EventType.TwoButtonPress) {
				g_spawn_command_line_async ("mono " + entries[idx].program, IntPtr.Zero); 
			}
		}
	}
}

// GnomeHelloWorld.cs - Basic Gnome/Gnome.UI sample app 
//
// Author: Rachel Hestilow <hestilow@ximian.com>
//
// (c) 2002 Rachel Hestilow

namespace GtkSamples {

	using Gtk;
	using Gdk;
	using Gnome;
	using System;
	using System.IO;
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
			IconList icons = new IconList (64, null, 0);

			foreach (DemoEntry entry in entries)
			{
				icons.Append ("pixmaps" + Path.DirectorySeparatorChar + entry.icon,
							     entry.desc);
			}

			icons.IconSelected += new Gnome.IconSelectedHandler (icon_selected_cb);

			return icons;
		}

		Gtk.MenuBar CreateMenus ()
		{
			AccelGroup group = new AccelGroup ();
			MenuBar bar = new MenuBar ();
			
			Menu file_menu = new Menu ();
			MenuItem file_menu_item = new MenuItem ("_File");
			file_menu_item.Submenu = file_menu;
			
			ImageMenuItem file_exit = new ImageMenuItem (Gtk.Stock.Quit, group);
			file_exit.Activated += new EventHandler (exit_cb);
			file_menu.Append (file_exit);
			bar.Append (file_menu_item);

			Menu help_menu = new Menu ();
			ImageMenuItem help_menu_item = new ImageMenuItem (Gtk.Stock.Help, group);
			help_menu_item.Submenu = help_menu;
			
			ImageMenuItem file_help = new ImageMenuItem (Gnome.Stock.About, group);
			file_help.Activated += new EventHandler (about_cb);
			help_menu.Append (file_help);
			bar.Append (help_menu_item);
			bar.ShowAll ();

			return bar;
		}

		public Gtk.Window CreateWindow ()
		{
			Gnome.App win = new Gnome.App ("gnome-hello-world", "Gnome# Hello World");
			win.DeleteEvent += new DeleteEventHandler (Window_Delete);

			win.Menus = CreateMenus ();
			
			VBox vbox = new VBox (false, 0);
			vbox.PackStart (new Label ("The following demos are available.\nTo run a demo, double click on its icon."), false, false, 4);
			vbox.PackStart (CreateList (), true, true, 4);
			win.Contents = vbox;

			win.DefaultWidth = 250;
			win.DefaultHeight = 200;

			return win;
		}
	
		public static int Main (string[] args)
		{
			Program kit = new Program ("gnome-hello-world", "0.0.1", Modules.UI,
												args);
			
			GnomeHelloWorld hello = new GnomeHelloWorld ();
			Gtk.Window win = hello.CreateWindow ();
			win.ShowAll ();
			kit.Run ();
			return 0;
		}

		static void Window_Delete (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
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
		
		void icon_selected_cb (object obj, Gnome.IconSelectedArgs args)
		{
			int idx = args.Num;
			EventButton ev = new EventButton (args.Event.Handle);

			if (ev.Type == EventType.TwoButtonPress && ev.Button == 1) {
				g_spawn_command_line_async ("mono " + entries[idx].program, IntPtr.Zero); 
			}
		}
	}
}

// notebook.cs - Gtk# Tutorial example
// 
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
	using Glib;
	using GlibSharp;
	using System;
	using System.Drawing;

	public class notebook
	{

		static void delete_event (object obj, DeleteEventArgs args)
		{
			Application.Quit();
		}

		static void exitbutton_event (object obj, EventArgs args)
		{
			Application.Quit();
		}

		static void nextPage (object obj, EventArgs args)
		{
			notebook1.NextPage ();
		}

		static void prevPage (object obj, EventArgs args)
		{
			notebook1.PrevPage ();
		}

		// FIXME
		static void rotate_book (object obj, EventArgs args)
		{
			// notebook1.TabPos = ((notebook1.TabPos + 1)% 4);
		}

		static void tabsborder_book (object obj, EventArgs args)
		{
			bool tval = false;
			bool bval = false;
			if (notebook1.ShowTabs == false)
				tval = true;
			if (notebook1.ShowBorder == false)
				bval = true;
			notebook1.ShowTabs = tval;
			notebook1.ShowBorder = bval;
		}

		static void remove_book (object obj, EventArgs args)
		{
			int page;
			page = notebook1.CurrentPage;
			notebook1.RemovePage (page);
			notebook1.QueueDraw();
		}

		static Gtk.Window window;
		static Button button;
		static Table table;
		static Notebook notebook1;
		static Frame frame;
		static Label label;
		static CheckButton checkbutton;			

		public static void Main (string[] args)
		{

			int i;
			string bufferf;
			string bufferl;


			Application.Init();

			window = new Gtk.Window ("Notebook");
			window.DeleteEvent += new DeleteEventHandler (delete_event);

			window.BorderWidth = 10;

			table = new Table (3, 6, false);
			window.Add (table);

			notebook1 = new Notebook();
			notebook1.TabPos = PositionType.Top;
			table.Attach (notebook1, 0, 6, 0 ,1);
			notebook1.Show();

			for (i=0; i<5; i++){
				bufferf = "Append Frame" + (i+1).ToString();
				bufferl = "Page " + (i+1).ToString();

				frame = new Frame (bufferf);
				frame.BorderWidth = 10;
				frame.SetSizeRequest (100, 75);
				frame.Show();

				label = new Label (bufferf);
				frame.Add (label);				       label.Show();

				label = new Label (bufferl);
				notebook1.AppendPage (frame, label);
			}

			checkbutton = new CheckButton ("Check me please!");
			checkbutton.SetSizeRequest (100, 75);
			checkbutton.Show();

			label = new Label ("Add page");
			notebook1.InsertPage (checkbutton, label, 2);

			for (i=0; i<5; i++) {
				bufferf = "Append Frame" + (i+1).ToString();
				bufferl = "Page " + (i+1).ToString();
				frame = new Frame (bufferf);
				frame.BorderWidth = 10;
				frame.SetSizeRequest (100, 75);
				frame.Show();

				label = new Label (bufferf);
				frame.Add (label);
				label.Show();	         
				label = new Label (bufferl);
				notebook1.PrependPage (frame, label);
			}
			notebook1.CurrentPage = 3;
			button = new Button ("close");
			button.Clicked += new EventHandler (exitbutton_event);
			table.Attach (button, 0, 1, 1, 2);
			button.Show();

			button = new Button ("next page");
			button.Clicked += new EventHandler (nextPage);
			table.Attach (button, 1, 2, 1, 2);
			button.Show();

			button = new Button ("prev page");
			button.Clicked += new EventHandler (prevPage);

			table.Attach (button, 2, 3, 1, 2);
			button.Show();

			button = new Button ("tab position");
			button.Clicked += new EventHandler (rotate_book);
			table.Attach (button, 3, 4, 1, 2);
			button.Show();

			button = new Button ("tables/border on/off");
			button.Clicked += new EventHandler (tabsborder_book);
			table.Attach (button, 4, 5, 1, 2);
			button.Show();

			button = new Button ("remove page");
			button.Clicked += new EventHandler (remove_book);
			table.Attach (button, 5, 6, 1, 2);
			button.Show();
			table.Show();
			window.Show();

			Application.Run();
		}
	}
}

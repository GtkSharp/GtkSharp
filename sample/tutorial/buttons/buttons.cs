// buttons.cs - Gtk# Tutorial example
//
// Author: Johannes Roith <johannes@jroith.de>
//
// (c) 2002 Johannes Roith

namespace GtkSharpTutorial {

	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;


	public class buttons
	{

		/* Create a new hbox with an image and a label packed into it
		 * and return the box. */

		static Widget xpm_label_box(string xpm_filename, string label_text )
		{


			/* Create box for image and label */
			HBox box = new HBox(false, 0);
			box.BorderWidth =  2;
			
			/* Now on to the image stuff */
			Gtk.Image image = new Gtk.Image(xpm_filename);
			
			/* Create a label for the button */
			Label label = new Label (label_text);
			
			/* Pack the image and label into the box */
			box.PackStart (image, false, false, 3);
			box.PackStart(label, false, false, 3);
			
			image.Show();
			label.Show();
			
			return box;
		}


		/* Our usual callback function */
		static void callback( object obj, EventArgs args)
		{
			Console.WriteLine("Hello again - cool button was pressed");
		}

		/* another callback */
		static void delete_event (object obj, DeleteEventArgs args)
		{
			Application.Quit();
		}

		public static void Main(string[] args)
		{

			Application.Init();
			
			/* Create a new window */
			Window window = new Window ("Pixmap'd Buttons!");
			
			/* It's a good idea to do this for all windows. */
			window.DeleteEvent += new DeleteEventHandler (delete_event);
			
			/* Sets the border width of the window. */
			window.BorderWidth = 10;
			
			/* Create a new button */
			Button button = new Button();
			
			/* Connect the "clicked" signal of the button to our callback */
			button.Clicked += new EventHandler (callback);
			
			/* This calls our box creating function */
			Widget box = xpm_label_box ("info.xpm", "cool button");
			
			/* Pack and show all our widgets */
			box.Show();
			
			button.Add(box);
			
			button.Show();
			
			window.Add(button);
			
			window.ShowAll();
			
			/* Rest in gtk_main and wait for the fun to begin! */
			Application.Run();
		}
	}
}
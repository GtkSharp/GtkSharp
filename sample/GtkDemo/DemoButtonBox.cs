//
// DemoButtonBox.cs, port of buttonbox.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2003, Ximian Inc.


/* Button Boxes
 *
 * The Button Box widgets are used to arrange buttons with padding.
 */

using System;

using Gtk;
using GtkSharp;

namespace GtkDemo 
{
	public class DemoButtonBox 	
	{
		private Gtk.Window window;
		
		public DemoButtonBox ()
		{
			// Create a Window
			window = new Gtk.Window ("Button Boxes");
			window.DeleteEvent += new DeleteEventHandler (WindowDelete);
			window.BorderWidth = 10;

			// Add Vertical Box
			VBox mainVbox = new VBox (false,0);
			window.Add (mainVbox);

			// Add Horizontal Frame
			Frame horizontalFrame =  new Frame ("Horizontal Button Boxes");
			mainVbox.PackStart (horizontalFrame);		
			VBox vbox = new VBox (false, 0) ;
			vbox.BorderWidth = 10;
			horizontalFrame.Add (vbox);

                        // Pack Buttons
			vbox.PackStart(CreateButtonBox (true, "Spread (spacing 40)", 40, 85, 20, ButtonBoxStyle.Spread));
			vbox.PackStart(CreateButtonBox (true, "Edge (spacing 30)", 30, 85, 20, ButtonBoxStyle.Edge));
			vbox.PackStart(CreateButtonBox (true, "Start (spacing 20)", 20, 85, 20, ButtonBoxStyle.Start));
			vbox.PackStart(CreateButtonBox (true, "End (spacing 10)", 10, 85, 20, ButtonBoxStyle.End));

			//  Add Vertical Frame 
			Frame verticalFrame = new Frame ("Vertical Button Boxes");
			mainVbox.PackStart (verticalFrame);
			HBox hbox = new HBox (false, 0);
			hbox.BorderWidth = 10;
			verticalFrame.Add (hbox);

                        // Pack Buttons
			hbox.PackStart(CreateButtonBox (false, "Spread (spacing 5)", 5, 85, 20, ButtonBoxStyle.Spread));
			hbox.PackStart(CreateButtonBox (false, "Edge (spacing 30)", 30, 85, 20, ButtonBoxStyle.Edge));
			hbox.PackStart(CreateButtonBox (false, "Start (spacing 20)", 20, 85, 20, ButtonBoxStyle.Start));
			hbox.PackStart(CreateButtonBox (false, "End (spacing 20)", 20, 85, 20, ButtonBoxStyle.End));
		
			window.ShowAll ();
		}

		// Create a Button Box with the specified parameters
		private Frame CreateButtonBox (bool horizontal, string title,  int spacing, int childW , int childH , ButtonBoxStyle layout) 
		{
			Frame frame = new Frame (title);
			Gtk.ButtonBox bbox ;
			
			if (horizontal == true)
			{
				bbox =  new Gtk.HButtonBox ();
			}
			else
			{
				bbox =  new Gtk.VButtonBox (); 
			}
		
			bbox.BorderWidth = 5;
			frame.Add (bbox);
		
			// Set the appearance of the Button Box
			bbox.Layout = layout;
			bbox.Spacing= spacing;
		
			Button buttonOk = new Button (Stock.Ok);
			bbox.Add (buttonOk);
			Button buttonCancel = new Button (Stock.Cancel);
			bbox.Add (buttonCancel);
			Button buttonHelp = new Button (Stock.Help);
			bbox.Add (buttonHelp);
		
			return frame;
		}

		private void WindowDelete (object o, DeleteEventArgs args)
		{
			window.Hide ();
			window.Destroy ();
		}
	}
}

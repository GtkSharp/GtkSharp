//
// DemoImages.cs - port of images.c gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2003, Ximian Inc.
// 


/* Images
 *
 * Gtk.Image is used to display an image; the image can be in a number of formats.
 * Typically, you load an image into a Gdk.Pixbuf, then display the pixbuf.
 *
 * This demo code shows some of the more obscure cases, in the simple
 * case a call to the constructor Gtk.Image (string filename) is all you need.
 *
 * If you want to put image data in your program compile it in as a resource.
 * This way you will not need to depend on loading external files, your
 * application binary can be self-contained.
 */

// TODO:
// Finish implementing the callback, I can't get the image
// to show up, and I'm stuck in the ProgressivePreparedCallback
// because I can't get a white background to appear

using System;
using System.IO;

using Gtk;
using Gdk;

namespace GtkDemo {
	public class DemoImages
	{
		private Gtk.Window window;
		private static Gtk.Image progressiveImage;
		private VBox vbox;
		public DemoImages ()
		{
			window = new Gtk.Window ("Images");
			window.DeleteEvent += new DeleteEventHandler (WindowDelete);
			window.BorderWidth = 8;
		
			vbox = new VBox (false, 8);
			vbox.BorderWidth = 8;
			window.Add (vbox);
			
			Gtk.Label label = new Gtk.Label ("<u>Image loaded from a file</u>");
			label.UseMarkup = true;
			vbox.PackStart (label, false, false, 0);
			
			Gtk.Frame frame = new Gtk.Frame ();
			frame.ShadowType = ShadowType.In;

			// The alignment keeps the frame from growing when users resize
			// the window
			Alignment alignment = new Alignment (0.5f, 0.5f, 0f, 0f);
			alignment.Add (frame);
			vbox.PackStart (alignment, false, false, 0);
			
			Gtk.Image image = new Gtk.Image ("images/gtk-logo-rgb.gif");
			frame.Add (image);			

			// Animation
			label = new Gtk.Label ("<u>Animation loaded from a file</u>");
			label.UseMarkup = true;
			vbox.PackStart (label);

			frame = new Gtk.Frame ();
			frame.ShadowType = ShadowType.In;

			alignment = new Alignment (0.5f, 0.5f, 0f, 0f);
			alignment.Add (frame);
			vbox.PackStart (alignment, false, false, 0);

			image = new Gtk.Image ("images/floppybuddy.gif");
			frame.Add (image);

			// Progressive
			label = new Gtk.Label ("<u>Progressive image loading</u>");
			label.UseMarkup = true;
			vbox.PackStart (label);

			frame = new Gtk.Frame ();
			frame.ShadowType = ShadowType.In;

			alignment = new Alignment (0.5f, 0.5f, 0f, 0f);
			alignment.Add (frame);
			vbox.PackStart (alignment, false, false, 0);

			// Create an empty image for now; the progressive loader
			// will create the pixbuf and fill it in.
			//

			progressiveImage = new Gtk.Image ();
			frame.Add (progressiveImage);

			StartProgressiveLoading ();
			
			// Sensitivity control
			Gtk.ToggleButton button = new Gtk.ToggleButton ("_Insensitive");
			vbox.PackStart (button, false, false, 0);
			button.Toggled += new EventHandler (ToggleSensitivity);

			window.ShowAll ();
		}
		
  		private void WindowDelete (object o, DeleteEventArgs args)
		{
			window.Hide ();
			window.Destroy ();
		}

  		private void ToggleSensitivity (object o, EventArgs args)
		{
			GLib.List children = vbox.Children;
			foreach (Widget widget in children)
				/* don't disable our toggle */
				if (widget.GetType () !=  o.GetType () )
					widget.Sensitive =  !widget.Sensitive;
		}


		private uint timeout_id;
		private void StartProgressiveLoading ()
		{
                /* This is obviously totally contrived (we slow down loading
		 * on purpose to show how incremental loading works).
		 * The real purpose of incremental loading is the case where
		 * you are reading data from a slow source such as the network.
		 * The timeout simply simulates a slow data source by inserting
		 * pauses in the reading process.
		 */
			timeout_id = GLib.Timeout.Add(150, new GLib.TimeoutHandler(ProgressiveTimeout));
		}

		static Gdk.PixbufLoader pixbufLoader;

		// TODO: Finish this callback
		// Decide if we want to perform crazy error handling
		private  bool ProgressiveTimeout()
		{
			Gtk.Image imageStream = new Gtk.Image ("images/alphatest.png");
			pixbufLoader = new Gdk.PixbufLoader ();
			pixbufLoader.AreaPrepared += new EventHandler (ProgressivePreparedCallback);
			pixbufLoader.AreaUpdated += new AreaUpdatedHandler (ProgressiveUpdatedCallback);
			return true;
		}

	static void ProgressivePreparedCallback (object obj, EventArgs args) 
		{
			Gdk.Pixbuf pixbuf = pixbufLoader.Pixbuf;
			pixbuf.Fill (0xaaaaaaff);
			progressiveImage.FromPixbuf = pixbuf;
		}

	static void ProgressiveUpdatedCallback (object obj, AreaUpdatedArgs args) 
		{

		}

	}
}

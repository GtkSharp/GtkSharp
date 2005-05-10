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

using System;
using System.IO;
using System.Reflection;

using Gtk;
using Gdk;

namespace GtkDemo
{
	[Demo ("Images", "DemoImages.cs")]
	public class DemoImages : Gtk.Window
	{
		private Gtk.Image progressiveImage;
		private VBox vbox;
		BinaryReader imageStream;

		public DemoImages () : base ("Images")
		{
			BorderWidth = 8;

			vbox = new VBox (false, 8);
			vbox.BorderWidth = 8;
			Add (vbox);

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

			Gtk.Image image = Gtk.Image.LoadFromResource ("gtk-logo-rgb.gif");
			frame.Add (image);

			// Animation
			label = new Gtk.Label ("<u>Animation loaded from a file</u>");
			label.UseMarkup = true;
			vbox.PackStart (label, false, false, 0);

			frame = new Gtk.Frame ();
			frame.ShadowType = ShadowType.In;

			alignment = new Alignment (0.5f, 0.5f, 0f, 0f);
			alignment.Add (frame);
			vbox.PackStart (alignment, false, false, 0);

			image = Gtk.Image.LoadFromResource ("floppybuddy.gif");
			frame.Add (image);

			// Progressive
			label = new Gtk.Label ("<u>Progressive image loading</u>");
			label.UseMarkup = true;
			vbox.PackStart (label, false, false, 0);

			frame = new Gtk.Frame ();
			frame.ShadowType = ShadowType.In;

			alignment = new Alignment (0.5f, 0.5f, 0f, 0f);
			alignment.Add (frame);
			vbox.PackStart (alignment, false, false, 0);

			// Create an empty image for now; the progressive loader
			// will create the pixbuf and fill it in.

			progressiveImage = new Gtk.Image ();
			frame.Add (progressiveImage);

			StartProgressiveLoading ();

			// Sensitivity control
			Gtk.ToggleButton button = new Gtk.ToggleButton ("_Insensitive");
			vbox.PackStart (button, false, false, 0);
			button.Toggled += new EventHandler (ToggleSensitivity);

			ShowAll ();
		}

		protected override void OnDestroyed ()
		{
			if (timeout_id != 0) {
				GLib.Source.Remove (timeout_id);
				timeout_id = 0;
			}

			if (pixbufLoader != null) {
				pixbufLoader.Close ();
				pixbufLoader = null;
			}

			if (imageStream != null) {
				imageStream.Close ();
				imageStream = null;
			}
		}

  		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}

  		private void ToggleSensitivity (object o, EventArgs args)
		{
			ToggleButton toggle = o as ToggleButton;

			foreach (Widget widget in vbox) {
				// don't disable our toggle
				if (widget != toggle)
					widget.Sensitive = !toggle.Active;
			}
		}

		private uint timeout_id;
		private void StartProgressiveLoading ()
		{
			// This is obviously totally contrived (we slow down loading
			// on purpose to show how incremental loading works).
			// The real purpose of incremental loading is the case where
			// you are reading data from a slow source such as the network.
			// The timeout simply simulates a slow data source by inserting
			// pauses in the reading process.

			timeout_id = GLib.Timeout.Add (150, new GLib.TimeoutHandler (ProgressiveTimeout));
		}

		Gdk.PixbufLoader pixbufLoader;

		// TODO: Decide if we want to perform the same crazy error handling
		// gtk-demo does
		private bool ProgressiveTimeout ()
		{
			if (imageStream == null) {
				Stream stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("alphatest.png");
				imageStream = new BinaryReader (stream);
				pixbufLoader = new Gdk.PixbufLoader ();
				pixbufLoader.AreaPrepared += new EventHandler (ProgressivePreparedCallback);
				pixbufLoader.AreaUpdated += new AreaUpdatedHandler (ProgressiveUpdatedCallback);
			}

			if (imageStream.PeekChar () != -1) {
				byte[] bytes = imageStream.ReadBytes (256);
				pixbufLoader.Write (bytes);
				return true; // leave the timeout active
			} else {
				imageStream.Close ();
				return false; // removes the timeout
			}
		}

		void ProgressivePreparedCallback (object obj, EventArgs args)
		{
			Gdk.Pixbuf pixbuf = pixbufLoader.Pixbuf;
			pixbuf.Fill (0xaaaaaaff);
			progressiveImage.FromPixbuf = pixbuf;
		}

		void ProgressiveUpdatedCallback (object obj, AreaUpdatedArgs args)
		{
			progressiveImage.QueueDraw ();
		}
	}
}

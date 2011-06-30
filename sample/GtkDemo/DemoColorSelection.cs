/* Color Selector
 *
 * GtkColorSelection lets the user choose a color. GtkColorSelectionDialog is
 * a prebuilt dialog containing a GtkColorSelection.
 *
 */

using System;
using Gdk;
using Gtk;

namespace GtkDemo
{
	[Demo ("Color Selection", "DemoColorSelection.cs")]
	public class DemoColorSelection : Gtk.Window
	{
		private Gdk.RGBA color;
		private Gtk.DrawingArea drawingArea;

		public DemoColorSelection () : base ("Color Selection")
		{
			BorderWidth = 8;
			VBox vbox = new VBox (false,8);
			vbox.BorderWidth = 8;
			Add (vbox);

			// Create the color swatch area
			Frame frame = new Frame ();
			frame.ShadowType = ShadowType.In;
			vbox.PackStart (frame, true, true, 0);

			drawingArea = new DrawingArea ();
			drawingArea.Drawn += new DrawnHandler (DrawnCallback);
			// set a minimum size
			drawingArea.SetSizeRequest (200,200);
			// set the color
			color.Red = 0;
			color.Green = 0;
			color.Blue = 1;
			color.Alpha = 1;
			drawingArea.OverrideBackgroundColor (StateFlags.Normal, color);
			frame.Add (drawingArea);

			Alignment alignment = new Alignment (1.0f, 0.5f, 0.0f, 0.0f);
			Button button = new Button ("_Change the above color");
			button.Clicked += new EventHandler (ChangeColorCallback);
			alignment.Add (button);
			vbox.PackStart (alignment, false, false, 0);

			ShowAll ();
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}

		// Drawn callback for the drawing area
		private void DrawnCallback (object o, DrawnArgs args)
		{
			Cairo.Context cr = args.Cr;
			
			Gdk.RGBA rgba = StyleContext.GetBackgroundColor (StateFlags.Normal);
			cr.SetSourceRGBA (rgba.Red, rgba.Green, rgba.Blue, rgba.Alpha);
			cr.Paint ();

			args.RetVal = true;
		}

		private void ChangeColorCallback (object o, EventArgs args)
		{
			using (ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog ("Changing color")) {
				colorSelectionDialog.TransientFor = this;
				colorSelectionDialog.ColorSelection.SetPreviousRgba (color);
				colorSelectionDialog.ColorSelection.CurrentRgba = color;
				colorSelectionDialog.ColorSelection.HasPalette = true;

				if (colorSelectionDialog.Run () == (int) ResponseType.Ok) {
					Gdk.RGBA selected = colorSelectionDialog.ColorSelection.CurrentRgba;
					drawingArea.OverrideBackgroundColor (StateFlags.Normal, selected);
				}

				colorSelectionDialog.Hide ();
			}
		}
	}
}


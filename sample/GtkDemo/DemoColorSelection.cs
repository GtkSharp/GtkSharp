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
		private Gdk.Color color;
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
			drawingArea.ExposeEvent += new ExposeEventHandler (ExposeEventCallback);
			// set a minimum size
			drawingArea.SetSizeRequest (200,200);
			// set the color
			color = new Gdk.Color (0, 0, 0xff);
			drawingArea.ModifyBg (StateType.Normal, color);
			frame.Add (drawingArea);

			Alignment alignment = new Alignment (1.0f, 0.5f, 0.0f, 0.0f);
			Button button = new Button ("_Change the above color");
			button.Clicked += new EventHandler (ChangeColorCallback);
			alignment.Add (button);
			vbox.PackStart (alignment);

			ShowAll ();
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}

		// Expose callback for the drawing area
		private void ExposeEventCallback (object o, ExposeEventArgs args)
		{
			EventExpose eventExpose = args.Event;
			Gdk.Window window = eventExpose.Window;
 			Rectangle area = eventExpose.Area;

			window.DrawRectangle (drawingArea.Style.BackgroundGC (StateType.Normal),
					      true,
					      area.X, area.Y,
					      area.Width, area.Height);
			args.RetVal = true;
		}

		private void ChangeColorCallback (object o, EventArgs args)
		{
			using (ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog ("Changing color")) {
				colorSelectionDialog.TransientFor = this;
				colorSelectionDialog.ColorSelection.PreviousColor = color;
				colorSelectionDialog.ColorSelection.CurrentColor = color;
				colorSelectionDialog.ColorSelection.HasPalette = true;

				if (colorSelectionDialog.Run () == (int) ResponseType.Ok) {
					Gdk.Color selected = colorSelectionDialog.ColorSelection.CurrentColor;
					drawingArea.ModifyBg (StateType.Normal, selected);
				}

				colorSelectionDialog.Hide ();
			}
		}
	}
}


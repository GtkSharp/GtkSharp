/* Button Boxes
 *
 * The Button Box widgets are used to arrange buttons with padding.
 */

using System;
using Gtk;

namespace GtkDemo
{
	[Demo ("Button Boxes", "DemoButtonBox.cs")]
	public class DemoButtonBox : Gtk.Window
	{
		public DemoButtonBox () : base ("Button Boxes")
		{
			BorderWidth = 10;

			// Add Vertical Box
			VBox mainVbox = new VBox (false,0);
			Add (mainVbox);

			// Add Horizontal Frame
			Frame horizontalFrame =  new Frame ("Horizontal Button Boxes");
			mainVbox.PackStart (horizontalFrame, true, true, 10);
			VBox vbox = new VBox (false, 0);
			vbox.BorderWidth = 10;
			horizontalFrame.Add (vbox);

                        // Pack Buttons
			vbox.PackStart (CreateButtonBox (true, "Spread", 40, ButtonBoxStyle.Spread), true, true, 0);
			vbox.PackStart (CreateButtonBox (true, "Edge", 40, ButtonBoxStyle.Edge), true, true, 5);
			vbox.PackStart (CreateButtonBox (true, "Start", 40, ButtonBoxStyle.Start), true, true, 5);
			vbox.PackStart (CreateButtonBox (true, "End", 40, ButtonBoxStyle.End), true, true, 5);

			//  Add Vertical Frame
			Frame verticalFrame = new Frame ("Vertical Button Boxes");
			mainVbox.PackStart (verticalFrame, true, true, 10);
			HBox hbox = new HBox (false, 0);
			hbox.BorderWidth = 10;
			verticalFrame.Add (hbox);

                        // Pack Buttons
			hbox.PackStart(CreateButtonBox (false, "Spread", 30, ButtonBoxStyle.Spread), true, true, 0);
			hbox.PackStart(CreateButtonBox (false, "Edge", 30, ButtonBoxStyle.Edge), true, true, 5);
			hbox.PackStart(CreateButtonBox (false, "Start", 30, ButtonBoxStyle.Start), true, true, 5);
			hbox.PackStart(CreateButtonBox (false, "End", 30, ButtonBoxStyle.End), true, true, 5);

			ShowAll ();
		}

		// Create a Button Box with the specified parameters
		private Frame CreateButtonBox (bool horizontal, string title, int spacing, ButtonBoxStyle layout)
		{
			Frame frame = new Frame (title);
			Gtk.ButtonBox bbox ;

			if (horizontal)
				bbox =  new Gtk.HButtonBox ();
			else
				bbox =  new Gtk.VButtonBox ();

			bbox.BorderWidth = 5;
			frame.Add (bbox);

			// Set the appearance of the Button Box
			bbox.Layout = layout;
			bbox.Spacing = spacing;

			bbox.Add (new Button (Stock.Ok));
			bbox.Add (new Button (Stock.Cancel));
			bbox.Add (new Button (Stock.Help));

			return frame;
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}
	}
}

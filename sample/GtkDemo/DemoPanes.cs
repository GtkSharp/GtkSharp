/* Paned Widgets
 *
 * The HPaned and VPaned Widgets divide their content
 * area into two panes with a divider in between that the
 * user can adjust. A separate child is placed into each
 * pane.
 *
 * There are a number of options that can be set for each pane.
 * This test contains both a horizontal (HPaned) and a vertical
 * (VPaned) widget, and allows you to adjust the options for
 * each side of each widget.
 */


using System;
using System.Collections;
using Gtk;

namespace GtkDemo
{
	[Demo ("Paned Widget", "DemoPanes.cs")]
	public class DemoPanes : Gtk.Window
	{
		Hashtable children = new Hashtable ();

		public DemoPanes () : base ("Panes")
		{
			VBox vbox = new VBox (false, 0);
			Add (vbox);

			VPaned vpaned = new VPaned ();
			vbox.PackStart (vpaned, true, true, 0);
			vpaned.BorderWidth = 5;

			HPaned hpaned = new HPaned ();
			vpaned.Add1 (hpaned);

		        Frame frame = new Frame ();
		        frame.ShadowType = ShadowType.In;
			frame.SetSizeRequest (60, 60);
			hpaned.Add1 (frame);

			Gtk.Button button = new Button ("_Hi there");
			frame.Add (button);

			frame = new Frame ();
			frame.ShadowType = ShadowType.In;
			frame.SetSizeRequest (80, 60);
			hpaned.Add2 (frame);

			frame = new Frame ();
			frame.ShadowType = ShadowType.In;
			frame.SetSizeRequest (60, 80);
			vpaned.Add2 (frame);

			// Now create toggle buttons to control sizing
			vbox.PackStart (CreatePaneOptions (hpaned,
							   "Horizontal",
							   "Left",
							   "Right"),
					false, false, 0);

			vbox.PackStart (CreatePaneOptions (vpaned,
							   "Vertical",
							   "Top",
							   "Bottom"),
					false, false, 0);

			ShowAll ();
		}

		Frame CreatePaneOptions (Paned paned, string frameLabel,
					 string label1, string label2)
		{
			Frame frame = new Frame (frameLabel);
			frame.BorderWidth = 4;

			Table table = new Table (3, 2, true);
			frame.Add (table);

			Label label = new Label (label1);
			table.Attach (label, 0, 1, 0, 1);

			CheckButton check = new CheckButton ("_Resize");
			table.Attach (check, 0, 1, 1, 2);
			check.Toggled += new EventHandler (ToggleResize);
			children[check] = paned.Child1;

			check = new CheckButton ("_Shrink");
			table.Attach (check, 0, 1, 2, 3);
			check.Active = true;
			check.Toggled += new EventHandler (ToggleShrink);
			children[check] = paned.Child1;

			label = new Label (label2);
			table.Attach (label, 1, 2, 0, 1);

			check = new CheckButton ("_Resize");
			table.Attach (check, 1, 2, 1, 2);
			check.Active = true;
			check.Toggled += new EventHandler (ToggleResize);
			children[check] = paned.Child2;

			check = new CheckButton ("_Shrink");
			table.Attach (check, 1, 2, 2, 3);
			check.Active = true;
			check.Toggled += new EventHandler (ToggleShrink);
			children[check] = paned.Child2;

			return frame;
		}

		private void ToggleResize (object obj, EventArgs args)
		{
			ToggleButton toggle = obj as ToggleButton;
			Widget child = children[obj] as Widget;
			Paned paned = child.Parent as Paned;

			Paned.PanedChild pc = paned[child] as Paned.PanedChild;
			pc.Resize = toggle.Active;
		}

		private void ToggleShrink (object obj, EventArgs args)
		{
			ToggleButton toggle = obj as ToggleButton;
			Widget child = children[obj] as Widget;
			Paned paned = child.Parent as Paned;

			Paned.PanedChild pc = paned[child] as Paned.PanedChild;
			pc.Shrink = toggle.Active;
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}
	}
}

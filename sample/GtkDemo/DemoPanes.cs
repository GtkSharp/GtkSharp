//
// DemoPanes.cs
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2002, Daniel Kornhauser, Ximian Inc.
//


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
using Gtk;

namespace GtkDemo
{
	[Demo ("Paned Widget", "DemoPanes.cs")]
	public class DemoPanes : Gtk.Window
	{
		private VPaned vpaned;
		private HPaned top;
		private Frame left;
		private Frame right;
		private Frame bottom;

		private CheckButton resizeLeft;
		private CheckButton shrinkLeft;
		private CheckButton resizeRight;
		private	CheckButton shrinkRight;	
		private CheckButton resizeTop;
		private CheckButton shrinkTop;
		private CheckButton resizeBottom;
		private CheckButton shrinkBottom;
		private Button button;

		public DemoPanes () : base ("Panes")
		{	
			this.DeleteEvent += new DeleteEventHandler (WindowDelete);
			this.BorderWidth = 0;

			VBox vbox = new VBox (false, 0);
			this.Add (vbox);

			vpaned = new VPaned ();
			vbox.PackStart (vpaned, true, true, 0);
			vpaned.BorderWidth = 5;

			top = new HPaned ();
			vpaned.Add1 (top);

		        left = new Frame ();
		        left.ShadowType = ShadowType.In;
			left.SetSizeRequest (60, 60);
			top.Add1 (left);
			button = new Button ("_Hi there");
			left.Add (button);

			right = new Frame ();
			right.ShadowType = ShadowType.In;
			right.SetSizeRequest (80, 60);
			top.Add2 (right);

			bottom = new Frame ();
			bottom.ShadowType = ShadowType.In;
			bottom.SetSizeRequest (80, 60);
			vpaned.Add2 (bottom);

			// Now create toggle buttons to control sizing 

			Frame frame = new Frame ("Horizonal");
			frame.BorderWidth = 4;
			vbox.PackStart (frame);

			Table table = new Table (3, 2, true);
			frame.Add (table);

			Label label = new Label ("Left");
			table.Attach (label, 0, 1, 0, 1);

			resizeLeft = new CheckButton ("_Resize");			
			table.Attach (resizeLeft, 0, 1, 1, 2);
			resizeLeft.Toggled += new EventHandler (LeftCB);

			shrinkLeft  = new CheckButton ("_Shrink");
			table.Attach (shrinkLeft, 0, 1, 2, 3);
			shrinkLeft.Active = true;
			shrinkLeft.Toggled += new EventHandler (LeftCB);

			label = new Label ("Right");
			table.Attach (label, 1, 2, 0, 1);
			
			resizeRight = new CheckButton ("_Resize");
			table.Attach (resizeRight, 1, 2, 1, 2);
			resizeRight.Active = true;
			resizeRight.Toggled += new EventHandler (RightCB);

			shrinkRight = new CheckButton ("_Shrink"); 
			table.Attach (shrinkRight, 1, 2, 2, 3);
			shrinkRight.Active = true;
			shrinkRight.Toggled += new EventHandler (RightCB);

			frame = new Frame ("Vertical");
			frame.BorderWidth = 4;
			vbox.PackStart (frame);
			
			table = new Table (3, 2, true);
			frame.Add (table);
			
			label = new Label ("Top");
			table.Attach (label, 0, 1, 0, 1);
			
			resizeTop = new CheckButton ("_Resize");			
			table.Attach (resizeTop, 0, 1, 1, 2);
			resizeTop.Toggled += new EventHandler (TopCB);
			
			shrinkTop = new CheckButton ("_Shrink");   
			table.Attach (shrinkTop, 0, 1, 2, 3);
			shrinkTop.Active = true;
			shrinkTop.Toggled += new EventHandler (TopCB);

			label = new Label ("Bottom");
			table.Attach (label, 1, 2, 0, 1);
			
			resizeBottom = new CheckButton ("_Resize"); 
			table.Attach (resizeBottom, 1, 2, 1, 2);
			resizeBottom.Active = true;
			resizeBottom.Toggled += new EventHandler (BottomCB);

			shrinkBottom = new CheckButton ("_Shrink"); 
			table.Attach (shrinkBottom, 1, 2, 2, 3);
			shrinkBottom.Active = true;
			shrinkBottom.Toggled += new EventHandler (BottomCB);

			this.ShowAll ();
		}			

		private void LeftCB (object o, EventArgs args)
                {
			top.Remove (left);
			top.Pack1 (left, resizeLeft.Active, shrinkLeft.Active);
		}
		
		private void RightCB (object o, EventArgs args)
                {
			top.Remove (right);
			top.Pack2 (right, resizeRight.Active, shrinkRight.Active);
		}

		private void TopCB (object o, EventArgs args)
                {
			vpaned.Remove (top);
			vpaned.Pack1 (top, resizeTop.Active, shrinkTop.Active);
		}

		private void BottomCB (object o, EventArgs args)
                {
			vpaned.Remove (bottom);
			vpaned.Pack2 (bottom, resizeBottom.Active, shrinkBottom.Active);
		}

 	    	private void WindowDelete (object o, DeleteEventArgs args)
		{
			this.Hide ();
			this.Destroy ();
		}
	}
}

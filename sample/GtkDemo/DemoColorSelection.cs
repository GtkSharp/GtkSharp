//
// DemoColorSelection.cs, port of colorsel.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2003, Ximian Inc.

/* 
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
	public class DemoColorSelection
	{
		private Gtk.Window window ;
		private Gdk.Color color;
		private ColorSelectionDialog colorSelectionDialog;
		private Gtk.DrawingArea drawingArea;

		public DemoColorSelection ()
		{
			window = new Gtk.Window ("Color Selection");
			window.DeleteEvent += new DeleteEventHandler (WindowDelete);
			window.BorderWidth = 8;
			VBox vbox = new VBox (false,8);
			vbox.BorderWidth = 8;
			window.Add (vbox);

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

			window.ShowAll ();
		}			
		
		private void WindowDelete (object o, DeleteEventArgs args)
		{
			window.Hide ();
			window.Destroy ();
		}
		
		// Expose callback for the drawing area		
		private void ExposeEventCallback (object o, ExposeEventArgs args)
		{
			EventExpose eventExpose = args.Event;
			Gdk.Window window = eventExpose.window;
 			Rectangle area = eventExpose.Area;

			window.DrawRectangle (drawingArea.Style.BackgroundGC(StateType.Normal),
					true,
					area.X, area.Y,
					area.Width, area.Height);
			args.RetVal = true;
		}
		
		private void ChangeColorCallback (object o, EventArgs args)
		{
			colorSelectionDialog = new ColorSelectionDialog ("Changing color");
			colorSelectionDialog.TransientFor = window;
			colorSelectionDialog.ColorSelection.PreviousColor = color;
			colorSelectionDialog.ColorSelection.CurrentColor = color;
			colorSelectionDialog.ColorSelection.HasPalette = true;			

			colorSelectionDialog.CancelButton.Clicked += new EventHandler (Color_Selection_Cancel);
			colorSelectionDialog.OkButton.Clicked += new EventHandler (Color_Selection_OK);

			colorSelectionDialog.ShowAll();
		}

		private void Color_Selection_OK (object o, EventArgs args)
		{
			Gdk.Color selected = colorSelectionDialog.ColorSelection.CurrentColor;
			drawingArea.ModifyBg (StateType.Normal, selected);
			colorSelectionDialog.Destroy ();
		}

		private void Color_Selection_Cancel (object o, EventArgs args)
		{
			colorSelectionDialog.Destroy ();
		}
	}
}

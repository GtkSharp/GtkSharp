// CustomCellRenderer.cs : C# implementation of an example custom cellrenderer
// from http://scentric.net/tutorial/sec-custom-cell-renderers.html
//
// Author: Todd Berman <tberman@sevenl.net>
//
// (c) 2004 Todd Berman

using System;

using Gtk;
using Gdk;
using GLib;

public class CustomCellRenderer : CellRenderer
{

	private float percent;
	
	public float Percentage
	{
		get {
			return percent;
		}
		set {
			percent = value;
		}
	}

	static CustomCellRenderer ()
	{
		OverrideGetSize (GLib.Object.LookupGType (typeof (CustomCellRenderer)));
		OverrideRender (GLib.Object.LookupGType (typeof (CustomCellRenderer)));
	}

	public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
	{
		int calc_width = (int) this.Xpad * 2 + 100;
		int calc_height = (int) this.Ypad * 2 + 10;

		width = calc_width;
		height = calc_height;

		x_offset = 0;
		y_offset = 0;
		if (!cell_area.Equals (Rectangle.Zero)) {
			x_offset = (int) (this.Xalign * (cell_area.Width - calc_width));
			x_offset = Math.Max (x_offset, 0);
			
			y_offset = (int) (this.Yalign * (cell_area.Height - calc_height));
			y_offset = Math.Max (y_offset, 0);
		}
	}

	protected override void Render (Drawable window, Widget widget, Rectangle background_area, Rectangle cell_area, Rectangle expose_area, CellRendererState flags)
	{
		int width = 0, height = 0, x_offset = 0, y_offset = 0;
		StateType state;
		GetSize (widget, ref cell_area, out x_offset, out y_offset, out width, out height);

		if (widget.HasFocus)
			state = StateType.Active;
		else
			state = StateType.Normal;

		width -= (int) this.Xpad * 2;
		height -= (int) this.Ypad * 2;


		//FIXME: Style.PaintBox needs some customization so that if you pass it
		//a Gdk.Rectangle.Zero it gives a clipping area big enough to draw
		//everything
		Gdk.Rectangle clipping_area = new Gdk.Rectangle ((int) (cell_area.X + x_offset + this.Xpad), (int) (cell_area.Y + y_offset + this.Ypad), width - 1, height - 1);
		
		Style.PaintBox (widget.Style, (Gdk.Window) window, StateType.Normal, ShadowType.In, clipping_area, widget, "trough", (int) (cell_area.X + x_offset + this.Xpad), (int) (cell_area.Y + y_offset + this.Ypad), width - 1, height - 1);

		Gdk.Rectangle clipping_area2 = new Gdk.Rectangle ((int) (cell_area.X + x_offset + this.Xpad), (int) (cell_area.Y + y_offset + this.Ypad), (int) (width * Percentage), height - 1);
		
		Style.PaintBox (widget.Style, (Gdk.Window) window, state, ShadowType.Out, clipping_area2, widget, "bar", (int) (cell_area.X + x_offset + this.Xpad), (int) (cell_area.Y + y_offset + this.Ypad), (int) (width * Percentage), height - 1);
	}
}

public class Driver : Gtk.Window
{
	public static void Main ()
	{
		Application.Init ();
		new Driver ();
		Application.Run ();
	}

	ListStore liststore;

	void ProgressData (Gtk.TreeViewColumn tree_column, Gtk.CellRenderer cell, Gtk.TreeModel tree_model, Gtk.TreeIter iter)
	{
		float perc = (float) liststore.GetValue (iter, 0);
		((CustomCellRenderer)cell).Percentage = perc;
	}
	
	public Driver () : base ("CustomCellRenderer")
	{
		DefaultSize = new Size (150, 100);
		this.DeleteEvent += new DeleteEventHandler (window_delete);

		liststore = new ListStore (typeof (float), typeof (string));
		liststore.AppendValues (0.5f, "50%");

		TreeView view = new TreeView (liststore);

		view.AppendColumn ("Progress", new CellRendererText (), "text", 1);
		
		//Note: This *MUST* be done here, as its the only place Progress is
		//accessible. Maybe there should be an attribute that will do some
		//magic for you and register a property with the gobject system.
		view.AppendColumn ("Progress", new CustomCellRenderer (), new TreeCellDataFunc (ProgressData));
		
		this.Add (view);
		this.ShowAll ();

		Gtk.Timeout.Add (50, new Gtk.Function (update_percent));
	}

	bool increasing = true;
	bool update_percent ()
	{
		TreeIter iter;
		liststore.GetIterFirst (out iter);
		float perc = (float) liststore.GetValue (iter, 0);

		if ((perc > 0.99) || (perc < 0.01 && perc > 0)) {
			increasing = !increasing;
		}

		if (increasing)
			perc += 0.01f;
		else
			perc -= 0.01f;

		liststore.SetValue (iter, 0, perc);
		liststore.SetValue (iter, 1, Convert.ToInt32 (perc * 100) + "%");

		return true;
	}

	void window_delete (object obj, DeleteEventArgs args)
	{
		Application.Quit ();
		args.RetVal = true;
	}
}


using GLib;
using Gtk;
using System;

class CustomScrollableWidgetTest {
	public static int Main (string[] args)
	{
		Gtk.Application.Init ();
		Window win = new Window ("Custom Scrollable Widget Test");
		win.DeleteEvent += new DeleteEventHandler (OnQuit);
		
		ScrolledWindow scroll = new ScrolledWindow ();
		scroll.HscrollbarPolicy = PolicyType.Automatic;
		scroll.VscrollbarPolicy = PolicyType.Automatic;
		
		CustomScrollableWidget cw = new CustomScrollableWidget ("This one label that is repeated");
		scroll.Add (cw);
		
		win.Add (scroll);
		win.DefaultWidth = 200;
		win.DefaultHeight = 200;
		win.ShowAll ();
		Gtk.Application.Run ();
		return 0;
	}

	static void OnQuit (object sender, DeleteEventArgs args)
	{
		Gtk.Application.Quit ();
	}
}

abstract class CustomBase : Widget
{
	public CustomBase () : base ()
	{ }
}

class CustomScrollableWidget : CustomBase, ScrollableImplementor {
	private int num_rows = 20;
	private string label;
	private Pango.Layout layout;

	public CustomScrollableWidget (string custom_label) : base ()
	{
		label = custom_label;
		layout = null;
		
		HasWindow = false;
	}

	private Pango.Layout Layout {
		get {
			if (layout == null)
				layout = CreatePangoLayout (label);
			return layout;
		}
	}

	private Gdk.Rectangle ContentArea {
		get {
			Gdk.Rectangle area;
			area.X = Allocation.X;
			area.Y = Allocation.Y;
			
			int layoutWidth, layoutHeight;
			Layout.GetPixelSize (out layoutWidth, out layoutHeight);
			area.Width = layoutWidth;
			area.Height = layoutHeight * num_rows;
			
			return area;
		}
	}

	protected override bool OnDrawn (Cairo.Context cr)
	{
		int layout_x = - HadjustmentValue;
		int layout_y = - VadjustmentValue;
		
		int layoutWidth, layoutHeight;
		Layout.GetPixelSize (out layoutWidth, out layoutHeight);
		
		for (int i = 0; i < num_rows; i++) {
			Layout.SetText (String.Format ("{0} {1}", label, i));
			StyleContext.RenderLayout (cr, layout_x, layout_y, Layout);
			layout_y += layoutHeight;
		}
		
		return base.OnDrawn (cr);
	}

	protected override void OnSizeAllocated (Gdk.Rectangle allocation)
	{
		base.OnSizeAllocated (allocation);

		if (hadjustment != null) {
			hadjustment.PageSize = allocation.Width;
			hadjustment.PageIncrement = allocation.Width;
			UpdateAdjustments ();
		}
		
		if (vadjustment != null) {
			vadjustment.PageSize = allocation.Height;
			vadjustment.PageIncrement = allocation.Height;
			UpdateAdjustments ();
		}
	}

	
	private Adjustment hadjustment;
	public Adjustment Hadjustment {
		get { return hadjustment; }
		set {
			if (value == hadjustment) {
				return;
			}
			hadjustment = value;
			if (hadjustment == null) {
				return;
			}
			hadjustment.ValueChanged += OnHadjustmentChanged;
			UpdateAdjustments ();
		}
	}
	
	private Adjustment vadjustment;
	public Adjustment Vadjustment {
		get { return vadjustment; }
		set {
			if (value == vadjustment) {
				return;
			}
			vadjustment = value;
			if (vadjustment == null) {
				return;
			}
			vadjustment.ValueChanged += OnVadjustmentChanged;
			UpdateAdjustments ();
		}
	}
	
	private int HadjustmentValue {
		get { return hadjustment == null ? 0 : (int)hadjustment.Value; }
	}

	private int VadjustmentValue {
		get { return vadjustment == null ? 0 : (int)vadjustment.Value; }
	}

	public Gtk.ScrollablePolicy HscrollPolicy {
		get; set;
	}
	
	public Gtk.ScrollablePolicy VscrollPolicy {
		get; set;
	}
	
	private void UpdateAdjustments ()
	{
		int layoutWidth, layoutHeight;
		Layout.GetPixelSize (out layoutWidth, out layoutHeight);
		
		if (hadjustment != null) {
			hadjustment.Upper = ContentArea.Width;
			hadjustment.StepIncrement = 10.0;
			if (hadjustment.Value + hadjustment.PageSize > hadjustment.Upper) {
				hadjustment.Value = hadjustment.Upper - hadjustment.PageSize;
			}
			Console.WriteLine (String.Format ("H adj={0} upper={1} PageSize={2} PageInc={3}",
				HadjustmentValue, hadjustment.Upper, hadjustment.PageSize, hadjustment.PageIncrement));
			hadjustment.Change ();
		}
		
		if (vadjustment != null) {
			vadjustment.Upper = ContentArea.Height;
			vadjustment.StepIncrement = layoutHeight;
			if (vadjustment.Value + vadjustment.PageSize > vadjustment.Upper) {
				vadjustment.Value = vadjustment.Upper - vadjustment.PageSize;
			}
			Console.WriteLine (String.Format ("V adj={0} upper={1} PageSize={2} PageInc={3}",
				VadjustmentValue, vadjustment.Upper, vadjustment.PageSize, vadjustment.PageIncrement));
			vadjustment.Change ();
		}
	}
	
	private void OnHadjustmentChanged (object o, EventArgs args)
	{
		UpdateAdjustments ();
		QueueDraw ();
	}
	
	private void OnVadjustmentChanged (object o, EventArgs args)
	{
		UpdateAdjustments ();
		QueueDraw ();
	}
}

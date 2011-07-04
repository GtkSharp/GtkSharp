using GLib;
using Gtk;
using System;

class CustomWidgetTest {
	public static int Main (string[] args)
	{
		Gtk.Application.Init ();
		Window win = new Window ("Custom Widget Test");
		win.DeleteEvent += new DeleteEventHandler (OnQuit);
		
		VPaned paned = new VPaned ();
		CustomWidget cw = new CustomWidget ();
		cw.Label = "This one contains a button";
		Button button = new Button ("Ordinary button");
		cw.Add (button);
		paned.Pack1 (cw, true, false);

		cw = new CustomWidget ();
		cw.Label = "And this one a TextView";
		cw.StockId = Stock.JustifyLeft;
		ScrolledWindow sw = new ScrolledWindow (null, null);
		sw.ShadowType = ShadowType.In;
		sw.HscrollbarPolicy = PolicyType.Automatic;
		sw.VscrollbarPolicy = PolicyType.Automatic;
		TextView textView = new TextView ();
		sw.Add (textView);
		cw.Add (sw);
		paned.Pack2 (cw, true, false);
		
		win.Add (paned);
		win.ShowAll ();
		Gtk.Application.Run ();
		return 0;
	}

	static void OnQuit (object sender, DeleteEventArgs args)
	{
		Gtk.Application.Quit ();
	}
}

class CustomWidget : Bin {
	private Gdk.Pixbuf icon;
	private string label;
	private Pango.Layout layout;
	private string stockid;

	public CustomWidget () : base ()
	{
		icon = null;
		label = "CustomWidget";
		layout = null;
		stockid = Stock.Execute;
		
		HasWindow = false;
	}

	private Gdk.Pixbuf Icon {
		get {
			if (icon == null)
				icon = RenderIconPixbuf (stockid, IconSize.Menu);
			return icon;
		}
	}

	public string Label {
		get {
			return label;
		}
		set {
			label = value;
			Layout.SetText (label);
		}
	}

	private Pango.Layout Layout {
		get {
			if (layout == null)
				layout = CreatePangoLayout (label);
			return layout;
		}
	}

	public string StockId {
		get {
			return stockid;
		}
		set {
			stockid = value;
			icon = RenderIconPixbuf (stockid, IconSize.Menu);
		}
	}

	private Gdk.Rectangle TitleArea {
		get {
			Gdk.Rectangle area;
			area.X = Allocation.X + (int)BorderWidth;
			area.Y = Allocation.Y + (int)BorderWidth;
			area.Width = (Allocation.Width - 2 * (int)BorderWidth);
			
			int layoutWidth, layoutHeight;
			Layout.GetPixelSize (out layoutWidth, out layoutHeight);
			area.Height = Math.Max (layoutHeight, icon.Height);
			
			return area;
		}
	}

	protected override bool OnDrawn (Cairo.Context cr)
	{
		Gdk.Rectangle titleArea = TitleArea;

		Gdk.CairoHelper.SetSourcePixbuf (cr, Icon, 0, 0);
		cr.Paint ();
		
		int layout_x = icon.Width + 1;
		titleArea.Width -= icon.Width - 1;
		
		int layoutWidth, layoutHeight;
		Layout.GetPixelSize (out layoutWidth, out layoutHeight);
		
		int layout_y = (titleArea.Height - layoutHeight) / 2;
		
		StyleContext.RenderLayout (cr, layout_x, layout_y, Layout);
	
		return base.OnDrawn (cr);
	}

	protected override void OnSizeAllocated (Gdk.Rectangle allocation)
	{
		base.OnSizeAllocated (allocation);
	
		int bw = (int)BorderWidth;

		Gdk.Rectangle titleArea = TitleArea;

		if (Child != null) {
			Gdk.Rectangle childAllocation;
			childAllocation.X = allocation.X + bw;
			childAllocation.Y = allocation.Y + bw + titleArea.Height;
			childAllocation.Width = allocation.Width - 2 * bw;
			childAllocation.Height = allocation.Height - 2 * bw - titleArea.Height;
			Child.SizeAllocate (childAllocation);
		}
	}

	protected override void OnGetPreferredWidth (out int minimum_width, out int natural_width)
	{
		minimum_width = natural_width = (int)BorderWidth * 2 + Icon.Width + 1;
		int layoutWidth, layoutHeight;
		Layout.GetPixelSize (out layoutWidth, out layoutHeight);
		
		if (Child != null && Child.Visible) {
			int child_min_width, child_nat_width;
			Child.GetPreferredWidth (out child_min_width, out child_nat_width);
			
			minimum_width += Math.Max (layoutWidth, child_min_width);
			natural_width += Math.Max (layoutWidth, child_nat_width);
		} else {
			minimum_width += layoutWidth;
			natural_width += layoutWidth;
		}
	}

	protected override void OnGetPreferredHeight (out int minimum_height, out int natural_height)
	{
		minimum_height = natural_height = (int)BorderWidth * 2;
		
		int layoutWidth, layoutHeight;
		Layout.GetPixelSize (out layoutWidth, out layoutHeight);
		minimum_height += layoutHeight;
		natural_height += layoutHeight;
		
		if (Child != null && Child.Visible) {
			int child_min_height, child_nat_height;
			Child.GetPreferredHeight (out child_min_height, out child_nat_height);
			
			minimum_height += Math.Max (layoutHeight, child_min_height);
			natural_height += Math.Max (layoutHeight, child_nat_height);
		}
	}
}

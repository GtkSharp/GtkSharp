using GLib;
using Gtk;
using System;

class CustomWidgetTest {
	public static int Main (string[] args)
	{
		Application.Init ();
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
		Application.Run ();
		return 0;
	}

	static void OnQuit (object sender, DeleteEventArgs args)
	{
		Application.Quit ();
	}
}

class CustomWidget : Bin {
	internal static GType customWidgetGType;
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
		
		WidgetFlags |= WidgetFlags.NoWindow;
	}

	private Gdk.Pixbuf Icon {
		get {
			if (icon == null)
				icon = RenderIcon (stockid, IconSize.Menu, "");
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
			icon = RenderIcon (stockid, IconSize.Menu, "");
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

	protected override bool OnExposeEvent (Gdk.EventExpose args)
	{
		Gdk.Rectangle exposeArea;
		Gdk.Rectangle titleArea = TitleArea;

		if (args.Area.Intersect (titleArea, out exposeArea))
			GdkWindow.DrawPixbuf (Style.BackgroundGC (State), Icon, 0, 0,
					      titleArea.X, titleArea.Y, Icon.Width,
					      Icon.Height, Gdk.RgbDither.None, 0, 0);
		
		titleArea.X += icon.Width + 1;
		titleArea.Width -= icon.Width - 1;
		
		if (args.Area.Intersect (titleArea, out exposeArea)) {
			int layoutWidth, layoutHeight;
			Layout.GetPixelSize (out layoutWidth, out layoutHeight);
		
			titleArea.Y += (titleArea.Height - layoutHeight) / 2;

			Style.PaintLayout (Style, GdkWindow, State,
					   true, exposeArea, this, null,
					   titleArea.X, titleArea.Y, Layout);
		}
	
		return base.OnExposeEvent (args);
	}

	protected override void OnRealized ()
	{
		WidgetFlags |= WidgetFlags.Realized;
		
		GdkWindow = ParentWindow;
		Style = Style.Attach (GdkWindow);
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

	protected override void OnSizeRequested (ref Requisition requisition)
	{
		requisition.Width = requisition.Height = (int)BorderWidth * 2;
		requisition.Width += Icon.Width + 1;
	
		int layoutWidth, layoutHeight;
		Layout.GetPixelSize (out layoutWidth, out layoutHeight);
		requisition.Height += layoutHeight;
		
		if (Child != null && Child.Visible) {
			Requisition childReq = Child.SizeRequest ();
			requisition.Height += childReq.Height;

			requisition.Width += Math.Max (layoutWidth, childReq.Width);
		} else {
			requisition.Width += layoutWidth;
		}
	}
}

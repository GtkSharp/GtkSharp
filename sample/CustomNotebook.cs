using GLib;
using Gtk;
using System;
using System.Collections;

class CustomNotebookTest {
	static CustomNotebook cn;
	static Notebook nb;
	static int tabCount;

	public static int Main (string[] args)
	{
		Application.Init ();
		Window win = new Window ("Custom Notebook Test");
		win.DeleteEvent += new DeleteEventHandler (OnQuit);
		
		VBox box = new VBox (false, 5);
		Button addButton = new Button ("Add Tab");
		//addButton.Clicked += OnAddTab;
		Button rmButton = new Button ("Remove Tab");
		rmButton.Clicked += OnRemoveTab;
		box.PackStart (addButton, false, false, 0);
		box.PackStart (rmButton, false, false, 0);
		HPaned paned = new HPaned ();
		
		cn = new CustomNotebook ();
		cn.BorderWidth = 5;
		cn.Scrollable = false;
		Gdk.Pixbuf icon = cn.RenderIcon (Stock.Execute, IconSize.Menu, "");
		//for (tabCount = 0; tabCount < 3; tabCount++)
			cn.AppendPage (new Label ("Custom Notebook"), Stock.Execute, "extended tab" + tabCount);
			cn.AppendPage (new Label ("Custom Notebook"), Stock.Execute, "tab" + (tabCount + 1));
			cn.AppendPage (new Label ("Custom Notebook"), Stock.Execute, "extended tab" + (tabCount + 2));
		paned.Pack1 (cn, true, false);

		nb = new Notebook ();
		nb.BorderWidth = 5;
		nb.Scrollable = false;
		for (tabCount = 0; tabCount < 3; tabCount++)
			nb.AppendPage (new Label ("Regular Notebook"), new Label ("tab" + tabCount));
		paned.Pack2 (nb, true, false);
		box.PackEnd (paned, true, true, 0);
		
		win.Add (box);
		win.ShowAll ();
		Application.Run ();
		return 0;
	}

	/*static void OnAddTab (object sender, EventArgs args)
	{
		cn.AppendPage (new Label ("Custom Notebook"), Stock.Execute, "tab" + tabCount + 1);
		cn.ShowAll ();
		nb.AppendPage (new Label ("Regular Notebook"), new Label ("tab" + tabCount + 1));
		nb.ShowAll ();
		tabCount++;
	}*/

	static void OnRemoveTab (object sender, EventArgs args)
	{
	}

	static void OnQuit (object sender, DeleteEventArgs args)
	{
		Application.Quit ();
	}
}

class CustomNotebookPage {
	private Gdk.Rectangle allocation;
	private Widget child = null;
	private Gdk.Pixbuf icon = null;
	private bool ellipsize = false;
	private string label = null;
	private Pango.Layout layout = null;
	private int layoutWidth = -1;
	private int layoutHeight = -1;
	private Requisition requisition;
	private string stockid = null;
	
	public Gdk.Rectangle Allocation {
		get {
			return allocation;
		}
		set {
			allocation = value;
		}
	}
	
	public Widget Child {
		get {
			return child;
		}
		set {
			child = value;
		}
	}
	
	public bool Ellipsize {
		get {
			return ellipsize;
		}
		set {
			ellipsize = value;
		}
	}
	
	public Gdk.Pixbuf Icon {
		get {
			if (icon == null && StockId != null) {
				icon = child.RenderIcon (StockId, IconSize.Menu, "");
			}

			return icon;
		}
		set {
			icon = value;
		}
	}
	
	public string Label {
		get {
			return label;
		}
		set {
			label = value;
			layout = null;
			layoutWidth = -1;
			layoutHeight = -1;
		}
	}
	
	public Pango.Layout Layout {
		get {
			if (layout == null && Label != null) {
				layout = child.CreatePangoLayout (label);
				layout.GetPixelSize (out layoutWidth, out layoutHeight);
			}
			
			return layout;
		}
	}
	
	public int LayoutWidth {
		get {
			if (Layout != null)
				return layoutWidth;
			else
				return -1;
		}
	}
	
	public int LayoutHeight {
		get {
			if (Layout != null)
				return layoutHeight;
			else
				return -1;
		}
	}
	
	public Requisition Requisition {
		get {
			return requisition;
		}
		set {
			requisition = value;
		}
	}
	
	public string StockId {
		get {
			return stockid;
		}
		set {
			stockid = value;
			icon = null;
		}
	}
	
	public CustomNotebookPage (Widget child, string label)
	{
		Child = child;
		Label = label;
	}
	
	public CustomNotebookPage (Widget child, Gdk.Pixbuf icon, string label)
	{
		Child = child;
		Icon = icon;
		Label = label;
	}
	
	public CustomNotebookPage (Widget child, string stockid, string label)
	{
		Child = child;
		Label = label;
		StockId = stockid;
	}
}

class CustomNotebook : Container {
	private readonly int tabCurvature = 1;
	private readonly int tabOverlap = 2;
	private ArrayList pages = new ArrayList ();
	private bool closable;
	private bool scrollable;
	private PositionType tabPosition;
	private int tabHBorder;
	private int tabVBorder;

	public bool Closable {
		get {
			return closable;
		}
		set {
			closable = value;
		}
	}
	
	private CustomNotebookPage CurrentPage {
		get {
			return (CustomNotebookPage)pages[0];
		}
	}

	public bool Scrollable {
		get {
			return scrollable;
		}
		set {
			scrollable = value;
		}
	}

	public Gtk.PositionType TabPosition {
		get {
			return tabPosition;
		}
		set {
			switch (value) {
				case PositionType.Left:
				case PositionType.Right:
					Console.WriteLine ("PositionType.Left or Right is not supported by this widget");
					break;
				default:
					tabPosition = value;
					break;
			}
		}
	}

	/*static CustomNotebook ()
	{
		Container.OverrideForall (CustomNotebook.GType);
	}*/

	public CustomNotebook () : base ()
	{
		closable = true;
		scrollable = false;
		tabPosition = PositionType.Top;
		tabHBorder = 2;
		tabVBorder = 2;

		Flags |= (int)WidgetFlags.NoWindow;
	}

	public void AppendPage (Widget child, string label)
	{
		pages.Add (new CustomNotebookPage (child, label));
		child.Parent = this;
	}

	public void AppendPage (Widget child, string stockid, string label)
	{
		pages.Add (new CustomNotebookPage (child, stockid, label));
		child.Parent = this;
	}

	public void AppendPage (Widget child, Gdk.Pixbuf icon, string label)
	{
		pages.Add (new CustomNotebookPage (child, icon, label));
		child.Parent = this;
	}

	private void EllipsizeLayout (Pango.Layout layout, int width)
	{
		if (width <= 0) {
			layout.SetText ("");
			return;
		}
		
		int layoutWidth, layoutHeight;
		layout.GetPixelSize (out layoutWidth, out layoutHeight);
		if (layoutWidth <= width)
			return;

		// Calculate ellipsis width.
		Pango.Layout ell = layout.Copy ();
		ell.SetText ("...");
		int ellWidth, ellHeight;
		ell.GetPixelSize (out ellWidth, out ellHeight);
		
		if (width < ellWidth) {
			// Not even ellipsis fits, so hide text.
			layout.SetText ("");
			return;
		}

		// Shrink total available width by the width of the ellipsis.
		width -= ellWidth;
		string text = layout.Text;
		Console.WriteLine ("layout text = {0}", text);
		Console.WriteLine ("line count: {0}", layout.LineCount);
		Pango.LayoutLine line = layout.Lines[0];
		//Console.WriteLine ("layout = {0}", line.layout.Text);
		//Console.WriteLine ("line = {0}", line.Length);
		int idx = 0, trailing = 0;
		if (line.XToIndex (width * 1024, out idx, out trailing)) {
			text = text.Substring (0, idx - 1);
			text += "...";
			layout.SetText (text);
		}
	}

	protected override bool OnExposeEvent (Gdk.EventExpose args)
	{
		int x, y, width, height, gapX, gapWidth;
		int bw = (int)BorderWidth;
		
		x = Allocation.X + bw;
		y = Allocation.Y + bw;
		width = Allocation.Width - 2 * bw;
		height = Allocation.Height - 2 * bw;

		switch (TabPosition) {
			case PositionType.Top:
				y += CurrentPage.Allocation.Height;
				height -= CurrentPage.Allocation.Height;
				break;
			case PositionType.Bottom:
				height -= CurrentPage.Allocation.Height;
				break;
			case PositionType.Left:
			case PositionType.Right:
				break;
		}

		gapX = gapWidth = 0;
		switch (TabPosition) {
			case PositionType.Top:
			case PositionType.Bottom:
				gapX = CurrentPage.Allocation.X - Allocation.X - bw;
				gapWidth = CurrentPage.Allocation.Width;
				break;
			case PositionType.Left:
			case PositionType.Right:
				break;
		}

		Style.PaintBoxGap (Style, GdkWindow, StateType.Normal,
				   ShadowType.Out, args.Area, this,
				   "notebook", x, y, width, height,
				   TabPosition, gapX, gapWidth);

		for (int i = pages.Count - 1; i >= 0; i--) {
			CustomNotebookPage page = (CustomNotebookPage)pages[i];
			Gdk.Rectangle pageAlloc = page.Allocation;

			StateType state = page == CurrentPage ? StateType.Normal : StateType.Active;
			Style.PaintExtension (Style, GdkWindow, state,
					      ShadowType.Out, args.Area, this,
					      "tab", pageAlloc.X, pageAlloc.Y,
					      pageAlloc.Width, pageAlloc.Height,
					      PositionType.Bottom);
			
			// FIXME: Only add YThickness when TabPosition = Top;
			y = pageAlloc.Y + Style.YThickness + FocusLineWidth + tabVBorder;
			height = pageAlloc.Height - Style.YThickness - 2 * (tabVBorder + FocusLineWidth);
			if (page.Icon != null) {
				x = pageAlloc.X + (pageAlloc.Width + 1 -
						   page.Icon.Width -
						   page.LayoutWidth) / 2;
				int iconY = y + (height - page.Icon.Height) / 2;
				
				GdkWindow.DrawPixbuf (Style.BackgroundGC (State),
						      page.Icon, 0, 0, x, iconY,
						      page.Icon.Width,
						      page.Icon.Height,
						      Gdk.RgbDither.None, 0, 0);
				x += page.Icon.Width + 1;
			} else {
				x = pageAlloc.X + (pageAlloc.Width - page.LayoutWidth) / 2;
			}
			
			y += (height - page.LayoutHeight) / 2;
			if (page.Ellipsize) {
				width = pageAlloc.Width - (page.Icon.Width + 1);
				Pango.Layout layout = page.Layout;
				EllipsizeLayout (layout, width);
				Console.WriteLine ("ellLayout = {0}", layout.Text);
				Style.PaintLayout (Style, GdkWindow, State,
						   true, args.Area, this, null,
						   x, y, layout);
			} else {
				Style.PaintLayout (Style, GdkWindow, State,
						   true, args.Area, this, null,
						   x, y, page.Layout);
			}
		}
		
		return base.OnExposeEvent (args);
	}

	protected override void ForAll (bool include_internals, CallbackInvoker invoker)
	{
		foreach (CustomNotebookPage page in pages) {
			invoker.Invoke (page.Child);
		}
	}

	protected override void OnRealized ()
	{
		Flags |= (int)WidgetFlags.Realized;
		
		GdkWindow = ParentWindow;
		Style = Style.Attach (GdkWindow);
	}
	
	protected override void OnSizeAllocated (Gdk.Rectangle allocation)
	{
		base.OnSizeAllocated (allocation);
	
		if (pages.Count == 0)
			return;

		int bw = (int)BorderWidth;

		Gdk.Rectangle childAlloc;
		childAlloc.X = allocation.X + bw + Style.XThickness;
		childAlloc.Y = allocation.Y + bw + Style.YThickness;
		childAlloc.Width = Math.Max (1, allocation.Width - 2 * bw - 
					     2 * Style.XThickness);
		childAlloc.Height = Math.Max (1, allocation.Height - 2 * bw - 
					      2 * Style.YThickness);
		
		switch (TabPosition) {
			case PositionType.Top:
				childAlloc.Y += CurrentPage.Requisition.Height;
				childAlloc.Height = Math.Max (1, childAlloc.Height -
							      CurrentPage.Requisition.Height);
				break;
			case PositionType.Bottom:
				childAlloc.Height = Math.Max (1, childAlloc.Height -
							      CurrentPage.Requisition.Height);
				break;
			case PositionType.Left:
			case PositionType.Right:
				break;
		}
		
		foreach (CustomNotebookPage page in pages) {
			page.Child.SizeAllocate (childAlloc);
		}
		
		// gtk_notebook_pages_allocate.
		childAlloc.X = allocation.X + bw;
		childAlloc.Y = allocation.Y + bw;
		
		switch (TabPosition) {
			case PositionType.Top:
				childAlloc.Height = CurrentPage.Requisition.Height;
				break;
			case PositionType.Bottom:
				childAlloc.Y = (allocation.Y + allocation.Height -
						CurrentPage.Requisition.Height - bw);
				childAlloc.Height = CurrentPage.Requisition.Height;
				break;
			case PositionType.Left:
			case PositionType.Right:
				break;
		}
		
		bool ellipsize = false;
		int avgWidth = 0;
		int tabX = childAlloc.X;
		if (!scrollable) {
			int tabWidth = 0;
			foreach (CustomNotebookPage page in pages) {
				tabWidth += page.Requisition.Width;
			}
			
			Console.WriteLine ("total tabwidth: {0}", tabWidth);
			Console.WriteLine ("allocated width: {0}", childAlloc.Width);
			
			if (tabWidth > childAlloc.Width) {
				ellipsize = true;
				avgWidth = childAlloc.Width / pages.Count;
				tabWidth = childAlloc.Width;
				Console.WriteLine ("average tabwidth: {0}", avgWidth);

				int count = pages.Count;
				foreach (CustomNotebookPage page in pages) {
					if (page.Requisition.Width <= avgWidth) {
						count--;
						tabWidth -= page.Requisition.Width;
					}
				}
				
				Console.WriteLine ("number of pages exceeding that: {0}", count);
				Console.WriteLine ("space per page available: {0}", tabWidth / count);

				// FIXME: check for TabPosition.
				int maxWidth = tabWidth / count;
				foreach (CustomNotebookPage page in pages) {
					Gdk.Rectangle pageAlloc = page.Allocation;
					pageAlloc.X = tabX;
					pageAlloc.Y = childAlloc.Y;
					
					if (page.Requisition.Width > maxWidth) {
						pageAlloc.Width = maxWidth + tabOverlap;
						page.Ellipsize = true;
					} else {
						pageAlloc.Width = page.Requisition.Width + tabOverlap;
					}

					pageAlloc.Height = childAlloc.Height;
					tabX += pageAlloc.Width - tabOverlap;
					
					if (page != CurrentPage) {
						pageAlloc.Y += Style.YThickness;
						pageAlloc.Height -= Style.YThickness;
					}
					
					page.Allocation = pageAlloc;
				}
			}
		} else {
			switch (TabPosition) {
				case PositionType.Top:
				case PositionType.Bottom:
					foreach (CustomNotebookPage page in pages) {
						Gdk.Rectangle pageAlloc = page.Allocation;
						pageAlloc.X = tabX;
						pageAlloc.Y = childAlloc.Y;
						pageAlloc.Width = page.Requisition.Width + tabOverlap;
						pageAlloc.Height = childAlloc.Height;
						tabX += pageAlloc.Width - tabOverlap;
						
						if (page != CurrentPage) {
							pageAlloc.Y += Style.YThickness;
							pageAlloc.Height -= Style.YThickness;
						}
						
						page.Allocation = pageAlloc;
					}
					break;
				case PositionType.Left:
				case PositionType.Right:
					break;
			}
		}
	}

	protected override void OnSizeRequested (ref Requisition requisition)
	{
		requisition.Width = requisition.Height = 0;
		
		foreach (CustomNotebookPage page in pages) {
			if (!page.Child.Visible)
				continue;
				
			Requisition childReq = page.Child.SizeRequest ();
			requisition.Width = Math.Max (requisition.Width, childReq.Width);
			requisition.Height = Math.Max (requisition.Height, childReq.Height);
		}

		requisition.Width += 2 * Style.XThickness;
		requisition.Height += 2 * Style.YThickness;

		int tabWidth = 0;
		int tabHeight = 0;
		int tabMax = 0;
		int padding;
		foreach (CustomNotebookPage page in pages) {
			Requisition pageReq;
			if (page.Icon != null) {
				pageReq.Width = page.Icon.Width + page.LayoutWidth +
						Style.XThickness * 2;
				pageReq.Height = Math.Max (page.Icon.Height, page.LayoutHeight) +
						 Style.YThickness * 2;
			} else {
				pageReq.Width = page.LayoutWidth + Style.XThickness * 2;
				pageReq.Height = page.LayoutHeight + Style.YThickness * 2;
			}
			
			switch (TabPosition) {
				case PositionType.Top:
				case PositionType.Bottom:
					pageReq.Height += (tabVBorder + FocusLineWidth) * 2;
					tabHeight = Math.Max (tabHeight, pageReq.Height);
					tabMax = Math.Max (tabMax, pageReq.Width);
					break;
				case PositionType.Left:
				case PositionType.Right:
					break;
			}
			
			page.Requisition = pageReq;
		}
		
		switch (TabPosition) {
			case PositionType.Top:
			case PositionType.Bottom:
				padding = 2 * (tabCurvature + FocusLineWidth
					       + tabHBorder) - tabOverlap;
				tabMax += padding;
				
				Requisition pageReq;
				foreach (CustomNotebookPage page in pages) {
					pageReq = page.Requisition;
					pageReq.Width += padding;
					tabWidth += pageReq.Width;
					pageReq.Height = tabHeight;
					page.Requisition = pageReq;
				}
				
				/*if (!Scrollable)
					requisition.Width = Math.Max (requisition.Width,
								      tabWidth + tabOverlap);*/
				requisition.Height += tabHeight;
				break;
			case PositionType.Left:
			case PositionType.Right:
				break;
		}
		
		requisition.Width += (int)BorderWidth * 2;
		requisition.Height += (int)BorderWidth * 2;
	}
}

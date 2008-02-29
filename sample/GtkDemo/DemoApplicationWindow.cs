/* Application main window
 *
 * Demonstrates a typical application window, with menubar, toolbar, statusbar.
 */

using System;
using Gtk;

namespace GtkDemo
{
	[Demo ("Application Window", "DemoApplicationWindow.cs")]
	public class DemoApplicationWindow : Window
	{
		Statusbar statusbar;
		VBox vbox;

		static DemoApplicationWindow ()
		{
			// Register our custom toolbar icons, for themability

			Gdk.Pixbuf pixbuf = Gdk.Pixbuf.LoadFromResource ("gtk-logo-rgb.gif");
			Gdk.Pixbuf transparent = pixbuf.AddAlpha (true, 0xff, 0xff, 0xff);

			IconFactory factory = new IconFactory ();
			factory.Add ("demo-gtk-logo", new IconSet (transparent));
			factory.AddDefault ();

			StockManager.Add (new StockItem ("demo-gtk-logo", "_GTK#", 0, 0, null));
		}

		const string uiInfo =
		"<ui>" +
		"  <menubar name='MenuBar'>" +
		"    <menu action='FileMenu'>" +
		"      <menuitem action='New'/>" +
		"      <menuitem action='Open'/>" +
		"      <menuitem action='Save'/>" +
		"      <menuitem action='SaveAs'/>" +
		"      <separator/>" +
		"      <menuitem action='Quit'/>" +
		"    </menu>" +
		"    <menu action='PreferencesMenu'>" +
		"      <menu action='ColorMenu'>" +
		"	<menuitem action='Red'/>" +
		"	<menuitem action='Green'/>" +
		"	<menuitem action='Blue'/>" +
		"      </menu>" +
		"      <menu action='ShapeMenu'>" +
		"        <menuitem action='Square'/>" +
		"        <menuitem action='Rectangle'/>" +
		"        <menuitem action='Oval'/>" +
		"      </menu>" +
		"      <menuitem action='Bold'/>" +
		"    </menu>" +
		"    <menu action='HelpMenu'>" +
		"      <menuitem action='About'/>" +
		"    </menu>" +
		"  </menubar>" +
		"  <toolbar  name='ToolBar'>" +
		"    <toolitem name='open' action='Open'/>" +
		"    <toolitem name='quit' action='Quit'/>" +
		"    <separator action='Sep1'/>" +
		"    <toolitem name='logo' action='Logo'/>" +
		"  </toolbar>" +
		"</ui>";

		public DemoApplicationWindow () : base ("Application Window")
		{
			SetDefaultSize (200, 200);

			vbox = new VBox (false, 0);
			Add (vbox);

			AddActions ();

			statusbar = new Statusbar ();
			UpdateStatus ();
			vbox.PackEnd (statusbar, false, false, 0);

			ScrolledWindow sw = new ScrolledWindow ();
			sw.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			sw.ShadowType = ShadowType.In;
			vbox.PackEnd (sw, true, true, 0);

			TextView textview = new TextView ();
			textview.Buffer.MarkSet += new MarkSetHandler (MarkSet);
			sw.Add (textview);

			textview.GrabFocus ();

			ShowAll ();
		}

		enum Color {
			Red,
			Green,
			Blue
		}

		enum Shape {
			Square,
			Rectangle,
			Oval
		}

		void AddActions ()
		{
			ActionEntry[] actions = new ActionEntry[]
				{
					new ActionEntry ("FileMenu", null, "_File", null, null, null),
					new ActionEntry ("PreferencesMenu", null, "_Preferences", null, null, null),
					new ActionEntry ("ColorMenu", null, "_Color", null, null, null),
					new ActionEntry ("ShapeMenu", null, "_Shape", null, null, null),
					new ActionEntry ("HelpMenu", null, "_Help", null, null, null),
					new ActionEntry ("New", Stock.New, "_New", "<control>N", "Create a new file", new EventHandler (ActionActivated)),
					new ActionEntry ("Open", Stock.Open, "_Open", "<control>O", "Open a file", new EventHandler (ActionActivated)),
					new ActionEntry ("Save", Stock.Save, "_Save", "<control>S", "Save current file", new EventHandler (ActionActivated)),
					new ActionEntry ("SaveAs", Stock.SaveAs, "Save _As", null, "Save to a file", new EventHandler (ActionActivated)),
					new ActionEntry ("Quit", Stock.Quit, "_Quit", "<control>Q", "Quit", new EventHandler (ActionActivated)),
					new ActionEntry ("About", null, "_About", "<control>A", "About", new EventHandler (ActionActivated)),
					new ActionEntry ("Logo", "demo-gtk-logo", null, null, "Gtk#", new EventHandler (ActionActivated))
				};

			ToggleActionEntry[] toggleActions = new ToggleActionEntry[]
				{
					new ToggleActionEntry ("Bold", Stock.Bold, "_Bold", "<control>B", "Bold", new EventHandler (ActionActivated), true)
				};

			RadioActionEntry[] colorActions = new RadioActionEntry[]
				{
					new RadioActionEntry ("Red", null, "_Red", "<control>R", "Blood", (int)Color.Red),
					new RadioActionEntry ("Green", null, "_Green", "<control>G", "Grass", (int)Color.Green),
					new RadioActionEntry ("Blue", null, "_Blue", "<control>B", "Sky", (int)Color.Blue)
				};

			RadioActionEntry[] shapeActions = new RadioActionEntry[]
				{
					new RadioActionEntry ("Square", null, "_Square", "<control>S", "Square", (int)Shape.Square),
					new RadioActionEntry ("Rectangle", null, "_Rectangle", "<control>R", "Rectangle", (int)Shape.Rectangle),
					new RadioActionEntry ("Oval", null, "_Oval", "<control>O", "Egg", (int)Shape.Oval)
				};

			ActionGroup group = new ActionGroup ("AppWindowActions");
			group.Add (actions);
			group.Add (toggleActions);
			group.Add (colorActions, (int)Color.Red, new ChangedHandler (RadioActionActivated));
			group.Add (shapeActions, (int)Shape.Square, new ChangedHandler (RadioActionActivated));

			UIManager uim = new UIManager ();
			uim.InsertActionGroup (group, 0);
			uim.AddWidget += new AddWidgetHandler (AddWidget);
			uim.AddUiFromString (uiInfo);

			AddAccelGroup (uim.AccelGroup);
		}

		private void ActionActivated (object sender, EventArgs a)
		{
			Gtk.Action action = sender as Gtk.Action;
			MessageDialog md;

			md = new MessageDialog (this, DialogFlags.DestroyWithParent,
						MessageType.Info, ButtonsType.Close,
						"You activated action: \"{0}\" of type \"{1}\"",
						action.Name, action.GetType ());
			md.Run ();
			md.Destroy ();
		}

		private void RadioActionActivated (object sender, ChangedArgs args)
		{
			RadioAction action = args.Current;
			MessageDialog md;

			if (action.Active) {
				md = new MessageDialog (this, DialogFlags.DestroyWithParent,
							MessageType.Info, ButtonsType.Close,
							"You activated radio action: \"{0}\" of type \"{1}\".\nCurrent value: {2}",
							action.Name, action.GetType (),
							args.Current.Value);
				md.Run ();
				md.Destroy ();
			}
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}

		protected override bool OnWindowStateEvent (Gdk.EventWindowState evt)
		{
			if ((evt.ChangedMask & (Gdk.WindowState.Maximized | Gdk.WindowState.Fullscreen)) != 0)
				statusbar.HasResizeGrip = (evt.NewWindowState & (Gdk.WindowState.Maximized | Gdk.WindowState.Fullscreen)) == 0;
			return false;
		}

		const string fmt = "Cursor at row {0} column {1} - {2} chars in document";
		int row, column, count = 0;

		void MarkSet (object o, MarkSetArgs args)
		{
			TextIter iter = args.Location;
			row = iter.Line + 1;
			column = iter.VisibleLineOffset;
			count = args.Mark.Buffer.CharCount;
			UpdateStatus ();
		}

		void UpdateStatus ()
		{
			statusbar.Pop (0);
			statusbar.Push (0, String.Format (fmt, row, column, count));
		}

		void AddWidget (object sender, AddWidgetArgs a)
		{
			a.Widget.Show ();
			vbox.PackStart (a.Widget, false, true, 0);
		}
	}
}

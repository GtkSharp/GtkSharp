/* UI Manager
 *
 * The GtkUIManager object allows the easy creation of menus
 * from an array of actions and a description of the menu hierarchy.
 */
using System;
using Gtk;

namespace GtkDemo
{
	[Demo ("UIManager", "DemoUIManager.cs")]
	public class DemoUIManager : Window
	{
		enum Color {
			Red,
			Green,
			Blue
		};

		enum Shape {
			Square,
			Rectangle,
			Oval
		};

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

		public DemoUIManager () : base ("UI Manager")
		{
			ActionEntry[] entries = new ActionEntry[] {
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

			ToggleActionEntry[] toggleEntries = new ToggleActionEntry[] {
				new ToggleActionEntry ("Bold", Stock.Bold, "_Bold", "<control>B", "Bold", new EventHandler (ActionActivated), true)
			};


			RadioActionEntry[] colorEntries = new RadioActionEntry[] {
				new RadioActionEntry ("Red", null, "_Red", "<control>R", "Blood", (int)Color.Red),
				new RadioActionEntry ("Green", null, "_Green", "<control>G", "Grass", (int)Color.Green),
				new RadioActionEntry ("Blue", null, "_Blue", "<control>B", "Sky", (int)Color.Blue)
			};

			RadioActionEntry[] shapeEntries = new RadioActionEntry[] {
				new RadioActionEntry ("Square", null, "_Square", "<control>S", "Square", (int)Shape.Square),
				new RadioActionEntry ("Rectangle", null, "_Rectangle", "<control>R", "Rectangle", (int)Shape.Rectangle),
				new RadioActionEntry ("Oval", null, "_Oval", "<control>O", "Egg", (int)Shape.Oval)
			};

			ActionGroup actions = new ActionGroup ("group");
			actions.Add (entries);
			actions.Add (toggleEntries);
			actions.Add (colorEntries, (int)Color.Red, new ChangedHandler (RadioActionActivated));
			actions.Add (shapeEntries, (int)Shape.Oval, new ChangedHandler (RadioActionActivated));

			UIManager uim = new UIManager ();
			uim.InsertActionGroup (actions, 0);
			AddAccelGroup (uim.AccelGroup);
			uim.AddUiFromString (uiInfo);

			VBox box1 = new VBox (false, 0);
			Add (box1);

			box1.PackStart (uim.GetWidget ("/MenuBar"), false, false, 0);

			Label label = new Label ("Type\n<alt>\nto start");
			label.SetSizeRequest (200, 200);
			label.SetAlignment (0.5f, 0.5f);
			box1.PackStart (label, true, true, 0);

			HSeparator separator = new HSeparator ();
			box1.PackStart (separator, false, true, 0);

			VBox box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, false, true, 0);

			Button button = new Button ("close");
			button.Clicked += new EventHandler (CloseClicked);
			box2.PackStart (button, true, true, 0);
			button.CanDefault = true;
			button.GrabDefault ();

			ShowAll ();
		}

		private void ActionActivated (object sender, EventArgs a)
		{
			Gtk.Action action = sender as Gtk.Action;
			Console.WriteLine ("Action \"{0}\" activated", action.Name);
		}

		private void RadioActionActivated (object sender, ChangedArgs args)
		{
			Console.WriteLine ("Radio action \"{0}\" selected", args.Current.Name);
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}

		void CloseClicked (object sender, EventArgs a)
		{
			Destroy ();
		}
	}
}

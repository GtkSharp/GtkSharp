using System;
using Gtk;

namespace GtkDemo 
{
	[Demo ("UIManager", "DemoUIManager.cs")]
	public class DemoUIManager : Window
	{
		VBox vbox;

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

		public DemoUIManager () : base ("Demo UIManager")
		{
			this.SetDefaultSize (400, 300);
			this.DeleteEvent += new DeleteEventHandler (WindowDelete);

			vbox = new VBox (false, 0);
			this.Add (vbox);

			AddActions ();

			Button close = new Button (Stock.Close);
			close.Clicked += new EventHandler (OnCloseClicked);
			vbox.PackEnd (close, false, true, 0);

			vbox.PackEnd (new Label ("test"), true, true, 0);

			this.ShowAll ();
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
				new ActionEntry ("New", Stock.New, "_New", "<control>N", "Create a new file", new EventHandler (OnActionActivated)),
				new ActionEntry ("Open", Stock.Open, "_Open", "<control>O", "Open a file", new EventHandler (OnActionActivated)),
				new ActionEntry ("Save", Stock.Save, "_Save", "<control>S", "Save current file", new EventHandler (OnActionActivated)),
				new ActionEntry ("SaveAs", Stock.SaveAs, "Save _As", null, "Save to a file", new EventHandler (OnActionActivated)),
				new ActionEntry ("Quit", Stock.Quit, "_Quit", "<control>Q", "Quit", new EventHandler (OnActionActivated)),
				new ActionEntry ("About", null, "_About", "<control>A", "About", new EventHandler (OnActionActivated)),
				new ActionEntry ("Logo", "demo-gtk-logo", "Gtk#", null, "Gtk#", new EventHandler (OnActionActivated))
			};

			ToggleActionEntry[] toggleActions = new ToggleActionEntry[]
			{
				new ToggleActionEntry ("Bold", Stock.Bold, "_Bold", "<control>B", "Bold", new EventHandler (OnActionActivated), false)
			};

			ActionEntry[] colorActions = new ActionEntry[]
			{
				new ActionEntry ("Red", null, "_Red", "<control>R", "Blood", new EventHandler (OnActionActivated)),
				new ActionEntry ("Green", null, "_Green", "<control>G", "Grass", new EventHandler (OnActionActivated)),
				new ActionEntry ("Blue", null, "_Blue", "<control>B", "Sky", new EventHandler (OnActionActivated))
			};

			ActionEntry[] shapeActions = new ActionEntry[]
			{
				new ActionEntry ("Square", null, "_Square", "<control>S", "Square", new EventHandler (OnActionActivated)),
				new ActionEntry ("Rectangle", null, "_Rectangle", "<control>R", "Rectangle", new EventHandler (OnActionActivated)),
				new ActionEntry ("Oval", null, "_Oval", "<control>O", "Oval", new EventHandler (OnActionActivated))
			};

			ActionGroup group = new ActionGroup ("group");
			group.Add (actions);
			group.Add (toggleActions);
			group.Add (colorActions);
			group.Add (shapeActions);

			UIManager uim = new UIManager ();
			uim.InsertActionGroup (group, (int) uim.NewMergeId ());
			uim.AddWidget += new AddWidgetHandler (OnAddWidget);
			uim.AddUiFromString (uiInfo);
		}
		
		private void OnActionActivated (object sender, EventArgs a)
		{
			Action action = sender as Action;
			Console.WriteLine ("** Message: Action \"{0}\" activated", action.Name);
		}

		private void WindowDelete (object o, DeleteEventArgs args)
		{
			this.Hide ();
			this.Destroy ();
			args.RetVal = true;
		}

		void OnCloseClicked (object sender, EventArgs a)
		{
			this.Hide ();
			this.Destroy ();
		}

		void OnAddWidget (object sender, AddWidgetArgs a)
		{
			a.Widget.Show ();
			vbox.PackStart (a.Widget, false, true, 0);
		}
	}
}


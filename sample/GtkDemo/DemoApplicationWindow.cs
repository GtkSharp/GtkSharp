//
// ApplicationWindow.cs, port of appwindow.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//	John Luke <john.luke@gmail.com>
//
// Copyright (C) 2003, Ximian Inc.


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
		// for the statusbar
		const int ctx = 1;
		const string fmt = "Cursor at row {0} column {1} - {2} chars in document";
		int row, column, count = 0;

		Statusbar statusbar;
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

		public DemoApplicationWindow () : base ("Demo Application Window")
		{
			this.SetDefaultSize (400, 300);
			this.DeleteEvent += new DeleteEventHandler (WindowDelete);

			vbox = new VBox (false, 0);
			this.Add (vbox);

			AddActions ();

			statusbar = new Statusbar ();
			UpdateStatus ();
			vbox.PackEnd (statusbar, false, false, 0);

			TextView textview = new TextView ();
			textview.Buffer.MarkSet += new MarkSetHandler (OnMarkSet);
			vbox.PackEnd (textview, true, true, 0);

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
				new ActionEntry ("Red", null, "_Red", "<control>R", "Blood", null),
				new ActionEntry ("Green", null, "_Green", "<control>G", "Grass", null),
				new ActionEntry ("Blue", null, "_Blue", "<control>B", "Sky", null)
			};

			ActionEntry[] shapeActions = new ActionEntry[]
			{
				new ActionEntry ("Square", null, "_Square", "<control>S", "Square", null),
				new ActionEntry ("Rectangle", null, "_Rectangle", "<control>R", "Rectangle", null),
				new ActionEntry ("Oval", null, "_Oval", "<control>O", "Oval", null)
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

			using (MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Close, String.Format ("You activated action: {0}", action.Name))) {
				md.Run ();
				md.Hide ();
			}
		}

		private void WindowDelete (object o, DeleteEventArgs args)
		{
			this.Hide ();
			this.Destroy ();
			args.RetVal = true;
		}

		void OnMarkSet (object o, MarkSetArgs args)
		{
			TextIter iter = args.Location;
			row = iter.Line + 1;
			column = iter.VisibleLineOffset;
			count = args.Mark.Buffer.CharCount;
			UpdateStatus ();
		}

		void UpdateStatus ()
		{
			statusbar.Pop (ctx);
			statusbar.Push (ctx, String.Format (fmt, row, column, count));
		}

		void OnAddWidget (object sender, AddWidgetArgs a)
		{
			a.Widget.Show ();
			vbox.PackStart (a.Widget, false, true, 0);
		}
	}
}


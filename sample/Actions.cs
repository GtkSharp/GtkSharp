// Actions.cs - Gtk.Action class Test implementation
//
// Author: Jeroen Zwartepoorte <jeroen@xs4all.nl
//
// (c) 2003 Jeroen Zwartepoorte

namespace GtkSamples {

	using Gtk;
	using System;

	public class Actions {
		static VBox box = null;
		static Statusbar statusbar = null;

		/* XML description of the menus for the test app.  The parser understands
		 * a subset of the Bonobo UI XML format, and uses GMarkup for parsing */
		const string ui_info = 
			"<menubar>" +
			"  <menu name=\"Menu _1\" action=\"Menu1Action\">" +
			"    <menuitem name=\"quit\" action=\"quit\" />" +
			"  </menu>" +
			"</menubar>" +
			"<toolbar name=\"toolbar\">" +
			"  <toolitem name=\"quit\" action=\"quit\" />" +
			"</toolbar>";

		public static int Main (string[] args)
		{
			Application.Init ();
			Window win = new Window ("Action Demo");
			win.DefaultSize = new Gdk.Size (200, 150);
			win.DeleteEvent += new DeleteEventHandler (OnWindowDelete);
			
			box = new VBox (false, 0);
			win.Add (box);
			
			ActionGroup group = new ActionGroup ("TestGroup");
			Action action = new Action ("quit", null, "Quit the program", Stock.Quit);
			action.Activated += new EventHandler (OnQuit);
			group.Add (action, "<control>Q");
			
			action = new Action ("Menu1Action", "_File", null, null);
			group.Add (action);

			UIManager uim = new UIManager ();
			uim.AddWidget += new AddWidgetHandler (OnWidgetAdd);
			uim.ConnectProxy += new ConnectProxyHandler (OnProxyConnect);
			uim.InsertActionGroup (group, 0);
			uim.AddUiFromString (ui_info);
			
			statusbar = new Statusbar ();
			box.PackEnd (statusbar, false, true, 0);

			Button button = new Button ("Blah");
			box.PackEnd (button, true, true, 0);

			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void OnWindowDelete (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}
		
		static void OnWidgetAdd (object obj, AddWidgetArgs args)
		{
			Console.WriteLine ("OnWidgetAdd {0}", args.Widget.Name);
			args.Widget.Show ();
			box.PackStart (args.Widget, false, true, 0);
		}

		static void OnSelect (object obj, EventArgs args)
		{
			Action action = ((GLib.Object)obj).Data["action"] as Action;
			if (action.Tooltip != null)
				statusbar.Push (0, action.Tooltip);
		}

		static void OnDeselect (object obj, EventArgs args)
		{
			statusbar.Pop (0);
		}

		static void OnProxyConnect (object obj, ConnectProxyArgs args)
		{
			Console.WriteLine ("ProxyConnect {0}, {1}", args.Action, args.Proxy.Name);
			if (args.Proxy is MenuItem) {
				((GLib.Object)args.Proxy).Data ["action"] = args.Action;
				((Item)args.Proxy).Selected += new EventHandler (OnSelect);
				((Item)args.Proxy).Deselected += new EventHandler (OnDeselect);
			}
		}

		static void OnQuit (object obj, EventArgs args)
		{
			Console.WriteLine ("quit");
			Application.Quit ();
		}
	}
}

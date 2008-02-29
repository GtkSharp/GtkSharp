// Actions.cs - Gtk.Action class Test implementation (port of testactions.c)
//
// Author: Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (c) 2004 Jeroen Zwartepoorte

namespace GtkSamples {

	using Gtk;
	using System;
	using System.Collections;

	public class Actions {
		static VBox box = null;
		static Statusbar statusbar = null;
		static ActionGroup group = null;
		static Toolbar toolbar = null;
		static SpinButton spin = null;
		static ActionGroup dynGroup = null;
		static uint mergeId = 0;
		static UIManager uim = null;
		static Hashtable actions = new Hashtable ();

		/* XML description of the menus for the test app.  The parser understands
		 * a subset of the Bonobo UI XML format, and uses GMarkup for parsing */
		const string ui_info = 
			"  <menubar>\n" +
			"    <menu name=\"Menu _1\" action=\"Menu1Action\">\n" +
			"      <menuitem name=\"cut\" action=\"cut\" />\n" +
			"      <menuitem name=\"copy\" action=\"copy\" />\n" +
			"      <menuitem name=\"paste\" action=\"paste\" />\n" +
			"      <separator name=\"sep1\" />\n" +
			"      <menuitem name=\"bold1\" action=\"bold\" />\n" +
			"      <menuitem name=\"bold2\" action=\"bold\" />\n" +
			"      <separator name=\"sep2\" />\n" +
			"      <menuitem name=\"toggle-cnp\" action=\"toggle-cnp\" />\n" +
			"      <separator name=\"sep3\" />\n" +
			"      <menuitem name=\"quit\" action=\"quit\" />\n" +
			"    </menu>\n" +
			"    <menu name=\"Menu _2\" action=\"Menu2Action\">\n" +
			"      <menuitem name=\"cut\" action=\"cut\" />\n" +
			"      <menuitem name=\"copy\" action=\"copy\" />\n" +
			"      <menuitem name=\"paste\" action=\"paste\" />\n" +
			"      <separator name=\"sep4\"/>\n" +
			"      <menuitem name=\"bold\" action=\"bold\" />\n" +
			"      <separator name=\"sep5\"/>\n" +
			"      <menuitem name=\"justify-left\" action=\"justify-left\" />\n" +
			"      <menuitem name=\"justify-center\" action=\"justify-center\" />\n" +
			"      <menuitem name=\"justify-right\" action=\"justify-right\" />\n" +
			"      <menuitem name=\"justify-fill\" action=\"justify-fill\" />\n" +
			"      <separator name=\"sep6\"/>\n" +
			"      <menuitem  name=\"customise-accels\" action=\"customise-accels\" />\n" +
			"      <separator name=\"sep7\"/>\n" +
			"      <menuitem action=\"toolbar-icons\" />\n" +
			"      <menuitem action=\"toolbar-text\" />\n" +
			"      <menuitem action=\"toolbar-both\" />\n" +
			"      <menuitem action=\"toolbar-both-horiz\" />\n" +
			"      <separator name=\"sep8\"/>\n" +
			"      <menuitem action=\"toolbar-small-icons\" />\n" +
			"      <menuitem action=\"toolbar-large-icons\" />\n" +
			"    </menu>\n" +
			"    <menu name=\"DynamicMenu\" action=\"Menu3Action\" />\n" +
			"  </menubar>\n" +
			"  <toolbar name=\"toolbar\">\n" +
			"    <toolitem name=\"cut\" action=\"cut\" />\n" +
			"    <toolitem name=\"copy\" action=\"copy\" />\n" +
			"    <toolitem name=\"paste\" action=\"paste\" />\n" +
			"    <separator name=\"sep9\" />\n" +
			"    <toolitem name=\"bold\" action=\"bold\" />\n" +
			"    <separator name=\"sep10\" />\n" +
			"    <toolitem name=\"justify-left\" action=\"justify-left\" />\n" +
			"    <toolitem name=\"justify-center\" action=\"justify-center\" />\n" +
			"    <toolitem name=\"justify-right\" action=\"justify-right\" />\n" +
			"    <toolitem name=\"justify-fill\" action=\"justify-fill\" />\n" +
			"    <separator name=\"sep11\"/>\n" +
			"    <toolitem name=\"quit\" action=\"quit\" />\n" +
			"  </toolbar>\n";

		static ActionEntry[] entries = new ActionEntry[] {
			new ActionEntry ("Menu1Action", null, "Menu _1", null, null, null),
			new ActionEntry ("Menu2Action", null, "Menu _2", null, null, null),
			new ActionEntry ("Menu3Action", null, "_Dynamic Menu", null, null, null),
			new ActionEntry ("cut", Stock.Cut, "C_ut", "<control>X",
					 "Cut the selected text to the clipboard",
					 new EventHandler (OnActivate)),
			new ActionEntry ("copy", Stock.Copy, "_Copy", "<control>C",
					 "Copy the selected text to the clipboard",
					 new EventHandler (OnActivate)),
			new ActionEntry ("paste", Stock.Paste, "_Paste", "<control>V",
					 "Paste the text from the clipboard",
					 new EventHandler (OnActivate)),
			new ActionEntry ("quit", Stock.Quit, null, "<control>Q",
					 "Quit the application", new EventHandler (OnQuit)),
			new ActionEntry ("customise-accels", null, "Customise _Accels", null,
					 "Customize keyboard shortcuts",
					 new EventHandler (OnCustomizeAccels)),
			new ActionEntry ("toolbar-small-icons", null, "Small Icons", null,
					 null, new EventHandler (OnToolbarSizeSmall)),
			new ActionEntry ("toolbar-large-icons", null, "Large Icons", null,
					 null, new EventHandler (OnToolbarSizeLarge))
		};

		static ToggleActionEntry[] toggleEntries = new ToggleActionEntry[] {
			new ToggleActionEntry ("bold", Stock.Bold, "_Bold", "<control>B",
					       "Change to bold face",
					       new EventHandler (OnToggle), false),
			new ToggleActionEntry ("toggle-cnp", null, "Enable Cut/Copy/Paste",
					       null, "Change the sensitivity of the cut, copy and paste actions",
					       new EventHandler (OnToggleCnp),  true)
		};

		enum Justify {
			Left,
			Center,
			Right,
			Fill
		};

		static RadioActionEntry[] radioEntries = new RadioActionEntry[] {
			new RadioActionEntry ("justify-left", Stock.JustifyLeft, "_Left",
					      "<control>L", "Left justify the text",
					      (int)Justify.Left),
			new RadioActionEntry ("justify-center", Stock.JustifyCenter, "C_enter",
					      "<control>E", "Center justify the text",
					      (int)Justify.Center),
			new RadioActionEntry ("justify-right", Stock.JustifyRight, "_Right",
					      "<control>R", "Right justify the text",
					      (int)Justify.Right),
			new RadioActionEntry ("justify-fill", Stock.JustifyFill, "_Fill",
					      "<control>J", "Fill justify the text",
					      (int)Justify.Fill)
		};

		static RadioActionEntry[] toolbarEntries = new RadioActionEntry[] {
			new RadioActionEntry ("toolbar-icons", null, "Icons", null,
					      null, (int)ToolbarStyle.Icons),
			new RadioActionEntry ("toolbar-text", null, "Text", null,
					      null, (int)ToolbarStyle.Text),
			new RadioActionEntry ("toolbar-both", null, "Both", null,
					      null, (int)ToolbarStyle.Both),
			new RadioActionEntry ("toolbar-both-horiz", null, "Both Horizontal",
					      null, null, (int)ToolbarStyle.BothHoriz)
		};

		public static int Main (string[] args)
		{
			Application.Init ();
			Window win = new Window ("Action Demo");
			win.DefaultSize = new Gdk.Size (200, 150);
			win.DeleteEvent += new DeleteEventHandler (OnWindowDelete);
			
			box = new VBox (false, 0);
			win.Add (box);
			
			group = new ActionGroup ("TestActions");
			group.Add (entries);
			group.Add (toggleEntries);
			group.Add (radioEntries, (int)Justify.Left, new ChangedHandler (OnRadio));
			group.Add (toolbarEntries, (int)ToolbarStyle.BothHoriz, new ChangedHandler (OnToolbarStyle));
			
			uim = new UIManager ();
			uim.AddWidget += new AddWidgetHandler (OnWidgetAdd);
			uim.ConnectProxy += new ConnectProxyHandler (OnProxyConnect);
			uim.InsertActionGroup (group, 0);
			uim.AddUiFromString (ui_info);
			
			statusbar = new Statusbar ();
			box.PackEnd (statusbar, false, true, 0);

			VBox vbox = new VBox (false, 5);
			Button button = new Button ("Blah");
			vbox.PackEnd (button, true, true, 0);
			HBox hbox = new HBox (false, 5);
			spin = new SpinButton (new Adjustment (100, 100, 10000, 1, 100, 100), 100, 0);
			hbox.PackStart (spin, true, true, 0);
			button = new Button ("Remove");
			button.Clicked += new EventHandler (OnDynamicRemove);
			hbox.PackEnd (button, false, false, 0);
			button = new Button ("Add");
			button.Clicked += new EventHandler (OnDynamicAdd);
			hbox.PackEnd (button, false, false, 0);
			vbox.PackEnd (hbox, false, false, 0);
			box.PackEnd (vbox, true, true, 0);

			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		static void OnActivate (object obj, EventArgs args)
		{
			Gtk.Action action = (Gtk.Action)obj;
			Console.WriteLine ("Action {0} (type={1}) activated",
					   action.Name, action.GetType ().FullName);
		}

		static void OnCustomizeAccels (object obj, EventArgs args)
		{
			Console.WriteLine ("Sorry, accel dialog not available");
		}

		static void OnToolbarSizeSmall (object obj, EventArgs args)
		{
			toolbar.IconSize = IconSize.SmallToolbar;
		}

		static void OnToolbarSizeLarge (object obj, EventArgs args)
		{
			toolbar.IconSize = IconSize.LargeToolbar;
		}

		static void OnToggle (object obj, EventArgs args)
		{
			ToggleAction action = (ToggleAction)obj;
			Console.WriteLine ("Action {0} (type={1}) activated (active={2})",
					   action.Name, action.GetType ().FullName, action.Active);
		}

		static void OnToggleCnp (object obj, EventArgs args)
		{
			Gtk.Action action = (ToggleAction)obj;
			bool sensitive = ((ToggleAction)action).Active;
			action = group.GetAction ("cut");
			action.Sensitive = sensitive;
			action = group.GetAction ("copy");
			action.Sensitive = sensitive;
			action = group.GetAction ("paste");
			action.Sensitive = sensitive;

			action = group.GetAction ("toggle-cnp");
			if (sensitive)
				action.Label = "Disable Cut and past ops";
			else
				action.Label = "Enable Cut and paste ops";
		}

		static void OnRadio (object obj, ChangedArgs args)
		{
			RadioAction action = (RadioAction)obj;
			Console.WriteLine ("Action {0} (type={1}) activated (active={2}) (value {3})",
					   action.Name, action.GetType ().FullName,
					   action.Active, action.CurrentValue);
		}

		static void OnToolbarStyle (object obj, ChangedArgs args)
		{
			RadioAction action = (RadioAction)obj;
			ToolbarStyle style = (ToolbarStyle)action.CurrentValue;
			toolbar.ToolbarStyle = style;
		}
		
		static void OnDynamicAdd (object obj, EventArgs args)
		{
			if (mergeId != 0 || dynGroup != null)
				return;
		
			int num = spin.ValueAsInt;
			dynGroup = new ActionGroup ("DynamicActions");
			uim.InsertActionGroup (dynGroup, 0);
			mergeId = uim.NewMergeId ();
			
			for (int i = 0; i < num; i++) {
				string name = "DynAction" + i;
				string label = "Dynamic Action " + i;
				Gtk.Action action = new Gtk.Action (name, label);
				dynGroup.Add (action);
				uim.AddUi (mergeId, "/menubar/DynamicMenu", name,
					   name, UIManagerItemType.Menuitem, false);
			}
			
			uim.EnsureUpdate ();
		}

		static void OnDynamicRemove (object obj, EventArgs args)
		{
			if (mergeId == 0 || dynGroup == null)
				return;
			
			uim.RemoveUi (mergeId);
			uim.EnsureUpdate ();
			mergeId = 0;
			uim.RemoveActionGroup (dynGroup);
			dynGroup = null;
		}

		static void OnWindowDelete (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}
		
		static void OnWidgetAdd (object obj, AddWidgetArgs args)
		{
			if (args.Widget is Toolbar)
				toolbar = (Toolbar)args.Widget;
			args.Widget.Show ();
			box.PackStart (args.Widget, false, true, 0);
		}

		static void OnSelect (object obj, EventArgs args)
		{
			Gtk.Action action = (Gtk.Action) actions[obj];
			if (action.Tooltip != null)
				statusbar.Push (0, action.Tooltip);
		}

		static void OnDeselect (object obj, EventArgs args)
		{
			statusbar.Pop (0);
		}

		static void OnProxyConnect (object obj, ConnectProxyArgs args)
		{
			if (args.Proxy is MenuItem) {
				actions[args.Proxy] = args.Action;
				((Item)args.Proxy).Selected += new EventHandler (OnSelect);
				((Item)args.Proxy).Deselected += new EventHandler (OnDeselect);
			}
		}

		static void OnQuit (object obj, EventArgs args)
		{
			Application.Quit ();
		}
	}
}

// TreeView.cs - Fun TreeView demo
//
// Author: Kristian Rietveld <kris@gtk.org>
//
// (c) 2002 Kristian Rietveld

namespace GtkSamples {
	using System;
	using System.Drawing;
	using System.Reflection;

	using GLib;
	using Gtk;
	using GtkSharp;

	public class TreeViewDemo {
		private static TreeStore store = null;
		private static Dialog dialog = null;
		private static Label dialog_label = null;

		private static void ProcessType (TreeIter parent, System.Type t)
		{
			TreeIter iter = new TreeIter ();

			// .GetMembers() won't work due to unimplemented
			// scary classes.
			foreach (MemberInfo mi in t.GetMethods ()) {
				GLib.Value Name = new GLib.Value (mi.Name);
				GLib.Value Type = new GLib.Value (mi.ToString ());

				store.Append (out iter, parent);
				store.SetValue (iter, 0, Name);
				store.SetValue (iter, 1, Type);
			}
		}

		private static void ProcessAssembly (TreeIter parent, Assembly asm)
		{
			TreeIter iter = new TreeIter ();
			string asm_name = asm.GetName ().Name;

			foreach (System.Type t in asm.GetTypes ()) {
				UpdateDialog ("Loading from {0}:\n{1}", asm_name, t.ToString ());

				GLib.Value Name = new GLib.Value (t.Name);
				GLib.Value Type = new GLib.Value (t.ToString ());

				store.Append (out iter, parent);
				store.SetValue (iter, 0, Name);
				store.SetValue (iter, 1, Type);

				ProcessType (iter, t);
			}
		}

		private static void PopulateStore ()
		{
			if (store != null)
				return;

			store = new TreeStore ((int)TypeFundamentals.TypeString,
					       (int)TypeFundamentals.TypeString);

			TreeIter iter = new TreeIter ();

			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies ()) {
				// we can't show corlib due to some unimplemented
				// stuff in some scary class.
				if (asm.GetName ().Name == "mscorlib") continue;

				UpdateDialog ("Loading {0}", asm.GetName ().Name);

				GLib.Value Name = new GLib.Value (asm.GetName ().Name);
				GLib.Value Type = new GLib.Value ("Assembly");

				store.Append (out iter);
				store.SetValue (iter, 0, Name);
				store.SetValue (iter, 1, Type);

				ProcessAssembly (iter, asm);
			}
		}

		public static void Main (string[] args)
		{
			Application.Init ();

			Idle.Add (new IdleHandler (IdleCB));

			Application.Run ();
		}

		public static bool IdleCB ()
		{
			PopulateStore ();

			Window win = new Window ("TreeView demo");
			win.DeleteEvent += new DeleteEventHandler (DeleteCB);
			win.DefaultSize = new Size (640,480);

			ScrolledWindow sw = new ScrolledWindow ();
			win.Add (sw);

			TreeView tv = new TreeView (store);
			tv.HeadersVisible = true;

			TreeViewColumn NameCol = new TreeViewColumn ();
			CellRenderer NameRenderer = new CellRendererText ();
			NameCol.Title = "Name";
			NameCol.PackStart (NameRenderer, true);
			NameCol.AddAttribute (NameRenderer, "text", 0);
			tv.AppendColumn (NameCol);

			TreeViewColumn TypeCol = new TreeViewColumn ();
			CellRenderer TypeRenderer = new CellRendererText ();
			TypeCol.Title = "Type";
			TypeCol.PackStart (TypeRenderer, false);
			TypeCol.AddAttribute (TypeRenderer, "text", 1);
			tv.AppendColumn (TypeCol);

			sw.Add (tv);
			
			dialog.Destroy ();
			dialog = null;

			win.ShowAll ();
			return false;
		}

		private static void DeleteCB (System.Object o, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}

		private static void UpdateDialog (string format, params object[] args)
		{
			string text = String.Format (format, args);

			if (dialog == null)
			{
				dialog = new Dialog ();
				dialog.Title = "Loading data from assemblies...";
				dialog.AddButton (Stock.Cancel, 1);
				dialog.Response += new ResponseHandler (ResponseCB);
				dialog.DefaultSize = new Size (480, 100);
					
				VBox vbox = dialog.VBox;
				HBox hbox = new HBox (false, 4);
				vbox.PackStart (hbox, true, true, 0);
				
				Gtk.Image icon = new Gtk.Image (Stock.DialogInfo, IconSize.Dialog);
				hbox.PackStart (icon, false, false, 0);
				dialog_label = new Label (text);
				hbox.PackStart (dialog_label, false, false, 0);
				dialog.ShowAll ();
			} else {
				dialog_label.Text = text;
				while (Application.EventsPending ())
					Application.RunIteration ();
	 		}
		}

		private static void ResponseCB (object obj, ResponseArgs args)
		{
			Application.Quit ();
			System.Environment.Exit (0);
		}
	}
}

// TreeView.cs - Fun TreeView demo
//
// Author: Kristian Rietveld <kris@gtk.org>
//
// (c) 2002 Kristian Rietveld

namespace GtkSamples {
	using System;
	using System.Drawing;
	using System.Reflection;
	using System.AppDomain;

	using GLib;
	using Gtk;
	using GtkSharp;

	public class TreeViewDemo {
		private static TreeStore store = null;

		private static void ProcessType (TreeIter parent, Type t)
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

			foreach (Type t in asm.GetTypes ()) {
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

			Console.WriteLine ("Loading data from assemblies...");

			store = new TreeStore ((int)TypeFundamentals.TypeString,
					       (int)TypeFundamentals.TypeString);

			TreeIter iter = new TreeIter ();

			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies ()) {
				// we can't show corlib due to some unimplemented
				// stuff in some scary class.
				if (asm.GetName ().Name == "mscorlib") continue;

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

			Window win = new Window ("TreeView demo");
			win.DeleteEvent += new DeleteEventHandler (DeleteCB);
			win.DefaultSize = new Size (640,480);

			ScrolledWindow sw = new ScrolledWindow ();
			win.Add (sw);

			PopulateStore ();

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

			win.ShowAll ();

			Application.Run ();
		}

		private static void DeleteCB (Object o, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}
	}
}

// TreeView.cs - Fun TreeView demo
//
// Author: Kristian Rietveld <kris@gtk.org>
//
// (c) 2002 Kristian Rietveld

namespace GtkSamples {
	using System;
	using System.Reflection;

	using Gtk;

	public class TreeViewDemo {
		private static TreeStore store = null;
		private static Dialog dialog = null;
		private static Label dialog_label = null;
		private static int count = 0;
		
		public TreeViewDemo ()
		{
			DateTime start = DateTime.Now;

			Application.Init ();
			
			PopulateStore ();

			Window win = new Window ("TreeView demo");
			win.DeleteEvent += new DeleteEventHandler (DeleteCB);
			win.SetDefaultSize (640,480);

			ScrolledWindow sw = new ScrolledWindow ();
			win.Add (sw);

			TreeView tv = new TreeView (store);
			tv.HeadersVisible = true;
			tv.EnableSearch = false;

			tv.AppendColumn ("Name", new CellRendererText (), "text", 0);
			tv.AppendColumn ("Type", new CellRendererText (), "text", 1);

			sw.Add (tv);
			
			dialog.Destroy ();
			dialog = null;

			win.ShowAll ();
			
			Console.WriteLine (count + " nodes added.");
			Console.WriteLine ("Startup time: " + DateTime.Now.Subtract (start));
			Application.Run ();
		}

		private static void ProcessType (TreeIter parent, System.Type t)
		{
			foreach (MemberInfo mi in t.GetMembers ()) {
 				store.AppendValues (parent, mi.Name, mi.ToString ());
				count++;
			}
		}

		private static void ProcessAssembly (TreeIter parent, Assembly asm)
		{
			string asm_name = asm.GetName ().Name;

			foreach (System.Type t in asm.GetTypes ()) {
				UpdateDialog ("Loading from {0}:\n{1}", asm_name, t.ToString ());
 				TreeIter iter = store.AppendValues (parent, t.Name, t.ToString ());
				count++;
				ProcessType (iter, t);
			}
		}

		private static void PopulateStore ()
		{
			if (store != null)
				return;

			store = new TreeStore (typeof (string), typeof (string));

			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies ()) {

				UpdateDialog ("Loading {0}", asm.GetName ().Name);

				TreeIter iter = store.AppendValues (asm.GetName ().Name, "Assembly");
				count++;
				ProcessAssembly (iter, asm);
			}
		}

		public static void Main (string[] args)
		{
			new TreeViewDemo ();	
		}

		private static void DeleteCB (System.Object o, DeleteEventArgs args)
		{
			Application.Quit ();
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
				dialog.SetDefaultSize (480, 100);
					
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

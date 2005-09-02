// NodeViewDemo.cs - rework of TreeViewDemo to use NodeView.
//
// Author: Mike Kestner  <mkestner@novell.com>
//
// Copyright (c) 2004 Novell, Inc.

namespace GtkSamples {

	using System;
	using System.Reflection;
	using Gtk;

	public class DemoTreeNode : TreeNode {
		string desc;
		static int count = 0;

		public DemoTreeNode (string name, string desc)
		{
			this.Name = name;
			this.desc = desc;
			count++;
		}
		
		// TreeNodeValues can come from both properties and fields
		[TreeNodeValue (Column=0)]	
		public string Name;
		
		[TreeNodeValue (Column=1)]
		public string Description {
			get { return desc; }
		}

		public static int Count {
			get {
				return count;
			}
		}
	}

	public class NodeViewDemo : Gtk.Window {

		NodeStore store;
		StatusDialog dialog;
		
		public NodeViewDemo () : base ("NodeView demo")
		{
			DeleteEvent += new DeleteEventHandler (DeleteCB);
			DefaultSize = new Gdk.Size (640,480);

			ScrolledWindow sw = new ScrolledWindow ();
			Add (sw);

			NodeView view = new NodeView (Store);
			view.HeadersVisible = true;
			view.AppendColumn ("Name", new CellRendererText (), "text", 0);
			view.AppendColumn ("Type", new CellRendererText (), new NodeCellDataFunc (DataCallback));

			sw.Add (view);
			
			dialog.Destroy ();
			dialog = null;
		}

		private void DataCallback (TreeViewColumn col, CellRenderer cell, ITreeNode node)
		{
			(cell as CellRendererText).Text = (node as DemoTreeNode).Description;
		}

		StatusDialog Dialog {
			get {
				if (dialog == null)
					dialog = new StatusDialog ();
				return dialog;
			}
		}

		void ProcessType (DemoTreeNode parent, System.Type t)
		{
			foreach (MemberInfo mi in t.GetMembers ())
 				parent.AddChild (new DemoTreeNode (mi.Name, mi.ToString ()));
		}

		void ProcessAssembly (DemoTreeNode parent, Assembly asm)
		{
			string asm_name = asm.GetName ().Name;

			foreach (System.Type t in asm.GetTypes ()) {
				Dialog.Update ("Loading from {0}:\n{1}", asm_name, t.ToString ());
				DemoTreeNode node = new DemoTreeNode (t.Name, t.ToString ());
				ProcessType (node, t);
 				parent.AddChild (node);
			}
		}

		NodeStore Store {
			get {
				if (store == null) {
					store = new NodeStore (typeof (DemoTreeNode));

					foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies ()) {
						Dialog.Update ("Loading {0}", asm.GetName ().Name);
						DemoTreeNode node = new DemoTreeNode (asm.GetName ().Name, "Assembly");
						ProcessAssembly (node, asm);
						store.AddNode (node);
					}
				}
				return store;
			}
		}

		public static void Main (string[] args)
		{
			DateTime start = DateTime.Now;
			Application.Init ();
			Gtk.Window win = new NodeViewDemo ();	
			win.ShowAll ();
			Console.WriteLine (DemoTreeNode.Count + " nodes created.");
			Console.WriteLine ("startup time: " + DateTime.Now.Subtract (start));
			Application.Run ();
		}

		void DeleteCB (System.Object o, DeleteEventArgs args)
		{
			Application.Quit ();
		}

	}

	public class StatusDialog : Gtk.Dialog  {

		Label dialog_label;

		public StatusDialog () : base ()
		{

			Title = "Loading data from assemblies...";
			AddButton (Stock.Cancel, 1);
			Response += new ResponseHandler (ResponseCB);
			DefaultSize = new Gdk.Size (480, 100);
					
			HBox hbox = new HBox (false, 4);
			VBox.PackStart (hbox, true, true, 0);
				
			Gtk.Image icon = new Gtk.Image (Stock.DialogInfo, IconSize.Dialog);
			hbox.PackStart (icon, false, false, 0);
			dialog_label = new Label ("");
			hbox.PackStart (dialog_label, false, false, 0);
			ShowAll ();
		}

		public void Update (string format, params object[] args)
		{
			string text = String.Format (format, args);
			dialog_label.Text = text;
			while (Application.EventsPending ())
				Application.RunIteration ();
		}

		private static void ResponseCB (object obj, ResponseArgs args)
		{
			Application.Quit ();
			System.Environment.Exit (0);
		}
	}
}

using System;
using System.Collections;
using System.IO;
using System.Reflection;

using Gdk;
using Gtk;
using Pango;

namespace GtkDemo
{
	public class DemoMain
	{
		private Gtk.Window window;
		private TextBuffer infoBuffer = new TextBuffer (null);
		private TextBuffer sourceBuffer = new TextBuffer (null);
		private TreeView treeView;
		private TreeStore store;
		private TreeIter oldSelection = TreeIter.Zero;

		public static void Main (string[] args)
		{
			Application.Init ();
			new DemoMain ();
			Application.Run ();
		}

		public DemoMain ()
		{
			SetupDefaultIcon ();
		   	window = new Gtk.Window ("Gtk# Code Demos");
		   	window.SetDefaultSize (600, 400);
			window.DeleteEvent += new DeleteEventHandler (WindowDelete);

			HBox hbox = new HBox (false, 0);
			window.Add (hbox);

			treeView = CreateTree ();
			hbox.PackStart (treeView, false, false, 0);

			Notebook notebook = new Notebook ();
			hbox.PackStart (notebook, true, true, 0);

			notebook.AppendPage (CreateText (infoBuffer, false), new Label ("_Info"));
			TextTag heading = new TextTag ("heading");
			heading.Font = "Sans 18";
			infoBuffer.TagTable.Add (heading);

			notebook.AppendPage (CreateText (sourceBuffer, true), new Label ("_Source"));

			window.ShowAll ();
		}

		private void LoadFile (string filename)
		{
			Stream file = Assembly.GetExecutingAssembly ().GetManifestResourceStream (filename);
			if (file != null) {
				LoadStream (file);
			} else if (File.Exists (filename)) {
				file = File.OpenRead (filename);
				LoadStream (file);
			} else {
				infoBuffer.Text = String.Format ("{0} was not found.", filename);
				sourceBuffer.Text = String.Empty;
			}

			Fontify ();
		}

		private enum LoadState {
			Title,
			Info,
			SkipWhitespace,
			Body
		};

		private void LoadStream (Stream file)
		{
			StreamReader sr = new StreamReader (file);
			LoadState state = LoadState.Title;
			bool inPara = false;

			infoBuffer.Text = "";
			sourceBuffer.Text = "";

			TextIter infoIter = infoBuffer.EndIter;
			TextIter sourceIter = sourceBuffer.EndIter;

			// mostly broken comment parsing for splitting
			// out the special comments to display in the infobuffer

			string line, trimmed;
			while (sr.Peek () != -1) {
				line = sr.ReadLine ();
				trimmed = line.Trim ();

				switch (state) {
				case LoadState.Title:
					if (trimmed.StartsWith ("/* ")) {
						infoBuffer.InsertWithTagsByName (ref infoIter, trimmed.Substring (3), "heading");
						state = LoadState.Info;
					}
					break;

				case LoadState.Info:
					if (trimmed == "*") {
						infoBuffer.Insert (ref infoIter, "\n");
						inPara = false;
					} else if (trimmed.StartsWith ("* ")) {
						if (inPara)
							infoBuffer.Insert (ref infoIter, " ");
						infoBuffer.Insert (ref infoIter, trimmed.Substring (2));
						inPara = true;
					} else if (trimmed.StartsWith ("*/"))
						state = LoadState.SkipWhitespace;
					break;

				case LoadState.SkipWhitespace:
					if (trimmed != "") {
						state = LoadState.Body;
						goto case LoadState.Body;
					}
					break;

				case LoadState.Body:
					sourceBuffer.Insert (ref sourceIter, line + "\n");
					break;
				}
			}
			sr.Close ();
			file.Close ();
		}

		// this is to highlight the sourceBuffer
		private void Fontify ()
		{
		}

		private void SetupDefaultIcon ()
		{
			Gdk.Pixbuf pixbuf = Gdk.Pixbuf.LoadFromResource ("gtk-logo-rgb.gif");

			if (pixbuf != null) {
				// The gtk-logo-rgb icon has a white background
				// make it transparent instead
				Pixbuf transparent  = pixbuf.AddAlpha (true, 0xff, 0xff, 0xff);

				Gtk.Window.DefaultIconList = new Gdk.Pixbuf [] { transparent };
			}
		}

		private TreeView CreateTree ()
		{
			TreeView view = new TreeView ();
			view.Model = FillTree ();

			CellRendererText cr = new CellRendererText ();
			TreeViewColumn column = new TreeViewColumn ("Widget (double click for demo)", cr, "text", 0);
			column.AddAttribute (cr, "style" , 2);
			view.AppendColumn (column);

			view.Selection.Changed += new EventHandler (TreeChanged);
			view.RowActivated += new RowActivatedHandler (RowActivated);
			view.ExpandAll ();
			view.SetSizeRequest (200, -1);
			view.Selection.Mode = Gtk.SelectionMode.Browse;
			return view;
		}

		private ScrolledWindow CreateText (TextBuffer buffer, bool IsSource)
		{
			ScrolledWindow scrolledWindow = new ScrolledWindow ();
			scrolledWindow.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			scrolledWindow.ShadowType = ShadowType.In;

			TextView textView = new TextView (buffer);
			textView.Editable = false;
			textView.CursorVisible = false;

			scrolledWindow.Add (textView);

			if (IsSource) {
				FontDescription fontDescription = FontDescription.FromString ("Courier 12");
				textView.ModifyFont (fontDescription);
				textView.WrapMode = Gtk.WrapMode.None;
			} else {
				// Make it a bit nicer for text
				textView.WrapMode = Gtk.WrapMode.Word;
				textView.PixelsAboveLines = 2;
				textView.PixelsBelowLines = 2;
			}

			return scrolledWindow;
	        }

		private TreeStore FillTree ()
		{
			// title, filename, italic
			store = new TreeStore (typeof (string), typeof (System.Type), typeof (bool));
			Hashtable parents = new Hashtable ();
			TreeIter parent;

			Type[] types = Assembly.GetExecutingAssembly ().GetTypes ();
			foreach (Type t in types) {
				object[] att = t.GetCustomAttributes (typeof (DemoAttribute), false);
				foreach (DemoAttribute demo in att) {
					if (demo.Parent != null) {
						if (!parents.Contains (demo.Parent))
							parents.Add (demo.Parent, store.AppendValues (demo.Parent));

						parent = (TreeIter) parents[demo.Parent];
						store.AppendValues (parent, demo.Label, t, false);
					} else {
						store.AppendValues (demo.Label, t, false);
					}
				}
			}
			store.SetSortColumnId (0, SortType.Ascending);
			return store;
		}

		private void TreeChanged (object o, EventArgs args)
		{
			TreeIter iter;
			TreeModel model;

			if (treeView.Selection.GetSelected (out model, out iter)) {
				Type type = (Type) model.GetValue (iter, 1);
				if (type != null) {
					object[] atts = type.GetCustomAttributes (typeof (DemoAttribute), false);
					string file = ((DemoAttribute) atts[0]).Filename;
					LoadFile (file);
				}

				model.SetValue (iter, 2, true);
				if (!oldSelection.Equals (TreeIter.Zero))
					model.SetValue (oldSelection, 2, false);
				oldSelection = iter;
			}
		}

		private void RowActivated (object o, RowActivatedArgs args)
		{
			TreeIter iter;

			if (treeView.Model.GetIter (out iter, args.Path)) {
				Type type = (Type) treeView.Model.GetValue (iter, 1);
				if (type != null)
					Activator.CreateInstance (type);
			}
		}

		private void WindowDelete (object o, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}
	}
}

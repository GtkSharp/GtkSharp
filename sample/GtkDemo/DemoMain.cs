//
// DemoMain.cs, port of main.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2003, Ximian Inc.

using System;
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
			notebook.AppendPage (CreateText (sourceBuffer, true), new Label ("_Source"));

			window.ShowAll ();
		}

		private void LoadFile (string filename)
		{
			Stream file = Assembly.GetExecutingAssembly ().GetManifestResourceStream (filename);
			if (file != null)
			{
				LoadStream (file, filename);
			}
			else if (File.Exists (filename))
			{
				file = File.OpenRead (filename);
				LoadStream (file, filename);
			}
			else
			{
				infoBuffer.Text = String.Format ("{0} was not found.", filename);
				sourceBuffer.Text = String.Empty;
			}

			Fontify ();
		}

		private void LoadStream (Stream file, string filename)
		{
			StreamReader sr = new StreamReader (file);
			string s = sr.ReadToEnd ();
			sr.Close ();
			file.Close ();

			infoBuffer.Text = filename;
			sourceBuffer.Text = s;
		}

		// this is to highlight the sourceBuffer
		private void Fontify ()
		{
		}

		private void SetupDefaultIcon ()
		{
			Gdk.Pixbuf pixbuf = Gdk.Pixbuf.LoadFromResource ("gtk-logo-rgb.gif");
		
			if (pixbuf != null)
			{
				// The gtk-logo-rgb icon has a white background
				// make it transparent instead
				Pixbuf transparent  = pixbuf.AddAlpha (true, 0xff, 0xff, 0xff);
			
				Gtk.Window.DefaultIconList = new Gdk.Pixbuf [] {transparent};
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
			
			view.Selection.Changed += new EventHandler (OnTreeChanged);
			view.RowActivated += new RowActivatedHandler (OnRowActivated);
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

			if (IsSource)
			{
				FontDescription fontDescription = FontDescription.FromString ("Courier 12");	
				textView.ModifyFont (fontDescription);
				textView.WrapMode = Gtk.WrapMode.None;
			}
			else
			{
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
        	store = new TreeStore (typeof (string), typeof (string), typeof (bool));
		TreeIter parent; 
       	
        	store.AppendValues ("Application Window", "DemoApplicationWindow.cs", false);
        	store.AppendValues ("Button Boxes", "DemoButtonBox.cs", false);
        	store.AppendValues ("Change Display (0%)", "DemoChangeDisplay.cs", false);
        	store.AppendValues ("Color Selector", "DemoColorSelection.cs", false);
        	store.AppendValues ("Dialog and Message Boxes", "DemoDialog.cs", false);
        	store.AppendValues ("Drawing Area", "DemoDrawingArea.cs", false);
        	store.AppendValues ("Images", "DemoImages.cs", false);
        	store.AppendValues ("Item Factory (5% complete)", "DemoItemFactory.cs", false);
        	store.AppendValues ("Menus", "DemoMenus.cs", false);
        	store.AppendValues ("Paned Widget", "DemoPanes.cs", false);
        	store.AppendValues ("Pixbuf", "DemoPixbuf.cs", false);
        	store.AppendValues ("Size Groups", "DemoSizeGroup.cs", false);
        	store.AppendValues ("Stock Item and Icon Browser (10% complete)", "DemoStockBrowser.cs", false);
        	parent = store.AppendValues ("Text Widget");
		store.AppendValues (parent, "HyperText (50%)", "DemoHyperText.cs", false);
		store.AppendValues (parent, "Multiple Views", "DemoTextView.cs", false);
        	parent = store.AppendValues ("Tree View");
        	store.AppendValues (parent, "Editable Cells", "DemoEditableCells.cs", false);
        	store.AppendValues (parent, "List Store", "DemoListStore.cs", false);
        	store.AppendValues (parent, "Tree Store", "DemoTreeStore.cs", false);
        	
        	return store;
        }
        
	//FIXME: italicize selected row
        private void OnTreeChanged (object o, EventArgs args)
	{
		TreeIter iter;
		TreeModel model;

		if (treeView.Selection.GetSelected (out model, out iter))
		{
			string file = (string) model.GetValue (iter, 1);
			if (file != null)
        			LoadFile (file);

			model.SetValue (iter, 2, true);
			if (!oldSelection.Equals (TreeIter.Zero))
				model.SetValue (oldSelection, 2, false);
			oldSelection = iter;
		}
        }

        private void OnRowActivated (object o, RowActivatedArgs args)
        {
        	switch (args.Path.ToString ()) {
        		case "0":
        			new DemoApplicationWindow ();
        			break;
        		case "1":
        			new DemoButtonBox ();
        			break;
        		case "2":
        			//
        			break;
        		case "3":
        			new DemoColorSelection ();
        			break;
        		case "4":
        			new DemoDialog ();
        			break;
        		case "5":
        			new DemoDrawingArea ();
        			break;
        		case "6":
        			new DemoImages ();
        			break;
        		case "7":
        			new DemoItemFactory ();
        			break;
        		case "8":
        			new DemoMenus ();
        			break;
        		case "9":
        			new DemoPanes ();
        			break;
        		case "10":
        			new DemoPixbuf ();
        			break;
        		case "11":
        			new DemoSizeGroup ();
        			break;
        		case "12":
        			new DemoStockBrowser ();
        			break;
        		case "13":
				ToggleRow (args.Path);
				break;
        		case "13:0":
        			new DemoHyperText ();
        			break;
        		case "13:1":
        			new DemoTextView ();
        			break;
        		case "14":
				ToggleRow (args.Path);
        			break;
        		case "14:0":
        			new DemoEditableCells ();
        			break;
        		case "14:1":
        			new DemoListStore ();
        			break;
        		case "14:2":
        			new DemoTreeStore ();
        			break;
        		default:
        			break;
		}
        }

		void ToggleRow (TreePath path)
		{
			bool isExpanded = treeView.GetRowExpanded (path);
			if (isExpanded)
				treeView.CollapseRow (path);
			else
				treeView.ExpandRow (path, false);
		}

		private void WindowDelete (object o, DeleteEventArgs args)
		{
			Application.Quit ();
			args.RetVal = true;
		}
	}
}


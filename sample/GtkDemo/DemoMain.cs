//
// DemoMain.cs, port of main.c from gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2003, Ximian Inc.

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
        	store = new TreeStore (typeof (string), typeof (System.Type), typeof (bool));
			Hashtable parents = new Hashtable ();
			TreeIter parent;
       	
			Type[] types = Assembly.GetExecutingAssembly ().GetTypes ();
			foreach (Type t in types)
			{
				if (t.IsDefined (typeof (DemoAttribute), false))
				{
					object[] att = t.GetCustomAttributes (typeof (DemoAttribute), false);
					foreach (DemoAttribute demo in att)
					{
						if (demo.Parent != null)
						{
							if (!parents.Contains (demo.Parent))
								parents.Add (demo.Parent, store.AppendValues (demo.Parent));

							parent = (TreeIter) parents[demo.Parent];
							store.AppendValues (parent, demo.Label, t, false);
						}
						else
						{
							store.AppendValues (demo.Label, t, false);
						}
					}
				}
			}
			store.SetSortColumnId (0, SortType.Ascending);
        	return store;
        }
        
        private void OnTreeChanged (object o, EventArgs args)
		{
			TreeIter iter;
			TreeModel model;

			if (treeView.Selection.GetSelected (out model, out iter))
			{
				Type type = (Type) model.GetValue (iter, 1);
				if (type != null)
				{
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

        private void OnRowActivated (object o, RowActivatedArgs args)
        {
			TreeIter iter;

			if (treeView.Model.GetIter (out iter, args.Path))
			{
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


// Gtk.FileChooserNative.cs - Gtk FileChooserNative class customizations
//
// Author: Mikkel Kruse Johnsen <mikkel@xmedicus.com>
//
// Copyright (c) 2016 XMedicus ApS
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

namespace Gtk {
	using GLib;
	using System;
	using System.Runtime.InteropServices;

	public partial class FileChooserNative : Gtk.NativeDialog, Gtk.IFileChooser {
		private FileChooserAdapter fileChooser;

		public FileChooserNative (IntPtr raw) : base(raw)
		{
			fileChooser = new FileChooserAdapter(raw);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_file_chooser_native_new(IntPtr title, IntPtr parent, int action, IntPtr accept_label, IntPtr cancel_label);
		static readonly d_gtk_file_chooser_native_new gtk_file_chooser_native_new = FuncLoader.LoadFunction<d_gtk_file_chooser_native_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_file_chooser_native_new"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate string d_gtk_file_chooser_native_get_accept_label(IntPtr self);
		static readonly d_gtk_file_chooser_native_get_accept_label gtk_file_chooser_native_get_accept_label = FuncLoader.LoadFunction<d_gtk_file_chooser_native_get_accept_label>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_file_chooser_native_get_accept_label"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate string d_gtk_file_chooser_native_set_accept_label(IntPtr self, string accept_label);
		static readonly d_gtk_file_chooser_native_set_accept_label gtk_file_chooser_native_set_accept_label = FuncLoader.LoadFunction<d_gtk_file_chooser_native_set_accept_label>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_file_chooser_native_set_accept_label"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate string d_gtk_file_chooser_native_get_cancel_label(IntPtr self);
		static readonly d_gtk_file_chooser_native_get_cancel_label gtk_file_chooser_native_get_cancel_label = FuncLoader.LoadFunction<d_gtk_file_chooser_native_get_cancel_label>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_file_chooser_native_get_cancel_label"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate string d_gtk_file_chooser_native_set_cancel_label(IntPtr self, string cancel_label);
		static readonly d_gtk_file_chooser_native_set_cancel_label gtk_file_chooser_native_set_cancel_label = FuncLoader.LoadFunction<d_gtk_file_chooser_native_set_cancel_label>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_file_chooser_native_set_cancel_label"));

		public string CurrentFolder => fileChooser.CurrentFolder;

		public IFile CurrentFolderFile => fileChooser.CurrentFolderFile;

		public string CurrentFolderUri => fileChooser.CurrentFolderUri;

		public string CurrentName { get => fileChooser.CurrentName; set => fileChooser.CurrentName = value; }

		public IFile File => fileChooser.File;

		public string Filename => fileChooser.Filename;

		public string[] Filenames => fileChooser.Filenames;

		public IFile[] Files => fileChooser.Files;

		public IFile PreviewFile => fileChooser.PreviewFile;

		public string PreviewFilename => fileChooser.PreviewFilename;

		public string PreviewUri => fileChooser.PreviewUri;

		public string Uri => fileChooser.Uri;

		public string[] Uris => fileChooser.Uris;

		public FileFilter[] Filters => fileChooser.Filters;

		public string[] ShortcutFolderUris => fileChooser.ShortcutFolderUris;

		public string[] ShortcutFolders => fileChooser.ShortcutFolders;

		public FileChooserAction Action { get => fileChooser.Action; set => fileChooser.Action = value; }
		public FileFilter Filter { get => fileChooser.Filter; set => fileChooser.Filter = value; }
		public bool LocalOnly { get => fileChooser.LocalOnly; set => fileChooser.LocalOnly = value; }
		public Widget PreviewWidget { get => fileChooser.PreviewWidget; set => fileChooser.PreviewWidget = value; }
		public bool PreviewWidgetActive { get => fileChooser.PreviewWidgetActive; set => fileChooser.PreviewWidgetActive = value; }
		public bool UsePreviewLabel { get => fileChooser.UsePreviewLabel; set => fileChooser.UsePreviewLabel = value; }
		public Widget ExtraWidget { get => fileChooser.ExtraWidget; set => fileChooser.ExtraWidget = value; }
		public bool SelectMultiple { get => fileChooser.SelectMultiple; set => fileChooser.SelectMultiple = value; }
		public bool ShowHidden { get => fileChooser.ShowHidden; set => fileChooser.ShowHidden = value; }
		public bool DoOverwriteConfirmation { get => fileChooser.DoOverwriteConfirmation; set => fileChooser.DoOverwriteConfirmation = value; }
		public bool CreateFolders { get => fileChooser.CreateFolders; set => fileChooser.CreateFolders = value; }

		public FileChooserNative (string title, Gtk.Window parent, Gtk.FileChooserAction action, string accept_label, string cancel_label) : base(FileChooserNativeCreate(title, parent, action, accept_label, cancel_label))
		{
			/*
			if (GetType () != typeof (FileChooserNative)) {
				var vals = new List<GLib.Value> ();
				var names = new List<string> ();
				names.Add ("title");
				vals.Add (new GLib.Value (title));
				names.Add ("parent");
				vals.Add (new GLib.Value (parent));
				names.Add ("action");
				vals.Add (new GLib.Value (action));
				names.Add ("accept_label");
				vals.Add (new GLib.Value (accept_label));
				names.Add ("cancel_label");
				vals.Add (new GLib.Value (cancel_label));
				CreateNativeObject (names.ToArray (), vals.ToArray ());
				return;
			}
			*/
			fileChooser = new FileChooserAdapter(Handle);
		}

		public event EventHandler FileActivated
		{
			add
			{
				fileChooser.FileActivated += value;
			}

			remove
			{
				fileChooser.FileActivated -= value;
			}
		}

		public event EventHandler SelectionChanged
		{
			add
			{
				fileChooser.SelectionChanged += value;
			}

			remove
			{
				fileChooser.SelectionChanged -= value;
			}
		}

		public event EventHandler CurrentFolderChanged
		{
			add
			{
				fileChooser.CurrentFolderChanged += value;
			}

			remove
			{
				fileChooser.CurrentFolderChanged -= value;
			}
		}

		public event ConfirmOverwriteHandler ConfirmOverwrite
		{
			add
			{
				fileChooser.ConfirmOverwrite += value;
			}

			remove
			{
				fileChooser.ConfirmOverwrite -= value;
			}
		}

		public event EventHandler UpdatePreview
		{
			add
			{
				fileChooser.UpdatePreview += value;
			}

			remove
			{
				fileChooser.UpdatePreview -= value;
			}
		}

		static IntPtr FileChooserNativeCreate (string title, Gtk.Window parent, Gtk.FileChooserAction action, string accept_label, string cancel_label)
		{
			IntPtr native_title = GLib.Marshaller.StringToPtrGStrdup(title);
			IntPtr native_accept_label = GLib.Marshaller.StringToPtrGStrdup(accept_label);
			IntPtr native_cancel_label = GLib.Marshaller.StringToPtrGStrdup(cancel_label);

			IntPtr raw = gtk_file_chooser_native_new(native_title, parent != null ? parent.Handle : IntPtr.Zero, (int) action, native_accept_label, native_cancel_label);

			GLib.Marshaller.Free(native_title);
			GLib.Marshaller.Free(native_accept_label);
			GLib.Marshaller.Free(native_cancel_label);

			return raw;
		}

		public void AddChoice(string id, string label, string options, string option_labels)
		{
			fileChooser.AddChoice(id, label, options, option_labels);
		}

		public void AddFilter(FileFilter filter)
		{
			fileChooser.AddFilter(filter);
		}

		public bool AddShortcutFolder(string folder)
		{
			return fileChooser.AddShortcutFolder(folder);
		}

		public bool AddShortcutFolderUri(string uri)
		{
			return fileChooser.AddShortcutFolderUri(uri);
		}

		public string GetChoice(string id)
		{
			return fileChooser.GetChoice(id);
		}

		public void RemoveChoice(string id)
		{
			fileChooser.RemoveChoice(id);
		}

		public void RemoveFilter(FileFilter filter)
		{
			fileChooser.RemoveFilter(filter);
		}

		public bool RemoveShortcutFolder(string folder)
		{
			return fileChooser.RemoveShortcutFolder(folder);
		}

		public bool RemoveShortcutFolderUri(string uri)
		{
			return fileChooser.RemoveShortcutFolderUri(uri);
		}

		public void SelectAll()
		{
			fileChooser.SelectAll();
		}

		public bool SelectFile(IFile file)
		{
			return fileChooser.SelectFile(file);
		}

		public bool SelectFilename(string filename)
		{
			return fileChooser.SelectFilename(filename);
		}

		public bool SelectUri(string uri)
		{
			return fileChooser.SelectUri(uri);
		}

		public void SetChoice(string id, string option)
		{
			fileChooser.SetChoice(id, option);
		}

		public bool SetCurrentFolder(string filename)
		{
			return fileChooser.SetCurrentFolder(filename);
		}

		public bool SetCurrentFolderFile(IFile file)
		{
			return fileChooser.SetCurrentFolderFile(file);
		}

		public bool SetCurrentFolderUri(string uri)
		{
			return fileChooser.SetCurrentFolderUri(uri);
		}

		public bool SetFile(IFile file)
		{
			return fileChooser.SetFile(file);
		}

		public bool SetFilename(string filename)
		{
			return fileChooser.SetFilename(filename);
		}

		public bool SetUri(string uri)
		{
			return fileChooser.SetUri(uri);
		}

		public void UnselectAll()
		{
			fileChooser.UnselectAll();
		}

		public void UnselectFile(IFile file)
		{
			fileChooser.UnselectFile(file);
		}

		public void UnselectFilename(string filename)
		{
			fileChooser.UnselectFilename(filename);
		}

		public void UnselectUri(string uri)
		{
			fileChooser.UnselectUri(uri);
		}
	}
}

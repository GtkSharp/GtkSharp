// Image.cs - Customizations to Gtk.Image
//
// Authors: Mike Kestner  <mkestner@novell.com>
// Authors: Stephane Delcroix  <sdelcroix@novell.com>
//
// Copyright (c) 2004-2008 Novell, Inc.
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

	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	public partial class Image {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_image_new_from_icon_set(IntPtr icon_set, int size);
		static d_gtk_image_new_from_icon_set gtk_image_new_from_icon_set = FuncLoader.LoadFunction<d_gtk_image_new_from_icon_set>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_image_new_from_icon_set"));

		public Image (Gtk.IconSet icon_set, Gtk.IconSize size) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (Image)) {
				var vals = new List<GLib.Value> ();
				var names = new List<string> ();
				names.Add ("icon_set");
				vals.Add (new GLib.Value (icon_set));
				names.Add ("icon_size");
				vals.Add (new GLib.Value ((int)size));
				CreateNativeObject (names.ToArray (), vals.ToArray ());
				return;
			}
			Raw = gtk_image_new_from_icon_set(icon_set.Handle, (int) size);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_image_new_from_stock(IntPtr stock_id, int size);
		static d_gtk_image_new_from_stock gtk_image_new_from_stock = FuncLoader.LoadFunction<d_gtk_image_new_from_stock>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_image_new_from_stock"));

		public Image (string stock_id, Gtk.IconSize size) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (Image)) {
				var vals = new List<GLib.Value> ();
				var names = new List<string> ();
				names.Add ("stock");
				vals.Add (new GLib.Value (stock_id));
				names.Add ("icon_size");
				vals.Add (new GLib.Value ((int)size));
				CreateNativeObject (names.ToArray (), vals.ToArray ());
				return;
			}
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (stock_id);
			Raw = gtk_image_new_from_stock(native, (int) size);
			GLib.Marshaller.Free (native);
		}

		void LoadFromStream (System.IO.Stream stream)
		{
			try {
				Gdk.PixbufAnimation anim = new Gdk.PixbufAnimation (stream);
				if (anim.IsStaticImage)
					Pixbuf = anim.StaticImage;
				else
					PixbufAnimation = anim;
			} catch {
				Stock = Gtk.Stock.MissingImage;
			}
		}

		public Image (System.IO.Stream stream) : this ()
		{
			LoadFromStream (stream);
		}

		public Image (System.Reflection.Assembly assembly, string resource) : this ()
		{
			if (assembly == null)
				assembly = System.Reflection.Assembly.GetCallingAssembly ();

			System.IO.Stream s = assembly.GetManifestResourceStream (resource);
			if (s == null)
				throw new ArgumentException ("'" + resource + "' is not a valid resource name of assembly '" + assembly + "'.");

			LoadFromStream (s);
		}

		static public Image LoadFromResource (string resource)
		{
			return new Image (System.Reflection.Assembly.GetCallingAssembly (), resource);
		}

		[Obsolete ("Use the Animation property instead")]
		public Gdk.PixbufAnimation FromAnimation {
			set {
				gtk_image_set_from_animation(Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}

		[Obsolete ("Use the File property instead")]
		public string FromFile {
			set {
				IntPtr native_value = GLib.Marshaller.StringToPtrGStrdup (value);
				gtk_image_set_from_file(Handle, native_value);
				GLib.Marshaller.Free (native_value);
			}
		}

		[Obsolete ("Use the Pixbuf property instead")]
		public Gdk.Pixbuf FromPixbuf {
			set {
				gtk_image_set_from_pixbuf(Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}
	}
}


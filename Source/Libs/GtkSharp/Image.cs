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
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;

	public partial class Image {
		// Image(stock_id, size): the stock registry is gone in Gtk 4; use the generated NewFromIconName.

		void LoadFromStream (System.IO.Stream stream)
		{
			// Gtk 4's GtkImage has no animation and no stock support: animated
			// content is a GdkPaintable now, and stock items are gone entirely.
			try {
				Pixbuf = new Gdk.Pixbuf (stream);
			} catch {
				IconName = "image-missing";
			}
		}

		public Image (System.IO.Stream stream) : this ()
		{
			LoadFromStream (stream);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public Image (System.Reflection.Assembly assembly, string resource) : this ()
		{
			if (assembly == null)
				assembly = System.Reflection.Assembly.GetCallingAssembly ();

			System.IO.Stream s = assembly.GetManifestResourceStream (resource);
			if (s == null)
				throw new ArgumentException ("'" + resource + "' is not a valid resource name of assembly '" + assembly + "'.");

			LoadFromStream (s);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static public Image LoadFromResource (string resource)
		{
			return new Image (System.Reflection.Assembly.GetCallingAssembly (), resource);
		}

		// FromAnimation: gtk_image_set_from_animation is gone; Gtk 4 animates through GdkPaintable.

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


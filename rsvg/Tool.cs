// rsvg/Tool.cs - Rsvg Tool class
//
// Author: Charles Iliya Krempeaux <charles@reptile.ca>
//         Mike Kestner  <mkestner@novell.com>
//
// Copyright (C) 2003 Reptile Consulting & Services Ltd.
// Copyright (C) 2003 Charles Iliya Krempeaux.
// Copyright (C) 2005 Novell, Inc.
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



namespace Rsvg {

	using System;
	using System.Runtime.InteropServices;

	public class Tool {

		[DllImport("rsvg-2")]
		static extern IntPtr rsvg_pixbuf_from_file (IntPtr file_name, out IntPtr error);

		[DllImport("rsvg-2")]
		static extern IntPtr rsvg_pixbuf_from_file_at_zoom (IntPtr file_name, double x_zoom, double y_zoom, out IntPtr error);

		[DllImport("rsvg-2")]
		static extern IntPtr rsvg_pixbuf_from_file_at_size (IntPtr file_name, int  width, int  height, out IntPtr error);

		[DllImport("rsvg-2")]
		static extern IntPtr rsvg_pixbuf_from_file_at_max_size (IntPtr file_name, int max_width, int max_height, out IntPtr error);

		[DllImport("rsvg-2")]
		static extern IntPtr rsvg_pixbuf_from_file_at_zoom_with_max (IntPtr file_name, double x_zoom, double y_zoom, int max_width, int max_height, out IntPtr error);


		public static Gdk.Pixbuf PixbufFromFile (string file_name)
		{
			IntPtr error = IntPtr.Zero;
			IntPtr native_filename = GLib.Marshaller.StringToPtrGStrdup (file_name);
			IntPtr raw_pixbuf = rsvg_pixbuf_from_file(native_filename, out error);
			GLib.Marshaller.Free (native_filename);

			if (IntPtr.Zero != error)
				throw new GLib.GException (error);

			return GLib.Object.GetObject (raw_pixbuf, true) as Gdk.Pixbuf;
		}

		public static Gdk.Pixbuf PixbufFromFileAtZoom (string file_name, double x_zoom, double y_zoom)
		{
			IntPtr error = IntPtr.Zero;
			IntPtr native_filename = GLib.Marshaller.StringToPtrGStrdup (file_name);
			IntPtr raw_pixbuf = rsvg_pixbuf_from_file_at_zoom (native_filename, x_zoom, y_zoom, out error);
			GLib.Marshaller.Free (native_filename);

			if (IntPtr.Zero != error)
				throw new GLib.GException (error);

			return GLib.Object.GetObject (raw_pixbuf, true) as Gdk.Pixbuf;
		}

		public static Gdk.Pixbuf PixbufFromFileAtSize(string file_name, int width, int height)
		{
			IntPtr error = IntPtr.Zero;
			IntPtr native_filename = GLib.Marshaller.StringToPtrGStrdup (file_name);
			IntPtr raw_pixbuf = rsvg_pixbuf_from_file_at_size (native_filename, width, height, out error);
			GLib.Marshaller.Free (native_filename);

			if (IntPtr.Zero != error)
				throw new GLib.GException (error);

			return GLib.Object.GetObject (raw_pixbuf, true) as Gdk.Pixbuf;
		}

		public static Gdk.Pixbuf PixbufFromFileAtMaxSize(string file_name, int max_width, int max_height)
		{
			IntPtr error = IntPtr.Zero;
			IntPtr native_filename = GLib.Marshaller.StringToPtrGStrdup (file_name);
			IntPtr raw_pixbuf = rsvg_pixbuf_from_file_at_max_size (native_filename, max_width, max_height, out error);
			GLib.Marshaller.Free (native_filename);

			if (IntPtr.Zero != error)
				throw new GLib.GException (error);

			return GLib.Object.GetObject (raw_pixbuf, true) as Gdk.Pixbuf;
		}

		public static Gdk.Pixbuf PixbufFromFileAtZoomWithMaxSize(string file_name, double x_zoom, double y_zoom, int max_width, int max_height)
		{
			IntPtr error = IntPtr.Zero;
			IntPtr native_filename = GLib.Marshaller.StringToPtrGStrdup (file_name);
			IntPtr raw_pixbuf = rsvg_pixbuf_from_file_at_zoom_with_max (native_filename, x_zoom, y_zoom, max_width, max_height, out error);
			GLib.Marshaller.Free (native_filename);

			if (IntPtr.Zero != error)
				throw new GLib.GException (error);

			return GLib.Object.GetObject (raw_pixbuf, true) as Gdk.Pixbuf;
		}
	}
}



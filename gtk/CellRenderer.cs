//
// CellRenderer.cs - Gtk CellRenderer class customizations
//
// Author: Todd Berman <tberman@sevenl.net>,
//         Peter Johanson <peter@peterjohanson.com>
//
// Copyright (C) 2004 Todd Berman
// Copyright (C) 2007 Peter Johanson
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
	using System.Runtime.InteropServices;

	public partial class CellRenderer {

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_cell_renderer_start_editing (IntPtr handle, IntPtr evnt, IntPtr widget, IntPtr path, ref Gdk.Rectangle bg_area, ref Gdk.Rectangle cell_area, int flags);

		public ICellEditable StartEditing (Widget widget, Gdk.Event evnt, string path, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, CellRendererState flags)
		{
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (path);
			IntPtr raw_ret = gtk_cell_renderer_start_editing (Handle, evnt.Handle, widget.Handle, native, ref background_area, ref cell_area, (int) flags);
			GLib.Marshaller.Free (native);
			var ret = (ICellEditable) GLib.Object.GetObject (raw_ret);
			return ret;
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_cell_renderer_render (IntPtr handle, IntPtr drawable, IntPtr widget, ref Gdk.Rectangle bg_area, ref Gdk.Rectangle cell_area, ref Gdk.Rectangle expose_area, int flags);
		
		public void Render (Cairo.Context context, Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, CellRendererState flags)
		{
			gtk_cell_renderer_render (Handle, context == null ? IntPtr.Zero : context.Handle, widget == null ? IntPtr.Zero : widget.Handle, ref background_area, ref cell_area, ref expose_area, (int) flags);
		}

		// We have to implement this VM manually because x_offset, y_offset, width and height params may be NULL and therefore cannot be treated as "out int"
		// TODO: Implement "nullable" attribute for value type parameters in GAPI
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void OnGetSizeDelegate (IntPtr item, IntPtr widget, IntPtr cell_area_ptr, IntPtr x_offset, IntPtr y_offset, IntPtr width, IntPtr height);

		static void OnGetSize_cb (IntPtr item, IntPtr widget, IntPtr cell_area_ptr, IntPtr x_offset, IntPtr y_offset, IntPtr width, IntPtr height)
		{
			try {
				CellRenderer obj = GLib.Object.GetObject (item, false) as CellRenderer;
				Gtk.Widget widg = GLib.Object.GetObject (widget, false) as Gtk.Widget;
				Gdk.Rectangle cell_area = Gdk.Rectangle.Zero;
				if (cell_area_ptr != IntPtr.Zero)
					cell_area = Gdk.Rectangle.New (cell_area_ptr);
				int a, b, c, d;

				obj.OnGetSize (widg, ref cell_area, out a, out b, out c, out d);
				if (x_offset != IntPtr.Zero)
					Marshal.WriteInt32 (x_offset, a);
				if (y_offset != IntPtr.Zero)
					Marshal.WriteInt32 (y_offset, b);
				if (width != IntPtr.Zero)
					Marshal.WriteInt32 (width, c);
				if (height != IntPtr.Zero)
					Marshal.WriteInt32 (height, d);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		[DllImport("gtksharpglue-3")]
		static extern void gtksharp_cellrenderer_override_get_size (IntPtr gtype, OnGetSizeDelegate cb);

		static OnGetSizeDelegate OnGetSizeCallback;
		static void OverrideOnGetSize (GLib.GType gtype)
		{
			if (OnGetSizeCallback == null)
				OnGetSizeCallback = new OnGetSizeDelegate (OnGetSize_cb);
			gtksharp_cellrenderer_override_get_size (gtype.Val, OnGetSizeCallback);
		}

		[GLib.DefaultSignalHandler (Type=typeof(Gtk.CellRenderer), ConnectionMethod="OverrideOnGetSize")] 
		protected virtual void OnGetSize (Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height) 
		{
			InternalOnGetSize (widget, ref cell_area, out x_offset, out y_offset, out width, out height);
		}

		[DllImport("gtksharpglue-3")]
		static extern void gtksharp_cellrenderer_base_get_size (IntPtr cell, IntPtr widget, IntPtr cell_area, out int x_offset, out int y_offset, out int width, out int height);

		private void InternalOnGetSize (Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height) 
		{
			IntPtr native_cell_area = GLib.Marshaller.StructureToPtrAlloc (cell_area);
			gtksharp_cellrenderer_base_get_size (Handle, widget == null ? IntPtr.Zero : widget.Handle, native_cell_area, out x_offset, out y_offset, out width, out height);
			cell_area = Gdk.Rectangle.New (native_cell_area);
			Marshal.FreeHGlobal (native_cell_area);
		}
	}
}

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

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_cell_renderer_start_editing (IntPtr handle, IntPtr evnt, IntPtr widget, IntPtr path, ref Gdk.Rectangle bg_area, ref Gdk.Rectangle cell_area, int flags);

		public ICellEditable StartEditing (Widget widget, Gdk.Event evnt, string path, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, CellRendererState flags)
		{
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (path);
			IntPtr raw_ret = gtk_cell_renderer_start_editing (Handle, evnt != null ? evnt.Handle : IntPtr.Zero, widget.Handle, native, ref background_area, ref cell_area, (int) flags);
			GLib.Marshaller.Free (native);
			var ret = (ICellEditable) GLib.Object.GetObject (raw_ret);
			return ret;
		}

		[DllImport (Global.GtkNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_cell_renderer_render (IntPtr handle, IntPtr drawable, IntPtr widget, ref Gdk.Rectangle bg_area, ref Gdk.Rectangle cell_area, ref Gdk.Rectangle expose_area, int flags);
		
		public void Render (Cairo.Context context, Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, CellRendererState flags)
		{
			gtk_cell_renderer_render (Handle, context == null ? IntPtr.Zero : context.Handle, widget == null ? IntPtr.Zero : widget.Handle, ref background_area, ref cell_area, ref expose_area, (int) flags);
		}

		// We have to implement this VM manually because x_offset, y_offset, width and height params may be NULL and therefore cannot be treated as "out int"
		// TODO: Implement "nullable" attribute for value type parameters in GAPI
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void OnGetSizeDelegate (IntPtr item, IntPtr widget, IntPtr cell_area_ptr, IntPtr x_offset, IntPtr y_offset, IntPtr width, IntPtr height);
	}
}

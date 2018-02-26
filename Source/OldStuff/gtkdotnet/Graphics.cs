// Graphics.cs - System.Drawing integration with Gtk#
//
// Author:  Miguel de Icaza  <miguel@novell.com>
//
// Copyright (c) 2004 Novell, Inc.
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


// API issues:
//    Maybe make the translation `out' parameters so they are explicit and the user knows about it?
//    Add a way to copy a Graphics into a drawable?
//

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Gtk.DotNet {
	public class Graphics {

		private const string GdkNativeDll = "libgdk-3-0.dll";
		
		private Graphics () {}

		[DllImport (GdkNativeDll, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gdk_win32_drawable_get_handle(IntPtr raw);

		[DllImport (GdkNativeDll, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gdk_win32_hdc_get(IntPtr drawable, IntPtr gc, int usage);
		
		[DllImport (GdkNativeDll, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gdk_win32_hdc_release(IntPtr drawable,IntPtr gc,int usage);

		[DllImport (GdkNativeDll, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gdk_x11_drawable_get_xdisplay (IntPtr raw);
		
		[DllImport (GdkNativeDll, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr gdk_x11_drawable_get_xid (IntPtr raw);
		
		public static System.Drawing.Graphics FromDrawable (Gdk.Window drawable)
		{
			return FromDrawable (drawable, true);
		}

		public static System.Drawing.Graphics FromDrawable(Gdk.Window drawable, bool double_buffered)
		{
#if FIXME30
			IntPtr x_drawable;
			int x_off = 0, y_off = 0;
			
			PlatformID osversion = Environment.OSVersion.Platform;

			if (osversion == PlatformID.Win32Windows || osversion == PlatformID.Win32NT ||
			    osversion == PlatformID.Win32S || osversion == PlatformID.WinCE){
				if (drawable is Gdk.Window && double_buffered)
				    ((Gdk.Window)drawable).GetInternalPaintInfo(out drawable, out x_off, out y_off);

				Cairo.Context gcc = new Gdk.GC(drawable);
				
				IntPtr windc = gdk_win32_hdc_get(drawable.Handle, gcc.Handle, 0);
				
				System.Drawing.Graphics g = System.Drawing.Graphics.FromHdc(windc);
				
				if (double_buffered) {
					gdk_win32_hdc_release(drawable.Handle, gcc.Handle, 0);
				}
				
				g.TranslateTransform(-x_off, -y_off);
				
				return g;
			} else {
				if (drawable is Gdk.Window && double_buffered)
					((Gdk.Window) drawable).GetInternalPaintInfo(out drawable, out x_off, out y_off);
				
				x_drawable = drawable.Handle;
				
				IntPtr display = gdk_x11_drawable_get_xdisplay (x_drawable);
				
				Type graphics = typeof (System.Drawing.Graphics);
				MethodInfo mi = graphics.GetMethod ("FromXDrawable", BindingFlags.Static | BindingFlags.NonPublic);
				if (mi == null)
					throw new NotImplementedException ("In this implementation I can not get a graphics from a drawable");
				object [] args = new object [2] { (IntPtr) gdk_x11_drawable_get_xid (drawable.Handle), (IntPtr) display };
				object r = mi.Invoke (null, args);
				System.Drawing.Graphics g = (System.Drawing.Graphics) r;
				
				g.TranslateTransform (-x_off, -y_off);
				
				return g;
			}
#else
			throw new NotSupportedException ();
#endif
		}
	}

}

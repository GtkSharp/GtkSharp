using System;
using Cairo;
using System.Runtime.InteropServices;

public class GtkCairo {

#region You can cut and paste this into your application
	[DllImport("libgdk-win32-2.0-0.dll")]
	static extern IntPtr gdk_x11_drawable_get_xdisplay (IntPtr raw);
	
	[DllImport("libgdk-win32-2.0-0.dll")]
	static extern IntPtr gdk_x11_drawable_get_xid (IntPtr raw);

	[DllImport("libgdk-win32-2.0-0.dll")]
	static extern void gdk_window_get_internal_paint_info(IntPtr raw, out IntPtr real_drawable, out int x_offset, out int y_offset);

	static public Cairo.Graphics GraphicsFromWindow (Gdk.Window window, out int offset_x, out int offset_y)
	{
                IntPtr real_drawable;
                Cairo.Graphics o = new Cairo.Graphics ();

                gdk_window_get_internal_paint_info (window.Handle, out real_drawable, out offset_x, out offset_y);
                IntPtr x11 = gdk_x11_drawable_get_xid (real_drawable);
                IntPtr display = gdk_x11_drawable_get_xdisplay (real_drawable);
                o.SetTargetDrawable (display, x11); 

                return o;
	}

	static public Cairo.Graphics GraphicsFromDrawable (Gdk.Drawable drawable)
	{
                Cairo.Graphics o = new Cairo.Graphics ();

                IntPtr display = gdk_x11_drawable_get_xdisplay (drawable.Handle);
                o.SetTargetDrawable (display, gdk_x11_drawable_get_xid (drawable.Handle));

                return o;
	}

	static GtkCairo ()
	{
		
	}
	
#endregion
}

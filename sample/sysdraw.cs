//
// System.Drawing integration with Gtk#
//
// Miguel de Icaza
//
// API issues:
//    Maybe make the translation `out' parameters so they are explicit and the user knows about it?
//    Add a way to copy a Graphics into a drawable?
//

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Gdk {
	public class Graphics {
		
		[DllImport("libgdk-win32-2.0-0.dll")]
		internal static extern IntPtr gdk_x11_drawable_get_xdisplay (IntPtr raw);
		
		[DllImport("libgdk-win32-2.0-0.dll")]
		internal static extern IntPtr gdk_x11_drawable_get_xid (IntPtr raw);
		
		public static System.Drawing.Graphics FromDrawable (Gdk.Drawable drawable)
		{
			IntPtr x_drawable;
			int x_off = 0, y_off = 0;
				
			
			if (drawable is Gdk.Window){
				((Gdk.Window) drawable).GetInternalPaintInfo(out drawable, out x_off, out y_off);
			} 
			x_drawable = drawable.Handle;
			
			IntPtr display = gdk_x11_drawable_get_xdisplay (x_drawable);
			
			Type graphics = typeof (System.Drawing.Graphics);
			MethodInfo mi = graphics.GetMethod ("FromXDrawable", BindingFlags.Static | BindingFlags.NonPublic);
			if (mi == null)
				throw new NotImplementedException ("In this implementation I can not get a graphics from a drawable");
			object [] args = new object [2] { (IntPtr) gdk_x11_drawable_get_xid (drawable.Handle), (IntPtr) display };
			object r = mi.Invoke (null, args);
			System.Drawing.Graphics g = (System.Drawing.Graphics) r;

			Console.WriteLine ("-> {0} / {1}", x_off, y_off);
			g.TranslateTransform (-x_off, -y_off);

			return g;
		}
	}

}

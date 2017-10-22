namespace Gdk {

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	public partial struct RGBA {


		public static explicit operator GLib.Value (Gdk.RGBA boxed)
		{
			GLib.Value val = GLib.Value.Empty;
			val.Init (Gdk.RGBA.GType);
			val.Val = boxed;
			return val;
		}

		public static explicit operator Gdk.RGBA (GLib.Value val)
		{
			return (Gdk.RGBA) val.Val;
		}
	}
}

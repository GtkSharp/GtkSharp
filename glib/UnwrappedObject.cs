// UnwrappedObject.cs - Class which holds an IntPtr without resolving it:
//
// Author: Rachel Hestilow <hestilow@ximian.com> 
//
// (c) 2002 Rachel Hestilow

namespace GLib {

	using System;
	using System.Runtime.InteropServices;
	
	// <summary> Unwrapped object class </summary>
	// <remarks> </remarks>
	public class UnwrappedObject {
		IntPtr obj;
		
		public UnwrappedObject (IntPtr obj) {
			this.obj = obj;
		}

		public static explicit operator System.IntPtr (UnwrappedObject obj) {
			return obj.obj;
		}
	}
}


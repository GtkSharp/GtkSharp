// EnumWrapper.cs - Class to hold arbitrary glib enums 
//
// Author: Rachel Hestilow <hestilow@ximian.com> 
//
// (c) 2002 Rachel Hestilow

namespace GLib {

	using System;
	using System.Runtime.InteropServices;
	
	// <summary> Enum wrapping class </summary>
	// <remarks> </remarks>
	public class EnumWrapper {
		int val;
		public bool flags;
		
		public EnumWrapper (int val, bool flags) {
			this.val = val;
			this.flags = flags;
		}

		public static explicit operator int (EnumWrapper wrap) {
			return wrap.val;
		}
	}
}


// SList.cs - GSList class wrapper implementation
//
// Authors: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002 Mike Kestner

namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	/// <summary>
	///	SList Class
	/// </summary>
	///
	/// <remarks>
	///	Wrapper class for GSList.
	/// </remarks>

	public class SList : ArrayList {

		/// <summary>
		///	Handle Property
		/// </summary>
		///
		/// <remarks>
		///	A raw GSList reference for marshaling situations.
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern IntPtr g_slist_append(IntPtr l, IntPtr d);

		public IntPtr Handle {
			get {
				IntPtr l = IntPtr.Zero;
				foreach (object o in this) {
					IntPtr data = IntPtr.Zero;
					if (o is GLib.Object)
						l = g_slist_append (l, ((GLib.Object)o).Handle);
					else
						throw new Exception();
				}
				return l;
			}
		}

	}
}

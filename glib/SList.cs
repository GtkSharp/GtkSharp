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

		private IntPtr list_ptr;

		[DllImport("gobject-2.0")]
		static extern void g_slist_free(IntPtr l);

		~SList ()
		{
			if (list_ptr != IntPtr.Zero)
				g_slist_free (list_ptr);
		}

		[DllImport("gobject-2.0")]
		static extern IntPtr g_slist_append(IntPtr l, IntPtr d);

		/// <summary>
		///	Handle Property
		/// </summary>
		///
		/// <remarks>
		///	A raw GSList reference for marshaling situations.
		/// </remarks>

		public IntPtr Handle {
			get {
				if (list_ptr != IntPtr.Zero)
					g_slist_free (list_ptr);

				IntPtr l = IntPtr.Zero;
				foreach (object o in this) {
					IntPtr data = IntPtr.Zero;
					if (o is GLib.Object)
						l = g_slist_append (l, ((GLib.Object)o).Handle);
					else
						throw new Exception();
				}

				list_ptr = l;
				return l;
			}
		}
		
		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_slist_get_data (IntPtr l);
		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_slist_get_next (IntPtr l);
		
		public SList (IntPtr raw)
		{
			IntPtr l = raw;
			while (l != IntPtr.Zero) {
				Add (gtksharp_slist_get_data (l));
				l = gtksharp_slist_get_next (l);
			}
		}

	}
}

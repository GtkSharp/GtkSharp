// SList.cs - GSList class wrapper implementation
//
// Authors: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002 Mike Kestner

namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	/// <summary>
	///	SList Class
	/// </summary>
	///
	/// <remarks>
	///	Wrapper class for GSList.
	/// </remarks>

	public class SList : ListBase {

		[DllImport("libglib-2.0-0.dll")]
		static extern IntPtr g_slist_copy (IntPtr l);
		
		public override object Clone ()
		{
			return new SList (g_slist_copy (Handle));
		}
		
		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_slist_get_data (IntPtr l);
		
		internal override IntPtr GetData (IntPtr current)
		{
			return gtksharp_slist_get_data (current);
		}

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_slist_get_next (IntPtr l);
		
		internal override IntPtr Next (IntPtr current)
		{
			return gtksharp_slist_get_next (current);
		}

		[DllImport("libglib-2.0-0.dll")]
		static extern int g_slist_length (IntPtr l);
		
		internal override int Length (IntPtr list)
		{
			return g_slist_length (list);
		}
		
		[DllImport("libglib-2.0-0.dll")]
		static extern void g_slist_free(IntPtr l);

		internal override void Free (IntPtr list)
		{
			if (list != IntPtr.Zero)
				g_slist_free (list);
		}

		[DllImport("libglib-2.0-0.dll")]
		static extern IntPtr g_slist_append (IntPtr l, IntPtr raw);

		internal override IntPtr Append (IntPtr list, IntPtr raw)
		{
			return g_slist_append (list, raw);
		}

		[DllImport("libglib-2.0-0.dll")]
		static extern IntPtr g_slist_prepend (IntPtr l, IntPtr raw);

		internal override IntPtr Prepend (IntPtr list, IntPtr raw)
		{
			return g_slist_prepend (list, raw);
		}


		[DllImport("libglib-2.0-0.dll")]
	        static extern IntPtr g_slist_nth_data (IntPtr l, uint n);

		internal override IntPtr NthData (uint n)
		{
			return g_slist_nth_data (Handle, n);
		}

		public SList (IntPtr raw) : base (raw)
		{
		}

		public SList (System.Type element_type) : base (IntPtr.Zero, element_type)
		{
		}

		public SList (IntPtr raw, System.Type element_type) : base (raw, element_type)
		{
		}
	}
}

// List.cs - GList class wrapper implementation
//
// Authors: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002 Mike Kestner

namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	/// <summary>
	///	List Class
	/// </summary>
	///
	/// <remarks>
	///	Wrapper class for GList.
	/// </remarks>

	public class List : ListBase {

		[DllImport("libglib-2.0-0.dll")]
		static extern IntPtr g_list_copy (IntPtr l);
		
		public override object Clone ()
		{
			return new List (g_list_copy (Handle));
		}
		
		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_list_get_data (IntPtr l);
		
		internal override IntPtr GetData (IntPtr current)
		{
			return gtksharp_list_get_data (current);
		}

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_list_get_next (IntPtr l);
		
		internal override IntPtr Next (IntPtr current)
		{
			return gtksharp_list_get_next (current);
		}

		[DllImport("libglib-2.0-0.dll")]
		static extern int g_list_length (IntPtr l);
		
		internal override int Length (IntPtr list)
		{
			return g_list_length (list);
		}
		
		[DllImport("libglib-2.0-0.dll")]
		static extern void g_list_free(IntPtr l);

		internal override void Free (IntPtr list)
		{
			if (list != IntPtr.Zero)
				g_list_free (list);
		}

		[DllImport("libglib-2.0-0.dll")]
		static extern IntPtr g_list_append (IntPtr l, IntPtr raw);

		internal override IntPtr Append (IntPtr list, IntPtr raw)
		{
			return g_list_append (list, raw);
		}

		[DllImport("libglib-2.0-0.dll")]
		static extern IntPtr g_list_prepend (IntPtr l, IntPtr raw);

		internal override IntPtr Prepend (IntPtr list, IntPtr raw)
		{
			return g_list_prepend (list, raw);
		}

		public List (IntPtr raw) : base (raw)
		{
		}

		public List (IntPtr raw, System.Type element_type) : base (raw, element_type)
		{
		}
	}
}

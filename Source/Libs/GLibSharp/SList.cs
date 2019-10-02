// SList.cs - GSList class wrapper implementation
//
// Authors: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2002 Mike Kestner
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


namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	public class SList : ListBase {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_slist_copy(IntPtr l);
		static d_g_slist_copy g_slist_copy = FuncLoader.LoadFunction<d_g_slist_copy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_slist_copy"));
		
		public override object Clone ()
		{
			return new SList (g_slist_copy (Handle));
		}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_slist_length(IntPtr l);
		static d_g_slist_length g_slist_length = FuncLoader.LoadFunction<d_g_slist_length>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_slist_length"));
		
		internal override int Length (IntPtr list)
		{
			return g_slist_length (list);
		}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_slist_free(IntPtr l);
		static d_g_slist_free g_slist_free = FuncLoader.LoadFunction<d_g_slist_free>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_slist_free"));

		internal override void Free (IntPtr list)
		{
			if (list != IntPtr.Zero)
				g_slist_free (list);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_slist_append(IntPtr l, IntPtr raw);
		static d_g_slist_append g_slist_append = FuncLoader.LoadFunction<d_g_slist_append>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_slist_append"));

		internal override IntPtr Append (IntPtr list, IntPtr raw)
		{
			return g_slist_append (list, raw);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_slist_prepend(IntPtr l, IntPtr raw);
		static d_g_slist_prepend g_slist_prepend = FuncLoader.LoadFunction<d_g_slist_prepend>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_slist_prepend"));

		internal override IntPtr Prepend (IntPtr list, IntPtr raw)
		{
			return g_slist_prepend (list, raw);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_slist_nth_data(IntPtr l, uint n);
		static d_g_slist_nth_data g_slist_nth_data = FuncLoader.LoadFunction<d_g_slist_nth_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_slist_nth_data"));

		internal override IntPtr NthData (uint n)
		{
			return g_slist_nth_data (Handle, n);
		}

		public SList (IntPtr raw) : this (raw, null) {}

		public SList (System.Type element_type) : this (IntPtr.Zero, element_type) {}

		public SList (IntPtr raw, System.Type element_type) : this (raw, element_type, false, false) {}

		public SList (IntPtr raw, System.Type element_type, bool owned, bool elements_owned) : base (raw, element_type, false, false) {}

		public SList (object[] members, System.Type element_type, bool owned, bool elements_owned) : this (IntPtr.Zero, element_type, owned, elements_owned)
		{
			foreach (object o in members)
				Append (o);
		}

		public SList (Array members, System.Type element_type, bool owned, bool elements_owned) : this (IntPtr.Zero, element_type, owned, elements_owned)
		{
			foreach (object o in members)
				Append (o);
		}
	}
}


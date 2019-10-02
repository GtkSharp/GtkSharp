// PtrArray.cs - PtrArray wrapper implementation
//
// Authors: Mike Gorse <mgorse@novell.com>
//
// Copyright (c) 2008 Novell, Inc.
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
	using System.Collections;
	using System.Runtime.InteropServices;

	public class PtrArray : IDisposable, ICollection, ICloneable, IWrapper {

		private IntPtr handle = IntPtr.Zero;
		private bool managed = false;
		internal bool elements_owned = false;
		protected System.Type element_type = null;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_ptr_array_sized_new(uint n_preallocs);
		static d_g_ptr_array_sized_new g_ptr_array_sized_new = FuncLoader.LoadFunction<d_g_ptr_array_sized_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_ptr_array_sized_new"));

		public PtrArray (uint n_preallocs, System.Type element_type, bool owned, bool elements_owned)
		{
			handle = g_ptr_array_sized_new (n_preallocs);
			this.element_type = element_type;
			managed = owned;
			this.elements_owned = elements_owned;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_ptr_array_new();
		static d_g_ptr_array_new g_ptr_array_new = FuncLoader.LoadFunction<d_g_ptr_array_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_ptr_array_new"));

		public PtrArray (System.Type element_type, bool owned, bool elements_owned)
		{
			handle = g_ptr_array_new ();
			this.element_type = element_type;
			managed = owned;
			this.elements_owned = elements_owned;
		}

		internal PtrArray (IntPtr raw, System.Type element_type, bool owned, bool elements_owned)
		{
			handle = raw;
			this.element_type = element_type;
			managed = owned;
			this.elements_owned = elements_owned;
		}
		public PtrArray (IntPtr raw, System.Type element_type) : this (raw, element_type, false, false) {}
		
		public PtrArray (IntPtr raw) : this (raw, null) {}

		~PtrArray ()
		{
			Dispose (false);
		}

		// IDisposable
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_ptr_array_free(IntPtr raw, bool free_seg);
		static d_g_ptr_array_free g_ptr_array_free = FuncLoader.LoadFunction<d_g_ptr_array_free>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_ptr_array_free"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_object_unref(IntPtr item);
		static d_g_object_unref g_object_unref = FuncLoader.LoadFunction<d_g_object_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_object_unref"));

		void Dispose (bool disposing)
		{
			if (Handle == IntPtr.Zero)
				return;

			if (elements_owned) {
				int count = Count;
				for (uint i = 0; i < count; i++)
					if (typeof (GLib.Object).IsAssignableFrom (element_type))
						g_object_unref (NthData (i));
					else if (typeof (GLib.Opaque).IsAssignableFrom (element_type))
						GLib.Opaque.GetOpaque (NthData (i), element_type, true).Dispose ();
					else 
						Marshaller.Free (NthData (i));
			}

			if (managed)
				g_ptr_array_free (Handle, true);

			handle = IntPtr.Zero;
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public IntPtr ArrayPtr {
			get {
				return Marshal.ReadIntPtr (Handle);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_ptr_array_add(IntPtr raw, IntPtr val);
		static d_g_ptr_array_add g_ptr_array_add = FuncLoader.LoadFunction<d_g_ptr_array_add>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_ptr_array_add"));

		public void Add (IntPtr val)
		{
			g_ptr_array_add (Handle, val);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_ptr_array_remove(IntPtr raw, IntPtr data);
		static d_g_ptr_array_remove g_ptr_array_remove = FuncLoader.LoadFunction<d_g_ptr_array_remove>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_ptr_array_remove"));

		public void Remove (IntPtr data)
		{
			g_ptr_array_remove (Handle, data);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_ptr_array_remove_range(IntPtr raw, uint index, uint length);
		static d_g_ptr_array_remove_range g_ptr_array_remove_range = FuncLoader.LoadFunction<d_g_ptr_array_remove_range>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_ptr_array_remove_range"));

		public void RemoveRange (IntPtr data, uint index, uint length)
		{
			g_ptr_array_remove_range (Handle, index, length);
		}

		struct GPtrArray {
			public IntPtr pdata;
			public uint len;
		}

		// ICollection
		public int Count {
			get {
				GPtrArray native = (GPtrArray) Marshal.PtrToStructure (Handle, typeof (GPtrArray));
				return (int) native.len;
			}
		}

		public object this [int index] { 
			get {
				IntPtr data = NthData ((uint) index);
				object ret = null;
				ret = DataMarshal (data);
				return ret;
			}
		}

		internal object DataMarshal (IntPtr data) 
		{
			object ret = null;
			if (element_type != null) {
				if (element_type == typeof (string))
					ret = Marshaller.Utf8PtrToString (data);
				else if (element_type == typeof (IntPtr))
					ret = data;
				else if (element_type.IsSubclassOf (typeof (GLib.Object)))
					ret = GLib.Object.GetObject (data, false);
				else if (element_type.IsSubclassOf (typeof (GLib.Opaque)))
					ret = GLib.Opaque.GetOpaque (data, element_type, elements_owned);
				else if (element_type == typeof (int))
					ret = (int) data;
				else if (element_type.IsValueType)
					ret = Marshal.PtrToStructure (data, element_type);
				else
					ret = Activator.CreateInstance (element_type, new object[] {data});

			} else if (Object.IsObject (data))
				ret = GLib.Object.GetObject (data, false);

			return ret;
		}

		internal IntPtr NthData (uint index)
		{
			return Marshal.ReadIntPtr (ArrayPtr, (int) index * IntPtr.Size);;
		}

		// Synchronization could be tricky here. Hmm.
		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { return null; }
		}

		public void CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("Array can't be null.");

			if (index < 0)
				throw new ArgumentOutOfRangeException ("Index must be greater than 0.");

			if (index + Count < array.Length)
				throw new ArgumentException ("Array not large enough to copy into starting at index.");
			
			for (int i = 0; i < Count; i++)
				((IList) array) [index + i] = this [i];
		}

		private class ListEnumerator : IEnumerator
		{
			private int current = -1;
			private PtrArray vals;

			public ListEnumerator (PtrArray vals)
			{
				this.vals = vals;
			}

			public object Current {
				get {
					if (current == -1)
						return null;
					return vals [current];
				}
			}

			public bool MoveNext ()
			{
				if (++current >= vals.Count) {
					current = -1;
					return false;
				}

				return true;
			}

			public void Reset ()
			{
				current = -1;
			}
		}
		
		// IEnumerable
		public IEnumerator GetEnumerator ()
		{
			return new ListEnumerator (this);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_ptr_array_copy(IntPtr raw);
		static d_g_ptr_array_copy g_ptr_array_copy = FuncLoader.LoadFunction<d_g_ptr_array_copy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_ptr_array_copy"));

		// ICloneable
		public object Clone ()
		{
			return new PtrArray (g_ptr_array_copy (Handle), element_type, false, false);
		}
	}
}


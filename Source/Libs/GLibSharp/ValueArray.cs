// ValueArray.cs - ValueArray wrapper implementation
//
// Authors: Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2003 Novell, Inc.
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
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	public class ValueArray : IDisposable, ICollection, ICloneable, IWrapper {

		private IntPtr handle = IntPtr.Zero;

		static private IList<IntPtr> PendingFrees = new List<IntPtr> ();
		static private bool idle_queued = false;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_array_new(uint n_preallocs);
		static d_g_value_array_new g_value_array_new = FuncLoader.LoadFunction<d_g_value_array_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_array_new"));

		public ValueArray (uint n_preallocs)
		{
			handle = g_value_array_new (n_preallocs);
		}

		public ValueArray (IntPtr raw)
		{
			handle = raw;
		}
		
		~ValueArray ()
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
		delegate void d_g_value_array_free(IntPtr raw);
		static d_g_value_array_free g_value_array_free = FuncLoader.LoadFunction<d_g_value_array_free>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_array_free"));

		void Dispose (bool disposing)
		{
			if (Handle == IntPtr.Zero)
				return;

			lock (PendingFrees) {
				PendingFrees.Add (handle);

				if (! idle_queued) {
					Timeout.Add (50, new TimeoutHandler (PerformFrees));
					idle_queued = true;
				}
			}

			handle = IntPtr.Zero;
		}

		static bool PerformFrees ()
		{
			IntPtr[] handles;

			lock (PendingFrees) {
				idle_queued = false;

				handles = new IntPtr [PendingFrees.Count];
				PendingFrees.CopyTo (handles, 0);
				PendingFrees.Clear ();
			}

			foreach (IntPtr h in handles)
				g_value_array_free (h);

			return false;
		}
		
		public IntPtr Handle {
			get {
				return handle;
			}
		}

		struct NativeStruct {
			public uint n_values;
			public IntPtr values;
			public uint n_prealloced;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof(NativeStruct)); }
		}

		public IntPtr ArrayPtr {
			get { return Native.values; }
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_array_append(IntPtr raw, ref GLib.Value val);
		static d_g_value_array_append g_value_array_append = FuncLoader.LoadFunction<d_g_value_array_append>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_array_append"));

		public void Append (GLib.Value val)
		{
			g_value_array_append (Handle, ref val);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_array_insert(IntPtr raw, uint idx, ref GLib.Value val);
		static d_g_value_array_insert g_value_array_insert = FuncLoader.LoadFunction<d_g_value_array_insert>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_array_insert"));

		public void Insert (uint idx, GLib.Value val)
		{
			g_value_array_insert (Handle, idx, ref val);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_array_prepend(IntPtr raw, ref GLib.Value val);
		static d_g_value_array_prepend g_value_array_prepend = FuncLoader.LoadFunction<d_g_value_array_prepend>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_array_prepend"));

		public void Prepend (GLib.Value val)
		{
			g_value_array_prepend (Handle, ref val);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_value_array_remove(IntPtr raw, uint idx);
		static d_g_value_array_remove g_value_array_remove = FuncLoader.LoadFunction<d_g_value_array_remove>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_array_remove"));

		public void Remove (uint idx)
		{
			g_value_array_remove (Handle, idx);
		}

		// ICollection
		public int Count {
			get { return (int) Native.n_values; }
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_array_get_nth(IntPtr raw, uint idx);
		static d_g_value_array_get_nth g_value_array_get_nth = FuncLoader.LoadFunction<d_g_value_array_get_nth>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_array_get_nth"));

		public object this [int index] { 
			get { 
				IntPtr raw_val = g_value_array_get_nth (Handle, (uint) index);
				return Marshal.PtrToStructure (raw_val, typeof (GLib.Value));
			}
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
			private ValueArray vals;

			public ListEnumerator (ValueArray vals)
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
		delegate IntPtr d_g_value_array_copy(IntPtr raw);
		static d_g_value_array_copy g_value_array_copy = FuncLoader.LoadFunction<d_g_value_array_copy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_array_copy"));

		// ICloneable
		public object Clone ()
		{
			return new ValueArray (g_value_array_copy (Handle));
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_value_array_get_type();
		static d_g_value_array_get_type g_value_array_get_type = FuncLoader.LoadFunction<d_g_value_array_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_value_array_get_type"));

		public static GLib.GType GType {
			get {
				return new GLib.GType (g_value_array_get_type ());
			}
		}
	}
}


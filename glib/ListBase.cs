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
	///	ListBase Class
	/// </summary>
	///
	/// <remarks>
	///	Base class for GList and GSList.
	/// </remarks>

	public abstract class ListBase : IDisposable, ICollection, GLib.IWrapper, ICloneable {

		private IntPtr list_ptr = IntPtr.Zero;
		private int length = -1;
		private bool managed = false;
		protected System.Type element_type = null;

		abstract internal IntPtr GetData (IntPtr current);
		abstract internal IntPtr Next (IntPtr current);
		abstract internal int Length (IntPtr list);
		abstract internal void Free (IntPtr list);
		abstract internal IntPtr Append (IntPtr current, IntPtr raw);
		abstract internal IntPtr Prepend (IntPtr current, IntPtr raw);

		private ListBase ()
		{
		}

		internal ListBase (IntPtr list, System.Type element_type)
		{
			list_ptr = list;
			this.element_type = element_type;
		}
		
		internal ListBase (IntPtr list)
		{
			list_ptr = list;
		}
		
		~ListBase ()
		{
			Dispose ();
		}
		
		public bool Managed {
			set { managed = value; }
		}
		
		/// <summary>
		///	Handle Property
		/// </summary>
		///
		/// <remarks>
		///	A raw list reference for marshaling situations.
		/// </remarks>

		public IntPtr Handle {
			get {
				return list_ptr;
			}
		}

		internal IntPtr Raw {
			get {
				return list_ptr;
			}
			set {
				if (managed && list_ptr != IntPtr.Zero)
					Dispose ();
				list_ptr = value;
			}
		}

		public void Append (IntPtr raw)
		{
			list_ptr = Append (list_ptr, raw);
		}

		public void Prepend (IntPtr raw)
		{
			list_ptr = Prepend (list_ptr, raw);
		}

		// ICollection
		public int Count {
			get {
				if (length == -1)
					length = Length (list_ptr);
				return length;
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
			object[] orig = new object[Count];
			int i = 0;
			foreach (object o in this)
				orig[i] = o;
			
			orig.CopyTo (array, index); 
		}

		private class ListEnumerator : IEnumerator
		{
			private IntPtr current = IntPtr.Zero;
			private ListBase list;

			public ListEnumerator (ListBase list)
			{
				this.list = list;
			}

			public object Current {
				get {
					IntPtr data = list.GetData (current);
					object ret = null;
					if (list.element_type != null)
					{
						if (list.element_type == typeof (string))
							ret = Marshal.PtrToStringAnsi (data);
						else if (list.element_type == typeof (int))
							ret = (int) data;
						else if (list.element_type.IsValueType)
							ret = Marshal.PtrToStructure (data, list.element_type);
						else
							ret = Activator.CreateInstance (list.element_type, new object[] {data});
					}
					else if (Object.IsObject (data))
						ret = GLib.Object.GetObject (data, true);

					return ret;
				}
			}

			public bool MoveNext ()
			{
				if (current == IntPtr.Zero)
					current = list.list_ptr;
				else
					current = list.Next (current);
				return (current != IntPtr.Zero);
			}

			public void Reset ()
			{
				current = IntPtr.Zero;
			}
		}
		
		// IEnumerable
		public IEnumerator GetEnumerator ()
		{
			return new ListEnumerator (this);
		}

		// IDisposable
		public void Dispose ()
		{
			if (!managed)
				return;

			if (list_ptr != IntPtr.Zero)
				Free (list_ptr);
			list_ptr = IntPtr.Zero;
			length = -1;
		}
		
		// ICloneable
		abstract public object Clone ();
	}
}

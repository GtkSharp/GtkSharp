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

	public class SList : IList {

		// Private class and instance members
		IntPtr _list;
		int rev_cnt;

		/// <summary>
		///	Object Constructor
		/// </summary>
		///
		/// <remarks>
		///	Dummy constructor needed for derived classes.
		/// </remarks>

		public SList () 
		{
			_list = IntPtr.Zero;
			rev_cnt = 0;
		}

		/// <summary>
		///	Handle Property
		/// </summary>
		///
		/// <remarks>
		///	The raw GSList reference. There should be no need to 
		///	use this other than marshaling situations.
		/// </remarks>

		public IntPtr Handle {
			get {
				return _list;
			}
		}

		// ------------------------------
		// ICollection Interface Implementation
		// ------------------------------

		/// <summary>
		///	Count Property
		/// </summary>
		///
		/// <remarks>
		///	The number of elements in the SList.
		/// </remarks>

		[DllImport("glib-1.3.dll", CallingConvention=CallingConvention.Cdecl)]
		static extern int g_slist_length(IntPtr raw);

		public int Count {
			get {
				return g_slist_length(_list);
			}
		}

		/// <summary>
		///	IsSynchronized Property
		/// </summary>
		///
		/// <remarks>
		///	Returns false. GSLists are not threadsafe.
		/// </remarks>

		public bool IsSynchronized {
			get {
				return false;
			}
		}

		/// <summary>
		///	SyncRoot Property
		/// </summary>
		///
		/// <remarks>
		///	Throws not implemented exception. GSLists are not
		///	thread safe.
		/// </remarks>

		public object SyncRoot {
			get {
				throw new NotImplementedException();
			}
		}

		/// <summary>
		///	CopyTo Method
		/// </summary>
		///
		/// <remarks>
		///	Copies the list to an Array.
		/// </remarks>

		public void CopyTo(Array target, int index)
		{
			foreach (IntPtr item in this) {
				target.SetValue(item, index++);
			}
		}

		// ------------------------------
		// IEnumerable Interface Implementation
		// ------------------------------

		public IEnumerator GetEnumerator()
		{
			return new SListEnumerator(this);
		}

		// ------------------------------
		// IList Interface Implementation
		// ------------------------------

		/// <summary>
		///	IsFixedSize Property
		/// </summary>
		///
		/// <remarks>
		///	Returns false.  Items can be added and removed from 
		///	an SList.
		/// </remarks>

		public bool IsFixedSize {
			get {
				return false;
			}
		}

		/// <summary>
		///	IsReadOnly Property
		/// </summary>
		///
		/// <remarks>
		///	Returns false.  Items of an SList can be modified.
		/// </remarks>

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		/// <summary>
		///	Item Property
		/// </summary>
		///
		/// <remarks>
		///	Indexer to access members of the SList.
		/// </remarks>

		[DllImport("glib-1.3.dll", CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr g_slist_nth_data(IntPtr raw, int index);

		public object this[int index] {
			get {
				return g_slist_nth_data(_list, index);
			}
			set {
				// FIXME: Set a data element.
				rev_cnt++;
			}
		}

		// FIXME: Just a stub
		public int Add(object o)
		{
			rev_cnt++;
			return 0;
		}

		// FIXME: Just a stub
		public void Clear()
		{
			rev_cnt++;
		}

		// FIXME: Just a stub
		public bool Contains(object o)
		{
			return false;
		}

		// FIXME: Just a stub
		public int IndexOf(object o)
		{
			return 0;
		}

		// FIXME: Just a stub
		public void Insert(int index, object o)
		{
			rev_cnt++;
		}

		// FIXME: Just a stub
		public void Remove(object o)
		{
			rev_cnt++;
		}

		// FIXME: Just a stub
		public void RemoveAt(int index)
		{
			rev_cnt++;
		}

		// --------------
		// object methods
		// --------------

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of two SLists.
		/// </remarks>

		public override bool Equals (object o)
		{
			if (!(o is SList))
				return false;

			return (Handle == ((SList) o).Handle);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>

		public override int GetHashCode ()
		{
			return Handle.GetHashCode ();
		}

		// Internal enumerator class

		public class SListEnumerator : IEnumerator {

			IntPtr _cursor;
			int i_rev;
			SList _list;
			bool virgin;

			public SListEnumerator (SList list)
			{
				_list = list;
				i_rev = list.rev_cnt;
				virgin = true;
			}

			public object Current {
				get {
					if (virgin || (i_rev != _list.rev_cnt)) {
						throw new InvalidOperationException();
					}

					if (_cursor == IntPtr.Zero) {
						return null;
					}

					return Marshal.ReadIntPtr(_cursor);
				}
			}

			public bool MoveNext()
			{
				if (i_rev != _list.rev_cnt) {
					throw new InvalidOperationException();
				}

				if (virgin) {
					_cursor = _list.Handle;
				} else if (_cursor != IntPtr.Zero) {
					_cursor = Marshal.ReadIntPtr(_cursor, IntPtr.Size);
				}

				return (_cursor != IntPtr.Zero);
			}

			public void Reset()
			{
				if (i_rev != _list.rev_cnt) {
					throw new InvalidOperationException();
				}

				virgin = true;
			}
		}

	}
}

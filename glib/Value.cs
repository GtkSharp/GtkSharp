// GLib.Value.cs - GLib Value class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	/// <summary> 
	///	Value Class
	/// </summary>
	///
	/// <remarks>
	///	An arbitrary data type similar to a CORBA Any which is used
	///	to get and set properties on Objects.
	/// </remarks>

	public class Value {

		IntPtr	_val;

		// We use g_malloc0 and g_free to put the GValue on the
		// heap to avoid some marshalling pain.
		[DllImport("glib-1.3.dll")]
		static extern IntPtr g_malloc0 (long n_bytes);
		[DllImport("glib-1.3.dll")]
		static extern void g_free (IntPtr mem);

		[DllImport("gobject-1.3.dll")]
		static extern void g_value_init (IntPtr val, 
						 TypeFundamentals type);
		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a spectified string.
		/// </remarks>

		[DllImport("gobject-1.3.dll")]
		static extern void g_value_set_string (IntPtr val,
						       String data);
		[DllImport("gobject-1.3.dll")]
		static extern void g_value_set_boolean (IntPtr val,
						        bool data);

		public Value (bool val)
		{
			_val = g_malloc0 (5 * IntPtr.Size);
			g_value_init (_val, TypeFundamentals.TypeBoolean);
			g_value_set_boolean (_val, val);
		}

		public Value (String str)
		{
			_val = g_malloc0 (5 * IntPtr.Size);
			g_value_init (_val, TypeFundamentals.TypeString);
			g_value_set_string (_val, str);
		}

		/// <summary>
		///	GetString Method
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a string from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	string value.  
		/// </remarks>

		[DllImport("gobject-1.3.dll")]
		static extern String g_value_get_string (IntPtr val);

		public String GetString ()
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_string (_val);
		}

		/// <summary>
		///	RawValue Property
		/// </summary>
		/// 
		/// <remarks>
		///	Read only. Accesses a pointer to the Raw GValue.
		/// </remarks>

		public IntPtr RawValue {
			get {
				return _val;
			}
		}
	}
}

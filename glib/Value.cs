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


		// Destructor is required since we are allocating unmananged
		// heap resources.

		[DllImport("glib-1.3.dll")]
		static extern void g_free (IntPtr mem);

		~Value ()
		{
			g_free (_val);
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Creates an uninitialized Value on the unmanaged heap.
		///	Use the Init method prior to attempting to assign a
		///	value to it.
		/// </remarks>

		[DllImport("glib-1.3.dll")]
		static extern IntPtr g_malloc0 (long n_bytes);

		public Value ()
		{
			_val = g_malloc0 (5 * IntPtr.Size);
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified boolean.
		/// </remarks>

		[DllImport("gobject-1.3.dll")]
		static extern void g_value_set_boolean (IntPtr val,
						        bool data);
		public Value (bool val) : this ()
		{
			g_value_init (_val, TypeFundamentals.TypeBoolean);
			g_value_set_boolean (_val, val);
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified string.
		/// </remarks>

		[DllImport("gobject-1.3.dll")]
		static extern void g_value_set_string (IntPtr val,
						       String data);
		public Value (String val) : this ()
		{
			g_value_init (_val, TypeFundamentals.TypeString);
			g_value_set_string (_val, val);
		}

		/// <summary>
		///	Init Method
		/// </summary>
		/// 
		/// <remarks>
		///	Prepares a raw value to hold a specified type.
		/// </remarks>

		[DllImport("gobject-1.3.dll")]
		static extern void g_value_init (IntPtr val, 
						 TypeFundamentals type);

		public void Init (TypeFundamentals type)
		{
			g_value_init (_val, type);
		}

		/// <summary>
		///	Value to Boolean Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a bool from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	boolean value.  
		/// </remarks>

		[DllImport("gobject-1.3.dll")]
		static extern bool g_value_get_boolean (IntPtr val);

		public static explicit operator bool (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_boolean (val._val);
		}

		/// <summary>
		///	Value to String Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a string from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	string value.  
		/// </remarks>

		[DllImport("gobject-1.3.dll")]
		static extern String g_value_get_string (IntPtr val);

		public static explicit operator String (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_string (val._val);
		}

		/// <summary>
		///	MarshalAs Property
		/// </summary>
		/// 
		/// <remarks>
		///	Read only. Accesses a pointer to the raw GValue.
		/// </remarks>

		public IntPtr MarshalAs {
			get {
				return _val;
			}
		}
	}
}

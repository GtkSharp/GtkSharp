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


		// Destructor is required since we are allocating unmanaged
		// heap resources.

		[DllImport("glib-2.0")]
		static extern void g_free (IntPtr mem);

		~Value ()
		{
			g_free (_val);
		}


		// import the glue function to allocate values on heap

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_value_create(TypeFundamentals type);

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_value_create_from_property(IntPtr obj, string name);

		// Constructor to wrap a raw GValue ref.  We need the dummy param
		// to distinguish this ctor from the TypePointer ctor.

		public Value (IntPtr val, IntPtr dummy)
		{
			_val = val;
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value corresponding to the type of the
		///	specified property.
		/// </remarks>

		public Value (IntPtr obj, string prop_name)
		{
			_val = gtksharp_value_create_from_property (obj, prop_name);
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified boolean.
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern void g_value_set_boolean (IntPtr val,
						        bool data);
		public Value (bool val)
		{
			_val = gtksharp_value_create(TypeFundamentals.TypeBoolean);
			g_value_set_boolean (_val, val);
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified boxed type.
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern void g_value_set_boxed (IntPtr val, IntPtr data);
		public Value (GLib.Boxed val)
		{
			_val = gtksharp_value_create(TypeFundamentals.TypeBoxed);
			g_value_set_boxed (_val, val.Handle);
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified double.
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern void g_value_set_double (IntPtr val, double data);

		public Value (double val)
		{
			_val = gtksharp_value_create (TypeFundamentals.TypeDouble);
			g_value_set_double (_val, val);
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified float.
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern void g_value_set_float (IntPtr val, float data);

		public Value (float val)
		{
			_val = gtksharp_value_create (TypeFundamentals.TypeFloat);
			g_value_set_float (_val, val);
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified integer.
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern void g_value_set_int (IntPtr val, int data);

		public Value (int val)
		{
			_val = gtksharp_value_create (TypeFundamentals.TypeInt);
			g_value_set_int (_val, val);
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified object.
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern void g_value_set_object (IntPtr val, IntPtr data);

		public Value (GLib.Object val)
		{
			_val = gtksharp_value_create (TypeFundamentals.TypeObject);
			g_value_set_object (_val, val.Handle);
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified pointer.
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern void g_value_set_pointer (IntPtr val, IntPtr data);

		public Value (IntPtr val)
		{
			_val = gtksharp_value_create (TypeFundamentals.TypePointer);
			g_value_set_pointer (_val, val); 
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified string.
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern void g_value_set_string (IntPtr val, string data);

		public Value (string val)
		{
			_val = gtksharp_value_create (TypeFundamentals.TypeString);
			g_value_set_string (_val, val); 
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified uint.
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern void g_value_set_uint (IntPtr val, uint data);

		public Value (uint val)
		{
			_val = gtksharp_value_create (TypeFundamentals.TypeUInt);
			g_value_set_uint (_val, val); 
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

		[DllImport("gobject-2.0")]
		static extern bool g_value_get_boolean (IntPtr val);

		public static explicit operator bool (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_boolean (val._val);
		}

		/// <summary>
		///	Value to Boxed Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a boxed type from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	boxed type value.  
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern IntPtr g_value_get_boxed (IntPtr val);

		public static explicit operator GLib.Boxed (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			// FIXME: Figure out how to wrap this boxed type
			return null;
		}

		/// <summary>
		///	Value to Double Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a double from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	double value.  
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern double g_value_get_double (IntPtr val);

		public static explicit operator double (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_double (val._val);
		}

		/// <summary>
		///	Value to Float Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a float from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	float value.  
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern float g_value_get_float (IntPtr val);

		public static explicit operator float (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_float (val._val);
		}

		/// <summary>
		///	Value to Integer Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts an int from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	integer value.  
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern int g_value_get_int (IntPtr val);

		public static explicit operator int (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_int (val._val);
		}

		/// <summary>
		///	Value to Object Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts an object from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	object value.  
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern IntPtr g_value_get_object (IntPtr val);

		public static explicit operator GLib.Object (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return GLib.Object.GetObject(g_value_get_object (val._val));
		}

		/// <summary>
		///	Value to Pointer Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a pointer from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	pointer value.  
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern IntPtr g_value_get_pointer (IntPtr val);

		public static explicit operator IntPtr (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_pointer (val._val);
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

		[DllImport("gobject-2.0")]
		static extern string g_value_get_string (IntPtr val);

		public static explicit operator String (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_string (val._val);
		}

		/// <summary>
		///	Value to Unsigned Integer Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts an uint from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	unsigned integer value.  
		/// </remarks>

		[DllImport("gobject-2.0")]
		static extern uint g_value_get_uint (IntPtr val);

		public static explicit operator uint (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_uint (val._val);
		}

		/// <summary>
		///	Handle Property
		/// </summary>
		/// 
		/// <remarks>
		///	Read only. Accesses a pointer to the raw GValue.
		/// </remarks>

		public IntPtr Handle {
			get {
				return _val;
			}
		}
	}
}

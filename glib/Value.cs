// GLib.Value.cs - GLib Value class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GLib {

	using System;
	using System.Runtime.InteropServices;
	using GLibSharp;

	/// <summary> 
	///	Value Class
	/// </summary>
	///
	/// <remarks>
	///	An arbitrary data type similar to a CORBA Any which is used
	///	to get and set properties on Objects.
	/// </remarks>

	public class Value : IDisposable {

		IntPtr	_val;


		// Destructor is required since we are allocating unmanaged
		// heap resources.

		[DllImport("libglib-2.0-0.dll")]
		static extern void g_free (IntPtr mem);

		~Value ()
		{
			Dispose ();
		}

		public void Dispose () {
			if (_val != IntPtr.Zero) {
				uint type = gtksharp_value_get_value_type (_val);
				if (type == ManagedValue.GType) {
					ManagedValue.Free (g_value_get_boxed (_val)); 
				}
			
				g_free (_val);
				_val = IntPtr.Zero; 
			}
		}


		// import the glue function to allocate values on heap

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_value_create(uint type);

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
		///	Constructs a new empty value that can be used
		///	to receive "out" GValue parameters.
		/// </remarks>

		public Value () {
			_val = gtksharp_value_create ((uint) TypeFundamentals.TypeInvalid);
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

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_boolean (IntPtr val,
						        bool data);

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified boolean.
		/// </remarks>

		public Value (bool val)
		{
			_val = gtksharp_value_create((uint) TypeFundamentals.TypeBoolean);
			g_value_set_boolean (_val, val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_boxed (IntPtr val, IntPtr data);

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified boxed type.
		/// </remarks>

		public Value (GLib.Boxed val)
		{
			_val = gtksharp_value_create((uint) TypeFundamentals.TypeBoxed);
			//g_value_set_boxed (_val, val.Handle);
		}

		public Value (IntPtr obj, string prop_name, Boxed val)
		{
			_val = gtksharp_value_create_from_property (obj, prop_name);
			//g_value_set_boxed (_val, val.Handle);
		}

		public Value (GLib.Opaque val)
		{
			_val = gtksharp_value_create((uint) TypeFundamentals.TypeBoxed);
			g_value_set_boxed (_val, val.Handle);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_double (IntPtr val, double data);

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified double.
		/// </remarks>

		public Value (double val)
		{
			_val = gtksharp_value_create ((uint) TypeFundamentals.TypeDouble);
			g_value_set_double (_val, val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_float (IntPtr val, float data);

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified float.
		/// </remarks>

		public Value (float val)
		{
			_val = gtksharp_value_create ((uint) TypeFundamentals.TypeFloat);
			g_value_set_float (_val, val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_int (IntPtr val, int data);

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified integer.
		/// </remarks>

		public Value (int val)
		{
			_val = gtksharp_value_create ((uint) TypeFundamentals.TypeInt);
			g_value_set_int (_val, val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_object (IntPtr val, IntPtr data);

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified object.
		/// </remarks>

		public Value (GLib.Object val)
		{
			_val = gtksharp_value_create (val.GetGType ());
			g_value_set_object (_val, val.Handle);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_pointer (IntPtr val, IntPtr data);

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified pointer.
		/// </remarks>

		public Value (IntPtr val)
		{
			_val = gtksharp_value_create ((uint) TypeFundamentals.TypePointer);
			g_value_set_pointer (_val, val); 
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_string (IntPtr val, string data);

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified string.
		/// </remarks>

		public Value (string val)
		{
			_val = gtksharp_value_create ((uint) TypeFundamentals.TypeString);
			g_value_set_string (_val, val); 
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_uint (IntPtr val, uint data);

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified uint.
		/// </remarks>

		public Value (uint val)
		{
			_val = gtksharp_value_create ((uint) TypeFundamentals.TypeUInt);
			g_value_set_uint (_val, val); 
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified ushort.
		/// </remarks>

		public Value (ushort val)
		{
			_val = gtksharp_value_create ((uint) TypeFundamentals.TypeUInt);
			g_value_set_uint (_val, val); 
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_enum (IntPtr val, int data);
		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_flags (IntPtr val, uint data);
		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_char (IntPtr val, char data);
		
		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified enum wrapper.
		/// </remarks>

		public Value (IntPtr obj, string prop_name, EnumWrapper wrap)
		{
			_val = gtksharp_value_create_from_property (obj, prop_name);
			if (wrap.flags)
				g_value_set_flags (_val, (uint) (int) wrap); 
			else
				g_value_set_enum (_val, (int) wrap); 
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from any object, including a managed
		///   type.
		/// </remarks>

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_boxed_take_ownership (IntPtr val, IntPtr data);

		public Value (object obj)
		{
			TypeFundamentals type = TypeConverter.LookupType (obj.GetType ());
			if (type == TypeFundamentals.TypeNone) {
				_val = gtksharp_value_create (ManagedValue.GType);
			} else if (type == TypeFundamentals.TypeObject) {
				_val = gtksharp_value_create (((GLib.Object) obj).GetGType ());
			} else if (type == TypeFundamentals.TypeInvalid) {
				throw new Exception ("Unknown type");
			} else {
				_val = gtksharp_value_create ((uint) type);
			}
			
			switch (type) {
			case TypeFundamentals.TypeNone:
				g_value_set_boxed_take_ownership (_val, ManagedValue.WrapObject (obj));
				break;
			case TypeFundamentals.TypeString:
				g_value_set_string (_val, (string) obj);
				break;
			case TypeFundamentals.TypeBoolean:
				g_value_set_boolean (_val, (bool) obj);
				break;
			case TypeFundamentals.TypeInt:
				g_value_set_int (_val, (int) obj);
				break;
			case TypeFundamentals.TypeDouble:
				g_value_set_double (_val, (double) obj);
				break;
			case TypeFundamentals.TypeFloat:
				g_value_set_float (_val, (float) obj);
				break;
			case TypeFundamentals.TypeChar:
				g_value_set_char (_val, (char) obj);
				break;
			case TypeFundamentals.TypeUInt:
				g_value_set_uint (_val, (uint) obj);
				break;
			case TypeFundamentals.TypeObject:
				g_value_set_object (_val, ((GLib.Object) obj).Handle);
				break;
			default:
				throw new Exception ("Unknown type");
			}
		}



		[DllImport("libgobject-2.0-0.dll")]
		static extern bool g_value_get_boolean (IntPtr val);

		/// <summary>
		///	Value to Boolean Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a bool from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	boolean value.  
		/// </remarks>

		public static explicit operator bool (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_boolean (val._val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_value_get_boxed (IntPtr val);

		public static explicit operator GLib.Opaque (Value val)
		{
			return GLib.Opaque.GetOpaque (g_value_get_boxed (val._val));
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

		public static explicit operator GLib.Boxed (Value val)
		{
			return new GLib.Boxed (g_value_get_boxed (val._val));
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern double g_value_get_double (IntPtr val);

		/// <summary>
		///	Value to Double Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a double from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	double value.  
		/// </remarks>

		public static explicit operator double (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_double (val._val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern float g_value_get_float (IntPtr val);

		/// <summary>
		///	Value to Float Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a float from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	float value.  
		/// </remarks>

		public static explicit operator float (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_float (val._val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern int g_value_get_int (IntPtr val);

		/// <summary>
		///	Value to Integer Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts an int from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	integer value.  
		/// </remarks>

		public static explicit operator int (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_int (val._val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_value_get_object (IntPtr val);

		/// <summary>
		///	Value to Object Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts an object from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	object value.  
		/// </remarks>

		public static explicit operator GLib.Object (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return GLib.Object.GetObject(g_value_get_object (val._val));
		}

		/// <summary>
		///	Value to Unresolved Object Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts an object from a Value without looking up its wrapping
		///   class.
		///   Note, this method will produce an exception if the Value does
		///   not hold a object value.  
		/// </remarks>

		public static explicit operator GLib.UnwrappedObject (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return new UnwrappedObject(g_value_get_object (val._val));
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_value_get_pointer (IntPtr val);

		/// <summary>
		///	Value to Pointer Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a pointer from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	pointer value.  
		/// </remarks>

		public static explicit operator IntPtr (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_pointer (val._val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_value_get_string (IntPtr val);

		/// <summary>
		///	Value to String Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a string from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	string value.  
		/// </remarks>

		public static explicit operator String (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return Marshal.PtrToStringAnsi (g_value_get_string (val._val));
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern uint g_value_get_uint (IntPtr val);

		/// <summary>
		///	Value to Unsigned Integer Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts an uint from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	unsigned integer value.  
		/// </remarks>

		public static explicit operator uint (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_uint (val._val);
		}

		/// <summary>
		///	Value to Unsigned Short Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a ushort from a Value.  Note, this method
		///	will produce an exception if the Value does not hold a
		///	unsigned integer value.  
		/// </remarks>

		public static explicit operator ushort (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return (ushort) g_value_get_uint (val._val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern int g_value_get_enum (IntPtr val);
		[DllImport("libgobject-2.0-0.dll")]
		static extern uint g_value_get_flags (IntPtr val);

		/// <summary>
		///	Value to Enum Conversion
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts an enum from a Value.  Note, this method
		///	will produce an exception if the Value does not hold an
		///	enum value.  
		/// </remarks>

		public static explicit operator EnumWrapper (Value val)
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			// FIXME: handle flags
			return new EnumWrapper (g_value_get_enum (val._val), false);
		}

		[DllImport("gtksharpglue")]
		static extern uint gtksharp_value_get_value_type (IntPtr val);
		
		public object Val
		{
			get {
				uint type = gtksharp_value_get_value_type (_val);
				if (type == ManagedValue.GType) {
					return ManagedValue.ObjectForWrapper (g_value_get_boxed (_val));
				}

				switch ((TypeFundamentals) type) {
				case TypeFundamentals.TypeString:
					return (string) this;
				case TypeFundamentals.TypeBoolean:
					return (bool) this;
				case TypeFundamentals.TypeInt:
					return (int) this;
				case TypeFundamentals.TypeDouble:
					return (double) this;
				case TypeFundamentals.TypeFloat:
					return (float) this;
				case TypeFundamentals.TypeChar:
					return (char) this;
				case TypeFundamentals.TypeUInt:
					return (uint) this;
				case TypeFundamentals.TypeObject:
					return (GLib.Object) this;
				default:
					throw new Exception ("Unknown type");
				}
			}
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

// GLib.Value.cs - GLib Value class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner, (c) 2003 Novell, Inc.

namespace GLib {

	using System;
	using System.Collections;
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
		bool needs_dispose = true;


		// Destructor is required since we are allocating unmanaged
		// heap resources.

		[DllImport("libglib-2.0-0.dll")]
		static extern void g_free (IntPtr mem);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_unset (IntPtr mem);

		~Value ()
		{
			Dispose ();
		}

		static bool idle_queued;
		static Queue idle_queue = new Queue ();

		static bool DoDispose ()
		{
			IntPtr [] vals;

			lock (idle_queue){
				vals = new IntPtr [idle_queue.Count];
				idle_queue.CopyTo (vals, 0);
				idle_queue.Clear ();
			}
			lock (typeof (Value))
				idle_queued = false;

			foreach (IntPtr v in vals) {
				if (v == IntPtr.Zero)
					continue;

				g_value_unset (v);
				g_free (v);
			}
			return false;
		}

		public void Dispose () {
			if (_val != IntPtr.Zero && needs_dispose) {
				IntPtr rawtype = gtksharp_value_get_value_type (_val);
				if (rawtype == ManagedValue.GType.Val) {
					ManagedValue.Free (g_value_get_boxed (_val)); 
				}
			
				lock (idle_queue) {
					idle_queue.Enqueue (_val);
					lock (typeof (Value)){
						if (!idle_queued) {
							Idle.Add (new IdleHandler (DoDispose));
							idle_queued = true;
						}
					}
				}
				_val = IntPtr.Zero; 
			}

			if (buf != IntPtr.Zero) {
				Marshal.FreeHGlobal (buf);
				buf = IntPtr.Zero;
			}
		}

		// import the glue function to allocate values on heap

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_value_create(IntPtr type);

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_value_create_from_property(IntPtr obj, string name);

		// Constructor to wrap a raw GValue ref.  We need the dummy param
		// to distinguish this ctor from the TypePointer ctor.

		public Value (IntPtr val, IntPtr dummy)
		{
			_val = val;
			needs_dispose = false;
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
			_val = gtksharp_value_create (GType.Invalid.Val);
		}

		/// <summary>
		///	Value Constructor
		/// </summary>
		///
		/// <remarks>
		///	Constructs a new empty initialized value that can be used
		///	to receive "out" GValue parameters.
		/// </remarks>

		public Value (GLib.GType gtype) {
			_val = gtksharp_value_create (gtype.Val);
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
		static extern void g_value_set_boolean (IntPtr val, bool data);

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified boolean.
		/// </remarks>

		public Value (bool val)
		{
			_val = gtksharp_value_create(GType.Boolean.Val);
			g_value_set_boolean (_val, val);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_value_set_boxed (IntPtr val, IntPtr data);

/*
		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from a specified boxed type.
		/// </remarks>

		public Value (GLib.Boxed val)
		{
			_val = gtksharp_value_create(GType.Boxed);
			//g_value_set_boxed (_val, val.Handle);
		}

		public Value (IntPtr obj, string prop_name, Boxed val)
		{
			_val = gtksharp_value_create_from_property (obj, prop_name);
			//g_value_set_boxed (_val, val.Handle);
		}
*/

		public Value (IntPtr obj, string prop_name, Opaque val)
		{
			_val = gtksharp_value_create_from_property (obj, prop_name);
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
			_val = gtksharp_value_create (GType.Double.Val);
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
			_val = gtksharp_value_create (GType.Float.Val);
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
			_val = gtksharp_value_create (GType.Int.Val);
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
			_val = gtksharp_value_create (val.GetGType ().Val);
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
			_val = gtksharp_value_create (GType.Pointer.Val);
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
			_val = gtksharp_value_create (GType.String.Val);
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
			_val = gtksharp_value_create (GType.UInt.Val);
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
			_val = gtksharp_value_create (GType.UInt.Val);
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

		[DllImport("libgobject-2.0-0.dll")]
		static extern bool g_type_is_a (IntPtr type, IntPtr is_a_type);

		IntPtr buf = IntPtr.Zero;

		/// <summary>
		///	Value Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Value from any object, including a managed
		///   type.
		/// </remarks>

		public Value (object obj)
		{
			GType type = TypeConverter.LookupType (obj.GetType ());
			if (type == GType.None) {
				_val = gtksharp_value_create (ManagedValue.GType.Val);
			} else if (type == GType.Object) {
				_val = gtksharp_value_create (((GLib.Object) obj).GetGType ().Val);
			} else {
				_val = gtksharp_value_create (type.Val);
			}
			
			if (type == GType.None)
				g_value_set_boxed (_val, ManagedValue.WrapObject (obj));
			else if (type == GType.String)
				g_value_set_string (_val, (string) obj);
			else if (type == GType.Boolean)
				g_value_set_boolean (_val, (bool) obj);
			else if (type == GType.Int)
				g_value_set_int (_val, (int) obj);
			else if (type == GType.Double)
				g_value_set_double (_val, (double) obj);
			else if (type == GType.Float)
				g_value_set_float (_val, (float) obj);
			else if (type == GType.Char)
				g_value_set_char (_val, (char) obj);
			else if (type == GType.UInt)
				g_value_set_uint (_val, (uint) obj);
			else if (type == GType.Object)
				g_value_set_object (_val, ((GLib.Object) obj).Handle);
			else if (type == GType.Pointer) {
				buf = Marshal.AllocHGlobal (Marshal.SizeOf (obj.GetType()));
				Marshal.StructureToPtr (obj, buf, false);
				g_value_set_pointer (_val, buf);
			} else if (g_type_is_a (type.Val, GLib.GType.Boxed.Val)) {
				buf = Marshal.AllocHGlobal (Marshal.SizeOf (obj.GetType()));
				Marshal.StructureToPtr (obj, buf, false);
				g_value_set_boxed (_val, buf);
			} else
				throw new Exception ("Unknown type");
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
			return GLib.Object.GetObject(g_value_get_object (val._val), true);
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
		static extern IntPtr gtksharp_value_get_value_type (IntPtr val);
		
		public object Val
		{
			get {
				GLib.GType type = new GLib.GType (gtksharp_value_get_value_type (_val));
				if (type == ManagedValue.GType) {
					return ManagedValue.ObjectForWrapper (g_value_get_boxed (_val));
				}

				if (type == GType.String)
					return (string) this;
				else if (type == GType.Boolean)
					return (bool) this;
				else if (type == GType.Int)
					return (int) this;
				else if (type == GType.Double)
					return (double) this;
				else if (type == GType.Float)
					return (float) this;
				else if (type == GType.Char)
					return (char) this;
				else if (type == GType.UInt)
					return (uint) this;
				else if (type == GType.Object)
					return (GLib.Object) this;
				else
					throw new Exception ("Unknown type");
			}
			set {
				GType type = GLibSharp.TypeConverter.LookupType (value.GetType());
				if (type == GType.None)
					g_value_set_boxed (_val, ManagedValue.WrapObject (value));
				else if (type == GType.String)
					g_value_set_string (_val, (string) value);
				else if (type == GType.Boolean)
					g_value_set_boolean (_val, (bool) value);
				else if (type == GType.Int)
					g_value_set_int (_val, (int) value);
				else if (type == GType.Double)
					g_value_set_double (_val, (double) value);
				else if (type == GType.Float)
					g_value_set_float (_val, (float) value);
				else if (type == GType.Char)
					g_value_set_char (_val, (char) value);
				else if (type == GType.UInt)
					g_value_set_uint (_val, (uint) value);
				else if (type == GType.Object)
					g_value_set_object (_val, ((GLib.Object) value).Handle);
				else
					throw new Exception ("Unknown type");
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

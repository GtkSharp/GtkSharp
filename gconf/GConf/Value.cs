namespace GConf
{
	using System;
	using System.Runtime.InteropServices;

	internal enum ValueType
	{
		Invalid,
		String,
		Int,
		Float,
		Bool,
		Schema,
		List,
		Pair
	};

	internal struct NativeValue
	{
		public ValueType type;
	}

	internal class InvalidValueTypeException : Exception
	{
	}
	
	internal class Value : IDisposable  
	{
		IntPtr Raw = IntPtr.Zero;
		ValueType val_type = ValueType.Invalid;
		bool managed = true;
		
		[DllImport("gconf-2")]
		static extern void gconf_value_set_string (IntPtr value, string data);
		[DllImport("gconf-2")]
		static extern void gconf_value_set_int (IntPtr value, int data);
		[DllImport("gconf-2")]
		static extern void gconf_value_set_float (IntPtr value, double data);
		[DllImport("gconf-2")]
		static extern void gconf_value_set_bool (IntPtr value, bool data);

		ValueType LookupType (object data)
		{
			if (data is string) {
				return ValueType.String;
			} else if (data is int) {
				return ValueType.Int;
			} else if (data is double) {
				return ValueType.Float;
			} else if (data is bool) {
				return ValueType.Bool;
			} else {
				return ValueType.Invalid;
			}
		}

		public void Set (object data)
		{
			if (data == null)
				throw new NullReferenceException ();
			
			ValueType type = LookupType (data);
			Set (data, type);
		}

		void Set (object data, ValueType type)
		{
			if (data == null)
				throw new NullReferenceException ();

			switch (type)
			{
				case ValueType.String:
					gconf_value_set_string (Raw, (string) data);
					break;
				case ValueType.Int:
					gconf_value_set_int (Raw, (int) data);
					break;
				case ValueType.Float:
					gconf_value_set_float (Raw, (double) data);
					break;
				case ValueType.Bool:
					gconf_value_set_bool (Raw, (bool) data);
					break;
				default:
					throw new InvalidValueTypeException ();
			}
		}

		[DllImport("gconf-2")]
		static extern IntPtr gconf_value_get_string (IntPtr value);
		
		[DllImport("gconf-2")]
		static extern int gconf_value_get_int (IntPtr value);
		
		[DllImport("gconf-2")]
		static extern double gconf_value_get_float (IntPtr value);
		
		[DllImport("gconf-2")]
		static extern bool gconf_value_get_bool (IntPtr value);
		
		public object Get ()
		{
			switch (val_type)
			{
				case ValueType.String:
					return Marshal.PtrToStringAnsi (gconf_value_get_string (Raw));
				case ValueType.Int:
					return gconf_value_get_int (Raw);
				case ValueType.Float:
					return gconf_value_get_float (Raw);
				case ValueType.Bool:
					return gconf_value_get_bool (Raw);
				default:
					throw new InvalidValueTypeException ();
			}
		}

		[DllImport("gconf-2")]
		static extern IntPtr gconf_value_new (ValueType type);
		
		public Value (ValueType type)
		{
			Raw = gconf_value_new (type);
		}

		void Initialize (object val, ValueType type)
		{
			Raw = gconf_value_new (type);
			val_type = type;
			Set (val, type);	
		}

		public Value (IntPtr raw)
		{
			Raw = raw;
			NativeValue val = (NativeValue) Marshal.PtrToStructure (raw, typeof (NativeValue));
			val_type = val.type;
		}
/*
		public Value (string val)
		{
			Initialize (val, ValueType.String);
		}

		public Value (int val)
		{
			Initialize (val, ValueType.Int);
		}
		
		public Value (double val)
		{
			Initialize (val, ValueType.Float);
		}

		public Value (bool val)
		{
			Initialize (val, ValueType.Bool);
		}
*/
		public Value (object val)
		{
			Initialize (val, LookupType (val));
		}
		public bool Managed
		{
			get { return managed; }
			set { managed = value; }
		}

		[DllImport("gconf-2")]
		static extern void gconf_value_free (IntPtr value);
		
		~Value ()
		{
			Dispose ();
		}
		
		public void Dispose ()
		{
			if (managed && Raw != IntPtr.Zero)
			{
				gconf_value_free (Raw);
				Raw = IntPtr.Zero;
			}
		}

		public IntPtr Handle
		{
			get {
				return Raw;
			}
		}
	}
}


// Value.cs -
//
// Author: Rachel Hestilow  <hestilow@nullenvoid.com>
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

namespace GConf
{
	using System;
	using System.Collections;
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
			} else if (data is ICollection) {
				return ValueType.List;
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

		[DllImport("gconf-2")]
		static extern IntPtr gconf_value_set_list_nocopy (IntPtr value, IntPtr list);
		
		[DllImport("gconf-2")]
		static extern IntPtr gconf_value_set_list_type (IntPtr value, ValueType vtype);
		
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
				case ValueType.List:
					ValueType listType;
					GLib.SList list = GetListFromCollection ((ICollection) data, out listType);
					gconf_value_set_list_type (Raw, listType);
					gconf_value_set_list_nocopy (Raw, list.Handle);
					break;
				default:
					throw new InvalidValueTypeException ();
			}
		}
		
		GLib.SList GetListFromCollection (ICollection data, out ValueType listType)
		{
			object [] arr = (object []) Array.CreateInstance (typeof (object), data.Count);
			data.CopyTo (arr, 0);

			listType = ValueType.Invalid;
			GLib.SList list = new GLib.SList (IntPtr.Zero);
			GC.SuppressFinalize (list);

			foreach (object o in arr) {
				ValueType type = LookupType (o);
				if (listType == ValueType.Invalid)
					listType = type;

				if (listType == ValueType.Invalid || type != listType)
					throw new InvalidValueTypeException ();

				Value v = new Value (o);
				GC.SuppressFinalize (v);
				list.Append (v.Raw);
			}
			
			return list;
		}

		[DllImport("gconf-2")]
		static extern IntPtr gconf_value_get_string (IntPtr value);
		
		[DllImport("gconf-2")]
		static extern int gconf_value_get_int (IntPtr value);
		
		[DllImport("gconf-2")]
		static extern double gconf_value_get_float (IntPtr value);
		
		[DllImport("gconf-2")]
		static extern bool gconf_value_get_bool (IntPtr value);
		
		[DllImport("gconf-2")]
		static extern IntPtr gconf_value_get_list (IntPtr value);
		
		public object Get ()
		{
			switch (val_type)
			{
				case ValueType.String:
					return GLib.Marshaller.Utf8PtrToString (gconf_value_get_string (Raw));
				case ValueType.Int:
					return gconf_value_get_int (Raw);
				case ValueType.Float:
					return gconf_value_get_float (Raw);
				case ValueType.Bool:
					return gconf_value_get_bool (Raw);
				case ValueType.List:
					GLib.SList list = new GLib.SList (gconf_value_get_list (Raw), typeof (Value));
					Array result = Array.CreateInstance (GetListType (), list.Count);
					int i = 0;
					foreach (Value v in list) {
						((IList) result) [i] =  v.Get ();
						v.managed = false; // This is the trick to prevent a crash
						i++;
					}

					return result;
				default:
					throw new InvalidValueTypeException ();
			}
		}

		[DllImport("gconf-2")]
		static extern ValueType gconf_value_get_list_type (IntPtr value);

		Type GetListType ()
		{
			ValueType vt = gconf_value_get_list_type (Raw);
			switch (vt) {
			case ValueType.String:
				return typeof (string);
			case ValueType.Int:
				return typeof (int);
			case ValueType.Float:
				return typeof (float);
			case ValueType.Bool:
				return typeof (bool);
			case ValueType.List:
				return typeof (GLib.SList);
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
			Dispose (false);
		}
		
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
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


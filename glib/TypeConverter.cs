// GLib.TypeConverter.cs : Convert between fundamental and .NET types 
//
// Author: Rachel Hestilow <hestilow@ximian.com>
//
// (c) 2002 Rachel Hestilow

namespace GLibSharp {
	using System;
	using System.Collections;
	using System.Reflection;
	using GLib;

	/// <summary>
	///	Fundamental type converter 
	/// </summary>
	///
	/// <remarks>
	///  Utilities for converting between TypeFundamentals and System.Type
	/// </remarks>
	public class TypeConverter {

		private TypeConverter () {}
		
		public static GType LookupType (System.Type type)
		{
			if (type.Equals (typeof (string)))
				return GType.String;
			if (type.Equals (typeof (bool)))
				return GType.Boolean;
			if (type.Equals (typeof (int)))
				return GType.Int;
			if (type.Equals (typeof (double)))
				return GType.Double;
			if (type.Equals (typeof (float)))
				return GType.Float;
			if (type.Equals (typeof (char)))
				return GType.Char;
			if (type.Equals (typeof (uint)))
				return GType.UInt;
			if (type.IsSubclassOf (typeof (GLib.Object)))
				return GType.Object;
			PropertyInfo pi = type.GetProperty ("GType", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			if (pi != null)
				return (GType) pi.GetValue (null, null); 
			if (type.IsValueType)
				return GType.Pointer;

			return GType.None;
		}
	}
}


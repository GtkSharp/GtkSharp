// GLib.TypeConverter.cs : Convert between fundamental and .NET types 
//
// Author: Rachel Hestilow <hestilow@ximian.com>
//
// (c) 2002 Rachel Hestilow

namespace GLibSharp {
	using System;
	using System.Collections;
	using GLib;

	/// <summary>
	///	Fundamental type converter 
	/// </summary>
	///
	/// <remarks>
	///  Utilities for converting between TypeFundamentals and System.Type
	/// </remarks>
	public class TypeConverter {
		public static TypeFundamentals LookupType (System.Type type)
		{
			if (type.Equals (typeof (string)))
				return TypeFundamentals.TypeString;

			if (!type.IsValueType) {
				if (type.IsSubclassOf (typeof (GLib.Object)))
					return TypeFundamentals.TypeObject;
				else if (type.IsSubclassOf (typeof (GLib.Boxed)))
					return TypeFundamentals.TypeBoxed;
				else
					return TypeFundamentals.TypeNone;
			}

			if (type.Equals (typeof (bool)))
				return TypeFundamentals.TypeBoolean;
			if (type.Equals (typeof (int)))
				return TypeFundamentals.TypeInt;
			if (type.Equals (typeof (double)))
				return TypeFundamentals.TypeDouble;
			if (type.Equals (typeof (float)))
				return TypeFundamentals.TypeFloat;
			if (type.Equals (typeof (char)))
				return TypeFundamentals.TypeChar;
			if (type.Equals (typeof (uint)))
				return TypeFundamentals.TypeUInt;

			return TypeFundamentals.TypeInvalid;
		}
	}
}


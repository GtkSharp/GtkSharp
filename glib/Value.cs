// GLib.GValue.cs - GLib Value class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct GValueStruct {
		uint 	type;
		IntPtr	data1;
		IntPtr	data2;
		IntPtr	data3;
		IntPtr	data4;
	}

	public class GValue {

		GValueStruct	_val;

		/// <summary>
		///	GValue Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a GValue from a string.
		/// </remarks>

		[DllImport("gobject-1.3")]
		static extern void g_value_set_string (ref GValueStruct val,
						       String data);

		public GValue (String data)
		{
			g_value_set_string (ref _val, data);
		}

		/// <summary>
		///	GetString Method
		/// </summary>
		/// 
		/// <remarks>
		///	Extracts a string from a GValue.  Note, this method
		///	will produce an exception if the GValue does not hold a
		///	string value.  
		/// </remarks>

		[DllImport("gobject-1.3")]
		static extern String g_value_get_string (ref GValueStruct val);

		public String GetString ()
		{
			// FIXME: Insert an appropriate exception here if
			// _val.type indicates an error.
			return g_value_get_string (ref _val);
		}

		/// <summary>
		///	ValueStruct Property
		/// </summary>
		/// 
		/// <remarks>
		///	Accesses a structure which can be easily marshalled
		///	via PInvoke to set properties on GObjects.
		/// </remarks>

		public GValueStruct ValueStruct {
			get {
				return _val;
			}
		}
	}
}

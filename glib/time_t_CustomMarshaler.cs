// time_t_CustomMarshaler.cs - Custom marshaling between time_t and DateTime
//
// Author: Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.

namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	public class time_t_CustomMarshaler : ICustomMarshaler {

		static time_t_CustomMarshaler marshaler;
		int utc_offset;
		DateTime local_epoch;

		private time_t_CustomMarshaler () 
		{
			utc_offset = DateTime.Now.Subtract (DateTime.UtcNow).Seconds;
			local_epoch = new DateTime (1970, 1, 1, 0, 0, 0);
		}

		public static ICustomMarshaler GetInstance (string cookie)
		{
			if (marshaler == null)
				marshaler = new time_t_CustomMarshaler ();

			return marshaler;
		}

		[DllImport ("gtksharpglue")]
		static extern int gtksharp_time_t_sizeof ();

		[DllImport ("gtksharpglue")]
		static extern void gtksharp_time_t_print (int time_t);

		public IntPtr MarshalManagedToNative (object obj)
		{
			DateTime dt = (DateTime) obj;
			int size = Marshal.SizeOf (typeof (int)) + gtksharp_time_t_sizeof ();
			IntPtr ptr = Marshal.AllocCoTaskMem (size);
			IntPtr time_t_ptr = new IntPtr (ptr.ToInt32 () + Marshal.SizeOf (typeof(int)));

			int secs = dt.Subtract (local_epoch).Seconds + utc_offset;
			Console.WriteLine ("Marshaling DateTime: " + dt);
			Marshal.WriteInt32 (time_t_ptr, secs);
			gtksharp_time_t_print (secs);
			return time_t_ptr;
		}

		public void CleanUpNativeData (IntPtr data)
		{
			IntPtr ptr = new IntPtr (data.ToInt32 () - Marshal.SizeOf (typeof (int)));
			Marshal.FreeCoTaskMem (ptr);
		}

		public object MarshalNativeToManaged (IntPtr data)
		{
			throw new NotImplementedException ();
		}

		public void CleanUpManagedData (object obj) {}

		public int GetNativeDataSize ()
		{
			throw new NotImplementedException ();
		}
	}
}

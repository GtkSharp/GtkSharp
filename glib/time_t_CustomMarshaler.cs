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
			utc_offset = (int) DateTime.Now.Subtract (DateTime.UtcNow).TotalSeconds;
			local_epoch = new DateTime (1970, 1, 1, 0, 0, 0);
		}

		public static ICustomMarshaler GetInstance (string cookie)
		{
			if (marshaler == null)
				marshaler = new time_t_CustomMarshaler ();

			return marshaler;
		}

		public IntPtr MarshalManagedToNative (object obj)
		{
			DateTime dt = (DateTime) obj;
			int size = Marshal.SizeOf (typeof (int)) + GetNativeDataSize ();
			IntPtr ptr = Marshal.AllocCoTaskMem (size);
			IntPtr time_t_ptr = new IntPtr (ptr.ToInt32 () + Marshal.SizeOf (typeof(int)));
			int secs = ((int)dt.Subtract (local_epoch).TotalSeconds) + utc_offset;
			if (GetNativeDataSize () == 4)
				Marshal.WriteInt32 (time_t_ptr, secs);
			else if (GetNativeDataSize () == 8)
				Marshal.WriteInt64 (time_t_ptr, secs);
			else
				throw new Exception ("Unexpected native size for time_t.");
			return time_t_ptr;
		}

		public void CleanUpNativeData (IntPtr data)
		{
			IntPtr ptr = new IntPtr (data.ToInt32 () - Marshal.SizeOf (typeof (int)));
			Marshal.FreeCoTaskMem (ptr);
		}

		public object MarshalNativeToManaged (IntPtr data)
		{
			int secs;
			if (GetNativeDataSize () == 4)
				secs = Marshal.ReadInt32 (data);
			else if (GetNativeDataSize () == 8)
				secs = (int) Marshal.ReadInt64 (data);
			else
				throw new Exception ("Unexpected native size for time_t.");

			TimeSpan span = new TimeSpan (secs - utc_offset);
			return local_epoch.Add (span);
		}

		public void CleanUpManagedData (object obj) {}

		[DllImport ("gtksharpglue")]
		static extern int gtksharp_time_t_sizeof ();

		public int GetNativeDataSize ()
		{
			return gtksharp_time_t_sizeof ();
		}
	}
}

// Device.cs - customizations to Gdk.Device
//
// Authors: Manuel V. Santos  <mvsl@telefonica.net>
//
// Copyright (c) 2004 Manuel V. Santos
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

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Device {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gdk_device_free_history(IntPtr events, int n_events);
		static d_gdk_device_free_history gdk_device_free_history = FuncLoader.LoadFunction<d_gdk_device_free_history>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_device_free_history"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gdk_device_get_history(IntPtr device, IntPtr window, uint start, uint stop, out IntPtr events, out int n_events);
		static d_gdk_device_get_history gdk_device_get_history = FuncLoader.LoadFunction<d_gdk_device_get_history>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_device_get_history"));

		public TimeCoord[] GetHistory (Gdk.Window window, uint start, uint stop)
		{
			IntPtr coords_handle;
			int count;

			if (gdk_device_get_history (Handle, window.Handle, start, stop, out coords_handle, out count)) {
				TimeCoord[] result = new TimeCoord [count];
				for (int i = 0; i < count; i++) {
					IntPtr ptr = Marshal.ReadIntPtr (coords_handle, i + IntPtr.Size);
					result [i] = TimeCoord.New (ptr);
				}
				gdk_device_free_history (coords_handle, count);
				return result;
			} else
				return new TimeCoord [0];
		}
	}
}



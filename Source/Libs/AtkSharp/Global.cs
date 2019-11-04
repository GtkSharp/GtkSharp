// Global.cs - Atk Global class customizations
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2008 Novell, Inc.
//
// This code is inserted after the automatically generated code.
//
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

namespace Atk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Global {
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_atk_add_global_event_listener(GLib.Signal.EmissionHookNative hook, IntPtr event_type);
        static d_atk_add_global_event_listener atk_add_global_event_listener = FuncLoader.LoadFunction<d_atk_add_global_event_listener>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Atk), "atk_add_global_event_listener"));


        public static uint AddGlobalEventListener (GLib.Signal.EmissionHook hook, string event_type)
		{
			IntPtr native_event_type = GLib.Marshaller.StringToPtrGStrdup (event_type);
			uint id = atk_add_global_event_listener (new GLib.Signal.EmissionHookMarshaler (hook).Callback, native_event_type);
			GLib.Marshaller.Free (native_event_type);
			return id;
		}
	}
}


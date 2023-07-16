// Gdk.EventWindowState.cs - Custom WindowState event wrapper 
//
// Author:  Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2004-2009 Novell, Inc.
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

	public class EventWindowState : Event {

		public EventWindowState (IntPtr raw) : base (raw) {} 

		[StructLayout (LayoutKind.Sequential)]
		struct NativeStruct {
			EventType type;
            readonly IntPtr window;
            readonly sbyte send_event;
			public WindowState changed_mask;
			public WindowState new_window_state;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof(NativeStruct)); }
		}

		public WindowState ChangedMask {
			get { return Native.changed_mask; }
			set {
				NativeStruct native = Native;
				native.changed_mask = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public WindowState NewWindowState {
			get { return Native.new_window_state; }
			set {
				NativeStruct native = Native;
				native.new_window_state = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}
	}
}


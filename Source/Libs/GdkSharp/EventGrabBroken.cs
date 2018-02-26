// Gdk.EventGrabBroken.cs - Custom GrabBroken event wrapper 
//
// Author:  Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2005-2009 Novell, Inc.
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

	public class EventGrabBroken : Event {

		public EventGrabBroken (IntPtr raw) : base (raw) {} 

		[StructLayout(LayoutKind.Sequential)]
		struct NativeStruct {
			EventType type;
			IntPtr window;
			sbyte send_event;
			public bool keyboard;
			public bool _implicit;
			public IntPtr grab_window;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof(NativeStruct)); }
		}

		public bool Keyboard {
			get { return Native.keyboard; }
			set {
				NativeStruct native = Native;
				native.keyboard = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public bool Implicit {
			get { return Native._implicit; }
			set {
				NativeStruct native = Native;
				native._implicit = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public Window GrabWindow {
			get { return GLib.Object.GetObject(Native.grab_window, false) as Window; }
			set {
				NativeStruct native = Native;
				native.grab_window = value == null ? IntPtr.Zero : value.Handle;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}
	}
}


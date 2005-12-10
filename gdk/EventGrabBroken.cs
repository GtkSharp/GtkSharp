// Gdk.EventGrabBroken.cs - Custom GrabBroken event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2005 Novell, Inc.
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


#if GTK_SHARP_2_8
namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventGrabBroken : Event {

		[StructLayout(LayoutKind.Sequential)]
		struct NativeEventGrabBroken {
			EventType type;
			IntPtr window;
			sbyte send_event;
			public bool Keyboard;
			public bool Implicit;
			public IntPtr GrabWindowHandle;
		}

		NativeEventGrabBroken native_struct;

		public EventGrabBroken (IntPtr raw) : base (raw) 
		{
			native_struct = (NativeEventGrabBroken) Marshal.PtrToStructure (raw, typeof (NativeEventGrabBroken));
		} 

		public bool Keyboard {
			get {
				return native_struct.Keyboard;
			}
		}

		public bool Implicit {
			get {
				return native_struct.Implicit;
			}
		}

		public Window GrabWindow {
			get {
				return GLib.Object.GetObject(native_struct.GrabWindowHandle) as Window;
			}
		}
	}
}
#endif


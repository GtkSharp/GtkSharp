// Gdk.EventKey.cs - Custom key event wrapper 
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

	public class EventKey : Event {

		public EventKey (IntPtr raw) : base (raw) {} 

		[StructLayout (LayoutKind.Sequential)]
		struct NativeStruct {
			EventType type;
			IntPtr window;
			sbyte send_event;
			public uint time;
			public uint state;
			public uint keyval;
			int length;
			IntPtr _string;
			public ushort hardware_keycode;
			public byte group;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof(NativeStruct)); }
		}

		public byte Group {
			get { return Native.group; }
			set {
				NativeStruct native = Native;
				native.group = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public ushort HardwareKeycode {
			get { return Native.hardware_keycode; }
			set {
				NativeStruct native = Native;
				native.hardware_keycode = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public Key Key {
			get { return (Key) KeyValue; }
		}

		public uint KeyValue {
			get { return Native.keyval; }
			set {
				NativeStruct native = Native;
				native.keyval = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public ModifierType State {
			get { return (ModifierType) Native.state; }
			set {
				NativeStruct native = Native;
				native.state = (uint) value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public uint Time {
			get { return Native.time; }
			set {
				NativeStruct native = Native;
				native.time = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}
	}
}


// Gdk.EventScroll.cs - Custom scroll event wrapper 
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

	public class EventScroll : Event {

		public EventScroll (IntPtr raw) : base (raw) {} 

		[StructLayout (LayoutKind.Sequential)]
		struct NativeStruct {
			EventType type;
			IntPtr window;
			sbyte send_event;
			public uint time;
			public double x;
			public double y;
			public uint state;
			public ScrollDirection direction;
			public IntPtr device;
			public double x_root;
			public double y_root;
			public double delta_x;
			public double delta_y;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof(NativeStruct)); }
		}

		public Device Device {
			get { return GLib.Object.GetObject (Native.device, false) as Device; }
			set {
				NativeStruct native = Native;
				native.device = value == null ? IntPtr.Zero : value.Handle;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public ScrollDirection Direction {
			get { return Native.direction; }
			set {
				NativeStruct native = Native;
				native.direction = value;
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

		public double X {
			get { return Native.x; }
			set {
				NativeStruct native = Native;
				native.x = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public double XRoot {
			get { return Native.x_root; }
			set {
				NativeStruct native = Native;
				native.x_root = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public double DeltaX {
			get { return Native.delta_x; }
			set {
				NativeStruct native = Native;
				native.delta_x = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public double Y {
			get { return Native.y; }
			set {
				NativeStruct native = Native;
				native.y = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public double YRoot {
			get { return Native.y_root; }
			set {
				NativeStruct native = Native;
				native.y_root = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public double DeltaY {
			get { return Native.delta_y; }
			set {
				NativeStruct native = Native;
				native.delta_y = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}
	}
}


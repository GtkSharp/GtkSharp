// Gdk.EventCrossing.cs - Custom crossing event wrapper 
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

	public class EventCrossing : Event {

		public EventCrossing (IntPtr raw) : base (raw) {} 

		[StructLayout (LayoutKind.Sequential)]
		struct NativeStruct {
			EventType type;
			IntPtr window;
			sbyte send_event;
			public IntPtr subwindow;
			public uint time;
			public double x;
			public double y;
			public double x_root;
			public double y_root;
			public CrossingMode mode;
			public NotifyType detail;
			public bool focus;
			public uint state;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof(NativeStruct)); }
		}

		public NotifyType Detail {
			get { return Native.detail; }
			set {
				NativeStruct native = Native;
				native.detail = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public bool Focus {
			get { return Native.focus; }
			set {
				NativeStruct native = Native;
				native.focus = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public CrossingMode Mode {
			get { return Native.mode; }
			set {
				NativeStruct native = Native;
				native.mode = value;
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

		public Window SubWindow {
			get { return GLib.Object.GetObject (Native.subwindow, false) as Window; }
			set {
				NativeStruct native = Native;
				native.subwindow = value == null ? IntPtr.Zero : value.Handle;
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
	}
}


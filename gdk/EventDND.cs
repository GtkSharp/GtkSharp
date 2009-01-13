// Gdk.EventDND.cs - Custom dnd event wrapper 
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

	public class EventDND : Event {

		public EventDND (IntPtr raw) : base (raw) {} 

		[StructLayout (LayoutKind.Sequential)]
		struct NativeStruct {
			EventType type;
			IntPtr window;
			sbyte send_event;
			public IntPtr context;
			public uint time;
			public short x_root;
			public short y_root;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof(NativeStruct)); }
		}

		public DragContext Context {
			get { return GLib.Object.GetObject (Native.context, false) as DragContext; }
			set {
				NativeStruct native = Native;
				native.context = value == null ? IntPtr.Zero : value.Handle;
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

		public short XRoot {
			get { return Native.x_root; }
			set {
				NativeStruct native = Native;
				native.x_root = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public short YRoot {
			get { return Native.y_root; }
			set {
				NativeStruct native = Native;
				native.y_root = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}
	}
}


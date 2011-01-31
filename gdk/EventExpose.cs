// Gdk.EventExpose.cs - Custom expose event wrapper 
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

	public class EventExpose : Event {

		public EventExpose (IntPtr raw) : base (raw) {} 

		[StructLayout (LayoutKind.Sequential)]
		struct NativeStruct {
			EventType type;
			IntPtr window;
			sbyte send_event;
			public Rectangle area;
			public IntPtr region;
			public int count;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof(NativeStruct)); }
		}

		public Rectangle Area {
			get { return Native.area; }
			set {
				NativeStruct native = Native;
				native.area = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public Cairo.Region Region {
			get { return new Cairo.Region (Native.region); }
			set {
				NativeStruct native = Native;
				native.region = value == null ? IntPtr.Zero : value.Handle;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public int Count {
			get { return Native.count; }
			set {
				NativeStruct native = Native;
				native.count = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}
	}
}


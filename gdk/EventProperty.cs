// Gdk.EventProperty.cs - Custom property event wrapper 
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

	public class EventProperty : Event {

		public EventProperty (IntPtr raw) : base (raw) {} 

		[StructLayout (LayoutKind.Sequential)]
		struct NativeStruct {
			EventType type;
			IntPtr window;
			sbyte send_event;
			public IntPtr atom;
			public uint time;
			public uint state;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof(NativeStruct)); }
		}

		public Atom Atom {
			get { return GLib.Opaque.GetOpaque (Native.atom, typeof (Atom), false) as Atom; }
			set {
				NativeStruct native = Native;
				native.atom = value == null ? IntPtr.Zero : value.Handle;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public PropertyState State {
			get { return (PropertyState) Native.state; }
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


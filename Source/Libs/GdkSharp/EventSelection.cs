// Gdk.EventSelection.cs - Custom selection event wrapper 
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

	public class EventSelection : Event {

		public EventSelection (IntPtr raw) : base (raw) {} 

		[StructLayout (LayoutKind.Sequential)]
		struct NativeStruct {
			EventType type;
			IntPtr window;
			sbyte send_event;
			public IntPtr selection;
			public IntPtr target;
			public IntPtr property;
			public uint time;
			public uint requestor;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof(NativeStruct)); }
		}

		public Atom Property {
			get { return GLib.Opaque.GetOpaque (Native.property, typeof (Atom), false) as Atom; }
			set {
				NativeStruct native = Native;
				native.property = value == null ? IntPtr.Zero : value.Handle;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public uint Requestor {
			get { return Native.requestor; }
			set {
				NativeStruct native = Native;
				native.requestor = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public Atom Selection {
			get { return GLib.Opaque.GetOpaque (Native.selection, typeof (Atom), false) as Atom; }
			set {
				NativeStruct native = Native;
				native.selection = value == null ? IntPtr.Zero : value.Handle;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public Atom Target {
			get { return GLib.Opaque.GetOpaque (Native.target, typeof (Atom), false) as Atom; }
			set {
				NativeStruct native = Native;
				native.target = value.Handle;
				native.target = value == null ? IntPtr.Zero : value.Handle;
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


// Gdk.EventOwnerChange.cs - Custom OwnerChange event wrapper 
//
// Author:  Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2008-2009 Novell, Inc.
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

	public class EventOwnerChange : Event {

		public EventOwnerChange (IntPtr handle) : base (handle) {}

		struct NativeStruct {
			public Gdk.EventType type;
			public IntPtr window;
			public sbyte send_event;
			public uint owner;
			public Gdk.OwnerChange reason;
			public IntPtr selection;
			public uint time;
			public uint selection_time;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof (NativeStruct)); }
		}

		public uint Owner {
			get { return Native.owner; }
			set {
				NativeStruct native = Native;
				native.owner = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public OwnerChange Reason {
			get { return Native.reason; }
			set {
				NativeStruct native = Native;
				native.reason = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public Gdk.Atom Selection {
			get { 
				IntPtr sel = Native.selection;
				return sel == IntPtr.Zero ? null : (Gdk.Atom) GLib.Opaque.GetOpaque (sel, typeof (Gdk.Atom), false);
			}
			set {
				NativeStruct native = Native;
				native.selection = value == null ? IntPtr.Zero : value.Handle;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public uint SelectionTime {
			get { return Native.selection_time; }
			set {
				NativeStruct native = Native;
				native.selection_time = value;
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


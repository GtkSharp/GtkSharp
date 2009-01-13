// Gdk.EventClient.cs - Custom client event wrapper 
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

	public class EventClient : Event {

		public EventClient (IntPtr raw) : base (raw) {} 

		struct NativeStruct {
			EventType type;
			IntPtr window;
			sbyte send_event;
			public IntPtr message_type;
			public ushort data_format;
			public IntPtr data_as_long0;
			public IntPtr data_as_long1;
			public IntPtr data_as_long2;
			public IntPtr data_as_long3;
			public IntPtr data_as_long4;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (Handle, typeof (NativeStruct)); }
		}

		public ushort DataFormat {
			get { return Native.data_format; }
			set {
				NativeStruct native = Native;
				native.data_format = value;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		public Atom MessageType {
			get { 
				IntPtr msg_type = Native.message_type;
				return msg_type == IntPtr.Zero ? null : (Atom) GLib.Opaque.GetOpaque (msg_type, typeof (Atom), false);
			}
			set {
				NativeStruct native = Native;
				native.message_type = value == null ? IntPtr.Zero : value.Handle;
				Marshal.StructureToPtr (native, Handle, false);
			}
		}

		IntPtr DataPointer {
			get {
				int offset = Marshal.SizeOf (typeof (NativeStruct)) - 5 * IntPtr.Size;
				return new IntPtr (Handle.ToInt64 () + offset);
			}
		}

		public Array Data {
			get {
				switch (DataFormat) {
				case 8:
					byte[] b = new byte [20];
					Marshal.Copy (b, 0, DataPointer, 20);
					return b;
				case 16:
					short[] s = new short [10];
					Marshal.Copy (s, 0, DataPointer, 10);
					return s;
				case 32:
					IntPtr data_ptr = DataPointer;
					long[] l = new long [5];
					for (int i = 0; i < 5; i++)
						l [i] = (long) Marshal.ReadIntPtr (data_ptr, i * IntPtr.Size);
					return l;
				default:
					throw new Exception ("Invalid Data Format: " + DataFormat);
				}
			}
		}
	}
}


// Gdk.EventClient.cs - Custom client event wrapper 
//
// Author:  Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace Gdk {

	using System;
	using System.Runtime.InteropServices;

	public class EventClient : Event {

		[DllImport("gtksharpglue")]
		static extern uint gtksharp_gdk_event_client_get_time (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_gdk_event_client_get_message_type (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern ushort gtksharp_gdk_event_client_get_data_format (IntPtr evt);

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_gdk_event_client_get_data (IntPtr evt);

		public EventClient (IntPtr raw) : base (raw) {} 

		public Atom MessageType {
			get {
				return new Atom (gtksharp_gdk_event_client_get_message_type (Handle));
			}
		}

		public ushort DataFormat {
			get {
				return gtksharp_gdk_event_client_get_data_format (Handle);
			}
		}

		public Array Data {
			get {
				switch (DataFormat) {
				case 8:
					byte[] b = new byte [20];
					Marshal.Copy (b, 0, gtksharp_gdk_event_client_get_data (Handle), 20);
					return b;
				case 16:
					short[] s = new short [10];
					Marshal.Copy (s, 0, gtksharp_gdk_event_client_get_data (Handle), 10);
					return s;
				case 32:
					long[] l = new long [5];
					Marshal.Copy (l, 0, gtksharp_gdk_event_client_get_data (Handle), 5);
					return l;
				default:
					throw new Exception ("Invalid Data Format: " + DataFormat);
				}
			}
		}
	}
}


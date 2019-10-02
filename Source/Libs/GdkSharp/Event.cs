// Gdk.Event.cs - Custom event wrapper 
//
// Authors: Rachel Hestilow <hestilow@ximian.com> 
//          Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2002 Rachel Hestilow
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

	public class Event : GLib.IWrapper {

		IntPtr raw;

		public Event(IntPtr raw) 
		{
			this.raw = raw;
		}

		public IntPtr Handle {
			get { return raw; }
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_event_get_type();
		static d_gdk_event_get_type gdk_event_get_type = FuncLoader.LoadFunction<d_gdk_event_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_event_get_type"));

		public static GLib.GType GType {
			get { return new GLib.GType (gdk_event_get_type ()); }
		}

		[StructLayout (LayoutKind.Sequential)]
		struct NativeStruct {
			public EventType type;
			public IntPtr window;
			public sbyte send_event;
		}

		NativeStruct Native {
			get { return (NativeStruct) Marshal.PtrToStructure (raw, typeof(NativeStruct)); }
		}

		public EventType Type {
			get { return Native.type; }
			set {
				NativeStruct native = Native;
				native.type = value;
				Marshal.StructureToPtr (native, raw, false);
			}
		}

		public Window Window {
			get { return GLib.Object.GetObject (Native.window, false) as Window; }
			set {
				NativeStruct native = Native;
				native.window = value == null ? IntPtr.Zero : value.Handle;
				Marshal.StructureToPtr (native, raw, false);
			}
		}

		public bool SendEvent {
			get { return Native.send_event != 0; }
			set {
				NativeStruct native = Native;
				native.send_event = (sbyte) (value ? 1 : 0);
				Marshal.StructureToPtr (native, raw, false);
			}
		}

		public static Event New (IntPtr raw)
		{
			return GetEvent (raw);
		}

		public static Event GetEvent (IntPtr raw)
		{
			if (raw == IntPtr.Zero)
				return null;

			NativeStruct native = (NativeStruct) Marshal.PtrToStructure (raw, typeof(NativeStruct));
			switch (native.type) {
			case EventType.Expose:
				return new EventExpose (raw);
			case EventType.MotionNotify:
				return new EventMotion (raw);
			case EventType.ButtonPress:
			case EventType.TwoButtonPress:
			case EventType.ThreeButtonPress:
			case EventType.ButtonRelease:
				return new EventButton (raw);
			case EventType.KeyPress:
			case EventType.KeyRelease:
				return new EventKey (raw);
			case EventType.EnterNotify:
			case EventType.LeaveNotify:
				return new EventCrossing (raw);
			case EventType.FocusChange:
				return new EventFocus (raw);
			case EventType.Configure:
				return new EventConfigure (raw);
			case EventType.PropertyNotify:
				return new EventProperty (raw);
			case EventType.SelectionClear:
			case EventType.SelectionRequest:
			case EventType.SelectionNotify:
				return new EventSelection (raw);
			case EventType.ProximityIn:
			case EventType.ProximityOut:
				return new EventProximity (raw);
			case EventType.DragEnter:
			case EventType.DragLeave:
			case EventType.DragMotion:
			case EventType.DragStatus:
			case EventType.DropStart:
			case EventType.DropFinished:
				return new EventDND (raw);
			case EventType.VisibilityNotify:
				return new EventVisibility (raw);
			case EventType.Scroll:
				return new EventScroll (raw);
			case EventType.WindowState:
				return new EventWindowState (raw);
			case EventType.Setting:
				return new EventSetting (raw);
			case EventType.OwnerChange:
				return new EventOwnerChange (raw);
			case EventType.GrabBroken:
				return new EventGrabBroken (raw);
			case EventType.Map:
			case EventType.Unmap:
			case EventType.Delete:
			case EventType.Destroy:
			default:
				return new Gdk.Event (raw);
			}
		}
	}
}



// GLib.Signal.cs - signal marshaling class
//
// Authors: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2005 Novell, Inc.
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


namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	[Flags]
	internal enum SignalFlags {
		RunFirst = 1 << 0,
		RunLast = 1 << 1,
		RunCleanup = 1 << 2,
		NoRecurse = 1 << 3,
		Detailed = 1 << 4,
		Action = 1 << 5,
		NoHooks = 1 << 6
	}

	[StructLayout (LayoutKind.Sequential)]
	internal struct InvocationHint {
		public uint signal_id;
		public uint detail;
		public SignalFlags run_type;
	}

	public class Signal {

		GCHandle gc_handle;
		IntPtr handle;
		string name;
		uint before_id = UInt32.MaxValue;
		uint after_id = UInt32.MaxValue;
		Delegate marshaler;

		static DestroyNotify notify = new DestroyNotify (OnNativeDestroy);
		[CDeclCallback]
		delegate void DestroyNotify (IntPtr data, IntPtr obj);
		static void OnNativeDestroy (IntPtr data, IntPtr obj)
		{
			GCHandle gch = (GCHandle) data;
			Signal s = gch.Target as Signal;
			s.DisconnectHandler (s.before_id);
			s.DisconnectHandler (s.after_id);
			gch.Free ();
		}

		private Signal (GLib.Object obj, string signal_name, Delegate marshaler)
		{
			handle = obj.Handle;
			name = signal_name;
			this.marshaler = marshaler;
			gc_handle = GCHandle.Alloc (this);
			IntPtr native_key = GLib.Marshaller.StringToPtrGStrdup (name + "_signal_marshaler");
			g_object_set_data_full (handle, native_key, (IntPtr) gc_handle, notify);
			GLib.Marshaller.Free (native_key);
		}

		public static Signal Lookup (GLib.Object obj, string name)
		{
			return Lookup (obj, name, EventHandlerDelegate);
		}

		public static Signal Lookup (GLib.Object obj, string name, Delegate marshaler)
		{
			IntPtr native_key = GLib.Marshaller.StringToPtrGStrdup (name + "_signal_marshaler");
			IntPtr data = g_object_get_data (obj.Handle, native_key);
			GLib.Marshaller.Free (native_key);
			if (data == IntPtr.Zero)
				return new Signal (obj, name, marshaler);

			GCHandle gch = (GCHandle) data;
			return gch.Target as Signal;
		}

		Delegate before_handler;
		Delegate after_handler;

		public Delegate Handler {
			get {
				InvocationHint hint = (InvocationHint) Marshal.PtrToStructure (g_signal_get_invocation_hint (handle), typeof (InvocationHint));
				if (hint.run_type == SignalFlags.RunFirst)
					return before_handler;
				else
					return after_handler;
			}
		}

		uint Connect (int flags)
		{
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			uint id = g_signal_connect_data (handle, native_name, marshaler, (IntPtr) gc_handle, IntPtr.Zero, flags);
			GLib.Marshaller.Free (native_name);
			return id;
		}

		public void AddDelegate (Delegate d)
		{
			if (d.Method.IsDefined (typeof (ConnectBeforeAttribute), false)) {
				if (before_handler == null) {
					before_handler = d;
					before_id = Connect (0);
				} else
					before_handler = Delegate.Combine (before_handler, d);
			} else {
				if (after_handler == null) {
					after_handler = d;
					after_id = Connect (1);
				} else
					after_handler = Delegate.Combine (after_handler, d);
			}
		}

		public void RemoveDelegate (Delegate d)
		{
			if (d.Method.IsDefined (typeof (ConnectBeforeAttribute), false)) {
				before_handler = Delegate.Remove (before_handler, d);
				if (before_handler == null) {
					DisconnectHandler (before_id);
					before_id = UInt32.MaxValue;
				}
			} else {
				after_handler = Delegate.Remove (after_handler, d);
				if (after_handler == null) {
					DisconnectHandler (after_id);
					after_id = UInt32.MaxValue;
				}
			}

			if (after_id == UInt32.MaxValue && before_id == UInt32.MaxValue)
				DisconnectObject ();
		}

		void DisconnectObject ()
		{
			IntPtr native_key = GLib.Marshaller.StringToPtrGStrdup (name + "_signal_marshaler");
			g_object_set_data (handle, native_key, IntPtr.Zero);
			GLib.Marshaller.Free (native_key);
		}

		void DisconnectHandler (uint handler_id)
		{
			if (handler_id != UInt32.MaxValue && g_signal_handler_is_connected (handle, handler_id))
				g_signal_handler_disconnect (handle, handler_id);
		}

		[CDeclCallback]
		delegate void voidObjectDelegate (IntPtr handle, IntPtr gch);

		static void voidObjectCallback (IntPtr handle, IntPtr gch)
		{
			Signal sig = ((GCHandle) gch).Target as Signal;
			if (sig == null)
				throw new Exception ("Unknown signal class GC handle received.");

			EventHandler handler = (EventHandler) sig.Handler;
			handler (Object.GetObject (handle), EventArgs.Empty);
		}

		static voidObjectDelegate event_handler_delegate;
		static voidObjectDelegate EventHandlerDelegate {
			get {
				if (event_handler_delegate == null)
					event_handler_delegate = new voidObjectDelegate (voidObjectCallback);
				return event_handler_delegate;
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_object_get_data (IntPtr instance, IntPtr key);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_set_data (IntPtr instance, IntPtr key, IntPtr data);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_set_data_full (IntPtr instance, IntPtr key, IntPtr data, DestroyNotify notify);

		[DllImport("libgobject-2.0-0.dll")]
		static extern uint g_signal_connect_data(IntPtr obj, IntPtr name, Delegate cb, IntPtr gc_handle, IntPtr dummy, int flags);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_signal_get_invocation_hint (IntPtr instance);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_signal_handler_disconnect (IntPtr instance, uint handler);

		[DllImport("libgobject-2.0-0.dll")]
		static extern bool g_signal_handler_is_connected (IntPtr instance, uint handler);
	}
}


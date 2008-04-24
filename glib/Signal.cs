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
	using System.Collections;
	using System.Runtime.InteropServices;

	[Flags]
	public enum ConnectFlags {
		After = 1 << 0,
		Swapped = 1 << 1,
	}

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
		ToggleRef tref;
		string name;
		uint before_id = UInt32.MaxValue;
		uint after_id = UInt32.MaxValue;
		Delegate marshaler;

		~Signal ()
		{
			gc_handle.Free ();
		}

		private Signal (GLib.Object obj, string signal_name, Delegate marshaler)
		{
			tref = obj.ToggleRef;
			name = signal_name;
			this.marshaler = marshaler;
			gc_handle = GCHandle.Alloc (this, GCHandleType.Weak);
			tref.Signals [name] = this;
		}

		internal void Free ()
		{
			DisconnectHandler (before_id);
			DisconnectHandler (after_id);
			before_handler = after_handler = marshaler = null;
			gc_handle.Free ();
			GC.SuppressFinalize (this);
		}

		public static Signal Lookup (GLib.Object obj, string name)
		{
			return Lookup (obj, name, EventHandlerDelegate);
		}

		public static Signal Lookup (GLib.Object obj, string name, Delegate marshaler)
		{
			Signal result = obj.ToggleRef.Signals [name] as Signal;
			if (result == null)
				result = new Signal (obj, name, marshaler);
			return result;
		}

		Delegate before_handler;
		Delegate after_handler;

		public Delegate Handler {
			get {
				InvocationHint hint = (InvocationHint) Marshal.PtrToStructure (g_signal_get_invocation_hint (tref.Handle), typeof (InvocationHint));
				if (hint.run_type == SignalFlags.RunFirst)
					return before_handler;
				else
					return after_handler;
			}
		}

		uint Connect (int flags)
		{
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			uint id = g_signal_connect_data (tref.Handle, native_name, marshaler, (IntPtr) gc_handle, IntPtr.Zero, flags);
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
				tref.Signals.Remove (name);
		}

		void DisconnectHandler (uint handler_id)
		{
			if (handler_id != UInt32.MaxValue && g_signal_handler_is_connected (tref.Handle, handler_id))
				g_signal_handler_disconnect (tref.Handle, handler_id);
		}

		[CDeclCallback]
		delegate void voidObjectDelegate (IntPtr handle, IntPtr gch);

		static void voidObjectCallback (IntPtr handle, IntPtr data)
		{
			try {
				if (data == IntPtr.Zero)
					return;
				GCHandle gch = (GCHandle) data;
				if (gch.Target == null)
					return;
				Signal sig = gch.Target as Signal;
				if (sig == null) {
					ExceptionManager.RaiseUnhandledException (new Exception ("Unknown signal class GC handle received."), false);
					return;
				}

				EventHandler handler = (EventHandler) sig.Handler;
				handler (Object.GetObject (handle), EventArgs.Empty);
			} catch (Exception e) {
				ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		static voidObjectDelegate event_handler_delegate;
		static voidObjectDelegate EventHandlerDelegate {
			get {
				if (event_handler_delegate == null)
					event_handler_delegate = new voidObjectDelegate (voidObjectCallback);
				return event_handler_delegate;
			}
		}
		
		public static object Emit (GLib.Object instance, string signal_name, string detail, params object[] args)
		{
			uint signal_id = GetSignalId (signal_name, instance);
			uint gquark = GetGQuarkFromString (detail);
			GLib.Value[] vals = new GLib.Value [args.Length + 1];
			GLib.ValueArray inst_and_params = new GLib.ValueArray ((uint) args.Length + 1);
			
			vals [0] = new GLib.Value (instance);
			inst_and_params.Append (vals [0]);
			for (int i = 1; i < vals.Length; i++) {
				vals [i] = new GLib.Value (args [i - 1]);
				inst_and_params.Append (vals [i]);
			}

			GLib.Value ret = GLib.Value.Empty;

			g_signal_emitv (inst_and_params.ArrayPtr, signal_id, gquark, ref ret);
			object ret_obj = ret.Val;
			
			foreach (GLib.Value val in vals)
				val.Dispose ();
			ret.Dispose ();
			
			return ret_obj;
		}
		
		private static uint GetGQuarkFromString (string str) {
			IntPtr native_string = GLib.Marshaller.StringToPtrGStrdup (str);
			uint ret = g_quark_from_string (native_string);
			GLib.Marshaller.Free (native_string);
			return ret;
		}

		private static uint GetSignalId (string signal_name, GLib.Object obj)
		{
			IntPtr typeid = gtksharp_get_type_id (obj.Handle);
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (signal_name);
			uint signal_id = g_signal_lookup (native_name, typeid);
			GLib.Marshaller.Free (native_name);
			return signal_id;
		}
		
		[DllImport("libgobject-2.0-0.dll")]
		static extern uint g_signal_connect_data(IntPtr obj, IntPtr name, Delegate cb, IntPtr gc_handle, IntPtr dummy, int flags);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_signal_get_invocation_hint (IntPtr instance);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_signal_handler_disconnect (IntPtr instance, uint handler);

		[DllImport("libgobject-2.0-0.dll")]
		static extern bool g_signal_handler_is_connected (IntPtr instance, uint handler);
		
		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_signal_emitv (IntPtr instance_and_params, uint signal_id, uint gquark_detail, ref GLib.Value return_value);
		
		[DllImport("libgobject-2.0-0.dll")]
		static extern uint g_signal_lookup (IntPtr name, IntPtr itype);
		
		//better not to expose g_quark_from_static_string () due to memory allocation issues
		[DllImport("libglib-2.0-0.dll")]
		static extern uint g_quark_from_string (IntPtr str);
		
		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_get_type_id (IntPtr raw);
	}
}


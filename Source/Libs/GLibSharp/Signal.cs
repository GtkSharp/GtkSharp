// GLib.Signal.cs - signal marshaling class
//
// Authors: Mike Kestner <mkestner@novell.com>
//          Andrés G. Aragoneses <aaragoneses@novell.com>
//
// Copyright (c) 2005,2008 Novell, Inc.
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
	public enum ConnectFlags {
		After = 1 << 0,
		Swapped = 1 << 1,
	}

	public class Signal {

		[Flags]
		public enum Flags {
			RunFirst = 1 << 0,
			RunLast = 1 << 1,
			RunCleanup = 1 << 2,
			NoRecurse = 1 << 3,
			Detailed = 1 << 4,
			Action = 1 << 5,
			NoHooks = 1 << 6
		}

		[StructLayout (LayoutKind.Sequential)]
		public struct InvocationHint {
			public uint signal_id;
			public uint detail;
			public Flags run_type;
		}

		[StructLayout (LayoutKind.Sequential)]
		struct Query {
			public uint signal_id;
			public IntPtr signal_name;
			public IntPtr itype;
			public Flags signal_flags;
			public IntPtr return_type;
			public uint n_params;
			public IntPtr param_types;
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate bool EmissionHookNative (ref InvocationHint hint, uint n_pvals, IntPtr pvals, IntPtr data);

		public delegate bool EmissionHook (InvocationHint ihint, object[] inst_and_param_values);

		public class EmissionHookMarshaler {

			EmissionHook handler;
			EmissionHookNative cb;
			IntPtr user_data;
			GCHandle gch;

			public EmissionHookMarshaler (EmissionHook handler)
			{
				this.handler = handler;
				cb = new EmissionHookNative (NativeCallback);
				gch = GCHandle.Alloc (this);
			}

			public EmissionHookMarshaler (EmissionHookNative callback, IntPtr user_data)
			{
				cb = callback;
				this.user_data = user_data;
				handler = new EmissionHook (NativeInvoker);
			}

			bool NativeCallback (ref InvocationHint hint, uint n_pvals, IntPtr pvals_ptr, IntPtr data)
			{
				object[] pvals = new object [n_pvals];
				for (int i = 0; i < n_pvals; i++) {
					IntPtr p = new IntPtr ((long) pvals_ptr + i * Marshal.SizeOf (typeof (Value)));
					Value v = (Value) Marshal.PtrToStructure (p, typeof (Value));
					pvals [i] = v.Val;
				}
				bool result = handler (hint, pvals);
				if (!result)
					gch.Free ();
				return result;
			}

			public EmissionHookNative Callback {
				get {
					return cb;
				}
			}

			bool NativeInvoker (InvocationHint ihint, object[] pvals)
			{
				int val_sz = Marshal.SizeOf (typeof (Value));
				IntPtr buf = Marshal.AllocHGlobal (pvals.Length * val_sz);
				Value[] vals = new Value [pvals.Length];
				for (int i = 0; i < pvals.Length; i++) {
					vals [i] = new Value (pvals [i]);
					IntPtr p = new IntPtr ((long) buf + i * val_sz);
					Marshal.StructureToPtr (vals [i], p, false);
				}
				bool result = cb (ref ihint, (uint) pvals.Length, buf, user_data);
				foreach (Value v in vals)
					v.Dispose ();
				Marshal.FreeHGlobal (buf);
				return result;	
			}

			public EmissionHook Invoker {
				get {
					return handler;
				}
			}
		}

		GLib.Object obj;
		string name;
		Type args_type;
		SignalClosure before_closure;
		SignalClosure after_closure;
		Delegate after_handler;
		Delegate before_handler;
		Delegate marshaler;

		internal Signal (GLib.Object obj, string name, Delegate marshaler)
		{
			this.obj = obj;
			this.name = name;
			this.marshaler = marshaler;
		}

		internal Signal (GLib.Object obj, string name, Type args_type)
		{
			this.obj = obj;
			this.name = name;
			this.args_type = args_type;
		}

		internal void Free ()
		{
			if (before_closure != null)
				before_closure.Dispose ();
			if (after_closure != null)
				after_closure.Dispose ();
			GC.SuppressFinalize (this);
		}

		void ClosureDisposedCB (object o, EventArgs args)
		{
			if (o == before_closure) {
				before_closure.Disposed -= new EventHandler (ClosureDisposedHandler);
				before_closure.Invoked -= new ClosureInvokedHandler (ClosureInvokedCB);
				before_closure = null;
				before_handler = null;
			} else if (o == after_closure) {
				after_closure.Disposed -= new EventHandler (ClosureDisposedHandler);
				after_closure.Invoked -= new ClosureInvokedHandler (ClosureInvokedCB);
				after_closure = null;
				after_handler = null;
			}
		}

		EventHandler closure_disposed_cb;
		EventHandler ClosureDisposedHandler {
			get {
				if (closure_disposed_cb == null)
					closure_disposed_cb = new EventHandler (ClosureDisposedCB);
				return closure_disposed_cb;
			}
		}

		void ClosureInvokedCB (object o, ClosureInvokedArgs args)
		{
			Delegate handler;
			if (o == before_closure)
				handler = before_handler;
			else
				handler = after_handler;

			if (handler != null)
				handler.DynamicInvoke (new object[] {args.Target, args.Args});
		}

		ClosureInvokedHandler closure_invoked_cb;
		ClosureInvokedHandler ClosureInvokedHandler {
			get {
				if (closure_invoked_cb == null)
					closure_invoked_cb = new ClosureInvokedHandler (ClosureInvokedCB);
				return closure_invoked_cb;
			}
		}

		public Delegate Handler {
			get {
				InvocationHint hint = (InvocationHint) Marshal.PtrToStructure (g_signal_get_invocation_hint (obj.Handle), typeof (InvocationHint));
				if (hint.run_type == Flags.RunFirst)
					return before_handler;
				else
					return after_handler;
			}
		}

		public void AddDelegate (Delegate d)
		{
			if (args_type == null)
				args_type = d.Method.GetParameters ()[1].ParameterType;

			if (d.Method.IsDefined (typeof (ConnectBeforeAttribute), false)) {
				before_handler = Delegate.Combine (before_handler, d);
				if (before_closure == null) {
					if (marshaler == null)
						before_closure = new SignalClosure (obj.Handle, name, args_type);
					else
						before_closure = new SignalClosure (obj.Handle, name, marshaler, this);
					before_closure.Disposed += ClosureDisposedHandler;
					before_closure.Invoked += ClosureInvokedHandler;
					before_closure.Connect (false);
				}
			} else {
				after_handler = Delegate.Combine (after_handler, d);
				if (after_closure == null) {
					if (marshaler == null)
						after_closure = new SignalClosure (obj.Handle, name, args_type);
					else
						after_closure = new SignalClosure (obj.Handle, name, marshaler, this);
					after_closure.Disposed += ClosureDisposedHandler;
					after_closure.Invoked += ClosureInvokedHandler;
					after_closure.Connect (true);
				}
			}
		}

		public void RemoveDelegate (Delegate d)
		{
			if (d.Method.IsDefined (typeof (ConnectBeforeAttribute), false)) {
				before_handler = Delegate.Remove (before_handler, d);
				if (before_handler == null && before_closure != null) {
					before_closure.Dispose ();
					before_closure = null;
				}
			} else {
				after_handler = Delegate.Remove (after_handler, d);
				if (after_handler == null && after_closure != null) {
					after_closure.Dispose ();
					after_closure = null;
				}
			}
		}

		// format: children-changed::add
		private static void ParseSignalDetail (string signal_detail, out string signal_name, out uint gquark)
		{
			//can't use String.Split because it doesn't accept a string arg (only char) in the 1.x profile
			int link_pos = signal_detail.IndexOf ("::");
			if (link_pos < 0) {
				gquark = 0;
				signal_name = signal_detail;
			} else if (link_pos == 0) {
				throw new FormatException ("Invalid detailed signal: " + signal_detail);
			} else {
				signal_name = signal_detail.Substring (0, link_pos);
				gquark = GetGQuarkFromString (signal_detail.Substring (link_pos + 2));
			}
		}
		
		public static object Emit (GLib.Object instance, string detailed_signal, params object[] args)
		{
			uint gquark, signal_id;
			string signal_name;
			ParseSignalDetail (detailed_signal, out signal_name, out gquark);
			signal_id = GetSignalId (signal_name, instance);
			if (signal_id <= 0)
				throw new ArgumentException ("Invalid signal name: " + signal_name);
			GLib.Value[] vals = new GLib.Value [args.Length + 1];
			GLib.ValueArray inst_and_params = new GLib.ValueArray ((uint) args.Length + 1);
			
			vals [0] = new GLib.Value (instance);
			inst_and_params.Append (vals [0]);
			for (int i = 1; i < vals.Length; i++) {
				vals [i] = new GLib.Value (args [i - 1]);
				inst_and_params.Append (vals [i]);
			}

			object ret_obj = null;
			Query query;
			g_signal_query (signal_id, out query);
			if (query.return_type != GType.None.Val) {
				GLib.Value ret = GLib.Value.Empty;
				g_signal_emitv (inst_and_params.ArrayPtr, signal_id, gquark, ref ret);
				ret_obj = ret.Val;
				ret.Dispose ();
			} else
				g_signal_emitv2 (inst_and_params.ArrayPtr, signal_id, gquark, IntPtr.Zero);
			
			foreach (GLib.Value val in vals)
				val.Dispose ();

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
			IntPtr typeid = GType.ValFromInstancePtr (obj.Handle);
			return GetSignalId (signal_name, typeid);
		}
		
		private static uint GetSignalId (string signal_name, IntPtr typeid)
		{
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (signal_name);
			uint signal_id = g_signal_lookup (native_name, typeid);
			GLib.Marshaller.Free (native_name);
			return signal_id;
		}
		
		public static ulong AddEmissionHook (string detailed_signal, GLib.GType type, EmissionHook handler_func)
		{
			uint gquark;
			string signal_name;
			ParseSignalDetail (detailed_signal, out signal_name, out gquark);
			uint signal_id = GetSignalId (signal_name, type.Val);
			if (signal_id <= 0)
				throw new Exception ("Invalid signal name: " + signal_name);
			return g_signal_add_emission_hook (signal_id, gquark, new EmissionHookMarshaler (handler_func).Callback, IntPtr.Zero, IntPtr.Zero);
		}
		
		internal static void OverrideDefaultHandler (GType gtype, string name, Delegate cb)
		{
			IntPtr closure = g_cclosure_new (cb, IntPtr.Zero, IntPtr.Zero);
			gtype.EnsureClass ();
			uint id = GetSignalId (name, gtype.Val);
			g_signal_override_class_closure (id, gtype.Val, closure);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_cclosure_new(Delegate cb, IntPtr data, IntPtr notify);
		static d_g_cclosure_new g_cclosure_new = FuncLoader.LoadFunction<d_g_cclosure_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_cclosure_new"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_signal_get_invocation_hint(IntPtr instance);
		static d_g_signal_get_invocation_hint g_signal_get_invocation_hint = FuncLoader.LoadFunction<d_g_signal_get_invocation_hint>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_signal_get_invocation_hint"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_signal_emitv(IntPtr instance_and_params, uint signal_id, uint gquark_detail, ref GLib.Value return_value);
		static d_g_signal_emitv g_signal_emitv = FuncLoader.LoadFunction<d_g_signal_emitv>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_signal_emitv"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_signal_emitv2(IntPtr instance_and_params, uint signal_id, uint gquark_detail, IntPtr return_value);
		static d_g_signal_emitv2 g_signal_emitv2 = FuncLoader.LoadFunction<d_g_signal_emitv2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_signal_emitv"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_signal_lookup(IntPtr name, IntPtr itype);
		static d_g_signal_lookup g_signal_lookup = FuncLoader.LoadFunction<d_g_signal_lookup>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_signal_lookup"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_signal_override_class_closure(uint id, IntPtr gtype, IntPtr closure);
		static d_g_signal_override_class_closure g_signal_override_class_closure = FuncLoader.LoadFunction<d_g_signal_override_class_closure>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_signal_override_class_closure"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_signal_query(uint signal_id, out Query query);
		static d_g_signal_query g_signal_query = FuncLoader.LoadFunction<d_g_signal_query>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_signal_query"));

		//better not to expose g_quark_from_static_string () due to memory allocation issues
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_quark_from_string(IntPtr str);
		static d_g_quark_from_string g_quark_from_string = FuncLoader.LoadFunction<d_g_quark_from_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_quark_from_string"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate ulong d_g_signal_add_emission_hook(uint signal_id, uint gquark_detail, EmissionHookNative hook_func, IntPtr hook_data, IntPtr data_destroy);
		static d_g_signal_add_emission_hook g_signal_add_emission_hook = FuncLoader.LoadFunction<d_g_signal_add_emission_hook>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_signal_add_emission_hook"));

	}
}



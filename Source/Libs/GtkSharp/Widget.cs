//
// Gtk.Widget.cs - Gtk Widget class customizations
//
// Authors: Rachel Hestilow <hestilow@ximian.com>,
//          Brad Taylor <brad@getcoded.net>
//
// Copyright (C) 2007 Brad Taylor
// Copyright (C) 2002 Rachel Hestilow 
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

namespace Gtk {

	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	public partial class Widget {

		[Obsolete ("Replaced by Window property.")]
		public Gdk.Window GdkWindow {
			get { return Window; }
			set { Window = value; }
		}

		public void AddAccelerator (string accel_signal, AccelGroup accel_group, AccelKey accel_key)
		{
			this.AddAccelerator (accel_signal, accel_group, (uint) accel_key.Key, accel_key.AccelMods, (Gtk.AccelFlags) accel_key.AccelFlags);
		}

		/*
		public int FocusLineWidth {
			get {
				return (int) StyleGetProperty ("focus-line-width");
			}
		}
		*/

		struct GClosure {
			long fields;
			IntPtr marshaler;
			IntPtr data;
			IntPtr notifiers;
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void ClosureMarshal (IntPtr closure, IntPtr return_val, uint n_param_vals, IntPtr param_values, IntPtr invocation_hint, IntPtr marshal_data);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_closure_new_simple(int closure_size, IntPtr dummy);
		static d_g_closure_new_simple g_closure_new_simple = FuncLoader.LoadFunction<d_g_closure_new_simple>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_closure_new_simple"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_closure_set_marshal(IntPtr closure, ClosureMarshal marshaler);
		static d_g_closure_set_marshal g_closure_set_marshal = FuncLoader.LoadFunction<d_g_closure_set_marshal>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_closure_set_marshal"));

		static IntPtr CreateClosure (ClosureMarshal marshaler) {
			IntPtr raw_closure = g_closure_new_simple (Marshal.SizeOf (typeof (GClosure)), IntPtr.Zero);
			g_closure_set_marshal (raw_closure, marshaler);
			return raw_closure;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_signal_newv(IntPtr signal_name, IntPtr gtype, GLib.Signal.Flags signal_flags, IntPtr closure, IntPtr accumulator, IntPtr accu_data, IntPtr c_marshaller, IntPtr return_type, uint n_params, [MarshalAs (UnmanagedType.LPArray)] IntPtr[] param_types);
		static d_g_signal_newv g_signal_newv = FuncLoader.LoadFunction<d_g_signal_newv>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_signal_newv"));

		static uint RegisterSignal (string signal_name, GLib.GType gtype, GLib.Signal.Flags signal_flags, GLib.GType return_type, GLib.GType[] param_types, ClosureMarshal marshaler)
		{
			IntPtr[] native_param_types = new IntPtr [param_types.Length];
			for (int parm_idx = 0; parm_idx < param_types.Length; parm_idx++)
				native_param_types [parm_idx] = param_types [parm_idx].Val;

			IntPtr native_signal_name = GLib.Marshaller.StringToPtrGStrdup (signal_name);
			try {
				return g_signal_newv (native_signal_name, gtype.Val, signal_flags, CreateClosure (marshaler), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, return_type.Val, (uint) param_types.Length, native_param_types);
			} finally {
				GLib.Marshaller.Free (native_signal_name);
			}
		}

		static void ActivateMarshal_cb (IntPtr raw_closure, IntPtr return_val, uint n_param_vals, IntPtr param_values, IntPtr invocation_hint, IntPtr marshal_data)
		{
			try {
				GLib.Value inst_val = (GLib.Value) Marshal.PtrToStructure (param_values, typeof (GLib.Value));
				Widget inst;
				try {
					inst = inst_val.Val as Widget;
				} catch (GLib.MissingIntPtrCtorException) {
					return;
				}
				inst.OnActivate ();
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		static ClosureMarshal ActivateMarshalCallback;

		static void ConnectActivate (GLib.GType gtype)
		{
			if (ActivateMarshalCallback == null)
				ActivateMarshalCallback = new ClosureMarshal (ActivateMarshal_cb);

			unsafe {
				uint* raw_ptr = (uint*)(((long) gtype.GetClassPtr()) + (long) class_abi.GetFieldOffset("activate_signal"));

				*raw_ptr = RegisterSignal ("activate_signal", gtype, GLib.Signal.Flags.RunLast, GLib.GType.None,
						new GLib.GType [0], ActivateMarshalCallback);
			}
		}

		[GLib.DefaultSignalHandler (Type=typeof (Gtk.Widget), ConnectionMethod="ConnectActivate")]
		protected virtual void OnActivate ()
		{
		}

		private class BindingInvoker {
			System.Reflection.MethodInfo mi;
			object[] parms;

			public BindingInvoker (System.Reflection.MethodInfo mi, object[] parms)
			{
				this.mi = mi;
				this.parms = parms;
			}

			public void Invoke (Widget w)
			{
				mi.Invoke (w, parms);
			}
		}

		/* As gtk_binding_entry_add_signall only allows passing long, double and string parameters
		 * to the specified signal, we cannot pass a pointer to the BindingInvoker directly to the signal.
		 * Instead, the signal takes the index of the BindingInvoker in binding_invokers.
		 */
		static IList<BindingInvoker> binding_invokers;

		static void BindingMarshal_cb (IntPtr raw_closure, IntPtr return_val, uint n_param_vals, IntPtr param_values, IntPtr invocation_hint, IntPtr marshal_data)
		{
			try {
				GLib.Value[] inst_and_params = new GLib.Value [n_param_vals];
				int gvalue_size = Marshal.SizeOf (typeof (GLib.Value));
				for (int idx = 0; idx < n_param_vals; idx++)
					inst_and_params [idx] = (GLib.Value) Marshal.PtrToStructure (new IntPtr (param_values.ToInt64 () + idx * gvalue_size), typeof (GLib.Value));

				Widget w = inst_and_params [0].Val as Widget;
				BindingInvoker invoker = binding_invokers [(int) (long) inst_and_params [1]];
				invoker.Invoke (w);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		static ClosureMarshal binding_delegate;
		static ClosureMarshal BindingDelegate {
			get {
				if (binding_delegate == null)
					binding_delegate = new ClosureMarshal (BindingMarshal_cb);
				return binding_delegate;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_binding_set_by_class(IntPtr class_ptr);
		static d_gtk_binding_set_by_class gtk_binding_set_by_class = FuncLoader.LoadFunction<d_gtk_binding_set_by_class>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_binding_set_by_class"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_binding_entry_add_signall(IntPtr binding_set, uint keyval, Gdk.ModifierType modifiers, IntPtr signal_name, IntPtr binding_args);
		static d_gtk_binding_entry_add_signall gtk_binding_entry_add_signall = FuncLoader.LoadFunction<d_gtk_binding_entry_add_signall>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_binding_entry_add_signall"));

		[StructLayout(LayoutKind.Sequential)]
		struct GtkBindingArg {
			public IntPtr arg_type;
			public GtkBindingArgData data;
		}

		[StructLayout(LayoutKind.Explicit)]
		struct GtkBindingArgData {
		#if WIN64LONGS
			[FieldOffset (0)] public int long_data;
		#else
			[FieldOffset (0)] public IntPtr long_data;
		#endif
			[FieldOffset (0)] public double double_data;
			[FieldOffset (0)] public IntPtr string_data;
		}

		static void ClassInit (GLib.GType gtype, Type t)
		{
			object[] attrs = t.GetCustomAttributes (typeof (BindingAttribute), true);
			if (attrs.Length == 0) return;

			string signame = t.Name.Replace (".", "_") + "_bindings";
			IntPtr native_signame = GLib.Marshaller.StringToPtrGStrdup (signame);
			RegisterSignal (signame, gtype, GLib.Signal.Flags.RunLast | GLib.Signal.Flags.Action, GLib.GType.None, new GLib.GType[] {GLib.GType.Long}, BindingDelegate);

			if (binding_invokers == null)
				binding_invokers = new List<BindingInvoker> ();

			foreach (BindingAttribute attr in attrs) {
				System.Reflection.MethodInfo mi = t.GetMethod (attr.Handler, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
				if (mi == null)
					throw new Exception ("Instance method " + attr.Handler + " not found in " + t);

				GtkBindingArg arg = new GtkBindingArg ();
				arg.arg_type = GLib.GType.Long.Val;

				var bi = new BindingInvoker (mi, attr.Parms);
				binding_invokers.Add (bi);
				int binding_invoker_idx = binding_invokers.IndexOf (bi);
#if WIN64LONGS
				arg.data.long_data = binding_invoker_idx;
#else
				arg.data.long_data = new IntPtr (binding_invoker_idx);
#endif

				GLib.SList binding_args = new GLib.SList (new object[] {arg}, typeof (GtkBindingArg), false, false);
				gtk_binding_entry_add_signall (gtk_binding_set_by_class (gtype.GetClassPtr ()), (uint) attr.Key, attr.Mod, native_signame, binding_args.Handle);
				binding_args.Dispose ();
			}
			GLib.Marshaller.Free (native_signame);
		}

		public object StyleGetProperty (string property_name)
		{
			GLib.Value value;
			try {
				value = StyleGetPropertyValue (property_name);
			} catch (ArgumentException) {
				return null;
			}
			object ret = value.Val;
			value.Dispose ();
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_widget_class_find_style_property(IntPtr class_ptr, IntPtr property_name);
		static d_gtk_widget_class_find_style_property gtk_widget_class_find_style_property = FuncLoader.LoadFunction<d_gtk_widget_class_find_style_property>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_class_find_style_property"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_widget_style_get_property(IntPtr inst, IntPtr property_name, ref GLib.Value value);
		static d_gtk_widget_style_get_property gtk_widget_style_get_property = FuncLoader.LoadFunction<d_gtk_widget_style_get_property>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_style_get_property"));

		internal GLib.Value StyleGetPropertyValue (string property_name)
		{
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (property_name);
			try {
				IntPtr pspec_ptr = gtk_widget_class_find_style_property (this.LookupGType ().GetClassPtr (), native_name);
				if (pspec_ptr == IntPtr.Zero)
					throw new ArgumentException (String.Format ("Cannot find style property \"{0}\"", property_name));

				GLib.Value value = new GLib.Value ((new GLib.ParamSpec (pspec_ptr)).ValueType);
				gtk_widget_style_get_property (Handle, native_name, ref value);
				return value;
			} finally {
				GLib.Marshaller.Free (native_name);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_widget_list_mnemonic_labels(IntPtr raw);
		static d_gtk_widget_list_mnemonic_labels gtk_widget_list_mnemonic_labels = FuncLoader.LoadFunction<d_gtk_widget_list_mnemonic_labels>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_list_mnemonic_labels"));

		public Widget[] ListMnemonicLabels ()
		{
			IntPtr raw_ret = gtk_widget_list_mnemonic_labels (Handle);
			if (raw_ret == IntPtr.Zero)
				return new Widget [0];
			GLib.List list = new GLib.List(raw_ret);
			Widget[] result = new Widget [list.Count];
			for (int i = 0; i < list.Count; i++)
				result [i] = list [i] as Widget;
			return result;
		}

		/*
		public void ModifyBase (Gtk.StateType state)
		{
			gtk_widget_modify_base (Handle, (int) state, IntPtr.Zero);
		}
		*/

		public void ModifyBg (Gtk.StateType state)
		{
			gtk_widget_modify_bg (Handle, (int) state, IntPtr.Zero);
		}

		public void ModifyFg (Gtk.StateType state)
		{
			gtk_widget_modify_fg (Handle, (int) state, IntPtr.Zero);
		}

		/*
		public void ModifyText (Gtk.StateType state)
		{
			gtk_widget_modify_text (Handle, (int) state, IntPtr.Zero);
		}
		*/

		public void Path (out string path, out string path_reversed)
		{
			uint len;
			Path (out len, out path, out path_reversed);
		}

		static IDictionary<IntPtr, Delegate> destroy_handlers;
		static IDictionary<IntPtr, Delegate> DestroyHandlers {
			get {
				if (destroy_handlers == null)
					destroy_handlers = new Dictionary<IntPtr, Delegate> ();
				return destroy_handlers;
			}
		}

		private static void OverrideDestroyed (GLib.GType gtype)
		{
			// Do Nothing.  We don't want to hook into the native vtable.
			// We will manually invoke the VM on signal invocation. The signal
			// always raises before the default handler because this signal
			// is RUN_CLEANUP.
		}

		[GLib.DefaultSignalHandler(Type=typeof(Gtk.Widget), ConnectionMethod="OverrideDestroyed")]
		protected virtual void OnDestroyed ()
		{
			if (DestroyHandlers.ContainsKey (Handle)) {
				EventHandler handler = (EventHandler) DestroyHandlers [Handle];
				handler (this, EventArgs.Empty);
				DestroyHandlers.Remove (Handle);
			}
		}

		[GLib.Signal("destroy")]
		public event EventHandler Destroyed {
			add {
				Delegate delegate_handler;
				DestroyHandlers.TryGetValue (Handle, out delegate_handler);
				var handler = delegate_handler as EventHandler;
				DestroyHandlers [Handle] = Delegate.Combine (handler, value);
			}
			remove {
				Delegate delegate_handler;
				DestroyHandlers.TryGetValue (Handle, out delegate_handler);
				var handler = delegate_handler as EventHandler;
				handler = (EventHandler) Delegate.Remove (handler, value);
				if (handler != null)
					DestroyHandlers [Handle] = handler;
				else
					DestroyHandlers.Remove (Handle);
			}
		}

		event EventHandler InternalDestroyed {
			add {
				AddSignalHandler ("destroy", value);
			}
			remove {
				RemoveSignalHandler ("destroy", value);
			}
		}

		static void NativeDestroy (object o, EventArgs args)
		{
			Gtk.Widget widget = o as Gtk.Widget;
			if (widget == null)
				return;
			widget.OnDestroyed ();
		}
		
		static EventHandler native_destroy_handler;
		static EventHandler NativeDestroyHandler {
			get {
				if (native_destroy_handler == null)
					native_destroy_handler = new EventHandler (NativeDestroy);
				return native_destroy_handler;
			}
		}

		protected override void CreateNativeObject (string[] names, GLib.Value[] vals)
		{
			base.CreateNativeObject (names, vals);
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle == IntPtr.Zero)
				return;

			if (disposing)
				gtk_widget_destroy (Handle);

			InternalDestroyed -= NativeDestroyHandler;

			base.Dispose (disposing);
		}

		protected override IntPtr Raw {
			get {
				return base.Raw;
			}
			set {
				base.Raw = value;
				if (value != IntPtr.Zero)
					InternalDestroyed += NativeDestroyHandler;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_widget_destroy(IntPtr raw);
		static d_gtk_widget_destroy gtk_widget_destroy = FuncLoader.LoadFunction<d_gtk_widget_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_destroy"));

		[Obsolete("Use Dispose")]
		public virtual void Destroy ()
		{
		}
	}
}


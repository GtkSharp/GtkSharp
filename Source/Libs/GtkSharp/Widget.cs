//
// Gtk.Widget.cs - Gtk Widget class customizations
//
// Authors: Rachel Hestilow <hestilow@ximian.com>,
//          Brad Taylor <brad@getcoded.net>
//          Marcel Tiede
//
// Copyright (C) 2019 Marcel Tiede
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
	using System.Reflection;
	using System.Runtime.InteropServices;

	public partial class Widget {

		// GtkSharp's GType -> managed type registry exists for the types whose
		// managed name the name mangler cannot guess, and codegen puts the call
		// that populates it in the static constructor of each such type. In this
		// assembly there is exactly one - GtkText, bound as Gtk.TextWidget - so
		// the registry was installed only by a program that had already named
		// Gtk.TextWidget. Until then any GtkText* Gtk handed back (a
		// GtkSpinButton's or GtkEntry's inner text widget, reached through
		// GetFirstAccessibleChild or a signal) resolved by mangling "GtkText"
		// into "Gtk.Text", which does not exist, and came back as a bare
		// Gtk.Widget from the parent GType.
		//
		// Widget is the root of everything Gtk hands out, so bootstrapping here
		// makes the registry complete before any of it can be wrapped. Initialize
		// is idempotent, and the re-entry through Gtk.TextWidget.GType finds the
		// flag already set.
		static Widget ()
		{
			GtkSharp.GtkSharp.ObjectManager.Initialize ();
		}

		// GdkWindow: Gtk 4 renamed GdkWindow to GdkSurface; Widget.Native gives the surface.

		struct TemplateData
		{
			public Dictionary<FieldInfo, string> FieldBindings;
			public SignalConnector SignalConnector;
			public bool ThrowOnUnknownChild;

			public static TemplateData Create()
			{
				var res = new TemplateData();
				res.FieldBindings = new Dictionary<FieldInfo, string>();
				return res;
			}
		}

		private static Dictionary<Type, TemplateData> Templates = new Dictionary<Type, TemplateData>();

		// AddAccelerator: GtkAccelGroup is gone; use a GtkShortcutController.

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
			IntPtr raw_closure = g_closure_new_simple (Marshal.SizeOf<GClosure> (), IntPtr.Zero);
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

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_widget_class_set_activate_signal(IntPtr widget_class, uint signal_id);
		static d_gtk_widget_class_set_activate_signal gtk_widget_class_set_activate_signal = FuncLoader.LoadFunction<d_gtk_widget_class_set_activate_signal>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_class_set_activate_signal"));

		// Gtk 3 stored the activation signal's id in a public GtkWidgetClass
		// field, and this wrote it there by hand. Gtk 4 made that field private:
		// GtkWidgetClass has no "activate_signal" member any more, so
		// class_abi.GetFieldOffset ("activate_signal") looked up a name the ABI
		// description does not contain and threw NullReferenceException out of
		// AbiStruct -- at class-init, before the first instance existed. Every
		// managed Widget subclass that overrode OnActivate was therefore
		// unconstructible, with an exception naming nothing.
		//
		// gtk_widget_class_set_activate_signal is the Gtk 4 way to say the same
		// thing, and it is what makes Widget.Activate () emit the signal.
		static void ConnectActivate (GLib.GType gtype)
		{
			if (ActivateMarshalCallback == null)
				ActivateMarshalCallback = new ClosureMarshal (ActivateMarshal_cb);

			// The signal keeps its Gtk 3 name rather than becoming "activate":
			// Button, Entry and several others already define a signal called
			// "activate", and registering a second one of that name on a
			// subclass's own GType is an error.
			uint id = RegisterSignal ("activate_signal", gtype, GLib.Signal.Flags.RunLast, GLib.GType.None,
					new GLib.GType [0], ActivateMarshalCallback);

			gtk_widget_class_set_activate_signal (gtype.GetClassPtr (), id);
		}

		[GLib.DefaultSignalHandler (Type=typeof (Gtk.Widget), ConnectionMethod="ConnectActivate")]
		protected virtual void OnActivate ()
		{
		}

		// The [Binding] machinery lived here: an invoker table, a closure
		// marshaller and gtk_binding_set_by_class / gtk_binding_entry_add_signall.
		// Gtk 4 removed GtkBindingSet entirely -- key bindings are installed on a
		// GtkShortcutController now -- so every one of those symbols resolved to
		// a null delegate and the whole path could only throw.

		static void ClassInit (GLib.GType gtype, Type t)
		{
			InitTemplateForType (gtype, t);
			InitCssName (gtype, t);
		}

		// InitBindings: GtkBindingSet is gone in Gtk 4. Key bindings are installed with a GtkShortcutController, which is not wrapped by [Binding] yet.

		static void InitTemplateForType (GLib.GType gtype, Type type)
		{
			var attr = type.GetCustomAttribute<TemplateAttribute> (true);
			if (attr == null) return;

			var data = TemplateData.Create ();
			data.ThrowOnUnknownChild = attr.ThrowOnUnknownChild;
			var resource_name = attr.Ui;
			var resource_stream = type.Assembly.GetManifestResourceStream (resource_name);

			if (resource_stream == null)
				throw new Exception ("Template resource '" + resource_name + "' not found");

			var template = new byte[(int) resource_stream.Length];
			resource_stream.Read (template, 0, template.Length);
			resource_stream.Dispose ();

			SetTemplate (gtype, template);
			BindTemplateChildren (gtype, type, data.FieldBindings);

			// Gtk 4 routes template signal connection through GtkBuilderScope,
			// which is not bound, so ConnectSignals throws. Only templates that
			// actually declare a <signal> need it: binding [Child] fields is a
			// separate mechanism and works either way. Asking first keeps the
			// useful subset working while still failing loudly -- rather than
			// silently ignoring every click -- for templates that do want
			// handlers wired.
			if (DeclaresSignals (template)) {
				data.SignalConnector = new SignalConnector (type);
				data.SignalConnector.ConnectSignals (gtype);
			}

			Templates[type] = data;
		}

		static bool DeclaresSignals (byte[] template)
		{
			return BuilderXml.DeclaresSignals (System.Text.Encoding.UTF8.GetString (template));
		}

		delegate IntPtr d_gtk_widget_class_set_template(IntPtr class_ptr, IntPtr template_bytes);
		static d_gtk_widget_class_set_template gtk_widget_class_set_template = FuncLoader.LoadFunction<d_gtk_widget_class_set_template>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_class_set_template"));

		static void SetTemplate (GLib.GType gtype, byte[] buffer)
		{
			var bytes = new GLib.Bytes (buffer);
			gtk_widget_class_set_template (gtype.GetClassPtr (), bytes.Handle);
			bytes.Dispose ();
		}

		delegate IntPtr d_gtk_widget_class_bind_template_child_full(IntPtr class_ptr, IntPtr name, bool internal_child, IntPtr struct_offset);
		static d_gtk_widget_class_bind_template_child_full gtk_widget_class_bind_template_child_full = FuncLoader.LoadFunction<d_gtk_widget_class_bind_template_child_full>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_class_bind_template_child_full"));

		static void BindTemplateChildren (GLib.GType gtype, Type type, Dictionary<FieldInfo, string> fields)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance;

			foreach (var field in type.GetFields (flags))
			{
				var attr = field.GetCustomAttribute<ChildAttribute> (true);

				if (attr == null)
					continue;

				var name = attr.Name ?? field.Name;

				var native_name = GLib.Marshaller.StringToPtrGStrdup (name);
				gtk_widget_class_bind_template_child_full (gtype.GetClassPtr (), native_name, attr.Internal, new IntPtr ((long)0));
				GLib.Marshaller.Free (native_name);

				fields[field] = name;
			}
		}

		static void InitCssName (GLib.GType gtype, Type t)
		{
			CssNameAttribute attr = t.GetCustomAttribute<CssNameAttribute>(true);
			if (attr != null)
				SetCssName (gtype, attr.Name);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_widget_init_template(IntPtr raw);
		static d_gtk_widget_init_template gtk_widget_init_template = FuncLoader.LoadFunction<d_gtk_widget_init_template>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_init_template"));

		void InitTemplateForInstance ()
		{
			var type = GetType ();
			if (Templates.TryGetValue(type, out TemplateData data))
			{
				GLib.GType gtype = LookupGType (type);

				// Only templates that declare a <signal> get a SignalConnector,
				// since connecting them throws under Gtk 4. The instance hand-off
				// exists purely for that connector, so it is skipped with it.
				if (data.SignalConnector != null)
					data.SignalConnector.template_object_instance = this;

				gtk_widget_init_template (Handle);

				if (data.SignalConnector != null)
					data.SignalConnector.template_object_instance = null;
				foreach (KeyValuePair<FieldInfo, string> pair in data.FieldBindings)
				{
					FieldInfo field = pair.Key;
					string name = pair.Value;
					GLib.Object child = GetTemplateChild (gtype, name);
					
					if (child != null)
					{
						const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance;
						field.SetValue (this, child, flags, null, null);
					}
					else if (data.ThrowOnUnknownChild)
					{
						throw new Exception ("Unknown child in template for type '" + type + "'");
					}
				}
			}
		}

		// StyleGetProperty: Gtk 4 removed widget style properties; everything they carried is CSS now.
		// StyleGetPropertyValue: Gtk 4 removed widget style properties; everything they carried is CSS now.
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


		/*
		public void ModifyText (Gtk.StateType state)
		{
			gtk_widget_modify_text (Handle, (int) state, IntPtr.Zero);
		}
		*/

		// Path: gtk_widget_path is gone; Gtk 4 has no widget paths.

		// Gtk 4 removed the GtkWidget::destroy signal outright, so the Destroyed
		// event that used to surface it cannot ever fire. It is removed rather
		// than left in place: a handler that silently never runs reads as a
		// window that ignores being closed, which is far harder to diagnose than
		// a compile error. Gtk.Window.CloseRequest is the Gtk 4 replacement.

		protected override void CreateNativeObject (string[] names, GLib.Value[] vals)
		{
			base.CreateNativeObject (names, vals);
			InitTemplateForInstance ();
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_object_ref(IntPtr raw);
		static d_g_object_ref g_object_ref = FuncLoader.LoadFunction<d_g_object_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_ref"));

		private bool destroyed;
		protected override void Dispose (bool disposing)
		{
			if (Handle == IntPtr.Zero)
				return;

			// Gtk 4 dropped gtk_widget_is_toplevel; a toplevel is a GtkWindow.
			if (disposing && !destroyed && this is Gtk.Window)
			{
				//If this is a TopLevel widget, then we do not hold a ref, only a toggle ref.
				//Freeing our toggle ref expects a normal ref to exist, and therefore does not check if the object still exists.
				//Take a ref here and let our toggle ref unref it.
				g_object_ref (Handle);
				gtk_window_destroy (Handle);
				destroyed = true;
			}

			base.Dispose (disposing);
		}

		// The Raw override that used to live here existed only to subscribe to
		// the destroy signal; with that signal gone it forwarded to base and
		// nothing else.

		// Gtk 4 removed gtk_widget_destroy. A toplevel is torn down with
		// gtk_window_destroy; every other widget is destroyed by being
		// unparented, which drops the parent's reference. Loading the old
		// symbol yielded a null delegate -- FuncLoader.LoadFunction returns
		// default(T) when the export is missing -- so this path threw a
		// NullReferenceException for every widget it ran on.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_window_destroy(IntPtr raw);
		static d_gtk_window_destroy gtk_window_destroy = FuncLoader.LoadFunction<d_gtk_window_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_window_destroy"));

		static void DestroyNative (Widget widget)
		{
			if (widget is Gtk.Window)
				gtk_window_destroy (widget.Handle);
			else if (widget.Parent != null)
				widget.Unparent ();
		}

		public virtual void Destroy ()
		{
			if (Handle == IntPtr.Zero)
				return;

			if (destroyed)
				return;

			DestroyNative (this);
			destroyed = true;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_widget_class_get_css_name(IntPtr widget_class);
		static d_gtk_widget_class_get_css_name gtk_widget_class_get_css_name = FuncLoader.LoadFunction<d_gtk_widget_class_get_css_name>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_class_get_css_name"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_widget_class_set_css_name(IntPtr widget_class, IntPtr name);
		static d_gtk_widget_class_set_css_name gtk_widget_class_set_css_name = FuncLoader.LoadFunction<d_gtk_widget_class_set_css_name>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_class_set_css_name"));

		public static string GetCssName (GLib.GType widget_type)
		{
			IntPtr class_ptr = widget_type.GetClassPtr ();
			IntPtr native_name = gtk_widget_class_get_css_name (class_ptr);
			string name = GLib.Marshaller.Utf8PtrToString (native_name);
			return name;
		}

		protected static void SetCssName (GLib.GType widget_type, string name)
		{
			IntPtr class_ptr = widget_type.GetClassPtr ();
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			gtk_widget_class_set_css_name (class_ptr, native_name);
			GLib.Marshaller.Free (native_name);
		}
	}
}


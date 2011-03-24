// Object.cs - GObject class wrapper implementation
//
// Authors: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
// Copyright (c) 2004-2005 Novell, Inc.
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
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;

	public class Object : IWrapper, IDisposable {

		IntPtr handle;
		ToggleRef tref;
		bool disposed = false;
		static Dictionary<IntPtr, ToggleRef> Objects = new Dictionary<IntPtr, ToggleRef>();
		static List<ToggleRef> PendingDestroys = new List<ToggleRef> ();
		static bool idle_queued;

		~Object ()
		{
			lock (PendingDestroys) {
				lock (Objects) {
					if (Objects[Handle] is ToggleRef)
						PendingDestroys.Add (Objects [Handle]);
					Objects.Remove (Handle);
				}
				if (!idle_queued){
					Timeout.Add (50, new TimeoutHandler (PerformQueuedUnrefs));
					idle_queued = true;
				}
			}
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_object_unref (IntPtr raw);
		
		static bool PerformQueuedUnrefs ()
		{
			ToggleRef[] references;

			lock (PendingDestroys){
				references = new ToggleRef [PendingDestroys.Count];
				PendingDestroys.CopyTo (references, 0);
				PendingDestroys.Clear ();
				idle_queued = false;
			}

			foreach (ToggleRef r in references)
				r.Free ();

			return false;
		}

		public virtual void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;
			ToggleRef toggle_ref;
			if (Objects.TryGetValue (Handle, out toggle_ref))
				Objects.Remove (Handle);
			try {
				if (toggle_ref != null)
					toggle_ref.Free ();
			} catch (Exception e) {
				Console.WriteLine ("Exception while disposing a " + this + " in Gtk#");
				throw e;
			}
			handle = IntPtr.Zero;
			GC.SuppressFinalize (this);
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_object_ref (IntPtr raw);

		public static Object GetObject(IntPtr o, bool owned_ref)
		{
			if (o == IntPtr.Zero)
				return null;

			Object obj = null;

			ToggleRef toggle_ref;
			if (Objects.TryGetValue (o, out toggle_ref)) {
				if (toggle_ref != null)
					obj = toggle_ref.Target;
			}

			if (obj != null && obj.Handle == o) {
				if (owned_ref)
					g_object_unref (obj.Handle);
				return obj;
			}

			if (!owned_ref)
				g_object_ref (o);

			obj = GLib.ObjectManager.CreateObject(o); 
			if (obj == null) {
				g_object_unref (o);
				return null;
			}

			return obj;
		}

		public static Object GetObject(IntPtr o)
		{
			return GetObject (o, false);
		}

		private static void ConnectDefaultHandlers (GType gtype, System.Type t)
		{
			foreach (MethodInfo minfo in t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)) {
				MethodInfo baseinfo = minfo.GetBaseDefinition ();
				if (baseinfo == minfo)
					continue;

				foreach (object attr in baseinfo.GetCustomAttributes (typeof (DefaultSignalHandlerAttribute), false)) {
					DefaultSignalHandlerAttribute sigattr = attr as DefaultSignalHandlerAttribute;
					MethodInfo connector = sigattr.Type.GetMethod (sigattr.ConnectionMethod, BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof (GType) }, new ParameterModifier [0]);
					object[] parms = new object [1];
					parms [0] = gtype;
					connector.Invoke (null, parms);
					break;
				}
			}
					
		}

		private static void InvokeTypeInitializers (GType gtype, System.Type t)
		{
			object[] parms = {gtype, t};

			BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;

			foreach (TypeInitializerAttribute tia in t.GetCustomAttributes (typeof (TypeInitializerAttribute), true)) {
				MethodInfo m = tia.Type.GetMethod (tia.MethodName, flags);
				if (m != null)
					m.Invoke (null, parms);
			}
 		}
		
		//  Key: The pointer to the ParamSpec of the property
		//  Value: The corresponding PropertyInfo object
		static Dictionary<IntPtr, PropertyInfo> properties;
		static Dictionary<IntPtr, PropertyInfo> Properties {
			get {
				if (properties == null)
					properties = new Dictionary<IntPtr, PropertyInfo> ();
				return properties;
			}
		}

		struct GTypeClass {
			public IntPtr gtype;
		}

		struct GObjectClass {
			public GTypeClass type_class;
			public IntPtr construct_props;
			public ConstructorDelegate constructor_cb;
			public SetPropertyDelegate set_prop_cb;
			public GetPropertyDelegate get_prop_cb;
			public IntPtr dispose;
			public IntPtr finalize;
			public IntPtr dispatch_properties_changed;
			public IntPtr notify;
			public IntPtr constructed;
			public IntPtr dummy1;
			public IntPtr dummy2;
			public IntPtr dummy3;
			public IntPtr dummy4;
			public IntPtr dummy5;
			public IntPtr dummy6;
			public IntPtr dummy7;
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr ConstructorDelegate (IntPtr gtype, uint n_construct_properties, IntPtr construct_properties);

		static ConstructorDelegate constructor_handler;

		static ConstructorDelegate ConstructorHandler {
			get {
				if (constructor_handler == null)
					constructor_handler = new ConstructorDelegate (ConstructorCallback);
				return constructor_handler;
			}
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_param_spec_get_name (IntPtr pspec);

		static IntPtr ConstructorCallback (IntPtr gtypeval, uint n_construct_properties, IntPtr construct_properties)
		{
			GType gtype = new GLib.GType (gtypeval);
			GObjectClass threshold_class = (GObjectClass) Marshal.PtrToStructure (gtype.GetThresholdType ().GetClassPtr (), typeof (GObjectClass));
			IntPtr raw = threshold_class.constructor_cb (gtypeval, n_construct_properties, construct_properties);
			bool construct_needed = true;
			for (int i = 0; i < n_construct_properties; i++) {
				IntPtr p = new IntPtr (construct_properties.ToInt64 () + i * 2 * IntPtr.Size);

				string prop_name = Marshaller.Utf8PtrToString (g_param_spec_get_name (Marshal.ReadIntPtr (p)));
				if (prop_name != "gtk-sharp-managed-instance")
					continue;

				Value val = (Value) Marshal.PtrToStructure (Marshal.ReadIntPtr (p, IntPtr.Size), typeof (Value));
				if ((IntPtr) val.Val != IntPtr.Zero) {
					GCHandle gch = (GCHandle) (IntPtr) val.Val;
					Object o = (GLib.Object) gch.Target;
					o.Raw = raw;
					construct_needed = false;
					break;
				}
			}

			if (construct_needed)
				GetObject (raw, false);

			return raw;
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_object_class_install_property (IntPtr klass, uint prop_id, IntPtr param_spec);

		static IntPtr RegisterProperty (GType type, string name, string nick, string blurb, uint property_id, GType property_type, bool can_read, bool can_write)
		{
			IntPtr declaring_class = type.GetClassPtr ();
			ParamSpec pspec = new ParamSpec (name, nick, blurb, property_type, can_read, can_write);

			g_object_class_install_property (declaring_class, property_id, pspec.Handle);
			return pspec.Handle;
		}

		static void AddProperties (GType gtype, System.Type t, bool register_instance_prop)
		{
			uint idx = 1;
			
			if (register_instance_prop) {
				IntPtr declaring_class = gtype.GetClassPtr ();
				ParamSpec pspec = new ParamSpec ("gtk-sharp-managed-instance", "", "", GType.Pointer, ParamFlags.Writable | ParamFlags.ConstructOnly);
				g_object_class_install_property (declaring_class, idx, pspec.Handle);
				idx++;				
			}

			bool handlers_overridden = register_instance_prop;
			foreach (PropertyInfo pinfo in t.GetProperties (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)) {
				foreach (object attr in pinfo.GetCustomAttributes (typeof (PropertyAttribute), false)) {
					if(pinfo.GetIndexParameters().Length > 0)
						throw(new InvalidOperationException(String.Format("GLib.RegisterPropertyAttribute cannot be applied to property {0} of type {1} because the property expects one or more indexed parameters", pinfo.Name, t.FullName)));
					
					if (!handlers_overridden) {
						IntPtr class_ptr = gtype.GetClassPtr ();
						GObjectClass gobject_class = (GObjectClass) Marshal.PtrToStructure (class_ptr, typeof (GObjectClass));
						gobject_class.get_prop_cb = GetPropertyHandler;
						gobject_class.set_prop_cb = SetPropertyHandler;
						Marshal.StructureToPtr (gobject_class, class_ptr, false);
						handlers_overridden = true;
					}
					PropertyAttribute property_attr = attr as PropertyAttribute;
					try {
						IntPtr param_spec = RegisterProperty (gtype, property_attr.Name, property_attr.Nickname, property_attr.Blurb, idx, (GType) pinfo.PropertyType, pinfo.CanRead, pinfo.CanWrite);
						Properties.Add (param_spec, pinfo);
						idx++;
					} catch (ArgumentException) {
						throw new InvalidOperationException (String.Format ("GLib.PropertyAttribute cannot be applied to property {0} of type {1} because the return type of the property is not supported", pinfo.Name, t.FullName));
					}
				}
			}
		}
		
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void GetPropertyDelegate (IntPtr GObject, uint property_id, ref GLib.Value value, IntPtr pspec);

		static void GetPropertyCallback (IntPtr handle, uint property_id, ref GLib.Value value, IntPtr param_spec)
		{
			if (!Properties.ContainsKey (param_spec))
				return;

			GLib.Object obj = GLib.Object.GetObject (handle, false);
			value.Val = Properties [param_spec].GetValue (obj, new object [0]);
		}

		static GetPropertyDelegate get_property_handler;
		static GetPropertyDelegate GetPropertyHandler {
			get {
				if (get_property_handler == null)
					get_property_handler = new GetPropertyDelegate (GetPropertyCallback);
				return get_property_handler;
			}
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void SetPropertyDelegate (IntPtr GObject, uint property_id, ref GLib.Value value, IntPtr pspec);

		static void SetPropertyCallback(IntPtr handle, uint property_id, ref GLib.Value value, IntPtr param_spec)
		{
			if (!Properties.ContainsKey (param_spec))
				return;

			GLib.Object obj = GLib.Object.GetObject (handle, false);
			Properties [param_spec].SetValue (obj, value.Val, new object [0]);
		}

		static SetPropertyDelegate set_property_handler;
		static SetPropertyDelegate SetPropertyHandler {
			get {
				if (set_property_handler == null)
					set_property_handler = new SetPropertyDelegate (SetPropertyCallback);
				return set_property_handler;
			}
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_type_add_interface_static (IntPtr gtype, IntPtr iface_type, ref GInterfaceInfo info);

		static void AddInterfaces (GType gtype, Type t)
		{
			foreach (Type iface in t.GetInterfaces ()) {
				if (!iface.IsDefined (typeof (GInterfaceAttribute), true) || iface.IsAssignableFrom (t.BaseType))
					continue;

				GInterfaceAttribute attr = iface.GetCustomAttributes (typeof (GInterfaceAttribute), false) [0] as GInterfaceAttribute;
				GInterfaceAdapter adapter = Activator.CreateInstance (attr.AdapterType, null) as GInterfaceAdapter;
				
				GInterfaceInfo info = adapter.Info;
				g_type_add_interface_static (gtype.Val, adapter.GType.Val, ref info);
			}
		}

		protected internal static GType RegisterGType (System.Type t)
		{
			GType gtype = GType.RegisterGObjectType (t);
			bool is_first_subclass = gtype.GetBaseType () == gtype.GetThresholdType ();
			if (is_first_subclass) {
				IntPtr class_ptr = gtype.GetClassPtr ();
				GObjectClass gobject_class = (GObjectClass) Marshal.PtrToStructure (class_ptr, typeof (GObjectClass));
				gobject_class.constructor_cb = ConstructorHandler;
				gobject_class.get_prop_cb = GetPropertyHandler;
				gobject_class.set_prop_cb = SetPropertyHandler;
				Marshal.StructureToPtr (gobject_class, class_ptr, false);
			}
			AddProperties (gtype, t, is_first_subclass);
			ConnectDefaultHandlers (gtype, t);
			InvokeTypeInitializers (gtype, t);
			AddInterfaces (gtype, t);
			return gtype;
		}

		protected GType LookupGType ()
		{
			if (Handle != IntPtr.Zero) {
				GTypeInstance obj = (GTypeInstance) Marshal.PtrToStructure (Handle, typeof (GTypeInstance));
				GTypeClass klass = (GTypeClass) Marshal.PtrToStructure (obj.g_class, typeof (GTypeClass));
				return new GLib.GType (klass.gtype);
			} else {
				return LookupGType (GetType ());
			}
		}

		protected internal static GType LookupGType (System.Type t)
		{
			return GType.LookupGObjectType (t);
		}

		protected Object (IntPtr raw)
		{
			Raw = raw;
		}

		protected Object ()
		{
			CreateNativeObject (new string [0], new GLib.Value [0]);
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_object_new (IntPtr gtype, IntPtr dummy);

		struct GParameter {
			public IntPtr name;
			public GLib.Value val;
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_object_newv (IntPtr gtype, int n_params, GParameter[] parms);

		protected virtual void CreateNativeObject (string[] names, GLib.Value[] vals)
		{
			GType gtype = LookupGType ();
			bool is_managed_subclass = gtype.ToString ().StartsWith ("__gtksharp");
			GParameter[] parms = new GParameter [is_managed_subclass ? names.Length + 1 : names.Length];
			for (int i = 0; i < names.Length; i++) {
				parms [i].name = GLib.Marshaller.StringToPtrGStrdup (names [i]);
				parms [i].val = vals [i];
			}

			if (is_managed_subclass) {
				GCHandle gch = GCHandle.Alloc (this);
				parms[names.Length].name = GLib.Marshaller.StringToPtrGStrdup ("gtk-sharp-managed-instance");
				parms[names.Length].val = new GLib.Value ((IntPtr) gch);
				Raw = g_object_newv (gtype.Val, parms.Length, parms);
				gch.Free ();
			} else {
				Raw = g_object_newv (gtype.Val, parms.Length, parms);
			}

			foreach (GParameter p in parms)
				GLib.Marshaller.Free (p.name);
		}

		protected virtual IntPtr Raw {
			get {
				return handle;
			}
			set {
				if (handle == value)
					return;

				if (handle != IntPtr.Zero) {
					Objects.Remove (handle);
					if (tref != null) {
						tref.Free ();
						tref = null;
					}
				}
				handle = value;
				if (value != IntPtr.Zero) {
					tref = new ToggleRef (this);
					Objects [value] = tref;
				}
			}
		}	

		public static GLib.GType GType {
			get { return GType.Object; }
		}

		protected string TypeName {
			get { return NativeType.ToString (); }
		}

		internal GLib.GType NativeType {
			get { return LookupGType (); }
		}

		internal ToggleRef ToggleRef {
			get { return tref; }
		}

		public IntPtr Handle {
			get { return handle; }
		}

		public IntPtr OwnedHandle {
			get { return g_object_ref (handle); }
		}

		public void AddNotification (string property, NotifyHandler handler)
		{
			AddSignalHandler ("notify::" + property, handler, typeof(NotifyArgs));
		}

		public void AddNotification (NotifyHandler handler)
		{
			AddSignalHandler ("notify", handler, typeof(NotifyArgs));
		}

		public void RemoveNotification (string property, NotifyHandler handler)
		{
			RemoveSignalHandler ("notify::" + property, handler);
		}

		public void RemoveNotification (NotifyHandler handler)
		{
			RemoveSignalHandler ("notify", handler);
		}

		public override int GetHashCode ()
		{
			return Handle.GetHashCode ();
		}

		System.Collections.Hashtable data;
		public System.Collections.Hashtable Data {
			get { 
				if (data == null)
					data = new System.Collections.Hashtable ();
				
				return data;
			}
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_object_get_property (IntPtr obj, IntPtr name, ref GLib.Value val);

		protected GLib.Value GetProperty (string name)
		{
			Value val = new Value (this, name);
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			g_object_get_property (Raw, native_name, ref val);
			GLib.Marshaller.Free (native_name);
			return val;
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_object_set_property (IntPtr obj, IntPtr name, ref GLib.Value val);

		protected void SetProperty (string name, GLib.Value val)
		{
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			g_object_set_property (Raw, native_name, ref val);
			GLib.Marshaller.Free (native_name);
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_object_notify (IntPtr obj, IntPtr property_name);

		protected void Notify (string property_name)
		{
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (property_name);
			g_object_notify (Handle, native_name);
			GLib.Marshaller.Free (native_name);
		}

		Dictionary<string, Signal> signals;
		Dictionary<string, Signal> Signals {
			get {
				if (signals == null)
					signals = new Dictionary<string, Signal> ();
				return signals;
			}
		}

		public void AddSignalHandler (string name, Delegate handler)
		{
			AddSignalHandler (name, handler, typeof (EventArgs));
		}

		public void AddSignalHandler (string name, Delegate handler, Delegate marshaler)
		{
			Signal sig;
			if (!Signals.TryGetValue (name, out sig)) {
				sig = new Signal (this, name, marshaler);
				Signals [name] = sig;
			}

			sig.AddDelegate (handler);
		}

		public void AddSignalHandler (string name, Delegate handler, Type args_type)
		{
			if (args_type == null)
				args_type = handler.Method.GetParameters ()[1].ParameterType;

			Signal sig;
			if (!Signals.TryGetValue (name, out sig)) {
				sig = new Signal (this, name, args_type);
				Signals [name] = sig;
			}

			sig.AddDelegate (handler);
		}

		public void RemoveSignalHandler (string name, Delegate handler)
		{
			Signal sig;
			if (Signals.TryGetValue (name, out sig))
				sig.RemoveDelegate (handler);
		}

		protected static void OverrideVirtualMethod (GType gtype, string name, Delegate cb)
		{
			Signal.OverrideDefaultHandler (gtype, name, cb);
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		protected static extern void g_signal_chain_from_overridden (IntPtr args, ref GLib.Value retval);

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_type_check_instance_is_a (IntPtr obj, IntPtr gtype);

		internal static bool IsObject (IntPtr obj)
		{
			return g_type_check_instance_is_a (obj, GType.Object.Val);
		}

		struct GTypeInstance {
			public IntPtr g_class;
		}

		struct GObject {
			public GTypeInstance type_instance;
			public uint ref_count;
			public IntPtr qdata;
		}

		protected int RefCount {
			get {
				GObject native = (GObject) Marshal.PtrToStructure (Handle, typeof (GObject));
				return (int) native.ref_count;
			}
		}

		internal void Harden ()
		{
			tref.Harden ();
		}

		static Object ()
		{
			if (Environment.GetEnvironmentVariable ("GTK_SHARP_DEBUG") != null)
				GLib.Log.SetLogHandler ("GLib-GObject", GLib.LogLevelFlags.All, new GLib.LogFunc (GLib.Log.PrintTraceLogFunction));
		}
	}
}

// Object.cs - GObject class wrapper implementation
//
// Authors: Mike Kestner <mkestner@speakeasy.net>
//          Andres G. Aragoneses <knocte@gmail.com>
//
// Copyright (c) 2001-2003 Mike Kestner
// Copyright (c) 2004-2005 Novell, Inc.
// Copyright (c) 2013 Andres G. Aragoneses
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
	using System.Reflection;
	using System.Runtime.InteropServices;

	public class Object : IWrapper, IDisposable {

		IntPtr handle;
		ToggleRef tref;
		bool disposed = false;
		static uint idx = 1;
		static Dictionary<IntPtr, ToggleRef> Objects = new Dictionary<IntPtr, ToggleRef>();

		~Object ()
		{
			if (WarnOnFinalize)
				Console.Error.WriteLine ("Unexpected finalization of " + GetType() + " instance.  Consider calling Dispose.");

			Dispose (false);
		}

		public void Dispose ()
		{
			if (disposed)
				return;

			Dispose (true);
			disposed = true;
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			ToggleRef tref;
			lock (Objects) {
				if (Objects.TryGetValue (Handle, out tref)) {
					Objects.Remove (Handle);
				}
			}

			handle = IntPtr.Zero;
			if (tref == null)
				return;
			
			if (disposing)
				tref.Dispose ();
			else
				tref.QueueUnref ();
		}

		public static bool WarnOnFinalize { get; set; }

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_object_ref (IntPtr raw);

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_object_unref (IntPtr raw);
		
		public static Object TryGetObject (IntPtr o)
		{
			if (o == IntPtr.Zero)
				return null;

			ToggleRef toggle_ref;
			lock (Objects) {
				toggle_ref = (ToggleRef) Objects[o];
			}

			if (toggle_ref != null) {
				return toggle_ref.Target;
			}

			return null;
		}

		public static Object GetObject(IntPtr o, bool owned_ref)
		{
			if (o == IntPtr.Zero)
				return null;

			Object obj = null;

			ToggleRef toggle_ref;
			lock (Objects) {
				if (Objects.TryGetValue (o, out toggle_ref)) {
					if (toggle_ref != null)
						obj = toggle_ref.Target;
				}
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

		//  Key: The Type for the set of properties
		//  Value->SubKey: The pointer to the ParamSpec of the property
		//  Value->SubValue: The corresponding PropertyInfo object
		static Dictionary<Type, Dictionary<IntPtr, PropertyInfo>> properties;
		static Dictionary<Type, Dictionary<IntPtr, PropertyInfo>> Properties {
			get {
				if (properties == null)
					properties = new Dictionary<Type, Dictionary<IntPtr, PropertyInfo>> ();
				return properties;
			}
		}

		static Dictionary<IntPtr, Dictionary<Type, PropertyInfo>> interface_properties;
		static Dictionary<IntPtr, Dictionary<Type, PropertyInfo>> IProperties {
			get {
				if (interface_properties == null)
					interface_properties = new Dictionary<IntPtr, Dictionary<Type, PropertyInfo>> ();
				return interface_properties;
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

		internal class ClassInitializer {

			internal Type Type { get; private set; }
			internal bool HandlersOverriden { get; private set; }
			internal GType.ClassInitDelegate ClassInitManagedDelegate { get; private set; }

			uint idx = 1;
			bool is_first_subclass;
			private GType gtype;
			private List<GInterfaceAdapter> adapters = new List<GInterfaceAdapter> ();

			internal ClassInitializer (Type type)
			{
				ClassInitManagedDelegate = this.ClassInit;
				Type = type;
				gtype = GType.RegisterGObjectType (this);
				is_first_subclass = gtype.GetBaseType () == gtype.GetThresholdType ();
			}

			internal GType Init ()
			{
				AddGInterfaces ();
				gtype.EnsureClass (); //calls class_init

				AddProperties ();
				ConnectDefaultHandlers ();
				InvokeTypeInitializers ();
				AddInterfaceProperties ();
				return gtype;
			}

			private void AddGInterfaces ()
			{
				foreach (Type iface in Type.GetInterfaces ()) {
					if (!iface.IsDefined (typeof (GInterfaceAttribute), true))
						continue;

					GInterfaceAttribute attr = iface.GetCustomAttributes (typeof (GInterfaceAttribute), false) [0] as GInterfaceAttribute;
					GInterfaceAdapter adapter = Activator.CreateInstance (attr.AdapterType, null) as GInterfaceAdapter;

					if (!iface.IsAssignableFrom (Type.BaseType)) {
						GInterfaceInfo info = adapter.Info;
						info.Data = gtype.Val;
						g_type_add_interface_static (gtype.Val, adapter.GType.Val, ref info);
						adapters.Add (adapter);
					}
				}
			}

			private void ClassInit (IntPtr gobject_class_handle)
			{
				bool override_ctor = is_first_subclass;

				bool override_props = is_first_subclass || adapters.Count > 0;

				OverrideHandlers (gobject_class_handle, override_ctor, override_props);

				foreach (GInterfaceAdapter adapter in adapters) {
					InitializeProperties (adapter, gobject_class_handle);
				}
			}

			private void InitializeProperties (GInterfaceAdapter adapter, IntPtr gobject_class_handle)
			{
				foreach (PropertyInfo pinfo in adapter.GetType ().GetProperties (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)) {
					foreach (object attr in pinfo.GetCustomAttributes (typeof (PropertyAttribute), false)) {
						if (pinfo.GetIndexParameters ().Length > 0)
							throw new InvalidOperationException (String.Format ("Property {0} of type {1} cannot be overriden because its GLib.PropertyAttribute is expected to have one or more indexed parameters",
							                                                    pinfo.Name, adapter.GetType ().FullName));

						PropertyAttribute property_attr = attr as PropertyAttribute;
						if (property_attr != null) {
							OverrideProperty (gobject_class_handle, property_attr.Name);
						}
					}
				}
			}

			void OverrideProperty (IntPtr declaring_class, string name)
			{
				Object.OverrideProperty (declaring_class, idx++, name);
				idx++;
			}

			private void OverrideHandlers (bool ctor, bool properties)
			{
				OverrideHandlers (gtype.GetClassPtr (), ctor, properties);
			}

			private void OverrideHandlers (IntPtr gobject_class_handle, bool ctor, bool properties)
			{
				if (HandlersOverriden || (ctor == false && properties == false))
					return;

				GObjectClass gobject_class = (GObjectClass)Marshal.PtrToStructure (gobject_class_handle, typeof(GObjectClass));
				if (ctor) {
					gobject_class.constructor_cb = GLib.Object.ConstructorHandler;
				}
				if (properties) {
					gobject_class.get_prop_cb = GLib.Object.GetPropertyHandler;
					gobject_class.set_prop_cb = GLib.Object.SetPropertyHandler;
				}
				Marshal.StructureToPtr (gobject_class, gobject_class_handle, false);
				HandlersOverriden = true;
			}

			void AddProperties ()
			{
				if (is_first_subclass) {
					IntPtr declaring_class = gtype.GetClassPtr ();
					ParamSpec pspec = new ParamSpec ("gtk-sharp-managed-instance", "", "", GType.Pointer, ParamFlags.Writable | ParamFlags.ConstructOnly);
					g_object_class_install_property (declaring_class, idx, pspec.Handle);
					idx++;
				}

				foreach (PropertyInfo pinfo in Type.GetProperties (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)) {
					foreach (object attr in pinfo.GetCustomAttributes (typeof (PropertyAttribute), false)) {
						if (pinfo.GetIndexParameters ().Length > 0)
							throw new InvalidOperationException(String.Format("GLib.RegisterPropertyAttribute cannot be applied to property {0} of type {1} because the property expects one or more indexed parameters",
							                                                  pinfo.Name, Type.FullName));

						OverrideHandlers (false, true);

						PropertyAttribute property_attr = attr as PropertyAttribute;
						try {
							IntPtr param_spec = RegisterProperty (gtype, property_attr.Name, property_attr.Nickname, property_attr.Blurb, idx, (GType) pinfo.PropertyType, pinfo.CanRead, pinfo.CanWrite);
							Type type = (Type)gtype;
							Dictionary<IntPtr, PropertyInfo> gtype_properties;
							if (!Properties.TryGetValue (type, out gtype_properties)) {
								gtype_properties = new Dictionary<IntPtr, PropertyInfo> ();
								Properties [type] = gtype_properties;
							}
							gtype_properties.Add (param_spec, pinfo);
							idx++;
						} catch (ArgumentException) {
							throw new InvalidOperationException (String.Format ("GLib.PropertyAttribute cannot be applied to property {0} of type {1} because the return type of the property is not supported",
							                                                    pinfo.Name, Type.FullName));
						}
					}
				}
			}

			void AddInterfaceProperties ()
			{
				foreach (Type iface in Type.GetInterfaces ()) {
					if (!iface.IsDefined (typeof (GInterfaceAttribute), true))
						continue;

					GInterfaceAttribute attr = iface.GetCustomAttributes (typeof (GInterfaceAttribute), false) [0] as GInterfaceAttribute;
					GInterfaceAdapter adapter = Activator.CreateInstance (attr.AdapterType, null) as GInterfaceAdapter;

					foreach (PropertyInfo p in iface.GetProperties ()) {
						PropertyAttribute[] attrs = p.GetCustomAttributes (typeof (PropertyAttribute), true) as PropertyAttribute [];
						if (attrs.Length == 0)
							continue;
						PropertyAttribute property_attr = attrs [0];
						PropertyInfo declared_prop = Type.GetProperty (p.Name, BindingFlags.Public | BindingFlags.Instance);
						if (declared_prop == null)
							continue;
						IntPtr param_spec = FindInterfaceProperty (adapter.GType, property_attr.Name);

						Dictionary<IntPtr, PropertyInfo> props;
						if (!Properties.TryGetValue (Type, out props)) {
							props = new Dictionary<IntPtr, PropertyInfo> ();
							Properties [Type] = props;
						}
						props [param_spec] = declared_prop;
					}
				}
			}

			void ConnectDefaultHandlers ()
			{
				foreach (MethodInfo minfo in Type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)) {
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

			void InvokeTypeInitializers ()
			{
				object[] parms = {gtype, Type};

				BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;

				foreach (TypeInitializerAttribute tia in Type.GetCustomAttributes (typeof (TypeInitializerAttribute), true)) {
					MethodInfo m = tia.Type.GetMethod (tia.MethodName, flags);
					if (m != null)
						m.Invoke (null, parms);
				}
			}
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
		static extern void g_object_class_override_property (IntPtr klass, uint prop_id, IntPtr name);

		public static void OverrideProperty (IntPtr oclass, uint property_id, string name)
		{
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			g_object_class_override_property (oclass, property_id, native_name);
		}

		[Obsolete ("Use OverrideProperty(oclass,property_id,name)")]
		public static void OverrideProperty (IntPtr declaring_class, string name)
		{
			OverrideProperty (declaring_class, idx, name);
			idx++;
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_object_class_find_property (IntPtr klass, IntPtr name);

		static IntPtr FindClassProperty (GType type, string name)
		{
			IntPtr g_iface = type.GetDefaultInterfacePtr ();
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			return g_object_class_find_property (g_iface, native_name);
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_object_interface_find_property (IntPtr klass, IntPtr name);

		static IntPtr FindInterfaceProperty (GType type, string name)
		{
			IntPtr g_iface = type.GetDefaultInterfacePtr ();
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			return g_object_interface_find_property (g_iface, native_name);
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

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void GetPropertyDelegate (IntPtr GObject, uint property_id, ref GLib.Value value, IntPtr pspec);

		static void GetPropertyCallback (IntPtr handle, uint property_id, ref GLib.Value value, IntPtr param_spec)
		{
			GLib.Object obj = GLib.Object.GetObject (handle, false);
			var type = (Type)obj.LookupGType ();

			Dictionary<IntPtr, PropertyInfo> props;
			if (!Properties.TryGetValue (type, out props))
				return;

			PropertyInfo prop;
			if (!props.TryGetValue (param_spec, out prop))
				return;

			value.Val = prop.GetValue (obj, new object [0]);
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
			// FIXME: Here is a big quick hack to avoid race condition when trying to set up adjustment with contructor
			// Because Raw is set too late
			if (param_spec != IntPtr.Zero) {
				ParamSpec foo =	new ParamSpec(param_spec);
				if (foo.Name == "gtk-sharp-managed-instance") {
					GCHandle gch = (GCHandle) (IntPtr) value.Val;
					Object o = (GLib.Object) gch.Target;
					o.Raw = handle;
				}
			}
			GLib.Object obj = GLib.Object.GetObject (handle, false);
			var type = (Type)obj.LookupGType ();

			Dictionary<IntPtr, PropertyInfo> props;
			if (!Properties.TryGetValue (type, out props))
				return;

			PropertyInfo prop;
			if (!props.TryGetValue (param_spec, out prop))
				return;

			prop.SetValue (obj, value.Val, new object [0]);
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

		protected internal static GType RegisterGType (System.Type t)
		{
			//this is a deprecated way of tracking a property counter,
			//but we may still need it for backwards compatibility
			idx = 1;

			return new ClassInitializer (t).Init ();
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

				lock (Objects) {
					if (handle != IntPtr.Zero) {
						Objects.Remove (handle);
						if (tref != null) {
							tref.Dispose ();
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

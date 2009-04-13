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
	using System.Collections;
	using System.ComponentModel;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;

	public class Object : IWrapper, IDisposable {

		IntPtr handle;
		ToggleRef tref;
		bool disposed = false;
		Hashtable data;
		static Hashtable Objects = new Hashtable();
		static ArrayList PendingDestroys = new ArrayList ();
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

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_unref (IntPtr raw);
		
		static bool PerformQueuedUnrefs ()
		{
			object [] references;

			lock (PendingDestroys){
				references = new object [PendingDestroys.Count];
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
			ToggleRef toggle_ref = Objects [Handle] as ToggleRef;
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

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_object_ref (IntPtr raw);

		public static Object GetObject(IntPtr o, bool owned_ref)
		{
			if (o == IntPtr.Zero)
				return null;

			Object obj = null;

			if (Objects.Contains (o)) {
				ToggleRef toggle_ref = Objects [o] as ToggleRef;
				if (toggle_ref != null && toggle_ref.IsAlive)
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

		private static void InvokeClassInitializers (GType gtype, System.Type t)
		{
			object[] parms = {gtype, t};

			BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;

			foreach (TypeInitializerAttribute tia in t.GetCustomAttributes (typeof (TypeInitializerAttribute), true)) {
				MethodInfo m = tia.Type.GetMethod (tia.MethodName, flags);
				if (m != null)
					m.Invoke (null, parms);
			}

			for (Type curr = t; curr != typeof(GLib.Object); curr = curr.BaseType) {
 
				if (curr.Assembly.IsDefined (typeof (IgnoreClassInitializersAttribute), false))
					continue;
 
				foreach (MethodInfo minfo in curr.GetMethods(flags))
					if (minfo.IsDefined (typeof (ClassInitializerAttribute), true))
						minfo.Invoke (null, parms);
			}
 		}
		
		//  Key: The pointer to the ParamSpec of the property
		//  Value: The corresponding PropertyInfo object
		static Hashtable properties;
		static Hashtable Properties {
			get {
				if (properties == null)
					properties = new Hashtable ();
				return properties;
			}
		}

		struct GTypeClass {
			IntPtr gtype;
		}

		struct GObjectClass {
			GTypeClass type_class;
			IntPtr construct_props;
			IntPtr constructor_cb;
			public SetPropertyDelegate set_prop_cb;
			public GetPropertyDelegate get_prop_cb;
			IntPtr dispose;
			IntPtr finalize;
			IntPtr dispatch_properties_changed;
			IntPtr notify;
			IntPtr constructed;
			IntPtr dummy1;
			IntPtr dummy2;
			IntPtr dummy3;
			IntPtr dummy4;
			IntPtr dummy5;
			IntPtr dummy6;
			IntPtr dummy7;
		}

		static void OverridePropertyHandlers (GType gtype, GetPropertyDelegate get_cb, SetPropertyDelegate set_cb)
		{
			IntPtr class_ptr = gtype.ClassPtr;
			GObjectClass klass = (GObjectClass) Marshal.PtrToStructure (class_ptr, typeof (GObjectClass));
			klass.get_prop_cb = get_cb;
			klass.set_prop_cb = set_cb;
			Marshal.StructureToPtr (klass, class_ptr, false);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_class_install_property (IntPtr klass, uint prop_id, IntPtr param_spec);

		static IntPtr RegisterProperty (GType type, string name, string nick, string blurb, uint property_id, GType property_type, bool can_read, bool can_write)
		{
			IntPtr declaring_class = type.ClassPtr;
			ParamSpec pspec = new ParamSpec (name, nick, blurb, property_type, can_read, can_write);

			g_object_class_install_property (declaring_class, property_id, pspec.Handle);
			return pspec.Handle;
		}

		static void AddProperties (GType gtype, System.Type t)
		{
			uint idx = 1;
			
			bool handlers_overridden = false;
			foreach (PropertyInfo pinfo in t.GetProperties (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)) {
				foreach (object attr in pinfo.GetCustomAttributes (typeof (PropertyAttribute), false)) {
					if(pinfo.GetIndexParameters().Length > 0)
						throw(new InvalidOperationException(String.Format("GLib.RegisterPropertyAttribute cannot be applied to property {0} of type {1} because the property expects one or more indexed parameters", pinfo.Name, t.FullName)));
					
					PropertyAttribute property_attr = attr as PropertyAttribute;
					if (!handlers_overridden) {
						OverridePropertyHandlers (gtype, GetPropertyHandler, SetPropertyHandler);
						handlers_overridden = true;
					}

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
		
		[GLib.CDeclCallback]
		delegate void GetPropertyDelegate (IntPtr GObject, uint property_id, ref GLib.Value value, IntPtr pspec);

		static void GetPropertyCallback (IntPtr handle, uint property_id, ref GLib.Value value, IntPtr param_spec)
		{
			GLib.Object obj = GLib.Object.GetObject (handle, false);
			value.Val = (Properties [param_spec] as PropertyInfo).GetValue (obj, new object [0]);
		}

		static GetPropertyDelegate get_property_handler;
		static GetPropertyDelegate GetPropertyHandler {
			get {
				if (get_property_handler == null)
					get_property_handler = new GetPropertyDelegate (GetPropertyCallback);
				return get_property_handler;
			}
		}

		[GLib.CDeclCallback]
		delegate void SetPropertyDelegate (IntPtr GObject, uint property_id, ref GLib.Value value, IntPtr pspec);

		static void SetPropertyCallback(IntPtr handle, uint property_id, ref GLib.Value value, IntPtr param_spec)
		{
			GLib.Object obj = GLib.Object.GetObject (handle, false);
			(Properties [param_spec] as PropertyInfo).SetValue (obj, value.Val, new object [0]);
		}

		static SetPropertyDelegate set_property_handler;
		static SetPropertyDelegate SetPropertyHandler {
			get {
				if (set_property_handler == null)
					set_property_handler = new SetPropertyDelegate (SetPropertyCallback);
				return set_property_handler;
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
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
			AddProperties (gtype, t);
			ConnectDefaultHandlers (gtype, t);
			InvokeClassInitializers (gtype, t);
			AddInterfaces (gtype, t);
			return gtype;
		}


		protected GType LookupGType ()
		{
			return LookupGType (GetType ());
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

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_object_new (IntPtr gtype, IntPtr dummy);

		[Obsolete]
		protected Object (GType gtype)
		{
			Raw = g_object_new (gtype.Val, IntPtr.Zero);
		}

		struct GParameter {
			public IntPtr name;
			public GLib.Value val;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_object_newv (IntPtr gtype, int n_params, GParameter[] parms);

		protected virtual void CreateNativeObject (string[] names, GLib.Value[] vals)
		{
			GParameter[] parms = new GParameter [names.Length];
			for (int i = 0; i < names.Length; i++) {
				parms [i].name = GLib.Marshaller.StringToPtrGStrdup (names [i]);
				parms [i].val = vals [i];
			}
			Raw = g_object_newv (LookupGType ().Val, parms.Length, parms);
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
			get {
				return GType.Object;
			}
		}

		protected string TypeName {
			get {
				return NativeType.ToString ();
			}
		}

		internal GLib.GType NativeType {
			get {
				return LookupGType ();
			}
		}

		internal ToggleRef ToggleRef {
			get {
				return tref;
			}
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public IntPtr OwnedHandle {
			get {
				return g_object_ref (handle);
			}
		}

		Hashtable before_signals;
		[Obsolete ("Replaced by GLib.Signal marshaling mechanism.")]
		protected internal Hashtable BeforeSignals {
			get {
				if (before_signals == null)
					before_signals = new Hashtable ();
				return before_signals;
			}
		}

		Hashtable after_signals;
		[Obsolete ("Replaced by GLib.Signal marshaling mechanism.")]
		protected internal Hashtable AfterSignals {
			get {
				if (after_signals == null)
					after_signals = new Hashtable ();
				return after_signals;
			}
		}

		EventHandlerList before_handlers;
		[Obsolete ("Replaced by GLib.Signal marshaling mechanism.")]
		protected EventHandlerList BeforeHandlers {
			get {
				if (before_handlers == null)
					before_handlers = new EventHandlerList ();
				return before_handlers;
			}
		}

		EventHandlerList after_handlers;
		[Obsolete ("Replaced by GLib.Signal marshaling mechanism.")]
		protected EventHandlerList AfterHandlers {
			get {
				if (after_handlers == null)
					after_handlers = new EventHandlerList ();
				return after_handlers;
			}
		}

		[CDeclCallback]
		delegate void NotifyDelegate (IntPtr handle, IntPtr pspec, IntPtr gch);

		void NotifyCallback (IntPtr handle, IntPtr pspec, IntPtr gch)
		{
			try {
				GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
				if (sig == null)
					throw new Exception("Unknown signal GC handle received " + gch);

				NotifyArgs args = new NotifyArgs ();
				args.Args = new object[1];
				args.Args[0] = pspec;
				NotifyHandler handler = (NotifyHandler) sig.Handler;
				handler (GLib.Object.GetObject (handle), args);
			} catch (Exception e) {
				ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		void ConnectNotification (string signal, NotifyHandler handler)
		{
			Signal sig = Signal.Lookup (this, signal, new NotifyDelegate (NotifyCallback));
			sig.AddDelegate (handler);
		}

		public void AddNotification (string property, NotifyHandler handler)
		{
			ConnectNotification ("notify::" + property, handler);
		}

		public void AddNotification (NotifyHandler handler)
		{
			ConnectNotification ("notify", handler);
		}

		void DisconnectNotification (string signal, NotifyHandler handler)
		{
			Signal sig = Signal.Lookup (this, signal, new NotifyDelegate (NotifyCallback));
			sig.RemoveDelegate (handler);
		}

		public void RemoveNotification (string property, NotifyHandler handler)
		{
			DisconnectNotification ("notify::" + property, handler);
		}

		public void RemoveNotification (NotifyHandler handler)
		{
			DisconnectNotification ("notify", handler);
		}

		public override int GetHashCode ()
		{
			return Handle.GetHashCode ();
		}

		public Hashtable Data {
			get { 
				if (data == null)
					data = new Hashtable ();
				
				return data;
			}
		}

		Hashtable persistent_data;
		protected Hashtable PersistentData {
			get {
				if (persistent_data == null)
					persistent_data = new Hashtable ();
				
				return persistent_data;
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_get_property (IntPtr obj, IntPtr name, ref GLib.Value val);

		protected GLib.Value GetProperty (string name)
		{
			Value val = new Value (this, name);
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			g_object_get_property (Raw, native_name, ref val);
			GLib.Marshaller.Free (native_name);
			return val;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_set_property (IntPtr obj, IntPtr name, ref GLib.Value val);

		protected void SetProperty (string name, GLib.Value val)
		{
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			g_object_set_property (Raw, native_name, ref val);
			GLib.Marshaller.Free (native_name);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_notify (IntPtr obj, IntPtr property_name);

		protected void Notify (string property_name)
		{
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (property_name);
			g_object_notify (Handle, native_name);
			GLib.Marshaller.Free (native_name);
		}

		protected static void OverrideVirtualMethod (GType gtype, string name, Delegate cb)
		{
			Signal.OverrideDefaultHandler (gtype, name, cb);
		}

		[DllImport("libgobject-2.0-0.dll")]
		protected static extern void g_signal_chain_from_overridden (IntPtr args, ref GLib.Value retval);

		[DllImport("libgobject-2.0-0.dll")]
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

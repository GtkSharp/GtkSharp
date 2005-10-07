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

		IntPtr _obj;
		bool disposed = false;
		Hashtable data;
		static Hashtable Objects = new Hashtable();
		static ArrayList PendingDestroys = new ArrayList ();
		static bool idle_queued;

		~Object ()
		{
			Dispose ();
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_unref (IntPtr raw);
		
		static bool PerformQueuedUnrefs ()
		{
			Object [] objects;

			lock (PendingDestroys){
				objects = new Object [PendingDestroys.Count];
				PendingDestroys.CopyTo (objects, 0);
				PendingDestroys.Clear ();
			}
			lock (typeof (Object))
				idle_queued = false;

			foreach (Object o in objects){
				if (o._obj == IntPtr.Zero)
					continue;

				try {
					g_object_unref (o._obj);
				} catch (Exception e) {
					Console.WriteLine ("Exception while disposing a " + o + " in Gtk#");
					throw e;
				}
				Objects.Remove (o._obj);
				o._obj = IntPtr.Zero;
			}
			return false;
		}

		public virtual void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;
			if (MainContext.Depth > 0) {
				g_object_unref (_obj);
				Objects.Remove (_obj);
				GC.SuppressFinalize (this);
				return;
			}

			lock (PendingDestroys){
				PendingDestroys.Add (this);
				lock (typeof (Object)){
					if (!idle_queued){
						Timeout.Add (50, new TimeoutHandler (PerformQueuedUnrefs));
						idle_queued = true;
					}
				}
			}
			GC.SuppressFinalize (this);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_ref (IntPtr raw);

		public static Object GetObject(IntPtr o, bool owned_ref)
		{
			if (o == IntPtr.Zero)
				return null;

			Object obj = null;
			WeakReference weak_ref = Objects[o] as WeakReference;

			if (weak_ref != null && weak_ref.IsAlive)
				obj = weak_ref.Target as Object;

			if (obj == null)
				obj = Objects[o] as Object;

			if (obj != null && obj._obj == o) {
				lock (PendingDestroys)
					PendingDestroys.Remove (obj);
				if (owned_ref)
					g_object_unref (obj._obj);
				obj.disposed = false;
				return obj;
			}

			obj = GLib.ObjectManager.CreateObject(o); 
			if (obj == null)
				return null;

			if (!owned_ref)
				g_object_ref (obj.Handle);
			Objects [o] = new WeakReference (obj);
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
					MethodInfo connector = sigattr.Type.GetMethod (sigattr.ConnectionMethod, BindingFlags.Static | BindingFlags.NonPublic);
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
			BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
			foreach (MethodInfo minfo in t.GetMethods(flags))
				if (minfo.IsDefined (typeof (ClassInitializerAttribute), true))
					minfo.Invoke (null, parms);
		}

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_register_type (IntPtr name, IntPtr parent_type);


		static int type_uid;
		static string BuildEscapedName (System.Type t)
		{
			string qn = t.FullName;
			// Just a random guess
			StringBuilder sb = new StringBuilder (20 + qn.Length);
			sb.Append ("__gtksharp_");
			sb.Append (type_uid++);
			sb.Append ("_");
			foreach (char c in qn) {
				if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
					sb.Append (c);
				else if (c == '.')
					sb.Append ('_');
				else if ((uint) c <= byte.MaxValue) {
					sb.Append ('+');
					sb.Append (((byte) c).ToString ("x2"));
				} else {
					sb.Append ('-');
					sb.Append (((uint) c).ToString ("x4"));
				}
			}
			return sb.ToString ();
		}

		protected static GType RegisterGType (System.Type t)
		{
			GType parent_gtype = LookupGType (t.BaseType);
			string name = BuildEscapedName (t);
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			GType gtype = new GType (gtksharp_register_type (native_name, parent_gtype.Val));
			GLib.Marshaller.Free (native_name);
			GLib.GType.Register (gtype, t);
			ConnectDefaultHandlers (gtype, t);
			InvokeClassInitializers (gtype, t);
			g_types[t] = gtype;
			return gtype;
		}


		static Hashtable g_types = new Hashtable ();

		protected GType LookupGType ()
		{
			return LookupGType (GetType ());
		}

		protected internal static GType LookupGType (System.Type t)
		{
			if (g_types.Contains (t))
				return (GType) g_types [t];
			
			PropertyInfo pi = t.GetProperty ("GType", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
			if (pi != null)
				return (GType) pi.GetValue (null, null);
			
			return RegisterGType (t);
		}

		protected Object (IntPtr raw)
		{
			Raw = raw;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_object_new (IntPtr gtype, IntPtr dummy);

		[Obsolete]
		protected Object (GType gtype)
		{
			Raw = g_object_new (gtype.Val, IntPtr.Zero);
		}

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_object_newv (IntPtr gtype, int n_params, IntPtr[] names, GLib.Value[] vals);

		protected virtual void CreateNativeObject (string[] names, GLib.Value[] vals)
		{
			IntPtr[] native_names = new IntPtr [names.Length];
			for (int i = 0; i < names.Length; i++)
				native_names [i] = GLib.Marshaller.StringToPtrGStrdup (names [i]);
			Raw = gtksharp_object_newv (LookupGType ().Val, names.Length, native_names, vals);
			Objects [_obj] = this;
			foreach (IntPtr p in native_names)
				GLib.Marshaller.Free (p);
		}

		protected virtual IntPtr Raw {
			get {
				return _obj;
			}
			set {
				if (_obj != IntPtr.Zero)
					Objects.Remove (_obj);
				_obj = value;
				if (value == IntPtr.Zero)
					return;
				Objects [value] = new WeakReference (this);
			}
		}	

		public static GLib.GType GType {
			get {
				return GType.Object;
			}
		}

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_get_type_name (IntPtr raw);

		protected string TypeName {
			get {
				return Marshaller.Utf8PtrToString (gtksharp_get_type_name (Raw));
			}
		}

		internal GLib.GType NativeType {
			get {
				return LookupGType ();
			}
		}

		public IntPtr Handle {
			get {
				return _obj;
			}
		}

		Hashtable before_signals;

		[Obsolete ("Replaced by GLib.Signal marshaling mechanism.")]
		protected Hashtable BeforeSignals {
			get {
				if (before_signals == null)
					before_signals = new Hashtable ();
				return before_signals;
			}
		}

		Hashtable after_signals;
		[Obsolete ("Replaced by GLib.Signal marshaling mechanism.")]
		protected Hashtable AfterSignals {
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
			GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
			if (sig == null)
				throw new Exception("Unknown signal GC handle received " + gch);

			NotifyArgs args = new NotifyArgs ();
			args.Args = new object[1];
			args.Args[0] = pspec;
			NotifyHandler handler = (NotifyHandler) sig.Handler;
			handler (GLib.Object.GetObject (handle), args);
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

		[Obsolete("Can cause instability due to garbage collection of GLib.Objects.")]
		public Hashtable Data {
			get { 
				if (data == null)
					data = new Hashtable ();
				
				return data;
			}
		}

		protected Hashtable PersistentData {
			get {
				return WeakObject.Lookup (Handle).Data;
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

		[DllImport("glibsharpglue-2")]
		static extern void gtksharp_override_virtual_method (IntPtr gtype, IntPtr name, Delegate cb);

		protected static void OverrideVirtualMethod (GType gtype, string name, Delegate cb)
		{
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			gtksharp_override_virtual_method (gtype.Val, native_name, cb);
			GLib.Marshaller.Free (native_name);
		}

		[DllImport("libgobject-2.0-0.dll")]
		protected static extern void g_signal_chain_from_overridden (IntPtr args, ref GLib.Value retval);

		[DllImport("glibsharpglue-2")]
		static extern bool gtksharp_is_object (IntPtr obj);

		internal static bool IsObject (IntPtr obj)
		{
			return gtksharp_is_object (obj);
		}

		[DllImport("glibsharpglue-2")]
		static extern int gtksharp_object_get_ref_count (IntPtr obj);

		protected int RefCount {
			get {
				return gtksharp_object_get_ref_count (Handle);
			}
		}
	}
}

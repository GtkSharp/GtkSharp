// Object.cs - GObject class wrapper implementation
//
// Authors: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
// Copyright (c) 2004 Novell, Inc.
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

	public class Object : IWrapper, IDisposable {

		IntPtr _obj;
		bool disposed = false;
		Hashtable data;
		static Hashtable Objects = new Hashtable();
		static Queue PendingDestroys = new Queue ();
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

				g_object_unref (o._obj);
				o._obj = IntPtr.Zero;
			}
			return false;
		}

		public void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;
			Objects.Remove (_obj);
			lock (PendingDestroys){
				PendingDestroys.Enqueue (this);
				lock (typeof (Object)){
					if (!idle_queued){
						Idle.Add (new IdleHandler (PerformQueuedUnrefs));
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
			Object obj;
			WeakReference weak_ref = Objects[o] as WeakReference;
			if (weak_ref != null && weak_ref.IsAlive) {
				obj = weak_ref.Target as GLib.Object;
				if (owned_ref)
					g_object_unref (obj._obj);
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

				foreach (object attr in baseinfo.GetCustomAttributes (true)) {
					if (attr.ToString () != "GLib.DefaultSignalHandlerAttribute")
						continue;

					DefaultSignalHandlerAttribute sigattr = attr as DefaultSignalHandlerAttribute;
					MethodInfo connector = sigattr.Type.GetMethod (sigattr.ConnectionMethod, BindingFlags.Static | BindingFlags.NonPublic);
					object[] parms = new object [1];
					parms [0] = gtype;
					connector.Invoke (null, parms);
					break;
				}
			}
					
		}

		[DllImport("glibsharpglue")]
		static extern IntPtr gtksharp_register_type (string name, IntPtr parent_type);

		protected static GType RegisterGType (System.Type t)
		{
			GType parent_gtype = LookupGType (t.BaseType);
			string name = t.FullName.Replace(".", "_");
			GLib.ObjectManager.RegisterType (name, t.FullName, t.Assembly.GetName().Name);
			GType gtype = new GType (gtksharp_register_type (name, parent_gtype.Val));
			ConnectDefaultHandlers (gtype, t);
			g_types[t] = gtype;
			return gtype;
		}


		static Hashtable g_types = new Hashtable ();

		protected GType LookupGType ()
		{
			return LookupGType (GetType ());
		}

		protected static GType LookupGType (System.Type t)
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

		protected Object (GType gtype)
		{
			Raw = g_object_new (gtype.Val, IntPtr.Zero);
		}

		[DllImport("glibsharpglue")]
		static extern IntPtr gtksharp_object_newv (IntPtr gtype, int n_params, string[] names, GLib.Value[] vals);

		protected void CreateNativeObject (string[] names, GLib.Value[] vals)
		{
			Raw = gtksharp_object_newv (LookupGType ().Val, names.Length, names, vals);
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

		[DllImport("glibsharpglue")]
		private static extern IntPtr gtksharp_get_type_id (IntPtr obj);

		public static GLib.GType GType {
			get {
				return GType.Object;
			}
		}

		[DllImport("glibsharpglue")]
		static extern IntPtr gtksharp_get_type_name (IntPtr raw);

		protected string TypeName {
			get {
				return Marshal.PtrToStringAnsi (gtksharp_get_type_name (Raw));
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
		protected Hashtable BeforeSignals {
			get {
				if (before_signals == null)
					before_signals = new Hashtable ();
				return before_signals;
			}
		}

		Hashtable after_signals;
		protected Hashtable AfterSignals {
			get {
				if (after_signals == null)
					after_signals = new Hashtable ();
				return after_signals;
			}
		}

		EventHandlerList before_handlers;
		protected EventHandlerList BeforeHandlers {
			get {
				if (before_handlers == null)
					before_handlers = new EventHandlerList ();
				return before_handlers;
			}
		}

		EventHandlerList after_handlers;
		protected EventHandlerList AfterHandlers {
			get {
				if (after_handlers == null)
					after_handlers = new EventHandlerList ();
				return after_handlers;
			}
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

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_get_property (IntPtr obj, string name, ref GLib.Value val);

		protected GLib.Value GetProperty (string name)
		{
			Value val = new Value (this, name);
			g_object_get_property (Raw, name, ref val);
			return val;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_set_property (IntPtr obj, string name, ref GLib.Value val);

		protected void SetProperty (string name, GLib.Value val)
		{
			g_object_set_property (Raw, name, ref val);
		}

		[DllImport("glibsharpglue")]
		static extern void gtksharp_override_virtual_method (IntPtr gtype, string name, Delegate cb);

		protected static void OverrideVirtualMethod (GType gtype, string name, Delegate cb)
		{
			gtksharp_override_virtual_method (gtype.Val, name, cb);
		}

		[DllImport("libgobject-2.0-0.dll")]
		protected static extern void g_signal_chain_from_overridden (IntPtr args, ref GLib.Value retval);

		[DllImport("glibsharpglue")]
		static extern bool gtksharp_is_object (IntPtr obj);

		internal static bool IsObject (IntPtr obj)
		{
			return gtksharp_is_object (obj);
		}

		[DllImport("glibsharpglue")]
		static extern int gtksharp_object_get_ref_count (IntPtr obj);

		protected int RefCount {
			get {
				return gtksharp_object_get_ref_count (Handle);
			}
		}
	}
}

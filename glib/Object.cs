// Object.cs - GObject class wrapper implementation
//
// Authors: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2003 Mike Kestner
//
// TODO:
//   Could remove `disposed' for a check if an object is on the dispose_queue_list.
//
namespace GLib {

	using System;
	using System.Collections;
	using System.ComponentModel;
	using System.Reflection;
	using System.Runtime.InteropServices;

	/// <summary>
	///	Object Class
	/// </summary>
	///
	/// <remarks>
	///	Wrapper class for GObject.
	/// </remarks>

	public class Object : IWrapper, IDisposable {

		// Private class and instance members
		IntPtr _obj;
		EventHandlerList _events;
		bool disposed = false;
		Hashtable data;
		static Hashtable Objects = new Hashtable();
		static Queue PendingDestroys = new Queue ();
		static bool idle_queued;

		//
		// The destructor is invoked by a thread
		//
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

		/// <summary>
		///	Dispose Method 
		/// </summary>
		///
		/// <remarks>
		///	Disposes of the raw object. Only override this if
		///	the Raw object should not be unref'd when the object
		///	is garbage collected.
		/// </remarks>

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

		/// <summary>
		///	GetObject Shared Method 
		/// </summary>
		///
		/// <remarks>
		///	Used to obtain a CLI typed object associated with a 
		///	given raw object pointer. This method is primarily
		///	used to wrap object references that are returned 
		///	by either the signal system or raw class methods that
		///	return GObject references.
		/// </remarks>
		///
		/// <returns>
		///	The wrapper instance.
		/// </returns>

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

			obj = GtkSharp.ObjectManager.CreateObject(o); 
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
			foreach (MethodInfo minfo in t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
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

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_register_type (string name, IntPtr parent_type);

		/// <summary>
		///	RegisterGType Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Shared method to register types with the GType system.
		///	This method should be called from the class constructor
		///	of subclasses.
		/// </remarks>

		public static GType RegisterGType (System.Type t)
		{
			System.Type parent = t.BaseType;
			PropertyInfo pi = parent.GetProperty ("GType", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
			if (pi == null) {
				Console.WriteLine ("null PropertyInfo");
				return GType.Invalid;
			}
			GType parent_gtype = (GType) pi.GetValue (null, null);
			string name = t.Namespace.Replace(".", "_") + t.Name;
			GtkSharp.ObjectManager.RegisterType (name, t.Namespace + t.Name, t.Assembly.GetName().Name);
			GType gtype = new GType (gtksharp_register_type (name, parent_gtype.Val));
			ConnectDefaultHandlers (gtype, t);
			return gtype;
		}

		/// <summary>
		///	Object Constructor
		/// </summary>
		///
		/// <remarks>
		///	Dummy constructor needed for derived classes.
		/// </remarks>

		protected Object () {}

		/// <summary>
		///	Object Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates an object from a raw object reference.
		/// </remarks>

		public Object (IntPtr raw)
		{
			Raw = raw;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_object_new (IntPtr gtype, IntPtr dummy);

		/// <summary>
		///	Object Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates an object from a specified GType.
		/// </remarks>

		protected Object (GType gtype)
		{
			Raw = g_object_new (gtype.Val, IntPtr.Zero);
		}

		/// <summary>
		///	Raw Property
		/// </summary>
		///
		/// <remarks>
		///	The raw GObject reference associated with this wrapper.
		///	Only subclasses of Object can access this read/write
		///	property.  For public read-only access, use the
		///	Handle property.
		/// </remarks>

		protected virtual IntPtr Raw {
			get {
				return _obj;
			}
			set {
				Objects [value] = new WeakReference (this);
				_obj = value;
			}
		}	

		/// <summary>
		///	GType Property
		/// </summary>
		///
		/// <remarks>
		///	The type associated with this object class.
		/// </remarks>

		[DllImport("gtksharpglue")]
		private static extern IntPtr gtksharp_get_type_id (IntPtr obj);

		public static GLib.GType GType {
			get {
				return GType.Invalid;
			}
		}

		[DllImport("gtksharpglue")]
		static extern IntPtr gtksharp_get_type_name (IntPtr raw);

		public string TypeName {
			get {
				return Marshal.PtrToStringAnsi (gtksharp_get_type_name (Raw));
			}
		}

		/// <summary>
		///	GetGType Method
		/// </summary>
		///
		/// <remarks>
		///	Returns the GType of this object.
		/// </remarks>

		public GLib.GType GetGType () {
			if (_obj == IntPtr.Zero)
				return GType.Invalid;

			return new GLib.GType (gtksharp_get_type_id (_obj));
		}

		/// <summary>
		///	Handle Property
		/// </summary>
		///
		/// <remarks>
		///	The raw GObject reference associated with this object.
		///	Subclasses can use Raw property for read/write
		///	access.
		/// </remarks>

		public IntPtr Handle {
			get {
				return _obj;
			}
		}

		/// <summary>
		///	EventList Property
		/// </summary>
		///
		/// <remarks>
		///	A list object containing all the events for this 
		///	object indexed by the Gtk+ signal name.
		/// </remarks>

		protected EventHandlerList EventList {
			get {
				if (_events == null)
					_events = new EventHandlerList ();
				return _events;
			}
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>

		public override int GetHashCode ()
		{
			return Handle.GetHashCode ();
		}

		/// <summary>
		///	Data property
		/// </summary>
		///
		/// <remarks>
		///	Stores and Accesses arbitrary data on the Object.
		/// </remarks>

		public Hashtable Data {
			get { 
				if (data == null)
					data = new Hashtable ();
				
				return data;
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_get_property (
				IntPtr obj, string name, IntPtr val);

		/// <summary>
		///	GetProperty Method
		/// </summary>
		///
		/// <remarks>
		///	Accesses a Value Property.
		/// </remarks>

		public void GetProperty (String name, GLib.Value val)
		{
			g_object_get_property (Raw, name, val.Handle);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_set_property (
				IntPtr obj, string name, IntPtr val);

		/// <summary>
		///	SetProperty Method
		/// </summary>
		///
		/// <remarks>
		///	Accesses a Value Property.
		/// </remarks>

		public void SetProperty (String name, GLib.Value val)
		{
			g_object_set_property (Raw, name, val.Handle);
		}

		[DllImport("gtksharpglue")]
		static extern void gtksharp_override_virtual_method (IntPtr gtype, string name, Delegate cb);

		protected static void OverrideVirtualMethod (GType gtype, string name, Delegate cb)
		{
			gtksharp_override_virtual_method (gtype.Val, name, cb);
		}

		[DllImport("libgobject-2.0-0.dll")]
		protected static extern void g_signal_chain_from_overridden (IntPtr args, IntPtr retval);

		[DllImport("gtksharpglue")]
		static extern bool gtksharp_is_object (IntPtr obj);

		internal static bool IsObject (IntPtr obj)
		{
			return gtksharp_is_object (obj);
		}

		[DllImport("gtksharpglue")]
		static extern int gtksharp_object_get_ref_count (IntPtr obj);

		public int RefCount {
			get {
				return gtksharp_object_get_ref_count (Handle);
			}
		}
	}
}

// Object.cs - GObject class wrapper implementation
//
// Authors: Bob Smith <bob@thestuff.net>
//	    Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Bob Smith and Mike Kestner

namespace GLib {

	using System;
	using System.Collections;
	using System.ComponentModel;
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
		Hashtable Data;
		static Hashtable Objects = new Hashtable();

		~Object ()
		{
			Dispose ();
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

			DisposeNative ();
			disposed = true;
		}

		[DllImport("gobject-2.0")]
		static extern void g_object_unref (IntPtr raw);
		
		protected virtual void DisposeNative ()
		{
			if (_obj == IntPtr.Zero)
				return;

			GC.SuppressFinalize (this);
			g_object_unref (_obj);
			_obj = IntPtr.Zero;
		}

		[DllImport("gobject-2.0")]
		static extern void g_object_ref (IntPtr raw);

		/// <summary>
		///   Ref Method
		/// </summary>
		///
		/// <remarks>
		///   Increases the reference count on the native object.
		///   This method is used by generated classes and structs,
		///   and should not be used in user code.
		/// </remarks>
		public virtual void Ref ()
		{
			if (_obj == IntPtr.Zero)
				return;

			g_object_ref (_obj);
		}
		
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

		public static Object GetObject(IntPtr o)
		{
			Object obj = (Object)Objects[o];
			if (obj != null) return obj;
			return GtkSharp.ObjectManager.CreateObject(o); 
		}

		/// <summary>
		///	Object Constructor
		/// </summary>
		///
		/// <remarks>
		///	Dummy constructor needed for derived classes.
		/// </remarks>

		public Object () {}

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

		protected IntPtr Raw {
			get {
				return _obj;
			}
			set {
				Objects [value] = this;
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

		public static int GType {
			get {
				return 0;
			}
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
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of two Objects.
		/// </remarks>

		public override bool Equals (object o)
		{
			if (!(o is Object))
				return false;

			return (Handle == ((Object) o).Handle);
		}

		public static bool operator == (Object a, Object b)
		{
			return a.Equals (b);
		}

		public static bool operator != (Object a, Object b)
		{
			return !a.Equals (b);
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
		///	GetData Method
		/// </summary>
		///
		/// <remarks>
		///	Accesses arbitrary data storage on the Object.
		/// </remarks>

		public object GetData (string key)
		{
			if (Data == null)
				return String.Empty;

			return Data [key];
		}

		/// <summary>
		///	SetData Method
		/// </summary>
		///
		/// <remarks>
		///	Stores arbitrary data on the Object.
		/// </remarks>

		public void SetData (string key, object val)
		{
			if (Data == null)
				Data = new Hashtable ();

			Data [key] = val;
		}

		[DllImport("gobject-2.0")]
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

		[DllImport("gobject-2.0")]
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
		static extern bool gtksharp_is_object (IntPtr obj);

		internal static bool IsObject (IntPtr obj)
		{
			return gtksharp_is_object (obj);
		}
	}
}

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

	public class Object  {

		// Private class and instance members
		IntPtr _obj;
		EventHandlerList _events;
		Hashtable Data;
		static Hashtable Objects = new Hashtable();

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
			Object obj = (Object)Objects[(int)o];
			if (obj != null) return obj;
			return null; //FIXME: Call TypeParser here eventually.
		}


		/// <summary>
		///	RawObject Property
		/// </summary>
		///
		/// <remarks>
		///	The raw GObject reference associated with this wrapper.
		///	Only subclasses of Object can access this read/write
		///	property.  For public read-only access, use the
		///	Handle property.
		/// </remarks>

		protected IntPtr RawObject {
			get {
				return _obj;
			}
			set {
				Objects [value] = this;
				_obj = value;
			}
		}       

		/// <summary>
		///	Handle Property
		/// </summary>
		///
		/// <remarks>
		///	The raw GObject reference associated with this object.
		///	Subclasses can use RawObject property for read/write
		///	access.
		/// </remarks>

		public IntPtr Handle {
			get {
				return _obj;
			}
		}

		/// <summary>
		///	Events Property
		/// </summary>
		///
		/// <remarks>
		///	A list object containing all the events for this 
		///	object indexed by the Gtk+ signal name.
		/// </remarks>

		protected EventHandlerList Events {
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

		public object GetData (String key)
		{
			if (Data == null)
				return null;

			return Data [key];
		}

		/// <summary>
		///	SetData Method
		/// </summary>
		///
		/// <remarks>
		///	Stores arbitrary data on the Object.
		/// </remarks>

		public void SetData (String key, object val)
		{
			if (Data == null)
				Data = new Hashtable ();

			Data [key] = val;
		}

		/// <summary>
		///	GetProperty Method
		/// </summary>
		///
		/// <remarks>
		///	Accesses a string Property.
		/// </remarks>

		[DllImport("gobject-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern void g_object_get (IntPtr obj, IntPtr name,
                                             	 out IntPtr val, IntPtr term);

		public void GetProperty (String name, out String val)
		{
			IntPtr propval;
			g_object_get (RawObject, 
				      Marshal.StringToHGlobalAnsi (name), 
				      out propval, new IntPtr (0));
			val = Marshal.PtrToStringAnsi (propval);
		}

		/// <summary>
		///	GetProperty Method
		/// </summary>
		///
		/// <remarks>
		///	Accesses a boolean Property.
		/// </remarks>

		[DllImport("gobject-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern void g_object_get (IntPtr obj, IntPtr name,
                                             	 out bool val, IntPtr term);

		public void GetProperty (String name, out bool val)
		{
			g_object_get (RawObject, 
				      Marshal.StringToHGlobalAnsi (name), 
				      out val, new IntPtr (0));
		}

		/// <summary>
		///	GetProperty Method
		/// </summary>
		///
		/// <remarks>
		///	Accesses an integer Property.
		/// </remarks>

		[DllImport("gobject-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern void g_object_get (IntPtr obj, IntPtr name,
                                             	 out int val, IntPtr term);

		public void GetProperty (String name, out int val)
		{
			g_object_get (RawObject, 
				      Marshal.StringToHGlobalAnsi (name), 
				      out val, new IntPtr (0));
		}

		/// <summary>
		///	GetProperty Method
		/// </summary>
		///
		/// <remarks>
		///	Accesses an Object Property.
		/// </remarks>

		public void GetProperty (String name, out GLib.Object val)
		{
			IntPtr obj;
			g_object_get (RawObject, 
				      Marshal.StringToHGlobalAnsi (name), 
				      out obj, new IntPtr (0));
			val = GLib.Object.GetObject (obj);
		}

		/// <summary>
		///	SetProperty Method
		/// </summary>
		///
		/// <remarks>
		///	Changes the value of a string Property.
		/// </remarks>

		[DllImport("gobject-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern void g_object_set (IntPtr obj, IntPtr name,
                                                 IntPtr val, IntPtr term);

		public void SetProperty (String name, String val)
		{
			g_object_set (RawObject, 
				      Marshal.StringToHGlobalAnsi (name), 
				      Marshal.StringToHGlobalAnsi (val), 
				      new IntPtr (0));
		}

		/// <summary>
		///	SetProperty Method
		/// </summary>
		///
		/// <remarks>
		///	Changes the value of an integer Property.
		/// </remarks>

		[DllImport("gobject-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern void g_object_set (IntPtr obj, IntPtr name,
                                                 int val, IntPtr term);

		public void SetProperty (String name, int val)
		{
			g_object_set (RawObject, 
				      Marshal.StringToHGlobalAnsi (name), 
				      val, new IntPtr (0));
		}

		/// <summary>
		///	SetProperty Method
		/// </summary>
		///
		/// <remarks>
		///	Changes the value of a boolean Property.
		/// </remarks>

		[DllImport("gobject-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern void g_object_set (IntPtr obj, IntPtr name,
                                                 bool val, IntPtr term);

		public void SetProperty (String name, bool val)
		{
			g_object_set (RawObject, 
				      Marshal.StringToHGlobalAnsi (name), 
				      val, new IntPtr (0));
		}

		/// <summary>
		///	SetProperty Method
		/// </summary>
		///
		/// <remarks>
		///	Changes the value of an Object Property.
		/// </remarks>

		public void SetProperty (String name, GLib.Object val)
		{
			g_object_set (RawObject, 
				      Marshal.StringToHGlobalAnsi (name), 
				      val.Handle, new IntPtr (0));
		}


/*
		[DllImport("gtk-1.3.dll")]
		static extern void g_object_set_data_full (
					IntPtr obj,
					String key,
					IntPtr data,
					DestroyNotify destroy );


GParamSpec* g_object_class_find_property (GObjectClass *oclass,
                                             const gchar *property_name);
GParamSpec** g_object_class_list_properties (GObjectClass *oclass,
                                             guint *n_properties);
gpointer g_object_ref (gpointer object);
void        g_object_unref (gpointer object);
void        g_object_weak_ref (GObject *object,
                                             GWeakNotify notify,
                                             gpointer data);
void        g_object_weak_unref (GObject *object,
                                             GWeakNotify notify,
                                             gpointer data);
void        g_object_add_weak_pointer (GObject *object,
                                             gpointer *weak_pointer_location);
void        g_object_remove_weak_pointer (GObject *object,
                                             gpointer *weak_pointer_location);
gpointer g_object_connect (gpointer object,
                                             const gchar *signal_spec,
                                             ...);
void        g_object_disconnect (gpointer object,
                                             const gchar *signal_spec,
                                             ...);
void        g_object_set (gpointer object,
                                             const gchar *first_property_name,
                                             ...);
void        g_object_get (gpointer object,
                                             const gchar *first_property_name,
                                             ...);
void        g_object_notify (GObject *object,
                                             const gchar *property_name);
void        g_object_freeze_notify (GObject *object);
void        g_object_thaw_notify (GObject *object);
void        g_object_set_data_full (GObject *object,
                                             const gchar *key,
                                             gpointer data,
                                             GDestroyNotify destroy);
gpointer g_object_steal_data (GObject *object,
                                             const gchar *key);
gpointer g_object_get_qdata (GObject *object,
                                             GQuark quark);
void        g_object_set_qdata (GObject *object,
                                             GQuark quark,
                                             gpointer data);
void        g_object_set_qdata_full (GObject *object,
                                             GQuark quark,
                                             gpointer data,
                                             GDestroyNotify destroy);
gpointer g_object_steal_qdata (GObject *object,
                                             GQuark quark);
void        g_object_watch_closure (GObject *object,
                                             GClosure *closure);
void        g_object_run_dispose (GObject *object);
*/

	}
}

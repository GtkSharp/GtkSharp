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
		///	Only subclasses of Object should need to access this
		///	unmanaged pointer.
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
		///	Events Property
		/// </summary>
		///
		/// <remarks>
		///	A list object containing all the events for this 
		///	object indexed by the Gtk+ signal name.
		/// </remarks>

		public EventHandlerList Events {
			get {
				if (_events == null)
					_events = new EventHandlerList ();
				return _events;
			}
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
void        g_object_set_property (GObject *object,
                                             const gchar *property_name,
                                             const GValue *value);
void        g_object_get_property (GObject *object,
                                             const gchar *property_name,
                                             GValue *value);
void        g_object_watch_closure (GObject *object,
                                             GClosure *closure);
void        g_object_run_dispose (GObject *object);
gpointer g_value_get_object (const GValue *value);
*/

	}
}

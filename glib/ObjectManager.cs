// ObjectManager.cs - GObject class wrapper helper implementation
//
// Author: Bob Smith <bob@thestuff.net>
//
// (c) 2001 Bob Smith

namespace Glib {

	using System;
	using System.Runtime.InteropServices;

	protected delegate void SignalFunc ();
	protected delegate void DestroyNotify (IntPtr data);

	public class ObjectManager  {
		public ObjectManager(IntPtr o)
		{
			if (o == null) throw new ArgumentNullException ();
			Object=o;
			g_object_set_data_full(Object, "gobject#-object-manager",
				this, new DestroyNotify(DestroyNotifyEvent));

		}
		private IntPtr _obj;

		IntPtr Object
		{
			get
			{
				return _obj;
			}
			set
			{
				_obj = value;
			}
		}
		private EventHandlerList _events;
		protected EventHandlerList Events
		{
			get
			{
				if (_events != null) return _events;
				_events = new EventHandlerList ();
			}
		}

		[DllImport("gtk-1.3")]
		static extern void g_object_set_data_full (
					IntPtr object,
					String key,
					IntPtr data,
					DestroyNotify destroy );

		private void DestroyNotifyEvent (IntPtr data)
		{
			_events = null;
		}
//TODO: Replace IntPtr's with Types.
		[DllImport("gtk-1.3")]
		static extern long g_signal_connect_closure_by_id (
					IntPtr instance,
					int signalID,
					IntPtr detail,
					IntPtr closure,
					bool after );
		[DllImport("gtk-1.3")]
		static extern int g_signal_lookup (
					String name,
					IntPtr itype );
		[DllImport("gtk-1.3")]
		static extern IntPtr g_cclosure_new (
					IntPtr callback_func,
					IntPtr user_data,
					IntPtr destroy_data );

		[DllImport("gtk-1.3")]
		static extern IntPtr g_cclosure_new_swap (
					IntPtr callback_func,
					IntPtr user_data,
					IntPtr destroy_data );
		public long SignalConnect(String name, SignalFunc func, IntPtr data){
			SignalConnectFull(name, func, data, null, 0, 0);
		}
		public long SignalConnectFull(String name, SignalFunc func, IntPtr data, DestroyNotify destroy, int objectSignal, int after){
			return g_signal_connect_closure_by_id (Object,
					g_signal_lookup (name, Object), 0, //Need to cast object?
						(object_signal ? g_cclosure_new_swap(func, data, destroy)
								: g_cclosure_new(func, data, destroy),
						after);

		}

	}
}

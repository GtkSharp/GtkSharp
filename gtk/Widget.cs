// GTK.Widget.cs - GTK Widget class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace Gtk {

        using System;
        using System.Runtime.InteropServices;
	using Glib;
	using Gdk;

        public abstract class Widget : Object {

		private static readonly string DeleteEvent = "delete-event";
		public event EventHandler DeleteEvent
		{
			add
			{
				AddGdkSimpleEvent(DeleteEvent, value);
			}
                        remove 
			{
				RemoveGdkSimpleEvent (DeleteEvent, value);
			}
		}

		public void AddSimpleEvent(Object type, string name, EventHandler value)
		{
			if (Events[type] == null)
			{
				ConnectSimpleSignal(name, type);
			}
			Events.AddHandler(type, value);
		}

		public void AddSimpleEvent(String type, EventHandle value)
		: this (type, type, value) {}

		public void RemoveSimpleEvent(Object type, string name, EventHander value)
		{
			Events.RemoveHandler(type, value);
		}

		public void RemoveSimpleEvent(String type, EventHandle value)
		: this (type, type, value) {}

		public void AddGdkSimpleEvent(Object type, string name, EventHandler value)
		{
			if (Events[type] == null)
			{
				ConnectGdkSimpleEventSignal(name, type);
			}
			Events.AddHandler(type, value);
		}

		public void AddGdkSimpleEvent(String type, EventHandle value)
		: this (type, type, value) {}

		public void RemoveGdkSimpleEvent(Object type, string name, EventHander value)
		{
			Events.RemoveHandler(type, value);
		}

		public void RemoveGdkSimpleEvent(String type, EventHandle value)
		: this (type, type, value) {}


		[DllImport("gtk-1.3")]
		static extern void gtk_signal_connect_full (
                                        IntPtr obj, string evname,
                                        SimpleDelegate cb, IntPtr unsupported,
                                        IntPtr data, IntPtr destroycb,
                                        int objsig, int after );

		public void ConnectSimpleSignal(string name, Object signal)
		{
			gtk_signal_connect_full(RawObject, name, Glib.Signals.Simple.Delegate,
					new IntPtr (0), new IntPtr (signal.GetHashCode()),
					new IntPtr (0), 0, 0);
		}

		public void ConnectGdkSimpleSignal(string name, Object signal)
		{
			gtk_signal_connect_full(RawObject, name, Gdk.Signals.SimpleEvent.Delegate,
					new IntPtr (0), new IntPtr (signal.GetHashCode()),
					new IntPtr (0), 0, 0);
		}

                /// <summary>
                ///     Show Method
                /// </summary>
                ///
                /// <remarks>
                ///     Makes the Widget visible on the display.
                /// </remarks>

                [DllImport("gtk-1.3")]
                static extern void gtk_widget_show (IntPtr obj);

                public void Show ()
                {
                        gtk_widget_show (RawObject);
                }
        }
}

// GTK.Widget.cs - GTK Widget class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace Gtk {

        using System;
        using System.Runtime.InteropServices;
	using GLib;
	using Gdk;


        public class Widget : Object {
		public Widget() {}
		~Widget()
		{
			/* FIXME: Find a valid way to Delete the handlers
			foreach (EventHandler e in Events[DelEvName])
			{
				DeleteEvent -= e;
			}
			*/
		}

		private static readonly string DelEvName = "delete-event";

		public event EventHandler DeleteEvent {
			add {
				if (Events [DelEvName] == null)
					ConnectSignal(DelEvName);

				Events.AddHandler(DelEvName, value);
			}
                        remove {
				Events.RemoveHandler(DelEvName, value);
				if (Events [DelEvName] == null)
					DisconnectSignal (DelEvName);
			}
		}
/*
		public void AddSimpleEvent(Object type, string name, EventHandler value)
		{
			if (Events[type] == null)
			{
				ConnectSimpleSignal(name, type);
			}
			Events.AddHandler(type, value);
		}

		public void RemoveSimpleEvent(Object type, string name, EventHandler value)
		{
			Events.RemoveHandler(type, value);
			if (Events[type] == null)
			{
				DisconnectSimpleSignal(name, type);
			}
		}

		public void AddGdkSimpleEvent(Object type, string name, EventHandler value)
		{
			if (Events[type] == null)
			{
				ConnectGdkSimpleEventSignal(name, type);
			}
			Events.AddHandler(type, value);
		}

		public void RemoveGdkSimpleEvent(Object type, string name, EventHandler value)
		{
			Events.RemoveHandler(type, value);
			if (Events[type] == null)
			{
				DisconnectGdkSimpleEventSignal(name, type);
			}
		}
*/
		[DllImport("gtk-1.3.dll")]
		static extern void gtk_signal_connect_full (
                                        IntPtr obj, string evname,
                                        SimpleDelegate cb, IntPtr unsupported,
                                        String data, IntPtr destroycb,
                                        int objsig, int after );

		public void ConnectSignal(string name)
		{
			gtk_signal_connect_full(RawObject, name, SimpleSignal.Delegate,
					new IntPtr (0), name,
					new IntPtr (0), 0, 0);
		}

		public void DisconnectSignal(string name)
		{
			SimpleSignal.Unref();
		}

/*
		public void ConnectGdkSimpleSignal(string name, Object signal)
		{
			gtk_signal_connect_full(RawObject, name, SimpleEvent.Delegate,
					new IntPtr (0), name,
					new IntPtr (0), 0, 0);
		}

		public void DisconnectGdkSimpleSignal(string name, Object signal)
		{
			SimpleEvent.Unref();
		}
*/
                /// <summary>
                ///     Show Method
                /// </summary>
                ///
                /// <remarks>
                ///     Makes the Widget visible on the display.
                /// </remarks>

                [DllImport("gtk-1.3.dll")]
                static extern void gtk_widget_show (IntPtr obj);

                public void Show ()
                {
                        gtk_widget_show (RawObject);
                }
        }
}

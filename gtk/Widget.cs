// GTK.Widget.cs - GTK Widget class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace Gtk {

        using System;
        using System.Collections;
        using System.Runtime.InteropServices;
	using GLib;
	using Gdk;


        public class Widget : Object {

		private static readonly string DelEvName = "delete-event";

		private Hashtable Signals = new Hashtable ();

		public event EventHandler DeleteEvent {
			add {
				if (Events [DelEvName] == null)
					Signals [DelEvName] = new SimpleEvent (
							this, RawObject,
							DelEvName, value);

				Events.AddHandler(DelEvName, value);
			}
                        remove {
				Events.RemoveHandler(DelEvName, value);
				if (Events [DelEvName] == null)
					Signals.Remove (DelEvName);
			}
		}

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

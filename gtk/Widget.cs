// GTK.Widget.cs - GTK Widget class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public abstract class Widget : Object {

		/// <summary>
		///	Delete Event
		/// </summary>
		///
		/// <remarks>
		///	Occurs when the Widget is deleted by the window
		///	manager.
		/// </remarks>

		private static readonly object DeleteEvent = new object ();

		public event EventHandler Delete
		{
			add
			{
                                if (Events[DeleteEvent] == null)
				{
					ConnectSignal ("delete-event", new SimpleCallback (EmitDeleteEvent));
				}
				Events.AddHandler (DeleteEvent, value);
			}
                        remove 
			{
				Events.RemoveHandler (DeleteEvent, value);
			}
		}

		private void EmitDeleteEvent (IntPtr obj)
		{
			EventHandler eh = (EventHandler)(Events[DeleteEvent]);
			if (eh != null)
			{
				EventArgs args = new EventArgs ();
				eh(this, args);
			}
		}

		/// <summary>
		///	Show Method
		/// </summary>
		///
		/// <remarks>
		///	Makes the Widget visible on the display.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_widget_show (IntPtr obj);

		public void Show () 
		{
			gtk_widget_show (obj);

		}
	}
}

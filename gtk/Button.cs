// GTK.Button.cs - GTK Button class implementation
//
// Author: Bob Smith <bob@thestuff.net>
//
// (c) 2001 Bob Smith

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public class Button : Widget {

		private static readonly object ClickedEvent = new object ();
		public event EventHandler Clicked
		{
			add
			{
                                if (Events[ClickedEvent] == null)
				{
					ConnectSignal ("clicked", new SimpleCallback (EmitDeleteEvent));
				}
				Events.AddHandler (ClickedEvent, value);
			}
                        remove 
			{
				Events.RemoveHandler (ClickedEvent, value);
			}
		}

		private void EmitClickedEvent (IntPtr obj)
		{
			EventHandler eh = (EventHandler)(Events[ClickedEvent]);
			if (eh != null)
			{
				EventArgs args = new EventArgs ();
				eh(this, args);
			}
		}

		/// <summary>
		///	Button Object Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Button Wrapper.
		/// </remarks>

		public Button (IntPtr o)
		{
			Object = o;
		}

		/// <summary>
		///	Button Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a new Button with the specified content.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern IntPtr gtk_label_new_with_label (String str);

		public Button (String str)
		{
			Object = gtk_button_new_with_label (str);
		}
	}
}

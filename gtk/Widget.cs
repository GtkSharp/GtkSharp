// GTK.Widget.cs - GTK Widget class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GTK {

	using System;
	using System.Runtime.InteropServices;

	public abstract class Widget : Object {

		/// <summary>
		///	ConnectEvents method
		/// </summary>
		///
		/// <remarks>
		///	Connects event handlers to the wrapped GTK widget.  
		///	It is not possible to perform this connection in a 
		///	constructor, since the leaf class constructor in which
		///	the wrapped object is created is not executed until
		///	after the base class' constructor.
		/// </remarks>

		protected void PrepareEvents ()
		{
			ConnectSignal ("delete-event", 
				       new SimpleCallback (EmitDeleteEvent));
		}

		private void EmitDeleteEvent (IntPtr obj)
		{
			if (Delete != null) {
				EventArgs args = new EventArgs ();
				Delete (this, args);
			}
		}

		/// <summary>
		///	Delete Event
		/// </summary>
		///
		/// <remarks>
		///	Occurs when the Widget is deleted by the window
		///	manager.
		/// </remarks>

		public event EventHandler Delete;

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

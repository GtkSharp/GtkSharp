// GTK.Button.cs - GTK Button class implementation
//
// Author: Bob Smith <bob@thestuff.net>
//
// (c) 2001 Bob Smith

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public class Button : Widget {

		private static readonly string ClickedEvent = "clicked";
/*
		public event EventHandler Clicked
		{
			add
			{
				AddSimpleEvent(ClickedEvent, value);
			}
                        remove 
			{
				RemoveSimpleEvent (ClickedEvent, value);
			}
		}
*/
		/// <summary>
		///	Button Object Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Button Wrapper.
		/// </remarks>

		public Button (IntPtr o)
		{
			RawObject = o;
		}

		~Button ()
		{
			/* FIXME: Find legal way to do this eventually.
			foreach (EventHandler e in Events[ClickedEvent])
			{
				Clicked -= e;
			}
			*/
		}

		/// <summary>
		///	Button Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a new Button with the specified content.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern IntPtr gtk_button_new_with_label (String str);

		public Button (String str)
		{
			RawObject = gtk_button_new_with_label (str);
		}
	}
}

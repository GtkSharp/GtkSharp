// GTK.Window.cs - GTK Window class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GTK {

	using System;
	using System.Drawing;
	using System.Runtime.InteropServices;

	public enum WindowType {
		TopLevel,
		Popup,
	}

	public class Window : Widget {

		/// <summary>
		///	Window Object Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Window Wrapper.
		/// </remarks>

		public Window (IntPtr o)
		{
			Object = o;
		}

		/// <summary>
		///	Window Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a new Window of type TopLevel.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern IntPtr gtk_window_new (GTK.WindowType type);

		public Window ()
		{
			Object = gtk_window_new (WindowType.TopLevel);
		}

		/// <summary>
		///	Window Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a new Window of type TopLevel with the 
		///	specified Title.
		/// </remarks>

		public Window (String title) : this ()
		{
			this.Title = title;
		}

/*
		/// <summary>
		///	AllowGrow Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates if the Window can be resized to larger than
		///	the default size.
		/// </remarks>

		public bool AllowGrow {
			get {;}
			set {;}
		}
			
		/// <summary>
		///	AllowShrink Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates if the Window can be resized to smaller than
		///	the default size.
		/// </remarks>

		public bool AllowShrink {
			get {;}
			set {;}
		}
			
		/// <summary>
		///	DefaultSize Property
		/// </summary>
		/// 
		/// <remarks>
		///	The default Size of the Window in Screen Coordinates.
		/// </remarks>

		public Size DefaultSize {
			get {;}
			set {;}
		}

		/// <summary>
		///	DestroyWithParent Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates if the Window should be destroyed when any
		///	associated parent Windows are destroyed.
		/// </remarks>

		public bool DestroyWithParent {
			get {;}
			set {;}
		}

		/// <summary>
		///	IsModal Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates if the Window is Modal. If true, the input
		///	focus is grabbed by the Window and other Windows in
		///	the application will not accept input until the Window
		///	is closed.
		/// </remarks>

		public bool IsModal {
			get {;}
			set {;}
		}
*/
		/// <summary>
		///	Position Property
		/// </summary>
		/// 
		/// <remarks>
		///	The Position of the Window in Screen Coordinates.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_window_set_position (IntPtr hnd,
							    int x, int y);

		public Point Position {
			set
			{
				gtk_window_set_position (
						obj, value.X, value.Y);
			}
		}

		/// <summary>
		///	Title Property
		/// </summary>
		/// 
		/// <remarks>
		///	The Title displayed in the Window's Title Bar.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_window_set_title (IntPtr hnd,
							 String title);

		public String Title {
			set
			{
				gtk_window_set_title (Object, value);
			}
		}
	}
}

// Gtk.Window.cs - GTK Window class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace Gtk {

	using GLib;
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
			RawObject = o;
		}

		/// <summary>
		///	Window Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a new Window of type TopLevel.
		/// </remarks>

		[DllImport("gtk-1.3.dll")]
		static extern IntPtr gtk_window_new (WindowType type);

		public Window ()
		{
			RawObject = gtk_window_new (WindowType.TopLevel);
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

		/// <summary>
		///	AllowGrow Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates if the Window can be resized to larger than
		///	the default size.
		/// </remarks>

		public bool AllowGrow {
			get {
				bool val;
				GetProperty ("allow-grow", out val);
				return (val);
			}
			set {
				SetProperty ("allow-grow", value);
			}
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
			get {
				bool val;
				GetProperty ("allow-shrink", out val);
				return (val);
			}
			set {
				SetProperty ("allow-shrink", value);
			}
		}
			
		/// <summary>
		///	DefaultHeight Property
		/// </summary>
		/// 
		/// <remarks>
		///	The default Height of the Window in Pixels.
		/// </remarks>

		public int DefaultHeight {
			get {
				int val;
				GetProperty ("default-height", out val);
				return (val);
			}
			set {
				SetProperty ("default-height", value);
			}
		}

		/// <summary>
		///	DefaultSize Property
		/// </summary>
		/// 
		/// <remarks>
		///	The default Size of the Window in Screen Coordinates.
		/// </remarks>

		public Size DefaultSize {
			get {
				return new Size (DefaultWidth, DefaultHeight);
			}
			set {
				DefaultWidth = value.Width;
				DefaultHeight = value.Height;
			}
		}

		/// <summary>
		///	DefaultWidth Property
		/// </summary>
		/// 
		/// <remarks>
		///	The default Width of the Window in Pixels.
		/// </remarks>

		public int DefaultWidth {
			get {
				int val;
				GetProperty ("default-width", out val);
				return (val);
			}
			set {
				SetProperty ("default-width", value);
			}
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
			get {
				bool val;
				GetProperty ("destroy-with-parent", out val);
				return (val);
			}
			set {
				SetProperty ("destroy-with-parent", value);
			}
		}

		/// <summary>
		///	Modal Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates if the Window is Modal. If true, the input
		///	focus is grabbed by the Window and other Windows in
		///	the application will not accept input until the Window
		///	is closed.
		/// </remarks>

		public bool Modal {
			get {
				bool val;
				GetProperty ("modal", out val);
				return (val);
			}
			set {
				SetProperty ("modal", value);
			}
		}

		/// <summary>
		///	Position Property
		/// </summary>
		/// 
		/// <remarks>
		///	The Position of the Window in Screen Coordinates.
		/// </remarks>

		[DllImport("gtk-1.3.dll")]
		static extern void gtk_window_set_position (IntPtr hnd,
							    int x, int y);

		public Point Position {
			set
			{
				gtk_window_set_position (
						RawObject, value.X, value.Y);
			}
		}

		/// <summary>
		///	Resizable Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates if the Height and Width of the Window can be 
		///	altered by the user.
		/// </remarks>

		public bool Resizable {
			get {
				bool val;
				GetProperty ("resizable", out val);
				return (val);
			}
			set {
				SetProperty ("resizable", value);
			}
		}

		/// <summary>
		///	Title Property
		/// </summary>
		/// 
		/// <remarks>
		///	The Title displayed in the Window's Title Bar.
		/// </remarks>

		public String Title {
			get {
				String val;
				GetProperty ("title", out val);
				return val;
			}
			set {
				SetProperty ("title", value); 
			}
		}
	}
}

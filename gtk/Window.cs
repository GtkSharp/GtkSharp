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
				Value val = new Value (
						TypeFundamentals.TypeBoolean);
				GetProperty ("allow-grow", val);
				return ((bool) val);
			}
			set {
				Value val = new Value (value);
				SetProperty ("allow-grow", val);
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
				Value val = new Value (
						TypeFundamentals.TypeBoolean);
				GetProperty ("allow-shrink", val);
				return ((bool) val);
			}
			set {
				Value val = new Value (value);
				SetProperty ("allow-shrink", val);
			}
		}
			
/*
		/// <summary>
		///	DefaultSize Property
		/// </summary>
		/// 
		/// <remarks>
		///	The default Size of the Window in Screen Coordinates.
		/// </remarks>

		public Size DefaultSize {
			get {
				GValue val = GetProp ("default-size");
				return (val != 0);
			}
			set {
				SetProp ("default-size", new GValue (value));
			}
		}
*/

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
				Value val = new Value (
						TypeFundamentals.TypeBoolean);
				GetProperty ("destroy-with-parent", val);
				return ((bool) val);
			}
			set {
				Value val = new Value (value);
				SetProperty ("destroy-with-parent", val);
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
				Value val = new Value (
						TypeFundamentals.TypeBoolean);
				GetProperty ("modal", val);
				return ((bool) val);
			}
			set {
				Value val = new Value (value);
				SetProperty ("modal", val);
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
				Value val = new Value (
						TypeFundamentals.TypeBoolean);
				GetProperty ("resizable", val);
				return ((bool) val);
			}
			set {
				Value val = new Value (value);
				SetProperty ("resizable", val);
			}
		}

		/// <summary>
		///	Title Property
		/// </summary>
		/// 
		/// <remarks>
		///	The Title displayed in the Window's Title Bar.
		/// </remarks>

		[DllImport("gobject-1.3.dll")]
		static extern void g_object_set (IntPtr obj, String name,
						 IntPtr val, IntPtr term);

		public String Title {
			set {
				g_object_set (RawObject, "title", 
					      Marshal.StringToHGlobalAnsi (value), new IntPtr (0));
/* FIXME: When the String value setting problem is solved.
				Value val = new Value (value);
				SetProperty ("title", val);
*/
			}
		}
	}
}

// Gtk.Container.cs - GtkContainer class wrapper implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	/// <summary>
	///	Container Class
	/// </summary>
	///
	/// <remarks>
	///	Abstract class which provides the capability to embed a
	///	widget within its boundaries.
	/// </remarks>

	public abstract class Container : Widget {

		/// <summary>
		///	BorderWidth Property
		/// </summary>
		///
		/// <remarks>
		///	The Width, in pixels, of the border around the 
		///	Container. 
		/// </remarks>

		public int BorderWidth {
			get {
				int val;
				GetProperty ("border-width", out val);
				return val;
			}
			set {
				SetProperty ("border-width", value);
			}
		}

		// FIXME: Implement Child property.

		/// <summary>
		///	ResizeMode Property
		/// </summary>
		///
		/// <remarks>
		///	Indicates the resizing policy for the Container.
		/// </remarks>

		public ResizeMode ResizeMode {
			get {
				int val;
				GetProperty ("border-width", out val);
				return (ResizeMode) val;
			}
			set {
				SetProperty ("border-width", (int) value);
			}
		}

		/// <summary>
		///	Add Method
		/// </summary>
		///
		/// <remarks>
		///	Adds a child Widget to the Container.
		/// </remarks>

		[DllImport("gtk-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern void gtk_container_add (IntPtr obj, IntPtr child);

		public void Add (Widget child)
		{
			gtk_container_add (Handle, child.Handle);
		}

		/// <summary>
		///	Remove Method
		/// </summary>
		///
		/// <remarks>
		///	Remove a child Widget from the Container.
		/// </remarks>

		[DllImport("gtk-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern void gtk_container_remove (IntPtr obj, 
							 IntPtr child);

		public void Remove (Widget child)
		{
			gtk_container_remove (Handle, child.Handle);
		}

	}
}

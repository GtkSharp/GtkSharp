//
// Gtk.Adjustment.cs - Allow customization of values in the GtkAdjustment
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.
//

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Adjustment {

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_adjustment_new(double value, double lower, double upper, double step_increment, double page_increment, double page_size);

		public Adjustment (double value, double lower, double upper, double step_increment, double page_increment, double page_size) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (Adjustment)) {
				CreateNativeObject (new string [0], new GLib.Value [0]);
				Value = value;
				Lower = lower;
				Upper = upper;
				StepIncrement = step_increment;
				PageIncrement = page_increment;
				PageSize = page_size;
				return;
			}

			Raw = gtk_adjustment_new(value, lower, upper, step_increment, page_increment, page_size);
		}

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_object_freeze_notify (IntPtr inst);

		[DllImport ("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_object_thaw_notify (IntPtr inst);

		public void SetBounds (double lower, double upper, double step_increment, double page_increment, double page_size)
		{
			// g_object_freeze_notify/g_object_thaw_notify calls are necessary to to avoid multiple emissions of the "changed" signal
			g_object_freeze_notify (this.Handle);

			Lower = lower;
			Upper = upper;
			StepIncrement = step_increment;
			PageIncrement = page_increment;
			PageSize = page_size;

			g_object_thaw_notify (this.Handle);
		}
	}
}

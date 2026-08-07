// Gtk.AlertDialog.cs - the constructor codegen cannot emit
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

namespace Gtk {

	using System;

	public partial class AlertDialog {

		// GtkAlertDialog's only C constructor is
		//
		//     GtkAlertDialog *gtk_alert_dialog_new (const char *format, ...);
		//
		// and codegen emits nothing for an ellipsis, so the generated class had
		// no public constructor at all: the type that replaced GtkMessageDialog
		// in Gtk 4 could not be created from C#.
		//
		// g_object_new is used rather than the varargs entry point on purpose.
		// gtk_alert_dialog_new runs its first argument through g_strdup_vprintf,
		// so a message carrying a percent sign -- "Copied 50% of the files" --
		// would be interpreted as a format string. Setting the property says
		// what the caller meant.
		public AlertDialog () : base (IntPtr.Zero)
		{
			CreateNativeObject (new string [0], new GLib.Value [0]);
		}

		public AlertDialog (string message) : this ()
		{
			if (message != null)
				Message = message;
		}

		public AlertDialog (string message, params string[] buttons) : this (message)
		{
			if (buttons != null && buttons.Length > 0)
				Buttons = buttons;
		}
	}
}

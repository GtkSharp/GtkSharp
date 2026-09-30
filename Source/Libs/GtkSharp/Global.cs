//  Authors:  Aaron Bockover <abockover@novell.com>
// 
//  Copyright 2007-2010 Novell, Inc.
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
	using System.Runtime.InteropServices;

	public partial class Global {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate int d_gtk_distribute_natural_allocation (int extra_space, uint n_requested_sizes, IntPtr sizes);
		static d_gtk_distribute_natural_allocation gtk_distribute_natural_allocation = FuncLoader.LoadFunction<d_gtk_distribute_natural_allocation> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gtk), "gtk_distribute_natural_allocation"));

		// gtk_distribute_natural_allocation reads n_requested_sizes structs and
		// writes the allocation it decided on back into each one's MinimumSize.
		// Codegen took the array as a single GtkRequestedSize by value, so with
		// more than one size Gtk wrote past a twenty-four byte block, and with
		// exactly one the answer was freed unread. The count comes from the array
		// here, and the results are copied back into it: this is an in/out
		// parameter, which is the whole reason to call the function.
		public static int DistributeNaturalAllocation (int extra_space, Gtk.RequestedSize[] sizes)
		{
			if (sizes == null)
				throw new ArgumentNullException ("sizes");
			if (sizes.Length == 0)
				return extra_space;

			int size = Marshal.SizeOf (typeof (Gtk.RequestedSize));
			IntPtr native = Marshal.AllocHGlobal (size * sizes.Length);
			try {
				for (int i = 0; i < sizes.Length; i++)
					Marshal.StructureToPtr (sizes [i], (IntPtr) ((long) native + i * size), false);

				int ret = gtk_distribute_natural_allocation (extra_space, (uint) sizes.Length, native);

				for (int i = 0; i < sizes.Length; i++)
					sizes [i] = (Gtk.RequestedSize) Marshal.PtrToStructure ((IntPtr) ((long) native + i * size), typeof (Gtk.RequestedSize));

				return ret;
			} finally {
				Marshal.FreeHGlobal (native);
			}
		}

		// Gtk 4's gtk_show_uri takes a parent window and a timestamp, and returns
		// void: it launches asynchronously and reports nothing back. Gtk 4.10 adds
		// GtkUriLauncher, which does report completion.
		public static void ShowUri (string uri)
		{
			ShowUri (null, uri, 0);
		}

		// Screen-based helpers are gone with GdkScreen.

		public static bool IsSupported => GLibrary.IsSupported(Library.Gtk);

		/// <summary>
		/// The position a list model reports when there is no position: no
		/// selection, an unbound row, a search that found nothing.
		/// </summary>
		/// <remarks>
		/// <c>GTK_INVALID_LIST_POSITION</c>. It is what
		/// <see cref="SingleSelection.Selected"/> holds when nothing is selected
		/// and what <see cref="StringList.Find"/> answers when the string is not
		/// there, so a caller who cannot name it has to compare against
		/// <c>uint.MaxValue</c> and hope that is what it means.
		///
		/// Here by hand for the same reason as
		/// <see cref="StyleProviderPriority"/>: it is a <c>&lt;constant&gt;</c> in
		/// Gtk-4.0.gir, and GirToGapi emits none of those. See Docs/testing.md.
		/// </remarks>
		public const uint InvalidListPosition = uint.MaxValue;
	}
}


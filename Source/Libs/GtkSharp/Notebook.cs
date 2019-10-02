// Notebook.cs - customization for Gtk.Notebook
//
// Authors: Xavier Amado (xavier@blackbloodstudios.com)
//          Mike Kestner (mkestner@ximian.com)
//
// Copyright (c) 2004 Novel, Inc.
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

	public partial class Notebook {

		public Widget CurrentPageWidget {
			get {
				return GetNthPage (CurrentPage);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_gtk_notebook_page_num(IntPtr handle, IntPtr child);
		static d_gtk_notebook_page_num gtk_notebook_page_num = FuncLoader.LoadFunction<d_gtk_notebook_page_num>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_notebook_page_num"));

		public int PageNum (Widget child)
		{
			return gtk_notebook_page_num (Handle, child.Handle);
		}
	}
}


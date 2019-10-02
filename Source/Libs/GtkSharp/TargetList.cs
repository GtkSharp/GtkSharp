// TargetList.cs - customizations for Gtk.TargetList
//
// Copyright (c) 2004  Novell, Inc.
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

	public partial class TargetList {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_target_list_new(Gtk.TargetEntry[] targets, uint n_targets);
		static d_gtk_target_list_new gtk_target_list_new = FuncLoader.LoadFunction<d_gtk_target_list_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_target_list_new"));

		public TargetList() : base(gtk_target_list_new(null, 0)) {}

		public TargetList (Gtk.TargetEntry[] targets) : this(gtk_target_list_new(targets, (uint) targets.Length)) {}

		public void Add(string target, uint flags, uint info) {
			Add(Gdk.Atom.Intern (target, false), flags, info);
		}

		public bool Find(string target, out uint info) {
			return Find(Gdk.Atom.Intern (target, false), out info);
		}

		public void Remove(string target) {
			Remove(Gdk.Atom.Intern (target, false));
		}

		public static explicit operator TargetEntry[] (TargetList list)
		{
			return Target.TableNewFromList (list);
		}
	}
}


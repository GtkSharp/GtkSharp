// Gtk.UiManager.cs - Gtk UiManager class customizations
//
// Author: John Luke  <john.luke@gmail.com>
//
// Copyright (C) 2004 Novell, Inc.
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

	public partial class UIManager {

		public uint AddUiFromResource (string resource)
		{
			if (resource == null)
				throw new ArgumentNullException ("resource");
			
			System.IO.Stream s = System.Reflection.Assembly.GetCallingAssembly ().GetManifestResourceStream (resource);
			if (s == null)
				throw new ArgumentException ("resource must be a valid resource name of 'assembly'.");

			return AddUiFromString (new System.IO.StreamReader (s).ReadToEnd ());
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_gtk_ui_manager_new_merge_id(IntPtr raw);
		static d_gtk_ui_manager_new_merge_id gtk_ui_manager_new_merge_id = FuncLoader.LoadFunction<d_gtk_ui_manager_new_merge_id>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_ui_manager_new_merge_id"));

		public uint NewMergeId ()
		{
			return gtk_ui_manager_new_merge_id (Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_ui_manager_get_toplevels(IntPtr raw, int types);
		static d_gtk_ui_manager_get_toplevels gtk_ui_manager_get_toplevels = FuncLoader.LoadFunction<d_gtk_ui_manager_get_toplevels>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_ui_manager_get_toplevels"));

		public Widget[] GetToplevels (Gtk.UIManagerItemType types) {
			IntPtr raw_ret = gtk_ui_manager_get_toplevels (Handle, (int) types);
			GLib.SList list = new GLib.SList (raw_ret);
 			Widget[] result = new Widget [list.Count];
			for (int i = 0; i < list.Count; i++)
				result [i] = list [i] as Widget;

			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_ui_manager_get_action_groups(IntPtr raw);
		static d_gtk_ui_manager_get_action_groups gtk_ui_manager_get_action_groups = FuncLoader.LoadFunction<d_gtk_ui_manager_get_action_groups>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_ui_manager_get_action_groups"));

		public ActionGroup[] ActionGroups { 
			get {
				IntPtr raw_ret = gtk_ui_manager_get_action_groups (Handle);
				GLib.List list = new GLib.List(raw_ret);
 				ActionGroup[] result = new ActionGroup [list.Count];
				for (int i = 0; i < list.Count; i++)
					result [i] = list [i] as ActionGroup;

				return result;
			}
		}
	}
}


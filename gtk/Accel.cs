// Accel.cs - customizations to Gtk.Accel
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004-2005 Novell, Inc.
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

	public partial class Accel {

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_accel_map_save(IntPtr file_name);

		[Obsolete("Moved to AccelMap class. Use AccelMap.Save instead")]
		public static void MapSave(string file_name) {
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (file_name);
			gtk_accel_map_save (native);
			GLib.Marshaller.Free (native);
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_accel_map_add_filter(IntPtr filter_pattern);

		[Obsolete("Moved to AccelMap class. Use AccelMap.AddFilter instead")]
		public static void MapAddFilter(string filter_pattern) {
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (filter_pattern);
			gtk_accel_map_add_filter (native);
			GLib.Marshaller.Free (native);
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_accel_map_foreach_unfiltered(IntPtr data, GtkSharp.AccelMapForeachNative foreach_func);

		[Obsolete("Moved to AccelMap class. Use AccelMap.ForeachUnfiltered instead")]
		public static void MapForeachUnfiltered(IntPtr data, Gtk.AccelMapForeach foreach_func) {
			GtkSharp.AccelMapForeachWrapper foreach_func_wrapper = new GtkSharp.AccelMapForeachWrapper (foreach_func);
			gtk_accel_map_foreach_unfiltered(data, foreach_func_wrapper.NativeDelegate);
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_accel_map_save_fd(int fd);

		[Obsolete("Moved to AccelMap class. Use AccelMap.SaveFd instead")]
		public static void MapSaveFd(int fd) {
			gtk_accel_map_save_fd(fd);
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_accel_map_add_entry(IntPtr accel_path, uint accel_key, int accel_mods);

		[Obsolete("Moved to AccelMap class. Use AccelMap.AddEntry instead")]
		public static void MapAddEntry(string accel_path, uint accel_key, Gdk.ModifierType accel_mods) {
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (accel_path);
			gtk_accel_map_add_entry(native, accel_key, (int) accel_mods);
			GLib.Marshaller.Free (native);
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_accel_map_load_fd(int fd);

		[Obsolete("Moved to AccelMap class. Use AccelMap.LoadFd instead")]
		public static void MapLoadFd(int fd) {
			gtk_accel_map_load_fd(fd);
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gtk_accel_map_lookup_entry(IntPtr accel_path, ref Gtk.AccelKey key);

		[Obsolete("Moved to AccelMap class. Use AccelMap.LookupEntry instead")]
		public static bool MapLookupEntry(string accel_path, Gtk.AccelKey key) {
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (accel_path);
			bool ret = gtk_accel_map_lookup_entry(native, ref key);
			GLib.Marshaller.Free (native);
			return ret;
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gtk_accel_map_change_entry(IntPtr accel_path, uint accel_key, int accel_mods, bool replace);

		[Obsolete("Moved to AccelMap class. Use AccelMap.ChangeEntry instead")]
		public static bool MapChangeEntry (string accel_path, uint accel_key, Gdk.ModifierType accel_mods, bool replace) {
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (accel_path);
			bool ret = gtk_accel_map_change_entry (native, accel_key, (int) accel_mods, replace);
			GLib.Marshaller.Free (native);
			return ret;
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_accel_map_load (IntPtr file_name);

		[Obsolete("Moved to AccelMap class. Use AccelMap.Load instead")]
		public static void MapLoad (string file_name) {
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (file_name);
			gtk_accel_map_load (native);
			GLib.Marshaller.Free (native);
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_accel_map_foreach(IntPtr data, GtkSharp.AccelMapForeachNative foreach_func);

		[Obsolete("Moved to AccelMap class. Use AccelMap.Foreach instead")]
		public static void MapForeach(IntPtr data, Gtk.AccelMapForeach foreach_func) {
			GtkSharp.AccelMapForeachWrapper foreach_func_wrapper = new GtkSharp.AccelMapForeachWrapper (foreach_func);
			gtk_accel_map_foreach(data, foreach_func_wrapper.NativeDelegate);
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_accel_groups_from_object (IntPtr obj);

		public static AccelGroup[] GroupsFromObject (GLib.Object obj)
		{
			IntPtr raw_ret = gtk_accel_groups_from_object(obj.Handle);
			if (raw_ret == IntPtr.Zero)
				return new AccelGroup [0];
			GLib.SList list = new GLib.SList(raw_ret);
			AccelGroup[] result = new AccelGroup [list.Count];
			for (int i = 0; i < list.Count; i++)
				result [i] = list [i] as AccelGroup;
			return result;
		}
	}
}

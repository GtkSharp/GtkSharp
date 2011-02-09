// StockManager.cs - Gtk.Stock item manager
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2005 Novell, Inc.
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

	public class StockManager {

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_stock_add_static(ref Gtk.StockItem items, uint n_items);

		[Obsolete ("Use StockManager.Add instead")]
		public static void AddStatic(Gtk.StockItem items, uint n_items) 
		{
			gtk_stock_add_static(ref items, n_items);
		}

		[StructLayout(LayoutKind.Sequential)]
		struct ConstStockItem {
			public IntPtr StockId;
			public IntPtr Label;
			public Gdk.ModifierType Modifier;
			public uint Keyval;
			public IntPtr TranslationDomain;

			public static explicit operator StockItem (ConstStockItem csi)
			{
				Gtk.StockItem item = new Gtk.StockItem ();
				item.StockId = GLib.Marshaller.Utf8PtrToString (csi.StockId);
				item.Label = GLib.Marshaller.Utf8PtrToString (csi.Label);
				item.Modifier = csi.Modifier;
				item.Keyval = csi.Keyval;
				item.TranslationDomain = GLib.Marshaller.Utf8PtrToString (csi.TranslationDomain);
				return item;
			}
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gtk_stock_lookup (IntPtr stock_id, out ConstStockItem item);

		public static bool Lookup (string stock_id, ref Gtk.StockItem item) 
		{
			ConstStockItem const_item;
			IntPtr native_id = GLib.Marshaller.StringToPtrGStrdup (stock_id);
			bool found = gtk_stock_lookup (native_id, out const_item);
			GLib.Marshaller.Free (native_id);
			if (!found)
				return false;
			item = (StockItem) const_item;
			return true;
		}

		public static bool LookupItem (string stock_id, out Gtk.StockItem item) 
		{
			item = StockItem.Zero;
			return Lookup (stock_id, ref item);
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_stock_add(ref Gtk.StockItem item, uint n_items);

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_stock_add(Gtk.StockItem[] items, uint n_items);

		[Obsolete ("Use the StockItem or StockItem[] overload instead.")]
		public static void Add (Gtk.StockItem items, uint n_items) 
		{
			gtk_stock_add(ref items, n_items);
		}

		public static void Add (Gtk.StockItem item)
		{
			gtk_stock_add (ref item, 1);
		}

		public static void Add (Gtk.StockItem[] items)
		{
			gtk_stock_add (items, (uint) items.Length);
		}

	}
}

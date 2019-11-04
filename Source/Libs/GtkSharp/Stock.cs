// Stock.cs - customizations to Gtk.Stock
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.
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

	public partial class Stock {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_stock_list_ids();
		static d_gtk_stock_list_ids gtk_stock_list_ids = FuncLoader.LoadFunction<d_gtk_stock_list_ids>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_stock_list_ids"));

		public static string[] ListIds ()
		{
			IntPtr raw_ret = gtk_stock_list_ids ();
			if (raw_ret == IntPtr.Zero)
				return new string [0];
			GLib.SList list = new GLib.SList(raw_ret, typeof (string));
			string[] result = new string [list.Count];
			for (int i = 0; i < list.Count; i++)
				result [i] = (string) list [i];
			return result;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct ConstStockItem {
			public IntPtr StockId;
			public IntPtr Label;
			public Gdk.ModifierType Modifier;
			public uint Keyval;
			public IntPtr TranslationDomain;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_stock_lookup(IntPtr stock_id, out ConstStockItem item);
		static d_gtk_stock_lookup gtk_stock_lookup = FuncLoader.LoadFunction<d_gtk_stock_lookup>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_stock_lookup"));

		public static Gtk.StockItem Lookup (string stock_id) {
			ConstStockItem const_item;

			IntPtr native_id = GLib.Marshaller.StringToPtrGStrdup (stock_id);
			bool result = gtk_stock_lookup (native_id, out const_item);
			GLib.Marshaller.Free (native_id);
			if (!result)
				return Gtk.StockItem.Zero;

			Gtk.StockItem item = new Gtk.StockItem ();
			item.StockId = GLib.Marshaller.Utf8PtrToString (const_item.StockId);
			item.Label = GLib.Marshaller.Utf8PtrToString (const_item.Label);
			item.Modifier = const_item.Modifier;
			item.Keyval = const_item.Keyval;
			item.TranslationDomain = GLib.Marshaller.Utf8PtrToString (const_item.TranslationDomain);
			return item;
		}
	}
}


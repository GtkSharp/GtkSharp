// IconTheme.cs - Manual implementation of GnomeIconTheme class in GTK+-2.4.
//
// Authors: Jeroen Zwartepoorte  <jeroen@xs4all.nl>
//
// Copyright (c) 2004 Novell, Inc.

namespace Gnome {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	public class IconTheme : Gtk.IconTheme {

		~IconTheme()
		{
			Dispose();
		}

		protected IconTheme(GLib.GType gtype) : base(gtype) {}
		public IconTheme(IntPtr raw) : base(raw) {}

		[DllImport("gnomeui-2")]
		static extern IntPtr gnome_icon_theme_new();

		public IconTheme() : base (IntPtr.Zero)
		{
			Raw = gnome_icon_theme_new();
		}

		[DllImport("gnomeui-2")]
		static extern bool gnome_icon_theme_get_allow_svg(IntPtr raw);

		[DllImport("gnomeui-2")]
		static extern void gnome_icon_theme_set_allow_svg(IntPtr raw, bool allow_svg);

		public bool AllowSvg { 
			get {
				bool raw_ret = gnome_icon_theme_get_allow_svg(Handle);
				bool ret = raw_ret;
				return ret;
			}
			set {
				gnome_icon_theme_set_allow_svg(Handle, value);
			}
		}

		[DllImport("gnomeui-2")]
		static extern IntPtr gnome_icon_theme_lookup_icon(IntPtr raw, IntPtr icon_name, int size, ref Gnome.IconData icon_data, out int base_size);

		public string LookupIcon(string icon_name, int size, Gnome.IconData icon_data, out int base_size) {
			IntPtr native_icon_name = GLib.Marshaller.StringToPtrGStrdup (icon_name);
			IntPtr raw_ret = gnome_icon_theme_lookup_icon(Handle, native_icon_name, size, ref icon_data, out base_size);
			GLib.Marshaller.Free (native_icon_name);
			string ret = GLib.Marshaller.PtrToStringGFree(raw_ret);
			return ret;
		}

		[DllImport("gnomeui-2")]
		static extern IntPtr gnome_icon_theme_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = gnome_icon_theme_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}
	}
}

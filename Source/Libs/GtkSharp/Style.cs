//
// Gtk.Style.cs - Gtk Style class customizations
//
// Authors: Rachel Hestilow <hestilow@ximian.com> 
//          Radek Doulik <rodo@matfyz.cz> 
//          Mike Kestner <mkestner@novell.com>
//
// Copyright (C) 2002, 2003 Rachel Hestilow, Radek Doulik
// Copyright (C) 2005 Novell, Inc.
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

	[Obsolete ("Replaced by StyleContext")]
	public partial class Style {

		[DllImport("gtksharpglue-3")]
		static extern IntPtr gtksharp_gtk_style_get_bg (IntPtr style, int i);

		public Gdk.Color Background (StateType state)
		{
			IntPtr raw = gtksharp_gtk_style_get_bg (Handle, (int) state);
			return Gdk.Color.New (raw);
		}

		public Gdk.Color[] Backgrounds {
			get {
				Gdk.Color[] ret = new Gdk.Color[5];
				for (int i = 0; i < 5; i++)
					ret[i] = Gdk.Color.New (gtksharp_gtk_style_get_bg (Handle, i));
				return ret;
			}
		}

		[DllImport("gtksharpglue-3")]
		static extern IntPtr gtksharp_gtk_style_get_fg (IntPtr style, int i);

		public Gdk.Color Foreground (StateType state)
		{
			IntPtr raw = gtksharp_gtk_style_get_fg (Handle, (int) state);
			return Gdk.Color.New (raw);
		}

		public Gdk.Color[] Foregrounds {
			get {
				Gdk.Color[] ret = new Gdk.Color[5];
				for (int i = 0; i < 5; i++)
					ret[i] = Gdk.Color.New (gtksharp_gtk_style_get_fg (Handle, i));
				return ret;
			}
		}

		[DllImport("gtksharpglue-3")]
		static extern IntPtr gtksharp_gtk_style_get_text (IntPtr style, int i);

		public Gdk.Color Text (StateType state)
		{
			IntPtr raw = gtksharp_gtk_style_get_text (Handle, (int) state);
			return Gdk.Color.New (raw);
		}

		public Gdk.Color[] TextColors {
			get {
				Gdk.Color[] ret = new Gdk.Color[5];
				for (int i = 0; i < 5; i++)
					ret[i] = Gdk.Color.New (gtksharp_gtk_style_get_text (Handle, i));
				return ret;
			}
		}

		[DllImport("gtksharpglue-3")]
		static extern IntPtr gtksharp_gtk_style_get_base (IntPtr style, int i);

		public Gdk.Color Base (StateType state)
		{
			IntPtr raw = gtksharp_gtk_style_get_base (Handle, (int) state);
			return Gdk.Color.New (raw);
		}

		public Gdk.Color[] BaseColors {
			get {
				Gdk.Color[] ret = new Gdk.Color[5];
				for (int i = 0; i < 5; i++)
					ret[i] = Gdk.Color.New (gtksharp_gtk_style_get_base (Handle, i));
				return ret;
			}
		}

		[DllImport("gtksharpglue-3")]
		static extern IntPtr gtksharp_gtk_style_get_light (IntPtr style, int i);

		public Gdk.Color Light (StateType state)
		{
			IntPtr raw = gtksharp_gtk_style_get_light (Handle, (int) state);
			return Gdk.Color.New (raw);
		}

		public Gdk.Color[] LightColors {
			get {
				Gdk.Color[] ret = new Gdk.Color[5];
				for (int i = 0; i < 5; i++)
					ret[i] = Gdk.Color.New (gtksharp_gtk_style_get_light (Handle, i));
				return ret;
			}
		}

		[DllImport("gtksharpglue-3")]
		static extern IntPtr gtksharp_gtk_style_get_mid (IntPtr style, int i);

		public Gdk.Color Mid (StateType state)
		{
			IntPtr raw = gtksharp_gtk_style_get_mid (Handle, (int) state);
			return Gdk.Color.New (raw);
		}

		public Gdk.Color[] MidColors {
			get {
				Gdk.Color[] ret = new Gdk.Color[5];
				for (int i = 0; i < 5; i++)
					ret[i] = Gdk.Color.New (gtksharp_gtk_style_get_mid (Handle, i));
				return ret;
			}
		}

		[DllImport("gtksharpglue-3")]
		static extern IntPtr gtksharp_gtk_style_get_dark (IntPtr style, int i);

		public Gdk.Color Dark (StateType state)
		{
			IntPtr raw = gtksharp_gtk_style_get_dark (Handle, (int) state);
			return Gdk.Color.New (raw);
		}

		public Gdk.Color[] DarkColors {
			get {
				Gdk.Color[] ret = new Gdk.Color[5];
				for (int i = 0; i < 5; i++)
					ret[i] = Gdk.Color.New (gtksharp_gtk_style_get_dark (Handle, i));
				return ret;
			}
		}

		[DllImport ("gtksharpglue-3")]
		static extern int gtksharp_gtk_style_get_thickness (IntPtr style, int x_axis);
		[DllImport ("gtksharpglue-3")]
		static extern void gtksharp_gtk_style_set_thickness (IntPtr style, int value);

		public int XThickness {
			get {
				return gtksharp_gtk_style_get_thickness (Handle, 0);
			}

			set {
				gtksharp_gtk_style_set_thickness (Handle, value);
			}
		}

		public int YThickness {
			get {
				return gtksharp_gtk_style_get_thickness (Handle, 1);
			}

			set {
				gtksharp_gtk_style_set_thickness (Handle, -value);
			}
		}

		[DllImport ("gtksharpglue-3")]
		static extern IntPtr gtksharp_gtk_style_get_font_description (IntPtr style);

		public Pango.FontDescription FontDescription {
			get {
				IntPtr Raw = gtksharp_gtk_style_get_font_description (Handle);

				if (Raw == IntPtr.Zero)
					return null;
				Pango.FontDescription ret = (Pango.FontDescription) GLib.Opaque.GetOpaque (Raw, typeof (Pango.FontDescription), false);
				if (ret == null)
					ret = new Pango.FontDescription (Raw);
				return ret;
			}
		}
	}
}

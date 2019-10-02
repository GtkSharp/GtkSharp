// Gtk.ColorSelection.cs - customizations and corrections for ColorSelection
// Author: Lee Mallabone <gnome@fonicmonkey.net>
// Author: Justin Malcolm
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

	public partial class ColorSelection {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_color_selection_palette_to_string(Gdk.Color[] colors, int n_colors);
		static d_gtk_color_selection_palette_to_string gtk_color_selection_palette_to_string = FuncLoader.LoadFunction<d_gtk_color_selection_palette_to_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_color_selection_palette_to_string"));

		/// <summary> PaletteToString Method </summary>
		public static string PaletteToString(Gdk.Color[] colors) {
			int n_colors = colors.Length;
			IntPtr raw_ret = gtk_color_selection_palette_to_string(colors, n_colors);
			string ret = GLib.Marshaller.PtrToStringGFree (raw_ret);
			return ret;
		}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_color_selection_palette_from_string(IntPtr str, out IntPtr colors, out int n_colors);
		static d_gtk_color_selection_palette_from_string gtk_color_selection_palette_from_string = FuncLoader.LoadFunction<d_gtk_color_selection_palette_from_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_color_selection_palette_from_string"));

		public static Gdk.Color[] PaletteFromString(string str) {
			IntPtr parsedColors;
			int n_colors;
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (str);
			bool raw_ret = gtk_color_selection_palette_from_string(native, out parsedColors, out n_colors);
			GLib.Marshaller.Free (native);
			
			// If things failed, return silently
			if (!raw_ret)
			{
				return null;
			}
			System.Console.WriteLine("Raw call finished, making " + n_colors + " actual colors");
			Gdk.Color[] colors = new Gdk.Color[n_colors];
			for (int i=0; i < n_colors; i++)
			{
				colors[i] = Gdk.Color.New(parsedColors);
				parsedColors = (IntPtr) ((int)parsedColors + Marshal.SizeOf(colors[i]));
			}
			return colors;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_color_selection_set_previous_color(IntPtr raw, ref Gdk.Color color);
		static d_gtk_color_selection_set_previous_color gtk_color_selection_set_previous_color = FuncLoader.LoadFunction<d_gtk_color_selection_set_previous_color>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_color_selection_set_previous_color"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_color_selection_get_previous_color(IntPtr raw, out Gdk.Color color);
		static d_gtk_color_selection_get_previous_color gtk_color_selection_get_previous_color = FuncLoader.LoadFunction<d_gtk_color_selection_get_previous_color>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_color_selection_get_previous_color"));

		// Create Gtk# property to replace two Gtk+ functions
		public Gdk.Color PreviousColor
		{
			get
			{
				Gdk.Color returnColor;
				gtk_color_selection_get_previous_color(Handle, out returnColor);	
				return returnColor;
			}
			set
			{
				gtk_color_selection_set_previous_color(Handle, ref value);
			}
		}
	}
}


// Gdk.Color.cs - Gdk Color class customizations
//
// Author: Jasper van Putten <Jaspervp@gmx.net>, Miguel de Icaza.
//
// Copyright (c) 2002 Jasper van Putten
// Copyright (c) 2003 Miguel de Icaza.
//
// This code is inserted after the automatically generated code.
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

namespace Gdk {

	using System.Runtime.InteropServices;

	public partial struct Color {

		public Color (byte r, byte g, byte b)
		{
			Red = (ushort) (r << 8 | r);
			Green = (ushort) (g << 8 | g);
			Blue = (ushort) (b << 8 | b);
			Pixel = 0;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_gdk_color_hash(ref Gdk.Color raw);
		static d_gdk_color_hash gdk_color_hash = FuncLoader.LoadFunction<d_gdk_color_hash>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_color_hash"));

		public override int GetHashCode() {
			return (int) gdk_color_hash(ref this);
		}
	}
}



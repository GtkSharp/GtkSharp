// PixbufAnimation.cs - GdkPixbufAnimation class customizations
//
// Copyright (c) 2005 Novell, Inc.
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

	using System;
	using System.Runtime.InteropServices;

	public partial class PixbufAnimation {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_pixbuf_animation_new_from_file(IntPtr filename, out IntPtr error);
		static d_gdk_pixbuf_animation_new_from_file gdk_pixbuf_animation_new_from_file = FuncLoader.LoadFunction<d_gdk_pixbuf_animation_new_from_file>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), 
			FuncLoader.IsWindows ? "gdk_pixbuf_animation_new_from_file_utf8" : "gdk_pixbuf_animation_new_from_file"));

		public PixbufAnimation(string filename) : base(IntPtr.Zero)
		{
			IntPtr native_filename = GLib.Marshaller.StringToPtrGStrdup(filename);
			IntPtr error = IntPtr.Zero;
			Raw = gdk_pixbuf_animation_new_from_file(native_filename, out error);
			GLib.Marshaller.Free(native_filename);
			if (error != IntPtr.Zero) throw new GLib.GException(error);
		}

		public PixbufAnimation (System.IO.Stream stream) : base (IntPtr.Zero)
		{
			using (var pl = new PixbufLoader (stream)) {
				Raw = pl.AnimationHandle;
			}
		}

		public PixbufAnimation (System.Reflection.Assembly assembly, string resource) : base (IntPtr.Zero)
		{
			if (assembly == null)
				assembly = System.Reflection.Assembly.GetCallingAssembly ();

			using (var pl = new PixbufLoader (assembly, resource)) {
				Raw = pl.AnimationHandle;
			}
		}

		static public PixbufAnimation LoadFromResource (string resource)
		{
			return new PixbufAnimation (System.Reflection.Assembly.GetCallingAssembly (), resource);
		}
	}
}


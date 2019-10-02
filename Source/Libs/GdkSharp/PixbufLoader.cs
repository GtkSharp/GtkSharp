// PixbufLoader.cs - Gdk PixbufLoader class customizations
//
// Authors: 
//	Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2003 Novell, Inc.
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

	public partial class PixbufLoader {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_object_ref(IntPtr handle);
		static d_g_object_ref g_object_ref = FuncLoader.LoadFunction<d_g_object_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_ref"));

		internal IntPtr PixbufHandle {
			get {
				return g_object_ref (gdk_pixbuf_loader_get_pixbuf (Handle));
			}
		}

		internal IntPtr AnimationHandle {
			get {
				return g_object_ref (gdk_pixbuf_loader_get_animation (Handle));
			}
		}

  		public bool Write (byte[] bytes)
  		{
			return this.Write (bytes, (ulong) bytes.Length);
  		}
  
		[Obsolete ("Replaced by Write (byte[], ulong) for 64 bit portability")]
		public bool Write (byte[] bytes, uint count)
		{
			return this.Write (bytes, (ulong) count);
		}

		private void LoadFromStream (System.IO.Stream input)
		{
			byte [] buffer = new byte [8192];
			int n;

			while ((n = input.Read (buffer, 0, 8192)) != 0)
				Write (buffer, (uint) n);
		}
		
		public PixbufLoader (string file, int width, int height) : this ()
		{
			SetSize(width, height);
			
			using(System.IO.FileStream stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				InitFromStream(stream);
			}
		}
		
		public PixbufLoader (System.IO.Stream stream) : this ()
		{
			InitFromStream(stream);
		}
		
		private void InitFromStream (System.IO.Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			try {
				LoadFromStream (stream);
			} finally {
				Close ();
			}
		}
		
		public PixbufLoader (System.IO.Stream stream, int width, int height) : this()
		{
			SetSize(width, height);
			InitFromStream(stream);
		}

		public PixbufLoader (System.Reflection.Assembly assembly, string resource) : this ()
		{
			InitFromAssemblyResource(assembly == null ? System.Reflection.Assembly.GetCallingAssembly () : assembly, resource);
		}
		
		private void InitFromAssemblyResource(System.Reflection.Assembly assembly, string resource)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");

			System.IO.Stream s = assembly.GetManifestResourceStream (resource);
			if (s == null)
				throw new ArgumentException ("'" + resource + "' is not a valid resource name of assembly '" + assembly + "'.");

			try {
				LoadFromStream (s);
			} finally {
				Close ();
			}
		}
		
		public PixbufLoader (System.Reflection.Assembly assembly, string resource, int width, int height) : this ()
		{
			SetSize(width, height);
			InitFromAssemblyResource(assembly == null ? System.Reflection.Assembly.GetCallingAssembly () : assembly, resource);
		}
		
		public PixbufLoader (byte[] buffer) : this()
		{
			InitFromBuffer(buffer);
		}
		
		private void InitFromBuffer (byte[] buffer)
		{
			try {
				Write (buffer, (uint)buffer.Length);
			} finally {
				Close ();
			}
		}
		
		
		public PixbufLoader (byte[] buffer, int width, int height) : this()
		{
			SetSize(width, height);
			InitFromBuffer(buffer);
		}

		static public PixbufLoader LoadFromResource (string resource)
		{
			return new PixbufLoader (System.Reflection.Assembly.GetCallingAssembly (), resource);
		}
	}
}



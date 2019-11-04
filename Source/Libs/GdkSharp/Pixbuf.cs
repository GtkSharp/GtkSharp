// Pixbuf.cs - Gdk Pixbuf class customizations
//
// Authors: 
// 	Vladimir Vukicevic <vladimir@pobox.com>
// 	Miguel de Icaza <miguel@ximian.com>
// 	Mike Kestner <mkestner@ximian.com>
// 	Duncan Mak <duncan@ximian.com>
//	Gonzalo Paniagua Javier <gonzalo@ximian.com>
//	Martin Willemoes Hansen <mwh@sysrq.dk>
//	Jose Faria   <spigaz@gmail.com> 
//
// Copyright (c) 2002 Vladimir Vukicevic
// Copyright (c) 2003 Ximian, Inc. (Miguel de Icaza)
// Copyright (c) 2003 Ximian, Inc. (Duncan Mak)
// Copyright (c) 2003 Ximian, Inc. (Gonzalo Paniagua Javier)
// Copyright (c) 2003 Martin Willemoes Hansen
// Copyright (c) 2004-2005 Novell, Inc.
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

	public partial class Pixbuf {

		public Pixbuf (System.IO.Stream stream) : base (IntPtr.Zero)
		{
			using (PixbufLoader pl = new PixbufLoader (stream)) {
				Raw = pl.PixbufHandle;
			}
		}
		
		public Pixbuf (System.IO.Stream stream, int width, int height) : base (IntPtr.Zero)
		{
			using (PixbufLoader pl = new PixbufLoader (stream, width, height)) {
				Raw = pl.PixbufHandle;
			}
		}
		
		public Pixbuf (System.Reflection.Assembly assembly, string resource) : base (IntPtr.Zero)
		{
			using (PixbufLoader pl = new PixbufLoader (assembly == null ? System.Reflection.Assembly.GetCallingAssembly () : assembly, resource)) {
				Raw = pl.PixbufHandle;
			}
		}
				
		public Pixbuf (System.Reflection.Assembly assembly, string resource, int width, int height) : base (IntPtr.Zero)
		{
			using (PixbufLoader pl = new PixbufLoader (assembly == null ? System.Reflection.Assembly.GetCallingAssembly () : assembly, resource, width, height)) {
				Raw = pl.PixbufHandle;
			}
		}
		
		public Pixbuf (byte[] buffer) : base (IntPtr.Zero)
		{
			using (PixbufLoader pl = new PixbufLoader (buffer)) {
				Raw = pl.PixbufHandle;
			}
		}

		public Pixbuf (byte[] buffer, int width, int height) : base (IntPtr.Zero)
		{
			using (PixbufLoader pl = new PixbufLoader (buffer, width, height)) {
				Raw = pl.PixbufHandle;
			}
		}

		static public Pixbuf LoadFromResource (string resource)
		{
			return new Pixbuf (System.Reflection.Assembly.GetCallingAssembly (), resource);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_pixbuf_scale_simple(IntPtr raw, int dest_width, int dest_height, int interp_type);
		static d_gdk_pixbuf_scale_simple gdk_pixbuf_scale_simple = FuncLoader.LoadFunction<d_gdk_pixbuf_scale_simple>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixbuf_scale_simple"));

		public Gdk.Pixbuf ScaleSimple(int dest_width, int dest_height, Gdk.InterpType interp_type) {
			IntPtr raw_ret = gdk_pixbuf_scale_simple(Handle, dest_width, dest_height, (int) interp_type);
			Gdk.Pixbuf ret = (Gdk.Pixbuf) GLib.Object.GetObject(raw_ret, true);
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_pixbuf_composite_color_simple(IntPtr raw, int dest_width, int dest_height, int interp_type, int overall_alpha, int check_size, uint color1, uint color2);
		static d_gdk_pixbuf_composite_color_simple gdk_pixbuf_composite_color_simple = FuncLoader.LoadFunction<d_gdk_pixbuf_composite_color_simple>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixbuf_composite_color_simple"));

		public Gdk.Pixbuf CompositeColorSimple(int dest_width, int dest_height, Gdk.InterpType interp_type, int overall_alpha, int check_size, uint color1, uint color2) {
			IntPtr raw_ret = gdk_pixbuf_composite_color_simple(Handle, dest_width, dest_height, (int) interp_type, overall_alpha, check_size, color1, color2);
			Gdk.Pixbuf ret = (Gdk.Pixbuf) GLib.Object.GetObject(raw_ret, true);
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_pixbuf_add_alpha(IntPtr raw, bool substitute_color, byte r, byte g, byte b);
		static d_gdk_pixbuf_add_alpha gdk_pixbuf_add_alpha = FuncLoader.LoadFunction<d_gdk_pixbuf_add_alpha>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixbuf_add_alpha"));

		public Gdk.Pixbuf AddAlpha(bool substitute_color, byte r, byte g, byte b) {
			IntPtr raw_ret = gdk_pixbuf_add_alpha(Handle, substitute_color, r, g, b);
			Gdk.Pixbuf ret = (Gdk.Pixbuf) GLib.Object.GetObject(raw_ret, true);
			return ret;
		}

		class DestroyHelper {

			GCHandle gch;
			GCHandle data_handle;
			PixbufDestroyNotify notify;

			public DestroyHelper (byte[] data, PixbufDestroyNotify notify)
			{
				gch = GCHandle.Alloc (this);
				data_handle = GCHandle.Alloc (data, GCHandleType.Pinned);
				this.notify = notify;
			}

			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			public delegate void NativeDelegate (IntPtr buf, IntPtr data);

			void ReleaseHandles (IntPtr buf, IntPtr data)
			{
				if (notify != null)
					notify ((byte[])data_handle.Target);
				data_handle.Free ();
				gch.Free ();
			}

			NativeDelegate handler;
			public NativeDelegate Handler {
				get {
					handler = new NativeDelegate (ReleaseHandles);
					return handler;
				}
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_pixbuf_new_from_data(byte[] data, int colorspace, bool has_alpha, int bits_per_sample, int width, int height, int rowstride, DestroyHelper.NativeDelegate destroy_fn, IntPtr destroy_fn_data);
		static d_gdk_pixbuf_new_from_data gdk_pixbuf_new_from_data = FuncLoader.LoadFunction<d_gdk_pixbuf_new_from_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixbuf_new_from_data"));

		public Pixbuf (byte[] data, Gdk.Colorspace colorspace, bool has_alpha, int bits_per_sample, int width, int height, int rowstride, Gdk.PixbufDestroyNotify destroy_fn) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (Pixbuf)) {
				throw new InvalidOperationException ("Can't override this constructor.");
			}
			DestroyHelper helper = new DestroyHelper (data, destroy_fn);
			Raw = gdk_pixbuf_new_from_data(data, (int) colorspace, has_alpha, bits_per_sample, width, height, rowstride, helper.Handler, IntPtr.Zero);
		}

		// overload to default the colorspace
		public Pixbuf(byte [] data, bool has_alpha, int bits_per_sample, int width, int height, int rowstride, Gdk.PixbufDestroyNotify destroy_fn) : this (data, Gdk.Colorspace.Rgb, has_alpha, bits_per_sample, width, height, rowstride, destroy_fn) {}

		public Pixbuf(byte [] data, bool has_alpha, int bits_per_sample, int width, int height, int rowstride) : this (data, Gdk.Colorspace.Rgb, has_alpha, bits_per_sample, width, height, rowstride, null) {}

		public Pixbuf(byte [] data, Gdk.Colorspace colorspace, bool has_alpha, int bits_per_sample, int width, int height, int rowstride) : this (data, colorspace, has_alpha, bits_per_sample, width, height, rowstride, null) {}

		/* gdk_pixbuf_new_from_inline has been deprecated since version 2.32 and should not be used in newly-written code. Use GResource instead */
		/*
		public unsafe Pixbuf(byte[] data, bool copy_pixels) : base (IntPtr.Zero)
		{
			IntPtr error = IntPtr.Zero;
			Raw = gdk_pixbuf_new_from_inline(data.Length, (IntPtr) data, copy_pixels, out error);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
		}
		*/
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_pixbuf_new_from_inline(int len, IntPtr data, bool copy_pixels, out IntPtr error);
		static d_gdk_pixbuf_new_from_inline gdk_pixbuf_new_from_inline = FuncLoader.LoadFunction<d_gdk_pixbuf_new_from_inline>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixbuf_new_from_inline"));

		public unsafe Pixbuf(int data_length, void *data, bool copy_pixels) : base (IntPtr.Zero)
		{
			IntPtr error = IntPtr.Zero;
			Raw = gdk_pixbuf_new_from_inline(data_length, (IntPtr) data, copy_pixels, out error);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
		}
	
//
// ICloneable interface
//

		public object Clone ()
		{
			return Copy ();
		}


//
// the 'Pixels' property
//
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_pixbuf_get_pixels(IntPtr raw);
		static d_gdk_pixbuf_get_pixels gdk_pixbuf_get_pixels = FuncLoader.LoadFunction<d_gdk_pixbuf_get_pixels>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixbuf_get_pixels"));

                public IntPtr Pixels {
                	get {
                		IntPtr ret = gdk_pixbuf_get_pixels (Handle);
                		return ret;
                	}
                }
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gdk_pixbuf_get_formats();
		static d_gdk_pixbuf_get_formats gdk_pixbuf_get_formats = FuncLoader.LoadFunction<d_gdk_pixbuf_get_formats>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixbuf_get_formats"));

		public static PixbufFormat[] Formats {
			get {
				IntPtr list_ptr = gdk_pixbuf_get_formats ();
				if (list_ptr == IntPtr.Zero)
					return new PixbufFormat [0];
				GLib.SList list = new GLib.SList (list_ptr, typeof (PixbufFormat));
				PixbufFormat[] result = new PixbufFormat [list.Count];
				for (int i = 0; i < list.Count; i++)
					result [i] = (PixbufFormat) list [i];
				return result;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gdk_pixbuf_save(IntPtr raw, IntPtr filename, IntPtr type, out IntPtr error, IntPtr dummy);
		static d_gdk_pixbuf_save gdk_pixbuf_save = FuncLoader.LoadFunction<d_gdk_pixbuf_save>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixbuf_save"));

		public unsafe bool Save(string filename, string type) {
			IntPtr error = IntPtr.Zero;
			IntPtr nfilename = GLib.Marshaller.StringToPtrGStrdup (filename);
			IntPtr ntype = GLib.Marshaller.StringToPtrGStrdup (type);
			bool ret = gdk_pixbuf_save(Handle, nfilename, ntype, out error, IntPtr.Zero);
			GLib.Marshaller.Free (nfilename);
			GLib.Marshaller.Free (ntype);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gdk_pixbuf_save_to_bufferv(IntPtr raw, out IntPtr buffer, out IntPtr buffer_size, IntPtr type, IntPtr[] option_keys, IntPtr[] option_values, out IntPtr error);
		static d_gdk_pixbuf_save_to_bufferv gdk_pixbuf_save_to_bufferv = FuncLoader.LoadFunction<d_gdk_pixbuf_save_to_bufferv>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixbuf_save_to_bufferv"));

		IntPtr[] NullTerm (string[] src)
		{
			if (src.Length == 0)
				return null;
			IntPtr[] result = new IntPtr [src.Length + 1];
			for (int i = 0; i < src.Length; i++)
				result [i] = GLib.Marshaller.StringToPtrGStrdup (src [i]);
			result [src.Length] = IntPtr.Zero;
			return result;
		}

		void ReleaseArray (IntPtr[] ptrs)
		{
			if (ptrs == null)
				return;
			foreach (IntPtr p in ptrs)
				GLib.Marshaller.Free (p);
		}

		public unsafe byte[] SaveToBuffer (string type)
		{
			return SaveToBuffer (type, new string [0], new string [0]);
		}

		public unsafe byte[] SaveToBuffer (string type, string[] option_keys, string[] option_values) 
		{
			IntPtr error = IntPtr.Zero;
			IntPtr buffer;
			IntPtr buffer_size;
			IntPtr ntype = GLib.Marshaller.StringToPtrGStrdup (type);
			IntPtr[] nkeys = NullTerm (option_keys);
			IntPtr[] nvals = NullTerm (option_values);
			bool saved = gdk_pixbuf_save_to_bufferv (Handle, out buffer, out buffer_size, ntype, nkeys, nvals, out error);
			GLib.Marshaller.Free (ntype);
			ReleaseArray (nkeys);
			ReleaseArray (nvals);

			if (!saved)
				throw new GLib.GException (error);
			byte[] result = new byte [(int)buffer_size];
			Marshal.Copy (buffer, result, 0, (int) buffer_size);
			GLib.Marshaller.Free (buffer);
			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gdk_pixbuf_save_to_callbackv(IntPtr raw, GdkSharp.PixbufSaveFuncNative save_func, IntPtr user_data, IntPtr type, IntPtr[] option_keys, IntPtr[] option_values, out IntPtr error);
		static d_gdk_pixbuf_save_to_callbackv gdk_pixbuf_save_to_callbackv = FuncLoader.LoadFunction<d_gdk_pixbuf_save_to_callbackv>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixbuf_save_to_callbackv"));

		public unsafe void SaveToCallback (PixbufSaveFunc save_func, string type)
		{
			SaveToCallback (save_func, type, new string [0], new string [0]);
		}

		public unsafe void SaveToCallback (PixbufSaveFunc save_func, string type, string[] option_keys, string[] option_values) 
		{
			GdkSharp.PixbufSaveFuncWrapper save_func_wrapper = new GdkSharp.PixbufSaveFuncWrapper (save_func);
			IntPtr error = IntPtr.Zero;
			IntPtr ntype = GLib.Marshaller.StringToPtrGStrdup (type);
			IntPtr[] nkeys = NullTerm (option_keys);
			IntPtr[] nvals = NullTerm (option_values);
			bool saved = gdk_pixbuf_save_to_callbackv (Handle, save_func_wrapper.NativeDelegate, IntPtr.Zero, ntype, nkeys, nvals, out error);
			GLib.Marshaller.Free (ntype);
			ReleaseArray (nkeys);
			ReleaseArray (nvals);

			if (!saved)
				throw new GLib.GException (error);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gdk_pixbuf_savev(IntPtr raw, IntPtr filename, IntPtr type, IntPtr[] option_keys, IntPtr[] option_values, out IntPtr error);
		static d_gdk_pixbuf_savev gdk_pixbuf_savev = FuncLoader.LoadFunction<d_gdk_pixbuf_savev>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GdkPixbuf), "gdk_pixbuf_savev"));

		public unsafe bool Savev(string filename, string type, string[] option_keys, string[] option_values) {
			IntPtr error = IntPtr.Zero;
			IntPtr native_filename = GLib.Marshaller.StringToPtrGStrdup (filename);
			IntPtr native_type = GLib.Marshaller.StringToPtrGStrdup (type);
			IntPtr[] native_option_keys = NullTerm (option_keys);
			IntPtr[] native_option_values = NullTerm (option_values);
			bool saved = gdk_pixbuf_savev(Handle, native_filename, native_type, native_option_keys, native_option_values, out error);
			GLib.Marshaller.Free (native_filename);
			GLib.Marshaller.Free (native_type);
			ReleaseArray (native_option_keys);
			ReleaseArray (native_option_values);

			if (!saved)
				throw new GLib.GException (error);
			return saved;
		}
	}
}



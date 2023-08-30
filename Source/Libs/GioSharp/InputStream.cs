using System;
using System.Runtime.InteropServices;

namespace GLib
{

	public partial class InputStream
	{

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_memory_input_stream_new_from_data(byte[] data, uint len, IntPtr destroy);

		static d_g_memory_input_stream_new_from_data g_memory_input_stream_new_from_data = FuncLoader.LoadFunction<d_g_memory_input_stream_new_from_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gio), "g_memory_input_stream_new_from_data"));

		// https://docs.gtk.org/gio/ctor.MemoryInputStream.new_from_data.html
		public static InputStream NewFromData(byte[] data)
		{

			var streamPtr = g_memory_input_stream_new_from_data(data, (uint)data.Length, IntPtr.Zero);
			var inputStream = new InputStream(streamPtr);

			return inputStream;

		}

	}

}
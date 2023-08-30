using System;
using System.Runtime.InteropServices;

namespace GLib
{

	public static class SignalExtensions
	{

		// missing in GtkSharp; also not found in *-api.xml's
		// https://docs.gtk.org/gobject/func.signal_connect_data.html
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate ulong d_g_signal_connect_data(IntPtr instance, string detailed_signal, IntPtr c_handler, IntPtr data, SignalClosure.ClosureNotify destroy_data, ConnectFlags connect_flags);

		static d_g_signal_connect_data g_signal_connect_data = FuncLoader.LoadFunction<d_g_signal_connect_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gio), "g_signal_connect_data"));

		public static ulong SignalConnectData<TDelegate>(this GLib.Object instance, string detailed_signal, TDelegate callBack, IntPtr data = default, ConnectFlags connect_flags = 0) where TDelegate : Delegate
		{
			var c_handler = Marshal.GetFunctionPointerForDelegate(callBack);

			return g_signal_connect_data(instance.Handle, detailed_signal, c_handler, data, null, connect_flags);
		}

	}

}
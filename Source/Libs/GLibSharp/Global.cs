// GLib.Global.cs - Global glib properties and methods.
//
// Authors: Andres G. Aragoneses <aaragoneses@novell.com>
//          Stephane Delcroix (stephane@delcroix.org)
//
// Copyright (c) 2008 Novell, Inc.
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


namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	public class Global
	{
		//this is a static class
		private Global () {}

		internal static bool IsWindowsPlatform {
			get {
				switch (Environment.OSVersion.Platform) {
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
					return true;
				default:
					return false;
				}
			}
		}

		public static string ProgramName {
			get {
				return GLib.Marshaller.Utf8PtrToString (g_get_prgname());
			}
			set { 
				IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (value);
				g_set_prgname (native_name);
				GLib.Marshaller.Free (native_name);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_set_prgname(IntPtr name);
		static d_g_set_prgname g_set_prgname = FuncLoader.LoadFunction<d_g_set_prgname>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_set_prgname"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_get_prgname();
		static d_g_get_prgname g_get_prgname = FuncLoader.LoadFunction<d_g_get_prgname>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_get_prgname"));

		public static string ApplicationName {
			get {
				return GLib.Marshaller.Utf8PtrToString (g_get_application_name());	
			}
			set {
				IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (value);
				g_set_application_name (native_name);
				GLib.Marshaller.Free (native_name);				
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_set_application_name(IntPtr name);
		static d_g_set_application_name g_set_application_name = FuncLoader.LoadFunction<d_g_set_application_name>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_set_application_name"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_get_application_name();
		static d_g_get_application_name g_get_application_name = FuncLoader.LoadFunction<d_g_get_application_name>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_get_application_name"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_format_size_for_display(long size);
		static d_g_format_size_for_display g_format_size_for_display = FuncLoader.LoadFunction<d_g_format_size_for_display>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_format_size_for_display"));
		
		static public string FormatSizeForDisplay (long size)
		{
			return Marshaller.PtrToStringGFree (g_format_size_for_display (size));
		}
	}
}


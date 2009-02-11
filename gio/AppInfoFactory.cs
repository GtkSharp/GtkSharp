//
// AppInfoFactory.cs
//
// Author(s):
//	Stephane Delcroix  <stephane@delcroix.org>
//
// Copyright (c) 2009 Stephane Delcroix
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

using System;
using System.Runtime.InteropServices;

namespace GLib
{
	public class AppInfoFactory
	{
		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_app_info_get_all();

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_app_info_create_from_commandline(IntPtr commandline, IntPtr application_name, int flags, out IntPtr error);

		public static GLib.AppInfo CreateFromCommandline(string commandline, string application_name, GLib.AppInfoCreateFlags flags) {
			IntPtr native_commandline = GLib.Marshaller.StringToPtrGStrdup (commandline);
			IntPtr native_application_name = GLib.Marshaller.StringToPtrGStrdup (application_name);
			IntPtr error = IntPtr.Zero;
			IntPtr raw_ret = g_app_info_create_from_commandline(native_commandline, native_application_name, (int) flags, out error);
			GLib.AppInfo ret = GLib.Object.GetObject (raw_ret, false) as AppInfo;
			GLib.Marshaller.Free (native_commandline);
			GLib.Marshaller.Free (native_application_name);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		public static GLib.List GetAll () {
			IntPtr raw_ret = g_app_info_get_all();
			GLib.List ret = new GLib.List(raw_ret);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_app_info_get_all_for_type(IntPtr content_type);

//		public static GLib.AppInfo[] GetAllForType(string content_type) {
//			IntPtr native_content_type = GLib.Marshaller.StringToPtrGStrdup (content_type);
//			IntPtr raw_ret = g_app_info_get_all_for_type(native_content_type);
//			GLib.AppInfo[] ret = (GLib.AppInfo[]) GLib.Marshaller.ListPtrToArray (raw_ret, typeof(GLib.List), true, false, typeof(GLib.AppInfo));
//			GLib.Marshaller.Free (native_content_type);
//			return ret;
//		}

		public static GLib.List GetAllForType (string content_type)
		{
			IntPtr native_content_type = GLib.Marshaller.StringToPtrGStrdup (content_type);
			IntPtr raw_ret = g_app_info_get_all_for_type(native_content_type);
			GLib.List ret = new GLib.List(raw_ret);
			GLib.Marshaller.Free (native_content_type);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_app_info_get_default_for_type(IntPtr content_type, bool must_support_uris);

		public static GLib.AppInfo GetDefaultForType(string content_type, bool must_support_uris) {
			IntPtr native_content_type = GLib.Marshaller.StringToPtrGStrdup (content_type);
			IntPtr raw_ret = g_app_info_get_default_for_type(native_content_type, must_support_uris);
			GLib.AppInfo ret = GLib.Object.GetObject (raw_ret, false) as AppInfo;
			GLib.Marshaller.Free (native_content_type);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_app_info_get_default_for_uri_scheme(IntPtr uri_scheme);

		public static GLib.AppInfo GetDefaultForUriScheme(string uri_scheme) {
			IntPtr native_uri_scheme = GLib.Marshaller.StringToPtrGStrdup (uri_scheme);
			IntPtr raw_ret = g_app_info_get_default_for_uri_scheme(native_uri_scheme);
			GLib.AppInfo ret = GLib.Object.GetObject (raw_ret, false) as AppInfo;
			GLib.Marshaller.Free (native_uri_scheme);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern bool g_app_info_launch_default_for_uri(IntPtr uri, IntPtr launch_context, out IntPtr error);

		public static bool LaunchDefaultForUri(string uri, GLib.AppLaunchContext launch_context) {
			IntPtr native_uri = GLib.Marshaller.StringToPtrGStrdup (uri);
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_app_info_launch_default_for_uri(native_uri, launch_context == null ? IntPtr.Zero : launch_context.Handle, out error);
			bool ret = raw_ret;
			GLib.Marshaller.Free (native_uri);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}
	}
}

// ApplicationCommandLine.cs - customizations to GLib.ApplicationCommandLine
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
	public partial class ApplicationCommandLine
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_application_command_line_get_arguments(IntPtr raw, out int argc);
		static d_g_application_command_line_get_arguments g_application_command_line_get_arguments = FuncLoader.LoadFunction<d_g_application_command_line_get_arguments>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gio), "g_application_command_line_get_arguments"));

		// The array is "gchar **" with its length in argc, and the gir marks it
		// zero-terminated="0" - so codegen, which only knows the NULL-terminated
		// shape, bound the return value as one string: the program name, with
		// every actual argument unreachable and the block leaked. The count is
		// what says how long the array is, so it is what this reads.
		//
		// Transfer is full for both the array and its elements: g_strfreev is
		// what the documentation names, and that is one free per string plus one
		// for the vector.
		public string[] GetArguments (out int argc)
		{
			IntPtr raw_ret = g_application_command_line_get_arguments (Handle, out argc);
			if (raw_ret == IntPtr.Zero) {
				argc = 0;
				return new string [0];
			}

			var ret = new string [argc < 0 ? 0 : argc];
			for (int i = 0; i < ret.Length; i++) {
				IntPtr item = Marshal.ReadIntPtr (raw_ret, i * IntPtr.Size);
				ret [i] = GLib.Marshaller.Utf8PtrToString (item);
				GLib.Marshaller.Free (item);
			}

			GLib.Marshaller.Free (raw_ret);
			return ret;
		}

		public string[] Arguments {
			get {
				int argc;
				return GetArguments (out argc);
			}
		}
	}
}

// SettingsSchemaSource.cs - customizations to GLib.SettingsSchemaSource
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

	public partial class SettingsSchemaSource {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_g_settings_schema_source_list_schemas (IntPtr raw, bool recursive, out IntPtr non_relocatable, out IntPtr relocatable);
		static d_g_settings_schema_source_list_schemas g_settings_schema_source_list_schemas = FuncLoader.LoadFunction<d_g_settings_schema_source_list_schemas> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gio), "g_settings_schema_source_list_schemas"));

		/// <summary>Lists the schema ids this source knows about.</summary>
		/// <remarks>
		/// Both out-parameters are <c>gchar ***</c> in C -- NULL-terminated
		/// string arrays the caller owns. Codegen has no rule for a triple
		/// pointer, so it produced "out string" and read the array of pointers
		/// as a UTF-8 string; see GioSharp.metadata for the whole story.
		/// </remarks>
		public void ListSchemas (bool recursive, out string[] nonRelocatable, out string[] relocatable)
		{
			IntPtr native_non_relocatable;
			IntPtr native_relocatable;
			g_settings_schema_source_list_schemas (Handle, recursive, out native_non_relocatable, out native_relocatable);
			nonRelocatable = GLib.Marshaller.NullTermPtrToStringArray (native_non_relocatable, true);
			relocatable = GLib.Marshaller.NullTermPtrToStringArray (native_relocatable, true);
		}
	}
}

// GLib.FileUtils.cs - GFileUtils class implementation
//
// Author: Martin Baulig <martin@gnome.org>
//
// (c) 2002 Ximian, Inc

namespace GLib {

	using System;
	using System.Text;
	using System.Runtime.InteropServices;

	public class FileUtils
	{
		[DllImport("libglib-2.0-0.dll")]
		extern static bool g_file_get_contents (string filename, out IntPtr contents, out int length, out IntPtr error);

		public static string GetFileContents (string filename)
		{
			int length;
			IntPtr contents, error;

			if (!g_file_get_contents (filename, out contents, out length, out error))
				throw new GException (error);

			return Marshal.PtrToStringAnsi (contents, length);
		}
	}
}

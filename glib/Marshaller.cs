// GLibSharp.Marshaller.cs : Marshalling utils 
//
// Author: Rachel Hestilow <rachel@nullenvoid.com>
//
// (c) 2002, 2003 Rachel Hestilow

namespace GLibSharp {
	using System;
	using System.Runtime.InteropServices;
	
	/// <summary>
	///  Marshalling utilities 
	/// </summary>
	///
	/// <remarks>
	///  Utility class for internal wrapper use
	/// </remarks>
	
	public class Marshaller {
	
		[DllImport("libglib-2.0-0.dll")]
		static extern void g_free (IntPtr mem);

		public static string PtrToStringGFree (IntPtr ptr) {
			string ret = Marshal.PtrToStringAnsi (ptr);
			g_free (ptr);
			return ret;
		}

		[DllImport("libglib-2.0-0.dll")]
		static extern void g_strfreev (IntPtr mem);

		public static string[] PtrToStringGFree (IntPtr[] ptrs) {
			// The last pointer is a null terminator.
			string[] ret = new string[ptrs.Length - 1];
			for (int i = 0; i < ret.Length; i++) {
				ret[i] = Marshal.PtrToStringAnsi (ptrs[i]);
				g_free (ptrs[i]);
			}
			return ret;
		}

		[DllImport("libglib-2.0-0.dll")]
		static extern IntPtr g_strdup (string str);

		public static IntPtr StringToPtrGStrdup (string str) {
			return g_strdup (str);
		}

	}
}


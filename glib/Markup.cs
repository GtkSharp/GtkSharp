//
// Markup.cs: Wrapper for the Markup code in Glib
//
// Authors:
//    Miguel de Icaza (miguel@ximian.com)
//
// (C) 2003 Ximian, Inc.
//
using System;
using System.Runtime.InteropServices;

namespace GLib {


	public class Markup {
		[DllImport("libglib-2.0-0.dll")]
		static extern IntPtr g_markup_escape_text (string text, int len);
		
		static public string EscapeText (string s)
		{
			if (s == null)
				return "";

			return GLibSharp.Marshaller.PtrToStringGFree (g_markup_escape_text (s, s.Length));
		}
	}
}

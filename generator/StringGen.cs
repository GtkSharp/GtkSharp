// GtkSharp.Generation.StringGen.cs - The String type Generatable.
//
// Author: Rachel Hestilow <rachel@nullenvoid.com>
//
// (c) 2003 Rachel Hestilow

namespace GtkSharp.Generation {

	using System;

	public class StringGen : ConstStringGen {

		public StringGen (string ctype) : base (ctype)
		{
		}
	
		public override string FromNativeReturn(String var)
		{
			return "GLib.Marshaller.PtrToStringGFree(" + var + ")";
		}

		public override string ToNativeReturn(String var)
		{
			return "GLib.Marshaller.StringToPtrGStrdup(" + var + ")";
		}
	}
}


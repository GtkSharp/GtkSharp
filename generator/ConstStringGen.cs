// GtkSharp.Generation.ConstStringGen.cs - The Const String type Generatable.
//
// Author: Rachel Hestilow <rachel@nullenvoid.com>
//
// (c) 2003 Rachel Hestilow

namespace GtkSharp.Generation {

	using System;

	public class ConstStringGen : SimpleGen {
		
		public ConstStringGen (string ctype) : base (ctype, "string")
		{
		}

		public override String MarshalReturnType {
			get
			{
				return "IntPtr";
			}
		}
		
		public override String FromNativeReturn(String var)
		{
			return "Marshal.PtrToStringAnsi(" + var + ")";
		}
	}
}


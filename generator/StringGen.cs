// GtkSharp.Generation.StringGen.cs - The String type Generatable.
//
// Author: Rachel Hestilow <rachel@nullenvoid.com>
//         Mike Kestner  <mkestner@novell.com>
//
// Copyright (c) 2003 Rachel Hestilow
// Copyright (c) 2004 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace GtkSharp.Generation {

	using System;

	public class StringGen : ConstStringGen {

		public StringGen (string ctype) : base (ctype)
		{
		}
	
		public override string ToNativeReturnType {
			get {
				return "IntPtr";
			}
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


// GtkSharp.Generation.ByRefGen.cs - The ByRef type Generatable.
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2003 Mike Kestner
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

	public class ByRefGen : SimpleBase {
		
		public ByRefGen (string ctype, string type) : base (ctype, type) {}
		
		public override string MarshalType {
			get {
				return "ref " + QualifiedName;
			}
		}

		public override string MarshalReturnType {
			get {
				return QualifiedName;
			}
		}

		public override string ToNativeReturnType {
			get {
				return QualifiedName;
			}
		}

		public override string CallByName (string var_name)
		{
			return "ref " + var_name;
		}
		
		public override string ToNativeReturn(string var)
		{
			return var;
		}
	}
}


// GtkSharp.Generation.ReturnValue.cs - The ReturnValue Generatable.
//
// Author: Mike Kestner <mkestner@novell.com>
//
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
	using System.Xml;

	public class ReturnValue  {
		
		private XmlElement elem;

		public ReturnValue (XmlElement elem) 
		{
			this.elem = elem;
		}

		public string CType {
			get {
				return elem == null ? String.Empty : elem.GetAttribute("type");
			}
		}

		public string CSType {
			get {
				return SymbolTable.Table.GetCSType (CType) + (IsArray ? "[]" : String.Empty);
			}
		}

		public string ElementType {
			get {
				return elem == null ? String.Empty : elem.GetAttribute("element_type");
			}
		}

		public bool IsArray {
			get {
				return elem == null ? false : elem.HasAttribute ("array");
			}
		}

		public string MarshalType {
			get {
				return SymbolTable.Table.GetMarshalReturnType (CType) + (IsArray ? "[]" : String.Empty);
			}
		}

		public string ToNativeType {
			get {
				return SymbolTable.Table.GetMarshalType (CType) + (IsArray ? "[]" : String.Empty);
			}
		}

		public bool Validate ()
		{
			if (MarshalType == "" || CSType == "") {
				Console.Write("rettype: " + CType);
				return false;
			}
			return true;
		}
	}
}


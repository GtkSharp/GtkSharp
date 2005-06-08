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
				if (IGen == null)
					return String.Empty;

				if (ElementType != String.Empty)
					return ElementType + "[]";

				return IGen.QualifiedName + (IsArray ? "[]" : String.Empty);
			}
		}

		string ElementType {
			get {
				return elem == null ? String.Empty : elem.GetAttribute("element_type");
			}
		}

		IGeneratable igen;
		IGeneratable IGen {
			get {
				if (igen == null)
					igen = SymbolTable.Table [CType];
				return igen;
			}
		}

		bool IsArray {
			get {
				return elem == null ? false : elem.HasAttribute ("array");
			}
		}

		public bool IsVoid {
			get {
				return CSType == "void";
			}
		}

		public string MarshalType {
			get {
				if (IGen == null)
					return String.Empty;
				return IGen.MarshalReturnType + (IsArray ? "[]" : String.Empty);
			}
		}

		bool Owned {
			get {
				return elem.GetAttribute ("owned") == "true";
			}
		}

		public string ToNativeType {
			get {
				if (IGen == null)
					return String.Empty;
				return IGen.ToNativeReturnType + (IsArray ? "[]" : String.Empty);
			}
		}

		public string FromNative (string var)
		{
			if (IGen == null)
				return String.Empty;
			if (Owned)
				var += ", true";
			else if (ElementType != String.Empty) {
				string type_str = "typeof (" + ElementType + ")";
				return String.Format ("({0}[]) GLib.Marshaller.ListToArray ({1}, {2});", ElementType, IGen.FromNativeReturn (var + ", " + type_str), type_str);
			}
			return IGen.FromNativeReturn (var);
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


// GtkSharp.Generation.ChildProperty.cs - GtkContainer child properties
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
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class ChildProperty : Property {

		public ChildProperty (XmlElement elem, ClassBase container_type) : base (elem, container_type) {}

		protected override string QuotedPropertyName (string cname) {
			if (cname.StartsWith ("child_"))
				return "\"" + cname.Substring (6) + "\"";
			else
				return "\"" + cname + "\"";
		}

		protected override string PropertyHeader (ref string indent, string modifiers, string cs_type, string name) {
			return "";
		}

		protected override string GetterHeader (string modifiers, string cs_type, string name) {
			return "public " + modifiers + cs_type + " Get" + name + " (Widget child)";
		}

		protected override string RawGetter (string qpname) {
			return "ChildGetProperty (child, " + qpname + ")";
		}

		protected override string SetterHeader (string modifiers, string cs_type, string name) {
			return "public " + modifiers + "void Set" + name + " (Widget child, " + cs_type + " value)";
		}

		protected override string RawSetter (string qpname) {
			return "ChildSetProperty(child, " + qpname + ", val)";
		}

		protected override string PropertyFooter (string indent) {
			return "";
		}

	}
}


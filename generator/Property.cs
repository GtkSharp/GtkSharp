// GtkSharp.Generation.Property.cs - The Property Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
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

	public class Property {

		protected XmlElement elem;
		protected ClassBase container_type;

		public string Name {
			get {
				return elem.GetAttribute ("name");
			}
		}

		public Property (XmlElement elem, ClassBase container_type)
		{
			this.elem = elem;
			this.container_type = container_type;
		}

		public bool Validate ()
		{
			string c_type = elem.GetAttribute("type");
			SymbolTable table = SymbolTable.Table;
			string cs_type = table.GetCSType(c_type);

			if (cs_type == "") {
				Console.Write("Property has unknown Type {0} ", c_type);
				Statistics.ThrottledCount++;
				return false;
			}

			if (table.IsInterface(c_type)) {
				// FIXME: Handle interface props properly.
				Console.Write("Interface property detected ");
				Statistics.ThrottledCount++;
				return false;
			}

			return true;
		}

		protected virtual string PropertyAttribute (string qpname) {
			return "[GLib.Property (" + qpname + ")]";
		}

		protected virtual string RawGetter (string qpname) {
			return "GetProperty (" + qpname + ")";
		}

		protected virtual string RawSetter (string qpname) {
			return "SetProperty(" + qpname + ", val)";
		}

		public void Generate (GenerationInfo gen_info, string indent)
		{
			SymbolTable table = SymbolTable.Table;
			StreamWriter sw = gen_info.Writer;

			string c_type = elem.GetAttribute("type");
			string cs_type = table.GetCSType(c_type);
			string modifiers = "";

			if (elem.HasAttribute("new_flag") || (container_type.Parent != null && container_type.Parent.GetPropertyRecursively (Name) != null))
				modifiers = "new ";

			XmlElement parent = (XmlElement) elem.ParentNode;
			string name = Name;
			if (name == container_type.Name) {
				name += "Prop";
			}
			string qpname = "\"" + elem.GetAttribute("cname") + "\"";

			string v_type = "";
			if (table.IsEnum(c_type)) {
				v_type = "(int) (GLib.EnumWrapper)";
			} else if (table.IsInterface(c_type)) {
				// FIXME: Handle interface props properly.
				Console.Write("Interface property detected ");
				Statistics.ThrottledCount++;
				return;
			} else if (table.IsObject(c_type)) {
				v_type = "(GLib.UnwrappedObject)";
			} else if (table.IsBoxed (c_type)) {
				v_type = "(GLib.Boxed)";
			} else if (table.IsOpaque (c_type)) {
				v_type = "(GLib.Opaque)";
			}

			if (elem.HasAttribute("construct-only") && !elem.HasAttribute("readable")) {
				return;
			}

			bool has_getter = false;
			bool has_setter = false;
			string getter_type = String.Empty;
			string setter_type = String.Empty;

			Method getter = container_type.GetMethod("Get" + Name);
			if (getter != null && getter.Validate () && getter.IsGetter)
				getter_type = getter.ReturnType;

			Method setter = container_type.GetMethod("Set" + Name);
			if (setter != null && setter.Validate () && setter.IsSetter)
				setter_type = setter.Signature.Types;

			if (getter_type != String.Empty && getter_type == setter_type) {
				has_getter = has_setter = true;
				getter.GenerateImport (sw);
				setter.GenerateImport (sw);
				cs_type = getter_type;
			} else {
				if (getter_type == cs_type) {
					has_getter = true;
					getter.GenerateImport(sw);
				}
				if (setter_type != String.Empty) {
					has_setter = true;
					setter.GenerateImport(sw);
				}

				if (has_setter && setter_type != cs_type)
					cs_type = setter_type;
				else if (has_getter && getter_type != cs_type)
					cs_type = getter_type;
			}

			sw.WriteLine();

			sw.WriteLine (indent + PropertyAttribute (qpname));
			sw.WriteLine (indent + "public " + modifiers + cs_type + " " + name + " {");
			indent += "\t";

			if (has_getter) {
				sw.Write(indent + "get ");
				getter.GenerateBody(gen_info, "\t");
				sw.WriteLine();
			} else if (elem.HasAttribute("readable")) {
				sw.WriteLine(indent + "get {");
				sw.WriteLine(indent + "\tGLib.Value val = " + RawGetter (qpname) + ";");
				if (table.IsObject (c_type)) {
					sw.WriteLine(indent + "\tSystem.IntPtr raw_ret = (System.IntPtr) {0} val;", v_type);
					sw.WriteLine(indent + "\t" + cs_type + " ret = " + table.FromNativeReturn(c_type, "raw_ret") + ";");
					if (!table.IsBoxed (c_type) && !table.IsObject (c_type))
						sw.WriteLine(indent + "\tif (ret == null) ret = new " + cs_type + "(raw_ret);");
				} else if (table.IsOpaque (c_type) || table.IsBoxed (c_type)) {
					sw.WriteLine(indent + "\t" + cs_type + " ret = (" + cs_type + ") val;");
				} else {
					sw.Write(indent + "\t" + cs_type + " ret = ");
					sw.Write ("(" + cs_type + ") ");
					if (v_type != "") {
						sw.Write(v_type + " ");
					}
					sw.WriteLine("val;");
				}

				sw.WriteLine(indent + "\tval.Dispose ();");
				sw.WriteLine(indent + "\treturn ret;");
				sw.WriteLine(indent + "}");
			}

			if (has_setter) {
				sw.Write(indent + "set ");
				setter.GenerateBody(gen_info, "\t");
				sw.WriteLine();
			} else if (elem.HasAttribute("writeable") && !elem.HasAttribute("construct-only")) {
				sw.WriteLine(indent + "set {");
				sw.Write(indent + "\tGLib.Value val = ");
				if (table.IsEnum(c_type)) {
					sw.WriteLine("new GLib.Value(new GLib.EnumWrapper ((int) value, {0}), \"{1}\");", table.IsEnumFlags (c_type) ? "true" : "false", c_type);
				} else if (table.IsBoxed (c_type)) {
					sw.WriteLine("(GLib.Value) value;");
				} else if (table.IsOpaque (c_type)) {
					sw.WriteLine("new GLib.Value(value, \"{0}\");", c_type);
				} else {
					sw.Write("new GLib.Value(");
					if (v_type != "" && !(table.IsObject (c_type) || table.IsOpaque (c_type))) {
						sw.Write(v_type + " ");
					}
					sw.WriteLine("value);");
				}
				sw.WriteLine(indent + "\t" + RawSetter (qpname) + ";");
				sw.WriteLine(indent + "\tval.Dispose ();");
				sw.WriteLine(indent + "}");
			}

			sw.WriteLine(indent.Substring (1) + "}");
			sw.WriteLine();

			Statistics.PropCount++;
		}
	}
}


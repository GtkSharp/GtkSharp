// GtkSharp.Generation.Property.cs - The Property Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class Property {

		private XmlElement elem;

		public Property (XmlElement elem)
		{
			this.elem = elem;
		}

		public bool Validate ()
		{
			string c_type = elem.GetAttribute("type");
			string cs_type = SymbolTable.GetCSType(c_type);

			if (cs_type == "") {
				Console.Write("Property has unknown Type {0} ", c_type);
				Statistics.ThrottledCount++;
				return false;
			}

			if (SymbolTable.IsInterface(c_type)) {
				// FIXME: Handle interface props properly.
				Console.Write("Interface property detected ");
				Statistics.ThrottledCount++;
				return false;
			}

			return true;
		}

		public void Generate (StreamWriter sw)
		{
			string c_type = elem.GetAttribute("type");
			string cs_type = SymbolTable.GetCSType(c_type);

			XmlElement parent = (XmlElement) elem.ParentNode;
			string name = elem.GetAttribute("name");
			if (name == parent.GetAttribute("name")) {
				name += "Prop";
			}
			string cname = "\"" + elem.GetAttribute("cname") + "\"";

			string v_type = "";
			if (SymbolTable.IsEnum(c_type)) {
				v_type = "int";
			} else if (SymbolTable.IsInterface(c_type)) {
				// FIXME: Handle interface props properly.
				Console.Write("Interface property detected ");
				Statistics.ThrottledCount++;
				return;
			} else if (SymbolTable.IsObject(c_type)) {
				v_type = "GLib.Object";
			} else if (SymbolTable.IsBoxed (c_type)) {
				v_type = "GLib.Boxed";
			}

			if (elem.HasAttribute("construct-only") && !elem.HasAttribute("readable")) {
				return;
			}

			sw.WriteLine("\t\tpublic " + cs_type + " " + name + " {");
			if (elem.HasAttribute("readable")) {
				sw.WriteLine("\t\t\tget {");
				sw.WriteLine("\t\t\t\tGLib.Value val = new GLib.Value (Handle, " + cname + ");");
				sw.WriteLine("\t\t\t\tGetProperty(" + cname + ", val);");
				sw.Write("\t\t\t\treturn (" + cs_type + ") ");
				if (v_type != "") {
					sw.Write("(" + v_type + ") ");
				}
				sw.WriteLine("val;");
				sw.WriteLine("\t\t\t}");
			}

			if (elem.HasAttribute("writeable") && !elem.HasAttribute("construct-only")) {
				sw.WriteLine("\t\t\tset {");
				sw.Write("\t\t\t\tSetProperty(" + cname + ", new GLib.Value(");
				if (v_type != "") {
					sw.Write("(" + v_type + ") ");
				}
				sw.WriteLine("value));");
				sw.WriteLine("\t\t\t}");
			}

			sw.WriteLine("\t\t}");
			sw.WriteLine();

			Statistics.PropCount++;
		}
	}
}


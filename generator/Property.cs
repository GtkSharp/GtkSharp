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
		private ClassBase container_type;

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

		public void Generate (StreamWriter sw)
		{
			SymbolTable table = SymbolTable.Table;

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
			string cname = "\"" + elem.GetAttribute("cname") + "\"";

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
			Method getter = container_type.GetMethod("Get" + Name);
			Method setter = container_type.GetMethod("Set" + Name);

			if (getter != null && getter.Validate() && getter.IsGetter && getter.ReturnType == cs_type) {
				has_getter = true;
				getter.GenerateImport(sw);
			}
			if (setter != null && setter.Validate() && setter.IsSetter) {
				has_setter = true;
				setter.GenerateImport(sw);
			}

			if (has_setter && setter.Params[0].CSType != cs_type)
				cs_type = setter.Params[0].CSType;
			else if (has_getter && getter.ReturnType != cs_type)
				cs_type = getter.ReturnType;

			sw.WriteLine();
			sw.WriteLine("\t\t/// <summary> " + name + " Property </summary>");
			sw.WriteLine("\t\t/// <remarks>");
			sw.WriteLine("\t\t/// </remarks>");

			sw.WriteLine("\t\tpublic " + modifiers + cs_type + " " + name + " {");
			if (has_getter) {
				sw.Write("\t\t\tget ");
				getter.GenerateBody(sw, "\t");
				sw.WriteLine();
			} else if (elem.HasAttribute("readable")) {
				sw.WriteLine("\t\t\tget {");
				sw.WriteLine("\t\t\t\tGLib.Value val = new GLib.Value (Handle, " + cname + ");");
				sw.WriteLine("\t\t\t\tGetProperty(" + cname + ", val);");
				if (table.IsObject (c_type) || table.IsOpaque (c_type) || table.IsBoxed (c_type)) {
					sw.WriteLine("\t\t\t\tSystem.IntPtr raw_ret = (System.IntPtr) {0} val;", v_type);
					sw.WriteLine("\t\t\t\t" + cs_type + " ret = " + table.FromNativeReturn(c_type, "raw_ret") + ";");
					if (!table.IsBoxed (c_type))
						sw.WriteLine("\t\t\t\tif (ret == null) ret = new " + cs_type + "(raw_ret);");
				} else {
					sw.Write("\t\t\t\t" + cs_type + " ret = ");
					sw.Write ("(" + cs_type + ") ");
					if (v_type != "") {
						sw.Write(v_type + " ");
					}
					sw.WriteLine("val;");
				}

				sw.WriteLine("\t\t\t\treturn ret;");
				sw.WriteLine("\t\t\t}");
			}

			if (has_setter) {
				sw.Write("\t\t\tset ");
				setter.GenerateBody(sw, "\t");
				sw.WriteLine();
			} else if (elem.HasAttribute("writeable") && !elem.HasAttribute("construct-only")) {
				sw.WriteLine("\t\t\tset {");
				sw.Write("\t\t\t\tSetProperty(" + cname + ", new GLib.Value(");
				if (table.IsEnum(c_type)) {
					sw.WriteLine("Handle, " + cname + ", new GLib.EnumWrapper ((int) value, {0})));", table.IsEnumFlags (c_type) ? "true" : "false");
				} else if (table.IsBoxed (c_type)) {
					sw.WriteLine("Handle, " + cname + ", new GLib.Boxed (value)));");
				} else {
					if (v_type != "" && !(table.IsObject (c_type) || table.IsOpaque (c_type))) {
						sw.Write(v_type + " ");
					}
					sw.WriteLine("value));");
				}
				sw.WriteLine("\t\t\t}");
			}

			sw.WriteLine("\t\t}");
			sw.WriteLine();

			Statistics.PropCount++;
		}
	}
}


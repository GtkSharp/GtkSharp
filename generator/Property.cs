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

		public void Generate (GenerationInfo gen_info)
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

			sw.WriteLine("\t\tpublic " + modifiers + cs_type + " " + name + " {");
			if (has_getter) {
				sw.Write("\t\t\tget ");
				getter.GenerateBody(gen_info, "\t");
				sw.WriteLine();
			} else if (elem.HasAttribute("readable")) {
				sw.WriteLine("\t\t\tget {");
				sw.WriteLine("\t\t\t\tGLib.Value val = GetProperty (" + cname + ");");
				if (table.IsObject (c_type)) {
					sw.WriteLine("\t\t\t\tSystem.IntPtr raw_ret = (System.IntPtr) {0} val;", v_type);
					sw.WriteLine("\t\t\t\t" + cs_type + " ret = " + table.FromNativeReturn(c_type, "raw_ret") + ";");
					if (!table.IsBoxed (c_type) && !table.IsObject (c_type))
						sw.WriteLine("\t\t\t\tif (ret == null) ret = new " + cs_type + "(raw_ret);");
				} else if (table.IsOpaque (c_type) || table.IsBoxed (c_type)) {
					sw.WriteLine("\t\t\t\t" + cs_type + " ret = (" + cs_type + ") val;");
				} else {
					sw.Write("\t\t\t\t" + cs_type + " ret = ");
					sw.Write ("(" + cs_type + ") ");
					if (v_type != "") {
						sw.Write(v_type + " ");
					}
					sw.WriteLine("val;");
				}

				sw.WriteLine("\t\t\t\tval.Dispose ();");
				sw.WriteLine("\t\t\t\treturn ret;");
				sw.WriteLine("\t\t\t}");
			}

			if (has_setter) {
				sw.Write("\t\t\tset ");
				setter.GenerateBody(gen_info, "\t");
				sw.WriteLine();
			} else if (elem.HasAttribute("writeable") && !elem.HasAttribute("construct-only")) {
				sw.WriteLine("\t\t\tset {");
				sw.Write("\t\t\t\tGLib.Value val = ");
				if (table.IsEnum(c_type)) {
					sw.WriteLine("new GLib.Value(Handle, " + cname + ", new GLib.EnumWrapper ((int) value, {0}));", table.IsEnumFlags (c_type) ? "true" : "false");
				} else if (table.IsBoxed (c_type)) {
					sw.WriteLine("(GLib.Value) value;");
				} else if (table.IsOpaque (c_type)) {
					sw.WriteLine("new GLib.Value(Handle, " + cname + ", value);");
				} else {
					sw.Write("new GLib.Value(");
					if (v_type != "" && !(table.IsObject (c_type) || table.IsOpaque (c_type))) {
						sw.Write(v_type + " ");
					}
					sw.WriteLine("value);");
				}
				sw.WriteLine("\t\t\t\tSetProperty(" + cname + ", val);");
				sw.WriteLine("\t\t\t\tval.Dispose ();");
				sw.WriteLine("\t\t\t}");
			}

			sw.WriteLine("\t\t}");
			sw.WriteLine();

			Statistics.PropCount++;
		}
	}
}


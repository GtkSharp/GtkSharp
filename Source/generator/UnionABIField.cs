namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;
	using System.Collections.Generic;

	public class UnionSubstruct {
		List<StructABIField> fields;
		XmlElement Elem;
		bool is_valid;
		bool unique_field;

		public UnionSubstruct(XmlElement elem, ClassBase container_type) {
			fields = new List<StructABIField> ();
			Elem = elem;
			is_valid = true;
			unique_field = false;

			if (Elem.Name == "struct") {
				foreach (XmlElement child_field in elem.ChildNodes) {
					if (child_field.Name != "field") {
						is_valid = false;
						continue;
					}

					fields.Add(new StructABIField (child_field, container_type));
				}
			} else if (Elem.Name == "field") {
				fields.Add(new StructABIField (Elem, container_type));
				unique_field = true;
			}
		}

		public void Generate(GenerationInfo gen_info, string indent){
			StreamWriter sw = gen_info.Writer;
			var name = Elem.GetAttribute("name");
			var cname = Elem.GetAttribute("cname");

			if (unique_field) {
				sw.WriteLine (indent + "[FieldOffset(0)]");
				foreach (StructABIField field in fields)
					field.Generate(gen_info, indent);

				return;
			}

			sw.WriteLine (indent + "struct __" + name + "{");
			foreach (StructABIField field in fields) {
				field.Generate(gen_info, indent + "\t");
			}
			sw.WriteLine (indent + "}");
			sw.WriteLine (indent + "[FieldOffset(0)]");
			sw.WriteLine (indent + "private __" + name + " " + cname + ";");
		}
	}

	public class UnionABIField : StructABIField {
		bool is_valid;
		XmlElement Elem;
		protected List<UnionSubstruct> substructs = new List<UnionSubstruct> ();


		public UnionABIField (XmlElement elem, ClassBase container_type) : base (elem, container_type) {
			Elem = elem;
			is_valid = true;
			foreach (XmlElement union_child in elem.ChildNodes) {
				substructs.Add(new UnionSubstruct(union_child, container_type));
			}
		}

		public override void Generate (GenerationInfo gen_info, string indent) {
			StreamWriter sw = gen_info.Writer;
			var name = Elem.GetAttribute("name");
			var cname = Elem.GetAttribute("cname");

			sw.WriteLine (indent + "[StructLayout(LayoutKind.Explicit)]");
			sw.WriteLine (indent + "struct __" + name + " {");
			foreach (UnionSubstruct _struct in substructs) {
				_struct.Generate(gen_info, indent + "\t");
			}
			sw.WriteLine (indent + "}");
			sw.WriteLine (indent + "private __" + name + " " + cname + ";");
		}

		public override bool Validate (LogWriter log)
		{

			if (!is_valid) {
				log.Warn("Can't generate ABI compatible union");
			}
			return is_valid;
		}
	}
}

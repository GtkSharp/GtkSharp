namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;
	using System.Collections.Generic;

	public class MethodABIField : StructABIField {
		bool is_valid;
		XmlElement Elem;


		public MethodABIField (XmlElement elem, ClassBase container_type, string info_name) :
				base (elem, container_type, info_name) {
			Elem = elem;
			is_valid = true;
		}

		public override string CType {
			get {
				return "gpointer";
			}
		}

		public override bool IsCPointer() {
			return true;
		}

		public new string Name {
			get {
				var name = elem.GetAttribute("vm");
				if (name == null || name == "")
					name = elem.GetAttribute("signal_vm");

				return name;
			}
		}

		public override string StudlyName {
			get {
				return Name;
			}
		}

		public override string CName {

			get {
				if (parent_structure_name != null)
					return parent_structure_name + '.' + Name;
				return Name;
			}
		}
	}
}


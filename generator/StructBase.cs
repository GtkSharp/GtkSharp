// GtkSharp.Generation.StructBase.cs - The Structure/Object Base Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class StructBase  {
		
		protected String ns;
		protected XmlElement elem;
		
		public StructBase (String ns, XmlElement elem) {
			
			this.ns = ns;
			this.elem = elem;
		}
		
		protected bool GenField (XmlElement field, SymbolTable table, StreamWriter sw)
		{
			String c_type;
			
			if (field.HasAttribute("bits") && (field.GetAttribute("bits") == "1")) {
				c_type = "gboolean";
			} else {
				c_type = field.GetAttribute("type");
			}
			char[] ast = {'*'};
			c_type = c_type.TrimEnd(ast);
			String cs_type = table.GetCSType(c_type);
			
			if (cs_type == "") {
				Console.WriteLine ("Field has unknown Type {0}", c_type);
				return false;
			}
			
			sw.Write ("\t\t public " + cs_type);
			if (field.HasAttribute("array_len")) {
				sw.Write ("[]");
			}
			sw.WriteLine (" " + field.GetAttribute("cname") + ";");
			return true;
		}
		
	}
}


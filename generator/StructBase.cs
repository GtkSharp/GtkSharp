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
		
		protected void GenField (XmlElement field, SymbolTable table, StreamWriter sw)
		{
				String c_type = field.GetAttribute("type");
				sw.Write ("\t\t" + table.GetCSType(c_type));
				if (field.HasAttribute("array_len")) {
					sw.Write ("[]");
				}
				
				sw.WriteLine (" " + field.GetAttribute("cname") + ";");
		}
		
	}
}


// GtkSharp.Generation.EnumGen.cs - The Enumeration Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class EnumGen : GenBase, IGeneratable  {
		
		public EnumGen (XmlElement ns, XmlElement elem) : base (ns, elem) {}
		
		public String MarshalType {
			get
			{
				return "int";
			}
		}
		
		public String CallByName (String var_name)
		{
			return "(int) " + var_name;
		}
		
		public String FromNative(String var)
		{
			return "(" + QualifiedName + ")" + var;
		}
		
		public void Generate ()
		{
			StreamWriter sw = CreateWriter ();

			if (Elem.GetAttribute("type") == "flags") {
				sw.WriteLine ("\tusing System;");
				sw.WriteLine ();
				sw.WriteLine ("\t[Flags]");
			}
			
			sw.WriteLine ("\tpublic enum " + Name + " {");
			sw.WriteLine ();
				
			foreach (XmlNode node in Elem.ChildNodes) {
				if (node.Name != "member") {
					continue;
				}
				
				XmlElement member = (XmlElement) node;
				sw.Write ("\t\t" + member.GetAttribute("name"));
				if (member.HasAttribute("value")) {
					sw.WriteLine (" = " + member.GetAttribute("value") + ",");
				} else {
					sw.WriteLine (",");
				}
			}
				
			sw.WriteLine ("\t}");
			CloseWriter (sw);
			Statistics.EnumCount++;
		}
		
	}
}


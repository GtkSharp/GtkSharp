// GtkSharp.Generation.EnumGen.cs - The Enumeration Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class EnumGen : IGeneratable  {
		
		private String ns;
		private XmlElement elem;
		
		public EnumGen (String ns, XmlElement elem) {
			
			this.ns = ns;
			this.elem = elem;
		}
		
		public String Name {
			get
			{
				return elem.GetAttribute("name");
			}
		}
		
		public String CName {
			get
			{
				return elem.GetAttribute("cname");
			}
		}
		
		public String QualifiedName {
			get
			{
				return ns + "." + elem.GetAttribute("name");
			}
		}
		
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
		
		public void Generate (SymbolTable table)
		{
			if (!Directory.Exists("..\\" + ns.ToLower() + "\\generated")) {
				Directory.CreateDirectory("..\\"+ns.ToLower()+"\\generated");
			}
			String filename = "..\\" + ns.ToLower() + "\\generated\\" + Name + ".cs";
			
			FileStream stream = new FileStream (filename, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter (stream);
			
			sw.WriteLine ("// Generated File.  Do not modify.");
			sw.WriteLine ("// <c> 2001 Mike Kestner");
			sw.WriteLine ();
			
			sw.WriteLine ("namespace " + ns + " {");
			sw.WriteLine ();
				
			if (elem.GetAttribute("type") == "flags") {
				sw.WriteLine ("\tusing System;");
				sw.WriteLine ();
				sw.WriteLine ("\t[Flags]");
			}
			
			sw.WriteLine ("\tpublic enum " + Name + " {");
			sw.WriteLine ();
				
			foreach (XmlNode node in elem.ChildNodes) {
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
			sw.WriteLine ();
			sw.WriteLine ("}");
			
			sw.Flush();
			sw.Close();
		}
		
	}
}


// GtkSharp.Generation.ObjectGen.cs - The Object Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class ObjectGen : StructBase, IGeneratable  {
		
		public ObjectGen (String ns, XmlElement elem) : base (ns, elem) {}
		
		public String Name {
			get
			{
				return elem.GetAttribute("name");
			}
		}
		
		public String QualifiedName {
			get
			{
				return ns + "." + elem.GetAttribute("name");
			}
		}
		
		public String CName {
			get
			{
				return elem.GetAttribute("cname");
			}
		}
		
		public String MarshalType {
			get
			{
				return "IntPtr";
			}
		}
		
		public String CallByName (String var_name)
		{
			return var_name + ".RawObject";
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
				
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Collections;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();

			String parent = elem.GetAttribute("parent");
			String cs_parent = table.GetCSType(parent);
			sw.Write ("\tpublic class " + Name);
			if (cs_parent == "") {
				sw.WriteLine (" {");
				Console.WriteLine ("Object " + Name + " Unknown parent " + parent);
			} else {
				sw.WriteLine (" : " + cs_parent + " {");
			}
			sw.WriteLine ();
				
			foreach (XmlNode node in elem.ChildNodes) {
				
				XmlElement member = (XmlElement) node;

				switch (node.Name) {
				case "field":
					//if (!GenField(member, table, sw)) {
					//	Console.WriteLine("in object " + CName);
					//}
					break;
					
				case "callback":
					break;
					
				case "constructor":
					break;
					
				case "method":
					break;
					
				case "property":
					break;
					
				case "signal":
					break;
					
				default:
					Console.WriteLine ("Unexpected node");
					break;
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


// GtkSharp.Generation.StructGen.cs - The Structure Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class StructGen : StructBase, IGeneratable  {
		
		public StructGen (String ns, XmlElement elem) : base (ns, elem) {}
		
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
			return var_name;
		}
		
		public void Generate (SymbolTable table)
		{
			String filename = "..\\" + ns.ToLower() + "\\generated\\" + Name + ".cs";
			
			FileStream stream = new FileStream (filename, FileMode.OpenOrCreate, FileAccess.Write);
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
			
			sw.WriteLine ("\t[StructLayout(LayoutKind.Sequential)]");
			sw.WriteLine ("\tpublic class " + Name + " {");
			sw.WriteLine ();
				
			foreach (XmlNode node in elem.ChildNodes) {
				
				XmlElement member = (XmlElement) node;

				switch (node.Name) {
				case "field":
					break;
					
				case "callback":
					break;
					
				case "constructor":
					break;
					
				case "method":
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


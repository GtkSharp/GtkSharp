// GtkSharp.Generation.BoxedGen.cs - The Boxed Type Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class BoxedGen : StructBase, IGeneratable  {
		
		public BoxedGen (String ns, XmlElement elem) : base (ns, elem) {}
		
		public String MarshalType {
			get
			{
				return "IntPtr";
			}
		}
		
		public String CallByName (String var_name)
		{
			return var_name + ".Raw";
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
			sw.WriteLine ("// <c> 2001-2002 Mike Kestner");
			sw.WriteLine ();
			
			sw.WriteLine ("namespace " + ns + " {");
			sw.WriteLine ();
				
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Collections;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();
			
			sw.WriteLine ("\t[StructLayout(LayoutKind.Sequential)]");
			sw.WriteLine ("\tpublic class " + Name + " : GtkSharp.Boxed {");
			sw.WriteLine ();
				
			foreach (XmlNode node in elem.ChildNodes) {
				
				XmlElement member = (XmlElement) node;

				switch (node.Name) {
				case "field":
					// GenField(member, table, sw);
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


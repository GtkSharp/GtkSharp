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
		
		public String MarshalType {
			get
			{
				return QualifiedName;
			}
		}
		
		public String CallByName (String var_name)
		{
			return var_name;
		}
		
		public String FromNative(String var)
		{
			return var;
		}

		public void Generate (SymbolTable table)
		{
			char sep = Path.DirectorySeparatorChar;
			string dir = ".." + sep + ns.ToLower() + sep + "generated";
			if (!Directory.Exists(dir)) {
				Directory.CreateDirectory(dir);
			}
			String filename = dir + sep + Name + ".cs";
			
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
			
			sw.WriteLine ("\t[StructLayout(LayoutKind.Sequential)]");
			sw.WriteLine ("\tpublic class " + Name + " {");
			sw.WriteLine ();
				
			foreach (XmlNode node in elem.ChildNodes) {
				
				XmlElement member = (XmlElement) node;

				switch (node.Name) {
				case "field":
					Statistics.IgnoreCount++;
					// GenField(member, table, sw);
					break;
					
				case "callback":
					Statistics.IgnoreCount++;
					break;
					
				case "constructor":
					Statistics.IgnoreCount++;
					break;
					
				case "method":
					Statistics.IgnoreCount++;
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
			Statistics.StructCount++;
		}		
	}
}


// GtkSharp.Generation.InterfaceGen.cs - The Interface Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class InterfaceGen : IGeneratable  {
		
		private String ns;
		private XmlElement elem;
		
		public InterfaceGen (String ns, XmlElement elem) {
			
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
				return "";
			}
		}
		
		public String CallByName (String var_name)
		{
			return "";
		}

		public String FromNative(String var)
		{
			return "";
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
			sw.WriteLine ();
			
			sw.WriteLine ("\tpublic interface " + Name + " {");
			sw.WriteLine ();
			
			foreach (XmlNode node in elem.ChildNodes) {
				if (node.Name != "member") {
					continue;
				}
				//FIXME: Generate the methods.
				XmlElement member = (XmlElement) node;
			}
			
			sw.WriteLine ("\t}");
			sw.WriteLine ();
			sw.WriteLine ("}");
			
			sw.Flush();
			sw.Close();
			Statistics.IFaceCount++;
		}
		
	}
}


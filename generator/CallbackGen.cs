// GtkSharp.Generation.CallbackGen.cs - The Callback Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class CallbackGen : IGeneratable  {
		
		private String ns;
		private XmlElement elem;
		
		public CallbackGen (String ns, XmlElement elem) {
			
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
			XmlElement ret_elem = elem["return-type"];
			if (ret_elem == null) {
				Console.WriteLine("Missing return type in callback " + CName);
				Statistics.ThrottledCount++;
				return;
			}
			
			string rettype = ret_elem.GetAttribute("type");
			string s_ret = table.GetCSType(rettype);
			if (s_ret == "") {
				Console.WriteLine("rettype: " + rettype + " in callback " + CName);
				Statistics.ThrottledCount++;
				return;
			}
			
			string parmstr = "";
			XmlNode params_elem = elem["parameters"];
			if (params_elem != null) {
				
				bool need_comma = false;
				foreach (XmlNode node in params_elem.ChildNodes) {
					if (node.Name != "parameter") {
						continue;
					}
					
					XmlElement param = (XmlElement) node;
					string type = param.GetAttribute("type");
					string cs_type = table.GetCSType(type);
					string name = param.GetAttribute("name");
					name = MangleName(name);

					if ((cs_type == "") || (name == "")) {
						Console.WriteLine("parmtype: " + type + " in callback " + CName);
						Statistics.ThrottledCount++;
						break;
					}

					if (need_comma)
						parmstr += ", ";

					parmstr += (cs_type + " " + name);
					need_comma = true;
				}
			}
				
			char sep = Path.DirectorySeparatorChar;
			string dir = ".." + sep + ns.ToLower() + sep + "generated";
			if (!Directory.Exists(dir)) {
				Directory.CreateDirectory(dir);
			}
			String filename = dir + sep + Name + ".cs";
			
			FileStream stream = new FileStream (filename, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter (stream);
			
			sw.WriteLine ("// Generated File.  Do not modify.");
			sw.WriteLine ("// <c> 2001-2002 Mike Kestner");
			sw.WriteLine ();
			
			sw.WriteLine ("namespace " + ns + " {");
			sw.WriteLine ();
				
			sw.WriteLine ("\tpublic delegate " + s_ret + " " + Name + "(" + parmstr + ");");

			sw.WriteLine ();
			sw.WriteLine ("}");
			
			sw.Flush();
			sw.Close();
			Statistics.CBCount++;
		}
		

		public string MangleName(string name)
		{
			switch (name) {
			case "string":
				name = "str1ng";
				break;
			case "event":
				name = "evnt";
				break;
			case "object":
				name = "objekt";
				break;
			default:
				break;
			}
			return name;
		}
	}
}


// GtkSharp.Generation.Parser.cs - The XML Parsing engine.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.Xml;

	public class Parser  {
		
		private XmlDocument doc;
		
		public Parser (String filename)
		{
			doc = new XmlDocument ();

			try {

				doc.Load (filename);

			} catch (XmlException e) {

				Console.WriteLine ("Invalid XML file.");
				Console.WriteLine (e.ToString());
			}

		}

		public void Parse ()
		{
			XmlElement root = doc.DocumentElement;

			if ((root == null) || !root.HasChildNodes) {
					Console.WriteLine ("No Namespaces found.");
					return;
			}

			foreach (XmlNode ns in root.ChildNodes) {
				if (ns.Name != "namespace") {
					continue;
				}

				XmlElement elem = (XmlElement) ns;
				ParseNamespace (elem);
			}
		}

		private void ParseNamespace (XmlElement ns)
		{
			String ns_name = ns.GetAttribute ("name");

			foreach (XmlNode def in ns.ChildNodes) {

				if (def.NodeType != XmlNodeType.Element) {
					continue;
				}

				XmlElement elem = (XmlElement) def;
				
				switch (def.Name) {

				case "alias":
					string aname = elem.GetAttribute("cname");
					string atype = elem.GetAttribute("type");
					if ((aname == "") || (atype == ""))
						continue;
					SymbolTable.AddAlias (aname, atype);
					break;
					
				case "boxed":
					SymbolTable.AddType (new BoxedGen (ns_name, elem));
					break;

				case "callback":
					SymbolTable.AddType (new CallbackGen (ns_name, elem));
					break;

				case "enum":
					SymbolTable.AddType (new EnumGen (ns_name, elem));
					break;

				case "interface":
					SymbolTable.AddType (new InterfaceGen (ns_name, elem));
					break;

				case "object":
					SymbolTable.AddType (new ObjectGen (ns_name, elem));
					break;

				case "struct":
					SymbolTable.AddType (new StructGen (ns_name, elem));
					break;

				default:
					Console.WriteLine ("Unexpected node named " + def.Name);
					break;
				}
			}
		}
	}
}

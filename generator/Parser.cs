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
		private SymbolTable table;
		
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

		public SymbolTable Parse ()
		{
			if (table != null) return table;
				
			XmlElement root = doc.DocumentElement;

			if ((root == null) || !root.HasChildNodes) {
					Console.WriteLine ("No Namespaces found.");
					return null;
			}

			table = new SymbolTable ();
			
			foreach (XmlNode ns in root.ChildNodes) {
				if (ns.Name != "namespace") {
					continue;
				}

				XmlElement elem = (XmlElement) ns;
				ParseNamespace (elem);
			}

			return table;
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
					break;

				case "callback":
					break;

				case "enum":
					table.AddType (new EnumGen (ns_name, elem));
					break;

				case "interface":
					break;

				case "object":
					table.AddType (new ObjectGen (ns_name, elem));
					break;

				case "struct":
					table.AddType (new StructGen (ns_name, elem));
					break;

				default:
					Console.WriteLine ("Unexpected node.");
					break;
				}
			}
		}
	}
}

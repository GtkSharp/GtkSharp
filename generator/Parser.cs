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
				if (!(ns is XmlElement) || ns.Name != "namespace") {
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
					if (elem.HasAttribute ("opaque"))
						SymbolTable.AddType (new OpaqueGen (ns, elem));
					else
						SymbolTable.AddType (new BoxedGen (ns, elem));
					break;

				case "callback":
					SymbolTable.AddType (new CallbackGen (ns, elem));
					break;

				case "enum":
					SymbolTable.AddType (new EnumGen (ns, elem));
					break;

				case "interface":
					SymbolTable.AddType (new InterfaceGen (ns, elem));
					break;

				case "object":
					SymbolTable.AddType (new ObjectGen (ns, elem));
					break;

				case "struct":
					if (elem.HasAttribute ("opaque"))
						SymbolTable.AddType (new OpaqueGen (ns, elem));
					else
						SymbolTable.AddType (new StructGen (ns, elem));
					break;

				default:
					Console.WriteLine ("Unexpected node named " + def.Name);
					break;
				}
			}
		}
	}
}

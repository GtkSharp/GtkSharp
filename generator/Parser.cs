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

		public void Parse (bool generate)
		{
			XmlElement root = doc.DocumentElement;

			if ((root == null) || !root.HasChildNodes) {
					Console.WriteLine ("No Namespaces found.");
					return;
			}

			foreach (XmlNode ns in root.ChildNodes) {
				XmlElement elem = ns as XmlElement;
				if (elem == null)
					continue;

				if (ns.Name == "namespace")
					ParseNamespace (elem, generate);
				else if (ns.Name == "symbol")
					ParseSymbol (elem);
			}
		}

		private void ParseNamespace (XmlElement ns, bool generate)
		{
			String ns_name = ns.GetAttribute ("name");

			foreach (XmlNode def in ns.ChildNodes) {

				if (def.NodeType != XmlNodeType.Element) {
					continue;
				}

				XmlElement elem = (XmlElement) def;
				IGeneratable igen = null;
				
				if (elem.HasAttribute("hidden"))
					continue;

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
						igen = new OpaqueGen (ns, elem);
					else
						igen = new BoxedGen (ns, elem);
					break;

				case "callback":
					igen = new CallbackGen (ns, elem);
					break;

				case "enum":
					igen = new EnumGen (ns, elem);
					break;

				case "interface":
					igen = new InterfaceGen (ns, elem);
					break;

				case "object":
					igen = new ObjectGen (ns, elem);
					break;

				case "struct":
					if (elem.HasAttribute ("opaque"))
						igen = new OpaqueGen (ns, elem);
					else
						igen = new StructGen (ns, elem);
					break;

				default:
					Console.WriteLine ("Unexpected node named " + def.Name);
					break;
				}

				if (igen != null) {
					igen.DoGenerate = generate;
					SymbolTable.AddType (igen);
				}
			}
		}

		private void ParseSymbol (XmlElement symbol)
		{
			string type = symbol.GetAttribute ("type");
			string cname = symbol.GetAttribute ("cname");
			string name = symbol.GetAttribute ("name");

			if (type == "simple")
				SymbolTable.AddSimpleType (cname, name);
			else if (type == "manual")
				SymbolTable.AddManualType (cname, name);
			else
				Console.WriteLine ("Unexpected symbol type " + type);
		}
	}
}

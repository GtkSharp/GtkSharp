// GtkSharp.Parser.cs - The XML Parsing engine.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp {

	using System;
	using System.Collections;
	using System.Xml;

	public class Parser  {
		
		private XmlDocument doc;
		private Hashtable types;
		
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
		

		public Hashtable Types {
			get 
			{
				if (types != null) return types;
				
				XmlElement root = doc.DocumentElement;

				if ((root == null) || !root.HasChildNodes) {
					Console.WriteLine ("No Namespaces found.");
					return null;
				}

				types = new Hashtable ();
			
				foreach (XmlNode ns in root.ChildNodes) {
					if (ns.Name != "namespace") {
						continue;
					}

					XmlElement elem = (XmlElement) ns;
					ParseNamespace (elem);
				}

				return types;
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
					break;

				case "callback":
					break;

				case "enum":
					IGeneratable gen = new EnumGen (ns_name, elem);
					types [gen.CName] = gen;
					break;

				case "object":
					break;

				case "struct":
					break;

				default:
					Console.WriteLine ("Unexpected node.");
					break;
				}
			}

		}

	}
}

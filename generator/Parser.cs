// GtkSharp.Generation.Parser.cs - The XML Parsing engine.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2003 Mike Kestner and Ximian Inc.

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class Parser  {
		
		private XmlDocument Load (string filename)
		{
			XmlDocument doc = new XmlDocument ();

			try {
				Stream stream = File.OpenRead (filename);
				doc.Load (stream);
				stream.Close ();
			} catch (XmlException e) {
				Console.WriteLine ("Invalid XML file.");
				Console.WriteLine (e);
				doc = null;
			}

			return doc;
		}

		public IGeneratable[] Parse (string filename)
		{
			XmlDocument doc = Load (filename);
			if (doc == null)
				return null;

			XmlElement root = doc.DocumentElement;

			if ((root == null) || !root.HasChildNodes) {
				Console.WriteLine ("No Namespaces found.");
				return null;
			}

			ArrayList gens = new ArrayList ();

			foreach (XmlNode child in root.ChildNodes) {
				XmlElement elem = child as XmlElement;
				if (elem == null)
					continue;

				switch (child.Name) {
				case "namespace":
					gens.AddRange (ParseNamespace (elem));
					break;
				case "symbol":
					gens.Add (ParseSymbol (elem));
					break;
				default:
					Console.WriteLine ("Parser::Parse - Unexpected child node: " + child.Name);
					break;
				}
			}

			return (IGeneratable[]) gens.ToArray (typeof (IGeneratable));
		}

		private ArrayList ParseNamespace (XmlElement ns)
		{
			ArrayList result = new ArrayList ();

			foreach (XmlNode def in ns.ChildNodes) {

				XmlElement elem = def as XmlElement;
				if (elem == null)
					continue;

				if (elem.HasAttribute("hidden"))
					continue;

				bool is_opaque = false;
				if (elem.HasAttribute ("opaque"))
					is_opaque = true;

				switch (def.Name) {
				case "alias":
					string aname = elem.GetAttribute("cname");
					string atype = elem.GetAttribute("type");
					if ((aname == "") || (atype == ""))
						continue;
					result.Add (new AliasGen (aname, atype));
					break;
				case "boxed":
					result.Add (is_opaque ? new OpaqueGen (ns, elem) as object : new BoxedGen (ns, elem) as object);
					break;
				case "callback":
					result.Add (new CallbackGen (ns, elem));
					break;
				case "enum":
					result.Add (new EnumGen (ns, elem));
					break;
				case "interface":
					result.Add (new InterfaceGen (ns, elem));
					break;
				case "object":
					result.Add (new ObjectGen (ns, elem));
					break;
				case "class":
					result.Add (new ClassGen (ns, elem));
					break;
				case "struct":
					result.Add (is_opaque ? new OpaqueGen (ns, elem) as object : new StructGen (ns, elem) as object);
					break;
				default:
					Console.WriteLine ("Parser::ParseNamespace - Unexpected node: " + def.Name);
					break;
				}
			}

			return result;
		}

		private IGeneratable ParseSymbol (XmlElement symbol)
		{
			string type = symbol.GetAttribute ("type");
			string cname = symbol.GetAttribute ("cname");
			string name = symbol.GetAttribute ("name");
			IGeneratable result = null;

			if (type == "simple")
				result = new SimpleGen (cname, name);
			else if (type == "manual")
				result = new ManualGen (cname, name);
			else if (type == "alias")
				result = new AliasGen (cname, name);
			else
				Console.WriteLine ("Parser::ParseSymbol - Unexpected symbol type " + type);

			return result;
		}
	}
}

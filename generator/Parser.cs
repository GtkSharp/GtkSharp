// GtkSharp.Generation.Parser.cs - The XML Parsing engine.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
// Copyright (c) 2003 Ximian Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace GtkSharp.Generation {

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using System.Xml.Schema;

	public class Parser  {
		const int curr_parser_version = 3;

		private XmlDocument Load (string filename, string schema_file)
		{
			XmlDocument doc = new XmlDocument ();

			try {
				XmlReaderSettings settings = new XmlReaderSettings ();
				if (!String.IsNullOrEmpty (schema_file)) {
					settings.Schemas.Add (null, schema_file);
					settings.ValidationType = ValidationType.Schema;
					settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
					settings.ValidationEventHandler += ValidationEventHandler;
				}

				Stream stream = File.OpenRead (filename);
				XmlReader reader = XmlReader.Create (stream, settings);
				doc.Load (reader);

				stream.Close ();
			} catch (XmlException e) {
				Console.WriteLine ("Invalid XML file.");
				Console.WriteLine (e);
				doc = null;
			}

			return doc;
		}

		private void ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			switch (e.Severity)
			{
			case XmlSeverityType.Error:
				Console.WriteLine("Error: {0}", e.Message);
				break;
			case XmlSeverityType.Warning:
				Console.WriteLine("Warning: {0}", e.Message);
				break;
			}
		}

		public IGeneratable[] Parse (string filename)
		{
			return Parse (filename, null);
		}

		public IGeneratable[] Parse (string filename, string schema_file)
		{
			return Parse (filename, schema_file, String.Empty);
		}

		public IGeneratable[] Parse (string filename, string schema_file, string gapidir)
		{
			XmlDocument doc = Load (filename, schema_file);
			if (doc == null)
				return null;

			XmlElement root = doc.DocumentElement;

			if ((root == null) || !root.HasChildNodes) {
				Console.WriteLine ("No Namespaces found.");
				return null;
			}

			int parser_version;
			if (root.HasAttribute ("parser_version")) {
				try {
					parser_version = int.Parse (root.GetAttribute ("parser_version"));
				} catch {
					Console.WriteLine ("ERROR: Unable to parse parser_version attribute value \"{0}\" to a number. Input file {1} will be ignored", root.GetAttribute ("parser_version"), filename);
					return null;
				}
			} else
				parser_version = 1;

			if (parser_version > curr_parser_version)
				Console.WriteLine ("WARNING: The input file {0} was created by a parser that was released after this version of the generator. Consider updating the code generator if you experience problems.", filename);

			var gens = new List<IGeneratable> ();

			foreach (XmlNode child in root.ChildNodes) {
				XmlElement elem = child as XmlElement;
				if (elem == null)
					continue;

				switch (child.Name) {
				case "include":
					string xmlpath;

					if (File.Exists (Path.Combine (gapidir, elem.GetAttribute ("xml"))))
						xmlpath = Path.Combine (gapidir, elem.GetAttribute ("xml"));
					else if (File.Exists (elem.GetAttribute ("xml")))
					   xmlpath = elem.GetAttribute ("xml");
					else {
						Console.WriteLine ("Parser: Could not find include " + elem.GetAttribute ("xml"));
						break;
					}

					IGeneratable[] curr_gens = Parse (xmlpath);
					SymbolTable.Table.AddTypes (curr_gens);
					break;
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

			return gens.ToArray ();
		}

		private IList<IGeneratable> ParseNamespace (XmlElement ns)
		{
			var result = new List<IGeneratable> ();

			foreach (XmlNode def in ns.ChildNodes) {

				XmlElement elem = def as XmlElement;
				if (elem == null)
					continue;

				if (elem.GetAttributeAsBoolean ("hidden"))
					continue;

				bool is_opaque = elem.GetAttributeAsBoolean ("opaque");
				bool is_native_struct = elem.GetAttributeAsBoolean ("native");

				switch (def.Name) {
				case "alias":
					string aname = elem.GetAttribute("cname");
					string atype = elem.GetAttribute("type");
					if ((aname == "") || (atype == ""))
						continue;
					result.Add (new AliasGen (aname, atype));
					break;
				case "boxed":
					if (is_opaque) {
						result.Add (new OpaqueGen (ns, elem));
					} else {
						result.Add (new BoxedGen (ns, elem));
					}
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
				case "union":
					result.Add (new UnionGen (ns, elem));
					break;
				case "struct":
					if (is_opaque) {
						result.Add (new OpaqueGen (ns, elem));
					} else if (is_native_struct) {
						result.Add (new NativeStructGen (ns, elem));
					} else {
						result.Add (new StructGen (ns, elem));
					}
					break;
				default:
					Console.WriteLine ("Parser::ParseNamespace - Unexpected node: " + def.Name);
					break;
				}
			}

			return result;
		}

		internal static int GetVersion (XmlElement document_element)
		{
			XmlElement root = document_element;
			return root.HasAttribute ("parser_version") ? int.Parse (root.GetAttribute ("parser_version")) : 1;
		}

		private IGeneratable ParseSymbol (XmlElement symbol)
		{
			string type = symbol.GetAttribute ("type");
			string cname = symbol.GetAttribute ("cname");
			string name = symbol.GetAttribute ("name");
			IGeneratable result = null;

			if (type == "simple") {
				if (symbol.HasAttribute ("default_value"))
					result = new SimpleGen (cname, name, symbol.GetAttribute ("default_value"));
				else {
					Console.WriteLine ("Simple type element " + cname + " has no specified default value");
					result = new SimpleGen (cname, name, String.Empty);
				}
			} else if (type == "manual")
				result = new ManualGen (cname, name);
			else if (type == "ownable")
				result = new OwnableGen (cname, name);
			else if (type == "alias")
				result = new AliasGen (cname, name);
			else if (type == "marshal") {
				string mtype = symbol.GetAttribute ("marshal_type");
				string call = symbol.GetAttribute ("call_fmt");
				string from = symbol.GetAttribute ("from_fmt");
				result = new MarshalGen (cname, name, mtype, call, from);
			} else if (type == "struct") {
				result = new ByRefGen (symbol.GetAttribute ("cname"), symbol.GetAttribute ("name"));
			} else
				Console.WriteLine ("Parser::ParseSymbol - Unexpected symbol type " + type);

			return result;
		}
	}
}

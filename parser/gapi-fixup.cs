// GtkSharp.Parsing.gapi-fixup.cs - xml alteration engine.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2003 Mike Kestner

namespace GtkSharp.Parsing {

	using System;
	using System.IO;
	using System.Xml;
	using System.Xml.XPath;

	public class Fixup  {

		public static int Main (string[] args)
		{
			if (args.Length != 2) {
				Console.WriteLine ("Usage: gapi-fixup --metadata=<filename> --api=<filename>");
				return 0;
			}

			string api_filename = "";
			XmlDocument api_doc = new XmlDocument ();
			XmlDocument meta_doc = new XmlDocument ();

			foreach (string arg in args) {

				if (arg.StartsWith("--metadata=")) {

					string meta_filename = arg.Substring (11);

					try {
						Stream stream = File.OpenRead (meta_filename);
						meta_doc.Load (stream);
						stream.Close ();
					} catch (XmlException e) {
						Console.WriteLine ("Invalid meta file.");
						Console.WriteLine (e);
						return 1;
					}

				} else if (arg.StartsWith ("--api=")) {

					api_filename = arg.Substring (6);

					try {
						Stream stream = File.OpenRead (api_filename);
						api_doc.Load (stream);
						stream.Close ();
					} catch (XmlException e) {
						Console.WriteLine ("Invalid api file.");
						Console.WriteLine (e);
						return 1;
					}

				} else {
					Console.WriteLine ("Usage: gapi-fixup --metadata=<filename> --api=<filename>");
					return 1;
				}
			}

			XPathNavigator meta_nav = meta_doc.CreateNavigator ();
			XPathNavigator api_nav = api_doc.CreateNavigator ();

			XPathNodeIterator attr_iter = meta_nav.Select ("//attr");
			while (attr_iter.MoveNext ()) {
				string path = attr_iter.Current.GetAttribute ("path", "");
				string attr_name = attr_iter.Current.GetAttribute ("name", "");
				XPathNodeIterator api_iter = api_nav.Select (path);
				while (api_iter.MoveNext ()) {
					XmlElement node = ((IHasXmlNode)api_iter.Current).GetNode () as XmlElement;
					node.SetAttribute (attr_name, attr_iter.Current.Value);
				}
			}

			XPathNodeIterator move_iter = meta_nav.Select ("//move-node");
			while (move_iter.MoveNext ()) {
				string path = move_iter.Current.GetAttribute ("path", "");
				string parent = move_iter.Current.Value;
				XPathNodeIterator parent_iter = api_nav.Select (parent);
				while (parent_iter.MoveNext ()) {
					XmlNode parent_node = ((IHasXmlNode)parent_iter.Current).GetNode ();
					XPathNodeIterator path_iter = parent_iter.Current.Clone ().Select (path);
					while (path_iter.MoveNext ()) {
						XmlNode node = ((IHasXmlNode)path_iter.Current).GetNode ();
						parent_node.AppendChild (node.Clone ());
						node.ParentNode.RemoveChild (node);
					}
				}
			}

			api_doc.Save (api_filename);
			return 0;
		}
	}
}

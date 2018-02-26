// extract-missing.cs - grab missing api elements from api-diff files.
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2005 Mike Kestner
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

namespace GtkSharp.Auditing {

	using System;
	using System.IO;
	using System.Xml;
	using System.Xml.XPath;

	public class ExtractMissing  {

		public static int Main (string[] args)
		{
			if (args.Length != 1 || !File.Exists (args [0])) {
				Console.WriteLine ("Usage: extract-missing <filename>");
				return 0;
			}

			XmlDocument doc = new XmlDocument ();

			try {
				Stream stream = File.OpenRead (args [0]);
				doc.Load (stream);
				stream.Close ();
			} catch (XmlException e) {
				Console.WriteLine ("Invalid apidiff file.");
				Console.WriteLine (e);
				return 1;
			}

			XPathNavigator nav = doc.CreateNavigator ();

			XPathNodeIterator iter = nav.Select ("//*[@presence='missing']");
			while (iter.MoveNext ()) {
				XmlElement node = ((IHasXmlNode)iter.Current).GetNode () as XmlElement;
				if (node.Name == "class")
					Console.WriteLine ("Missing type: " + node.GetAttribute ("name"));
				else if (node.ParentNode.ParentNode.Name == "class")
					Console.WriteLine ("Missing " + node.Name + " " + (node.ParentNode.ParentNode as XmlElement).GetAttribute ("name") + "." + node.GetAttribute ("name"));
				else if (node.Name == "attribute") {
					if (node.ParentNode.ParentNode.Name == "class")
						Console.WriteLine ("Missing attribute (" + (node as XmlElement).GetAttribute ("name") + ") on type: " + (node.ParentNode.ParentNode as XmlElement).GetAttribute ("name"));
					else if (node.ParentNode.ParentNode.ParentNode.ParentNode.Name == "class")
						Console.WriteLine ("Missing attribute (" + (node as XmlElement).GetAttribute ("name") + ") on " + (node.ParentNode.ParentNode.ParentNode.ParentNode as XmlElement).GetAttribute ("name") + "." + (node.ParentNode.ParentNode as XmlElement).GetAttribute ("name"));
					else
						Console.WriteLine ("oopsie: " + node.Name + " " + node.ParentNode.ParentNode.Name);
				} else
					Console.WriteLine ("oopsie: " + node.Name + " " + node.ParentNode.ParentNode.Name);
			}

			return 0;
		}
	}
}

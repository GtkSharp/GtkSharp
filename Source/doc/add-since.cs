// add-since.cs - Adds a since element to a Type document.
//
// Author: Mike Kestner  <mkestner@novell.com>
//
// Copyright (c) 2007 Novell, Inc.
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

namespace GtkSharp.Docs {

	using System;
	using System.Collections;
	using System.IO;
	using System.Reflection;
	using System.Xml;
	using System.Xml.XPath;

	public class GenHandlerArgsDocs  {

		public static int Main (string[] args)
		{
			string version = null;
			ArrayList files = new ArrayList ();

			foreach (string arg in args) {
				if (arg.StartsWith ("--version=")) {
					version = arg.Substring (10);
				} else {
					files.Add (arg);
				}
			}

			if (version == null) {
				Console.WriteLine ("Usage: add-since --version=<version> <paths>");
				return 1;
			}

			Console.WriteLine ("version: " + version);
			XmlDocument api_doc = new XmlDocument ();

			foreach (string file in files) {
				Console.WriteLine ("file: " + file);
				try {
					Stream stream = File.OpenRead (file);
					api_doc.Load (stream);
					stream.Close ();
				} catch (XmlException e) {
					Console.WriteLine (e);
					return 1;
				}

				XPathNavigator api_nav = api_doc.CreateNavigator ();
				XPathNodeIterator iter = api_nav.Select ("/Type/Docs");
				if (iter.MoveNext ()) {
					XmlElement docs = ((IHasXmlNode)iter.Current).GetNode () as XmlElement;
					XmlElement since = api_doc.CreateElement ("since");
					since.SetAttribute ("version", version);
					docs.AppendChild (since);
					api_doc.Save (file);
				}
			}
			return 0;
		}
	}
}

//
// gen-apidiff-html.cs - Converts a set of apidiff files to HTML by
//                       merging them and applying an XSL file.
//
// Author:
//   Bertrand Lorentz (bertrand.lorentz@gmail.com)
//
// Copyright (C) 2013 Bertrand Lorentz
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace Mono.AssemblyCompare
{
	public class Driver
	{
		static int Main (string [] args)
		{
			if (args.Length != 2) {
				Console.WriteLine ("Usage: mono gen-apidiff-html.exe <diff_dir> <html_file>");
				return 1;
			}

			string diff_dir = args[0];
			string out_file = args[1];

			var all = new XmlDocument ();
			all.AppendChild(all.CreateElement ("assemblies"));
			foreach (string file in Directory.EnumerateFiles(diff_dir, "*.apidiff")) {
				Console.WriteLine ("Merging " + file);
				var doc = new XmlDocument ();
				doc.Load (file);
				foreach (XmlNode child in doc.GetElementsByTagName ("assembly")) {
					XmlNode imported = all.ImportNode (child, true);
					all.DocumentElement.AppendChild (imported);
				}
			}

			var transform = new XslCompiledTransform ();
			transform.Load ("mono-api.xsl");
			var writer = new StreamWriter (out_file);

			Console.WriteLine (String.Format ("Transforming to {0}...", out_file));
			transform.Transform (all.CreateNavigator (), null, writer);
			writer.Close ();

			return 0;
		}
	}
}


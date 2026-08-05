// Program.cs - gir-to-gapi driver.
//
// Converts a GObject-Introspection .gir into the gapi api.xml dialect that
// GapiFixup and GapiCodegen consume.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.

namespace GtkSharp.GirConversion {

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using GtkSharp.GirConversion.Emit;
	using GtkSharp.GirConversion.Gir;

	public class GirToGapi {

		public static int Main (string[] args)
		{
			var girPaths = new List<string> ();
			string outPath = null;
			string assemblyName = null;
			var includes = new List<string> ();

			foreach (var arg in args) {
				if (arg.StartsWith ("--gir="))
					girPaths.Add (arg.Substring ("--gir=".Length));
				else if (arg.StartsWith ("--out="))
					outPath = arg.Substring ("--out=".Length);
				else if (arg.StartsWith ("--assembly-name="))
					assemblyName = arg.Substring ("--assembly-name=".Length);
				else if (arg.StartsWith ("--include="))
					includes.Add (arg.Substring ("--include=".Length));
				else if (arg == "--help" || arg == "-h") {
					Usage ();
					return 0;
				} else {
					Console.WriteLine ("gir-to-gapi: unrecognised argument '{0}'", arg);
					Usage ();
					return 64;
				}
			}

			if (girPaths.Count == 0 || string.IsNullOrEmpty (outPath)) {
				Usage ();
				return 64;
			}

			foreach (var girPath in girPaths) {
				if (!File.Exists (girPath)) {
					Console.WriteLine ("gir-to-gapi: no such file: {0}", girPath);
					return 1;
				}
			}

			try {
				var docs = girPaths.Select (GirDocument.Load).ToList ();

				// Included .gir files are needed so that cross-namespace type
				// references resolve to a C type name. They are not emitted.
				var registry = new TypeRegistry ();

				foreach (var doc in docs)
					registry.Add (doc);

				foreach (var include in includes) {
					if (!File.Exists (include)) {
						Console.WriteLine ("gir-to-gapi: could not find include '{0}'", include);
						return 1;
					}
					registry.Add (GirDocument.Load (include));
				}

				var log = new ConversionLog ();
				var api = ApiWriter.Document (
					docs.Select (doc => new ApiWriter (doc, registry, log).Write ()));

				var directory = Path.GetDirectoryName (Path.GetFullPath (outPath));
				if (!string.IsNullOrEmpty (directory))
					Directory.CreateDirectory (directory);

				var settings = new XmlWriterSettings {
					Indent = true,
					IndentChars = "  ",
					Encoding = new UTF8Encoding (false),
					NewLineChars = "\n",
				};

				using (var writer = XmlWriter.Create (outPath, settings))
					api.Save (writer);

				log.Report ();

				Console.WriteLine ("gir-to-gapi: {0} -> {1}{2}",
					string.Join (" + ", girPaths.Select (Path.GetFileName)), outPath,
					assemblyName == null ? string.Empty : " (" + assemblyName + ")");

				return 0;
			} catch (GirException e) {
				Console.WriteLine ("gir-to-gapi: {0}", e.Message);
				return 1;
			}
		}

		static void Usage ()
		{
			Console.WriteLine ("Usage: gir-to-gapi --gir=<file.gir>... --out=<file-api.xml>");
			Console.WriteLine ("                   [--assembly-name=<name>] [--include=<file.gir>]...");
			Console.WriteLine ();
			Console.WriteLine ("  --gir=            GObject-Introspection file to convert. Repeatable:");
			Console.WriteLine ("                    each one becomes a <namespace> in the output, as");
			Console.WriteLine ("                    GdkSharp needs for Gdk plus GdkPixbuf.");
			Console.WriteLine ("  --out=            api.xml file to write.");
			Console.WriteLine ("  --assembly-name=  Assembly the api.xml belongs to; informational.");
			Console.WriteLine ("  --include=        Additional .gir consulted for type resolution");
			Console.WriteLine ("                    only. Repeatable, one per dependency.");
		}
	}
}

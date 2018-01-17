// GtkSharp.Generation.CodeGenerator.cs - The main code generation engine.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
// Copyright (c) 2003-2004 Novell Inc.
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

	public class CodeGenerator  {

		public static ulong Counter = 0;

		static LogWriter log = new LogWriter ("CodeGenerator");

		public static int Main (string[] args)
		{
			bool show_help = false;
			bool all_opaque = true;
			string dir = "";
			string assembly_name = "";
			string gapidir = "";
			string abi_cs_usings = "";
			string abi_cs_file = "";
			string abi_c_file = "";
			string glue_filename = "";
			string glue_includes = "";
			string gluelib_name = "";
			string schema_name = "";

			SymbolTable table = SymbolTable.Table;
			var gens = new List<IGeneratable> ();

			var filenames = new List<string> ();
			var includes = new List<string> ();

			var options = new OptionSet () {
				{ "generate=", "Generate the C# code for this GAPI XML file.",
					(string v) => { filenames.Add (v); } },
				{ "I|include=", "GAPI XML file that contain symbols used in the main GAPI XML file.",
					(string v) => { includes.Add (v); } },
				{ "outdir=", "Directory where the C# files will be generated.",
					(string v) => { dir = v; } },
				{ "assembly-name=", "Name of the assembly for which the code is generated.",
					(string v) => { assembly_name = v; } },
				{ "gapidir=", "GAPI xml data  folder.",
					(string v) => { gapidir = v; } },
				{ "abi-cs-filename=", "Filename for the generated CSharp ABI checker.",
					(string v) => { abi_cs_file = v; } },
				{ "abi-cs-usings=", "Namespaces to use in the CS ABI checker.",
					(string v) => { abi_cs_usings = v; } },
				{ "abi-c-filename=", "Filename for the generated C ABI checker.",
					(string v) => { abi_c_file = v; } },
				{ "glue-filename=", "Filename for the generated C glue code.",
					(string v) => { glue_filename = v; } },
				{ "glue-includes=", "Content of #include directive to add in the generated C glue code.",
					(string v) => { glue_includes = v; } },
				{ "gluelib-name=", "Name of the C library into which the C glue code will be compiled. " +
					"Used to generated correct DllImport attributes.",
					(string v) => { gluelib_name = v; } },
				{ "schema=", "Validate all GAPI XML files against this XSD schema.",
					(string v) => { schema_name  = v; } },
				{ "h|help",  "Show this message and exit",
					v => show_help = v != null },
			};

			List<string> extra;
			try {
				extra = options.Parse (args);
			}
			catch (OptionException e) {
				Console.Write ("gapi-codegen: ");
				Console.WriteLine (e.Message);
				Console.WriteLine ("Try `gapi-codegen --help' for more information.");
				return 64;
			}

			if (show_help) {
				ShowHelp (options);
				return 0;
			}

			if (filenames.Count == 0) {
				Console.WriteLine ("You need to specify a file to process using the --generate option.");
				Console.WriteLine ("Try `gapi-codegen --help' for more information.");
				return 64;
			}

			if (extra.Exists (v => { return v.StartsWith ("--customdir"); })) {
				Console.WriteLine ("Using .custom files is not supported anymore, use partial classes instead.");
				return 64;
			}

			if (!String.IsNullOrEmpty (schema_name) && !File.Exists (schema_name)) {
				Console.WriteLine ("WARNING: Could not find schema file at '{0}', no validation will be done.", schema_name);
				schema_name = null;
			}

			Parser p = new Parser ();
			foreach (string include in includes) {
				log.Info("Parsing included gapi: " + include);
				IGeneratable[] curr_gens = p.Parse (include, schema_name, gapidir);
				table.AddTypes (curr_gens);
			}

			foreach (string filename in filenames) {
			log.Info("Parsing included gapi: " + filename);
				IGeneratable[] curr_gens = p.Parse (filename, schema_name, gapidir);
				table.AddTypes (curr_gens);
				gens.AddRange (curr_gens);
			}

			// Now that everything is loaded, validate all the to-be-
			// generated generatables and then remove the invalid ones.
			var invalids = new List<IGeneratable> ();
			foreach (IGeneratable gen in gens) {
				if (!gen.Validate ())
					invalids.Add (gen);
			}
			foreach (IGeneratable gen in invalids)
				gens.Remove (gen);

			GenerationInfo gen_info = null;
			if (dir != "" || assembly_name != "" || glue_filename != "" || glue_includes != "" || gluelib_name != "")
				gen_info = new GenerationInfo (dir, assembly_name, glue_filename, glue_includes, gluelib_name,
						abi_c_file, abi_cs_file, abi_cs_usings);
			
			foreach (IGeneratable gen in gens) {
				if (gen_info == null)
					gen.Generate ();
				else
					gen.Generate (gen_info);
			}

			ObjectGen.GenerateMappers ();

			if (gen_info != null)
				gen_info.CloseWriters ();

			Statistics.Report();
			return 0;
		}

		static void ShowHelp (OptionSet p)
		{
			Console.WriteLine ("Usage: gapi-codegen [OPTIONS]+");
			Console.WriteLine ();
			Console.WriteLine ("Options:");
			p.WriteOptionDescriptions (Console.Out);
		}
	}
}

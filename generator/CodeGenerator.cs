// GtkSharp.Generation.CodeGenerator.cs - The main code generation engine.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2003 Mike Kestner and Ximian Inc.

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.Xml;

	public class CodeGenerator  {

		public static int Main (string[] args)
		{
			if (args.Length < 2) {
				Console.WriteLine ("Usage: codegen --generate <filename1...>");
				return 0;
			}

			bool generate = false;
			bool include = false;
			string dir = "";
			string custom_dir = "";
			string assembly_name = "";

			SymbolTable table = SymbolTable.Table;
			ArrayList gens = new ArrayList ();
			foreach (string arg in args) {
				if (arg == "--generate") {
					generate = true;
					include = false;
					continue;
				} else if (arg == "--include") {
					generate = false;
					include = true;
					continue;
				} else if (arg.StartsWith ("--outdir=")) {
					include = generate = false;
					dir = arg.Substring (9);
					continue;
				} else if (arg.StartsWith ("--customdir=")) {
					include = generate = false;
					custom_dir = arg.Substring (12);
					continue;
				} else if (arg.StartsWith ("--assembly-name=")) {
					include = generate = false;
					assembly_name = arg.Substring (16);
					continue;
				}

				Parser p = new Parser ();
				IGeneratable[] curr_gens = p.Parse (arg);
				table.AddTypes (curr_gens);
				if (generate)
					gens.AddRange (curr_gens);
			}

			GenerationInfo gen_info = null;
			if (dir != "" || assembly_name != "")
				gen_info = new GenerationInfo (dir, custom_dir, assembly_name);
			
			foreach (IGeneratable gen in gens) {
				if (gen_info == null)
					gen.Generate ();
				else
					gen.Generate (gen_info);
			}

			ObjectGen.GenerateMappers ();

			Statistics.Report();
			return 0;
		}
	}
}

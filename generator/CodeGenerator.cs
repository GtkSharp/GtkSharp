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
				}

				Parser p = new Parser ();
				IGeneratable[] curr_gens = p.Parse (arg);
				table.AddTypes (curr_gens);
				if (generate)
					gens.AddRange (curr_gens);
			}
			
			foreach (IGeneratable gen in gens) {
				gen.DoGenerate = true;
				gen.Generate ();
			}

			ObjectGen.GenerateMapper ();

			Statistics.Report();
			return 0;
		}
	}
}

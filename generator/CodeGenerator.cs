// GtkSharp.Generation.CodeGenerator.cs - The main code generation engine.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

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
			foreach (string arg in args) {
					if (arg == "--generate") {
						generate = true;
						continue;
					} else if (arg == "--include") {
						generate = false;
						continue;
					}
					
					Parser p = new Parser (arg);
					p.Parse (generate);
			}
			
			foreach (IGeneratable gen in SymbolTable.Table.Generatables) {
				gen.Generate ();
			}

			ObjectGen.GenerateMapper ();

			Statistics.Report();
			return 0;
		}

	}
}

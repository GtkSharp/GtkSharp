// GtkSharp.CodeGenerator.cs - The main code generation engine.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp {

	using System;
	using System.Collections;
	using System.Xml;

	public class CodeGenerator  {

		public static int Main (string[] args)
		{
			Parser p = new Parser (args[0]);
			Hashtable types = p.Types;
			Console.WriteLine (types.Count);
			
			IDictionaryEnumerator de = types.GetEnumerator();
			while (de.MoveNext()) {
				IGeneratable gen = (IGeneratable) de.Value;
				gen.Generate ();
			}

			return 0;
		}

	}
}

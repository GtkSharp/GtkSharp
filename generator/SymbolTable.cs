// GtkSharp.Generation.SymbolTable.cs - The Symbol Table Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;

	public class SymbolTable {
		
		private Hashtable complex_types = new Hashtable ();
		private Hashtable simple_types;
		
		public void AddType (IGeneratable gen)
		{
			complex_types [gen.CName] = gen;
		}
		
		public int Count {
			get
			{
				return complex_types.Count;
			}
		}
		
		public IDictionaryEnumerator GetEnumerator ()
		{
			return complex_types.GetEnumerator();
		}
		
		public String GetCSType (String c_type)
		{
			return "";
		}
		
	}
}


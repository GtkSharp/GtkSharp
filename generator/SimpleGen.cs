// GtkSharp.Generation.SimpleGen.cs - The Simple type Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2003 Mike Kestner

namespace GtkSharp.Generation {

	using System;

	public class SimpleGen : IGeneratable  {
		
		string type;
		string ctype;

		public SimpleGen (string ctype, string type)
		{
			this.ctype = ctype;
			this.type = type;
		}
		
		public string CName {
			get
			{
				return ctype;
			}
		}

		public string Name {
			get
			{
				return type;
			}
		}

		public string QualifiedName {
			get
			{
				return type;
			}
		}

		public String MarshalType {
			get
			{
				return type;
			}
		}
		public String MarshalReturnType {
			get
			{
				return type;
			}
		}

		public String CallByName (String var_name)
		{
			return var_name;
		}
		
		public String FromNative(String var)
		{
			return var;
		}
		
		public String FromNativeReturn(String var)
		{
			return var;
		}

		public bool DoGenerate {
			get {
				return false;
			}
			set {
			}
		}

		public void Generate ()
		{
		}
		
	}
}


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

		public string MarshalType {
			get
			{
				return type;
			}
		}
		public virtual string MarshalReturnType {
			get
			{
				return type;
			}
		}

		public string CallByName (string var_name)
		{
			return var_name;
		}
		
		public string FromNative(string var)
		{
			return var;
		}
		
		public virtual string FromNativeReturn(string var)
		{
			return var;
		}

		public virtual string ToNativeReturn(string var)
		{
			return var;
		}

		public void Generate ()
		{
		}
		
		public void Generate (GenerationInfo gen_info)
		{
		}
	}
}


// GtkSharp.Generation.ByRefGen.cs - The ByRef type Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2003 Mike Kestner

namespace GtkSharp.Generation {

	using System;

	public class ByRefGen : IGeneratable  {
		
		string type;
		string ctype;
		string ns = "";

		public ByRefGen (string ctype, string type)
		{
			string[] toks = type.Split('.');
			this.ctype = ctype;
			this.type = toks[toks.Length - 1];
			if (toks.Length > 2)
				this.ns = String.Join (".", toks, 0, toks.Length - 2);
			else if (toks.Length == 2)
				this.ns = toks[0];
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
				return ns + "." + type;
			}
		}

		public string MarshalType {
			get
			{
				return "ref " + QualifiedName;
			}
		}
		public virtual string MarshalReturnType {
			get
			{
				return QualifiedName;
			}
		}

		public string CallByName (string var_name)
		{
			return "ref " + var_name;
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


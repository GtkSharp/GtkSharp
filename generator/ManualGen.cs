// GtkSharp.Generation.ManualGen.cs - The Manually wrapped type Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2003 Mike Kestner

namespace GtkSharp.Generation {

	using System;

	public class ManualGen : IGeneratable  {
		
		string handle;
		string ctype;
		string type;
		string ns = "";

		public ManualGen (string ctype, string type) : this (ctype, type, "Handle") {}

		public ManualGen (string ctype, string type, string handle)
		{
			string[] toks = type.Split('.');
			this.handle = handle;
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
				return "IntPtr";
			}
		}
		public string MarshalReturnType {
			get
			{
				return "IntPtr";
			}
		}

		public string CallByName (string var_name)
		{
			return var_name + "." + handle;
		}
		
		public string FromNative(string var)
		{
			return "new " + QualifiedName + "(" + var + ")";
		}
		
		public string FromNativeReturn(string var)
		{
			return FromNative (var);
		}

		public virtual String ToNativeReturn(String var)
		{
			return CallByName (var);
		}
		
		public void Generate ()
		{
		}
		
		public void Generate (GenerationInfo gen_info)
		{
		}
	}
}


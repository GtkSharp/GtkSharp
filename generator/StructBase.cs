// GtkSharp.Generation.StructBase.cs - The Structure/Object Base Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class StructBase  {
		
		protected String ns;
		protected XmlElement elem;
		
		public StructBase (String ns, XmlElement elem) {
			
			this.ns = ns;
			this.elem = elem;
		}

		public String Name {
			get
			{
				return elem.GetAttribute("name");
			}
		}
		
		public String QualifiedName {
			get
			{
				return ns + "." + elem.GetAttribute("name");
			}
		}
		
		public String CName {
			get
			{
				return elem.GetAttribute("cname");
			}
		}
		

		protected bool GenCtor(XmlElement ctor, SymbolTable table, StreamWriter sw)
		{
			String sig, isig, call;
			XmlElement parms = ctor["parameters"];
			
			if (parms == null) {
				sig = "()";
				isig = call = "();";
			//} else if (!GetSignature(parms, table, out sig) ||
			//    	   !GetImportSig(parms, table, out isig) ||
			//           !GetCallString(parms, table, out call)) {
			//	Console.Write("ctor ");
			//	return false;       	
			} else {
				Console.Write("ctor with parms ");
				return false;
			}
			
			String cname = ctor.GetAttribute("cname");
			
			sw.WriteLine("\t\t[DllImport(\"" + table.GetDllName(ns) + 
			             "\", CallingConvention=CallingConvention.Cdecl)]");
			sw.WriteLine("\t\tstatic extern IntPtr " + cname + isig);
			sw.WriteLine();
			sw.WriteLine("\t\tpublic " + Name + sig);
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\tRawObject = " + cname + call);
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			return true;
		}

		protected bool GenField (XmlElement field, SymbolTable table, StreamWriter sw)
		{
			String c_type;
			
			if (field.HasAttribute("bits") && (field.GetAttribute("bits") == "1")) {
				c_type = "gboolean";
			} else {
				c_type = field.GetAttribute("type");
			}
			char[] ast = {'*'};
			c_type = c_type.TrimEnd(ast);
			String cs_type = table.GetCSType(c_type);
			
			if (cs_type == "") {
				Console.WriteLine ("Field has unknown Type {0}", c_type);
				return false;
			}
			
			sw.Write ("\t\t public " + cs_type);
			if (field.HasAttribute("array_len")) {
				sw.Write ("[]");
			}
			sw.WriteLine (" " + field.GetAttribute("cname") + ";");
			return true;
		}

		private bool GetCallString(XmlElement parms, SymbolTable table, out String call)
		{
			call = "(";
			
			foreach (XmlNode parm in parms.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement elem = (XmlElement) parm;
			}
			
			call += ");";
			return true;
		}

		private bool GetImportSig(XmlElement parms, SymbolTable table, out String isig)
		{
			isig = "(";
			
			foreach (XmlNode parm in parms.ChildNodes) {
				if (parm.Name != "namespace") {
					continue;
				}

				XmlElement elem = (XmlElement) parm;
			}
			
			isig += ");";
			return true;
		}
		
		private bool GetSignature(XmlElement parms, SymbolTable table, out String sig)
		{
			sig = "(";
			
			foreach (XmlNode parm in parms.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement elem = (XmlElement) parm;
			}
			
			sig += ")";
			return true;
		}
	}
}


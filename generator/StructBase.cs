// GtkSharp.Generation.StructBase.cs - The Structure/Object Base Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Text.RegularExpressions;
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
		

		protected bool GenCtor(XmlElement ctor, SymbolTable table, StreamWriter sw, Hashtable clash_map)
		{
			String sig, isig, call, sigtypes;
			XmlElement parms = ctor["parameters"];
			
			if (parms == null) {
				call = sig = "()";
				isig = "();";
				sigtypes = "";
			} else if (GetSignature(parms, table, out sig, out sigtypes) &&
			    	   GetImportSig(parms, table, out isig) &&
			           GetCallString(parms, table, out call)) {
				sig = "(" + sig + ")";
				isig = "(" + isig + ");";
				call = "(" + call + ")";
			} else {
				Console.Write("ctor ");
				return false;       	
			}
			
			bool clash = false;
			if (clash_map.ContainsKey(sigtypes)) {
				clash = true;
			} else {
				clash_map[sigtypes] = ctor;
			}			
			
			String cname = ctor.GetAttribute("cname");
			
			sw.WriteLine("\t\t[DllImport(\"" + table.GetDllName(ns) + 
			             "\", CallingConvention=CallingConvention.Cdecl)]");
			sw.WriteLine("\t\tstatic extern IntPtr " + cname + isig);
			sw.WriteLine();
			
			if (clash) {
				String mname = cname.Substring(cname.IndexOf("new"));
				// mname = Regex.Replace(mname, "_(\\w)", "\\u\\1");
				sw.WriteLine("\t\tpublic static " + Name + " " + mname + sig);
				sw.WriteLine("\t\t{");
				sw.WriteLine("\t\t\treturn new " + Name + "(" + cname + call + ");");
			} else {
				sw.WriteLine("\t\tpublic " + Name + sig);
				sw.WriteLine("\t\t{");
				sw.WriteLine("\t\t\tRawObject = " + cname + call + ";");
			}
			
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

		protected bool GenMethod(XmlElement method, SymbolTable table, StreamWriter sw)
		{
			String sig, isig, call, sigtypes;
			XmlElement parms = method["parameters"];
			
			if (parms == null) {
				call = "(Handle)";
				sig = "()";
				isig = "(IntPtr raw);";
				sigtypes = "";
			} else if (GetSignature(parms, table, out sig, out sigtypes) &&
			    	   GetImportSig(parms, table, out isig) &&
			           GetCallString(parms, table, out call)) {
				sig = "(" + sig + ")";
				isig = "(IntPtr raw, " + isig + ");";
				call = "(Handle, " + call + ")";
			} else {
				Console.Write("method ");
				return false;       	
			}
			
			String rettype = "void";
			if (method.HasAttribute("return-type")) {
				rettype = method.GetAttribute("return-type");
			}
			
			String m_ret = table.GetMarshalType(rettype);
			String s_ret = table.GetCSType(rettype);
			if (m_ret == "" || s_ret == "") {
				Console.Write("rettype: " + rettype + " method ");
				return false;
			}
			
			String cname = method.GetAttribute("cname");
			String name = method.GetAttribute("name");
			
			sw.WriteLine("\t\t[DllImport(\"" + table.GetDllName(ns) + 
			             "\", CallingConvention=CallingConvention.Cdecl)]");
			sw.Write("\t\tstatic extern " + m_ret + " " + cname + isig);
			sw.WriteLine();
			
			sw.WriteLine("\t\tpublic " + s_ret + " " + name + sig);
			sw.WriteLine("\t\t{");
			sw.Write("\t\t\t");
			if (m_ret == "void") {
				sw.WriteLine(cname + call + ";");
			} else {
				sw.WriteLine("return " + table.FromNative(rettype, cname + call) + ";");
			}
			
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			return true;
		}

		private bool GetCallString(XmlElement parms, SymbolTable table, out String call)
		{
			call = "";
			
			bool need_comma = false;
			
			foreach (XmlNode parm in parms.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement elem = (XmlElement) parm;
				String type = elem.GetAttribute("type");
				String name = elem.GetAttribute("name");
				name = MangleName(name);
				String call_parm = table.CallByName(type, name);
				
				if (call_parm == "") {
					Console.Write("Name: " + name + " Type: " + type + " ");
					return false;
				}
				
				if (need_comma) {
					call += ", ";
				} else {
					need_comma = true;
				}
				call += call_parm;
			}
			
			return true;
		}

		private bool GetImportSig(XmlElement parms, SymbolTable table, out String isig)
		{
			isig = "";
			
			bool need_comma = false;
			
			foreach (XmlNode parm in parms.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement elem = (XmlElement) parm;
				String type = elem.GetAttribute("type");
				String m_type = table.GetMarshalType(type);
				String name = elem.GetAttribute("name");
				name = MangleName(name);
				
				if ((m_type == "") || (name == "")) {
					Console.Write("Name: " + name + " Type: " + type + " ");
					return false;
				}
				
				if (elem.HasAttribute("array")) {
					m_type += "[]";
				}
				
				if (need_comma) {
					isig += ", ";
				} else {
					need_comma = true;
				}
				isig += (m_type + " " + name);
			}
			
			return true;
		}
		
		private bool GetSignature(XmlElement parms, SymbolTable table, out String sig, out String sigtypes)
		{
			sigtypes = sig = "";
			bool need_comma = false;
			
			foreach (XmlNode parm in parms.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement elem = (XmlElement) parm;
				String type = elem.GetAttribute("type");
				String cs_type = table.GetCSType(type);
				String name = elem.GetAttribute("name");
				name = MangleName(name);
				
				if ((cs_type == "") || (name == "")) {
					Console.Write("Name: " + name + " Type: " + type + " ");
					return false;
				}
				
				if (elem.HasAttribute("array")) {
					cs_type += "[]";
				}
				
				if (need_comma) {
					sig += ", ";
					sigtypes += ":";
				} else {
					need_comma = true;
				}
				sig += (cs_type + " " + name);
				sigtypes += cs_type;
			}
			
			return true;
		}
		
		private String MangleName(String name)
		{
			if (name == "string") {
				return "str1ng";
			} else {
				return name;
			}
		}
	}
}


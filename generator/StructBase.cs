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

	public class StructBase : ClassBase {
		
		public StructBase (XmlElement ns, XmlElement elem) : base (ns, elem) {}

		protected bool GenCtor(XmlElement ctor, StreamWriter sw, Hashtable clash_map)
		{
			String sig, isig, call, sigtypes;
			XmlElement parms = ctor["parameters"];
			
			if (parms == null) {
				call = sig = "()";
				isig = "();";
				sigtypes = "";
			} else if (GetSignature(parms, out sig, out sigtypes) &&
			    	   GetImportSig(parms, out isig) &&
			           GetCallString(parms, out call)) {
				sig = "(" + sig + ")";
				isig = "(" + isig + ");";
				call = "(" + call + ")";
			} else {
				Console.Write("ctor ");
				Statistics.ThrottledCount++;
				return false;       	
			}
			
			bool clash = false;
			if (clash_map.ContainsKey(sigtypes)) {
				clash = true;
			} else {
				clash_map[sigtypes] = ctor;
			}			
			
			String cname = ctor.GetAttribute("cname");
			
			sw.WriteLine("\t\t[DllImport(\"" + LibraryName + 
			             "\", CallingConvention=CallingConvention.Cdecl)]");
			sw.WriteLine("\t\tstatic extern IntPtr " + cname + isig);
			sw.WriteLine();
			
			if (clash) {
				String mname = cname.Substring(cname.IndexOf("new"));
				mname = mname.Substring(0,1).ToUpper() + mname.Substring(1);
				int idx;
				while ((idx = mname.IndexOf("_")) > 0) {
					mname = mname.Substring(0, idx) + mname.Substring(idx+1, 1).ToUpper() + mname.Substring(idx+2);
				}
				
				sw.WriteLine("\t\tpublic static " + Name + " " + mname + sig);
				sw.WriteLine("\t\t{");
				sw.WriteLine("\t\t\treturn new " + Name + "(" + cname + call + ");");
			} else {
				sw.WriteLine("\t\tpublic " + Name + sig);
				sw.WriteLine("\t\t{");
				sw.WriteLine("\t\t\tRaw = " + cname + call + ";");
			}
			
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			Statistics.CtorCount++;
			return true;
		}

		protected bool GenField (XmlElement field, StreamWriter sw)
		{
			String c_type;
			
			if (field.HasAttribute("bits") && (field.GetAttribute("bits") == "1")) {
				c_type = "gboolean";
			} else {
				c_type = field.GetAttribute("type");
			}
			char[] ast = {'*'};
			c_type = c_type.TrimEnd(ast);
			String cs_type = SymbolTable.GetCSType(c_type);
			
			if (cs_type == "") {
				Console.WriteLine ("Field has unknown Type {0}", c_type);
				Statistics.ThrottledCount++;
				return false;
			}
			
			sw.Write ("\t\t public " + cs_type);
			if (field.HasAttribute("array_len")) {
				sw.Write ("[]");
			}
			sw.WriteLine (" " + field.GetAttribute("cname") + ";");
			return true;
		}

		protected bool GenMethod(XmlElement method, StreamWriter sw)
		{
			String sig, isig, call, sigtypes;
			XmlElement parms = method["parameters"];
			
			if (parms == null) {
				call = "(Handle)";
				sig = "()";
				isig = "(IntPtr raw);";
				sigtypes = "";
			} else if (GetSignature(parms, out sig, out sigtypes) &&
			    	   GetImportSig(parms, out isig) &&
			           GetCallString(parms, out call)) {
				sig = "(" + sig + ")";
				isig = "(IntPtr raw, " + isig + ");";
				call = "(Handle, " + call + ")";
			} else {
				Console.Write("method ");
				Statistics.ThrottledCount++;
				return false;       	
			}
			
			XmlElement ret_elem = method["return-type"];
			if (ret_elem == null) {
				Console.Write("Missing return type in method ");
				Statistics.ThrottledCount++;
				return false;
			}
			
			String rettype = ret_elem.GetAttribute("type");
			
			String m_ret = SymbolTable.GetMarshalType(rettype);
			String s_ret = SymbolTable.GetCSType(rettype);
			if (m_ret == "" || s_ret == "") {
				Console.Write("rettype: " + rettype + " method ");
				Statistics.ThrottledCount++;
				return false;
			}
			
			String cname = method.GetAttribute("cname");
			String name = method.GetAttribute("name");
			
			if (cname[0] == '_') {
				Statistics.ThrottledCount++;
				return true;
			}

			sw.WriteLine("\t\t[DllImport(\"" + LibraryName + 
			             "\", CallingConvention=CallingConvention.Cdecl)]");
			sw.Write("\t\tstatic extern " + m_ret + " " + cname + isig);
			sw.WriteLine();
			
			sw.WriteLine("\t\tpublic " + s_ret + " " + name + sig);
			sw.WriteLine("\t\t{");
			sw.Write("\t\t\t");
			if (m_ret == "void") {
				sw.WriteLine(cname + call + ";");
			} else {
				sw.WriteLine("return " + SymbolTable.FromNative(rettype, cname + call) + ";");
			}
			
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			Statistics.MethodCount++;
			return true;
		}

		private bool GetCallString(XmlElement parms, out String call)
		{
			call = "";
			
			bool need_comma = false;
			
			foreach (XmlNode parm in parms.ChildNodes) {
				if (parm.Name != "parameter") {
					Console.Write(parm.Name + " node ");
					return false;
				}

				XmlElement elem = (XmlElement) parm;
				String type = elem.GetAttribute("type");
				String name = elem.GetAttribute("name");
				name = MangleName(name);
				String call_parm = SymbolTable.CallByName(type, name);
				
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

		private bool GetImportSig(XmlElement parms, out String isig)
		{
			isig = "";
			
			bool need_comma = false;
			
			foreach (XmlNode parm in parms.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement elem = (XmlElement) parm;
				String type = elem.GetAttribute("type");
				String m_type = SymbolTable.GetMarshalType(type);
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
		
		private bool GetSignature(XmlElement parms, out String sig, out String sigtypes)
		{
			sigtypes = sig = "";
			bool need_comma = false;
			
			foreach (XmlNode parm in parms.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement elem = (XmlElement) parm;
				String type = elem.GetAttribute("type");
				String cs_type = SymbolTable.GetCSType(type);
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
			} else if (name == "event") {
				return "evnt";
			} else if (name == "object") {
				return "objekt";
			} else {
				return name;
			}
		}
	}
}


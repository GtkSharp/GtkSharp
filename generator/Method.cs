// GtkSharp.Generation.Method.cs - The Method Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class Method  {
		
		private string ns;
		private XmlElement elem;
		private Parameters parms;
		
		public Method (string ns, XmlElement elem) 
		{
			this.ns = ns;
			this.elem = elem;
			if (elem["parameters"] != null)
				parms = new Parameters (elem["parameters"]);
		}

		public string Name {
			get {
				return elem.GetAttribute("name");
			}
			set {
				elem.SetAttribute("name", value);
			}
		}

		public Parameters Params {
			get {
				return parms;
			}
		}

		public override bool Equals (object o)
		{
			if (!(o is Method))
				return false;

			return (this == (Method) o);
		}

		public static bool operator == (Method a, Method b)
		{
			if (a.Name != b.Name)
				return false;

			if (a.Params == null)
				return b.Params == null;

			if (b.Params == null)
				return false;

			return (a.Params.SignatureTypes == b.Params.SignatureTypes);
		}

		public static bool operator != (Method a, Method b)
		{
			if (a.Name == b.Name)
				return false;

			if (a.Params == null)
				return b.Params != null;

			if (b.Params == null)
				return true;

			return (a.Params.SignatureTypes != b.Params.SignatureTypes);
		}

		public bool Validate ()
		{
			XmlElement ret_elem = elem["return-type"];
			if (ret_elem == null) {
				Console.Write("Missing return type in method ");
				Statistics.ThrottledCount++;
				return false;
			}

			string rettype = ret_elem.GetAttribute("type");
			string m_ret = SymbolTable.GetMarshalType(rettype);
			string s_ret = SymbolTable.GetCSType(rettype);
			if (m_ret == "" || s_ret == "") {
				Console.Write("rettype: " + rettype + " method ");
				Statistics.ThrottledCount++;
				return false;
			}

			if (Params == null)
				return true;

			return Params.Validate ();
		}

		public void Generate (StreamWriter sw)
		{
			string sig, isig, call;
			
			if (parms != null) {
				sig = "(" + parms.Signature + ")";
				isig = "(IntPtr raw, " + parms.ImportSig + ");";
				call = "(Handle, " + parms.CallString + ")";
			} else {
				sig = "()";
				isig = "(IntPtr raw);";
				call = "(Handle)";
			}

			string rettype = elem["return-type"].GetAttribute("type");
			string m_ret = SymbolTable.GetMarshalType(rettype);
			string s_ret = SymbolTable.GetCSType(rettype);
			string cname = elem.GetAttribute("cname");

			if (cname[0] == '_') {
				Statistics.ThrottledCount++;
				return;
			}

			sw.WriteLine("\t\t[DllImport(\"" + SymbolTable.GetDllName(ns) + 
			             "\", CallingConvention=CallingConvention.Cdecl)]");
			sw.Write("\t\tstatic extern " + m_ret + " " + cname + isig);
			sw.WriteLine();

			sw.Write("\t\tpublic ");
			bool is_get = (parms != null && parms.IsAccessor && Name.Substring(0, 3) == "Get");
			if (is_get) {
				s_ret = parms.AccessorReturnType;
				sw.Write(s_ret);
				sw.Write(" ");
				sw.Write(Name.Substring (3));
				sw.Write(" { get");
			} else {
				if (elem.HasAttribute("new_flag"))
					sw.Write("new ");
				sw.WriteLine(s_ret + " " + Name + sig);
			}
			sw.WriteLine("\t\t{");
			if (parms != null)
				parms.Initialize(sw, is_get);
			sw.Write("\t\t\t");
			if (is_get || m_ret == "void") {
				sw.WriteLine(cname + call + ";");
			} else {
				sw.WriteLine("return " + SymbolTable.FromNative(rettype, cname + call) + ";");
			}
			
			if (is_get) 
				sw.WriteLine ("\t\t\treturn " + parms.AccessorName + ";"); 


			sw.Write("\t\t}");
			if (is_get)
				sw.Write(" }");
			
			sw.WriteLine();
			sw.WriteLine();

			Statistics.MethodCount++;
		}
	}
}


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
		
		private string libname;
		private XmlElement elem;
		private Parameters parms;
		private ClassBase container_type;

		private bool initialized = false;
		private string sig, isig, call;
		private string rettype, m_ret, s_ret;
		private string element_type = null;
		private string name, cname, safety;
		private string protection = "public";
		private bool is_get, is_set;
		private bool needs_ref = false;

		public Method (string libname, XmlElement elem, ClassBase container_type) 
		{
			this.elem = elem;
			if (elem["parameters"] != null)
				parms = new Parameters (elem["parameters"], container_type.NS);
			this.container_type = container_type;
			this.name = elem.GetAttribute("name");
			if (name == "GetType")
				name = "GetGType";
			// override library - used in pixbuf API fixage 
			if (elem.HasAttribute ("library"))
				this.libname = elem.GetAttribute ("library");
			else
				this.libname = libname;
			
			// caller does not own reference?
			if (elem.HasAttribute ("needs_ref"))
				this.needs_ref = (elem.GetAttribute ("needs_ref") == "1");
		}

		public bool IsGetter {
			get {
				return is_get;
			}
		}

		public bool IsSetter {
			get {
				return is_set;
			}
		}

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public Parameters Params {
			get {
				return parms;
			}
		}

		public string Protection {
			get {
				return protection;
			}
			set {
				protection = value;
			}
		}

		public string ReturnType {
			get {
				return s_ret;
			}
		}

		public override bool Equals (object o)
		{
			if (!(o is Method))
				return false;
/*
			return (this == (Method) o);
		}

		public static bool operator == (Method a, Method b)
		{
			if (a == null)
				return (b 
*/
			Method a = this;
			Method b = (Method) o;
			if (a.Name != b.Name)
				return false;

			if (a.Params == null)
				return b.Params == null;

			if (b.Params == null)
				return false;

			return (a.Params.SignatureTypes == b.Params.SignatureTypes);
		}
/*
		public static bool operator != (Method a, Method b)
		{
			return !(
			if (a.Name == b.Name)
				return false;

			if (a.Params == null)
				return b.Params != null;

			if (b.Params == null)
				return true;

			return (a.Params.SignatureTypes != b.Params.SignatureTypes);
		}
*/
		private bool Initialize ()
		{
			if (initialized)
				return true;

			if (parms != null && !parms.Validate ()) {
				Console.Write ("in method " + Name + " ");
				return false;
			}

			XmlElement ret_elem = elem["return-type"];
			if (ret_elem == null) {
				Console.Write("Missing return type in method ");
				Statistics.ThrottledCount++;
				return false;
			}
			
			SymbolTable table = SymbolTable.Table;

			rettype = ret_elem.GetAttribute("type");
			m_ret = table.GetMarshalReturnType(rettype);
			s_ret = table.GetCSType(rettype);
			cname = elem.GetAttribute("cname");
			if (ret_elem.HasAttribute("element_type"))
				element_type = ret_elem.GetAttribute("element_type");
			bool is_shared = elem.HasAttribute("shared");
			
			if (ret_elem.HasAttribute("array")) {
					s_ret += "[]";
					m_ret += "[]";
				}

			if (parms != null && parms.ThrowsException)
				safety = "unsafe ";
			else
				safety = "";

			is_get = (((parms != null && ((parms.IsAccessor && s_ret == "void") || (parms.Count == 0 && s_ret != "void"))) || (parms == null && s_ret != "void")) && Name.Length > 3 && (Name.StartsWith ("Get") || Name.StartsWith ("Is") || Name.StartsWith ("Has")));
			is_set = ((parms != null && (parms.IsAccessor || (parms.Count == 1 && s_ret == "void"))) && (Name.Length > 3 && Name.Substring(0, 3) == "Set"));
			
			if (parms != null) {
				parms.Static = is_shared;
				parms.CreateSignature (is_set);
				sig = "(" + parms.Signature + ")";
				isig = "(" + (is_shared ? "" : container_type.MarshalType + " raw, ") + parms.ImportSig + ");";
				call = "(" + (is_shared ? "" : container_type.CallByName () + ", ") + parms.CallString + ")";
			} else {
				sig = "()";
				isig = "(" + (is_shared ? "" : container_type.MarshalType + " raw") + ");";
				call = "(" + (is_shared ? "" : container_type.CallByName ()) + ")";
			}

			initialized = true;
			return true;
		}
		
		public bool Validate ()
		{
			if (!Initialize ())
				return false;

			if (m_ret == "" || s_ret == "") {
				Console.Write("rettype: " + rettype + " method ");
				Statistics.ThrottledCount++;
				return false;
			}
			return true;
		}
		
		private Method GetComplement ()
		{
			char complement;
			if (is_get)
				complement = 'S';
			else
				complement = 'G';
			
			return container_type.GetMethod (complement + elem.GetAttribute("name").Substring (1));
		}
		
		private void GenerateDeclCommon (StreamWriter sw, ClassBase implementor)
		{
			if (elem.HasAttribute("shared"))
				sw.Write("static ");
			sw.Write(safety);
			Method dup = null;
			if (Name == "ToString" && Params == null)
				sw.Write("override ");
			else if (Name == "GetGType" && container_type is ObjectGen)
				sw.Write("new ");
			else if (elem.HasAttribute("new_flag") || (container_type != null && (dup = container_type.GetMethodRecursively (Name)) != null) || (implementor != null && (dup = implementor.GetMethodRecursively (Name)) != null)) {
				if (dup != null && dup.parms != null)
					dup.parms.CreateSignature (false);
				if (elem.HasAttribute("new_flag") || (dup != null && ((dup.parms != null && dup.parms.Signature == parms.Signature) || (dup.parms == null && parms == null))))
					sw.Write("new ");
			}

			if (is_get || is_set) {
				if (s_ret == "void")
					s_ret = parms.AccessorReturnType;
				sw.Write(s_ret);
				sw.Write(" ");
				if (Name.StartsWith ("Get") || Name.StartsWith ("Set"))
					sw.Write (Name.Substring (3));
				else
					sw.Write (Name);
				sw.WriteLine(" { ");
			} else {
				sw.Write(s_ret + " " + Name + sig);
			}
		}

		public void GenerateDecl (StreamWriter sw)
		{
			if (!Initialize ()) 
				return;

			if (elem.HasAttribute("shared"))
				return;

			if (is_get || is_set)
			{
				Method comp = GetComplement ();
				if (comp != null && comp.Validate () && is_set)
					return;
			
				sw.Write("\t\t");
				GenerateDeclCommon (sw, null);

				sw.Write("\t\t\t");
				sw.Write ((is_get) ? "get;" : "set;");

				if (comp != null && comp.is_set)
					sw.WriteLine (" set;");
				else
					sw.WriteLine ();

				sw.WriteLine ("\t\t}");
			}
			else
			{
				sw.Write("\t\t");
				GenerateDeclCommon (sw, null);
				sw.WriteLine (";");
			}

			Statistics.MethodCount++;
		}

		public void GenerateImport (StreamWriter sw)
		{
			sw.WriteLine("\t\t[DllImport(\"" + libname + "\")]");
			sw.WriteLine("\t\tstatic extern " + safety + m_ret + " " + cname + isig);
			sw.WriteLine();
		}

		public void Generate (GenerationInfo gen_info, ClassBase implementor)
		{
			Method comp = null;

			if (!Initialize ()) 
				return;

			if (implementor != null && elem.HasAttribute("shared"))
				return;

			/* we are generated by the get Method, if there is one */
			if (is_set || is_get)
			{
				if (!elem.HasAttribute("new_flag") && container_type.GetPropertyRecursively (Name.Substring (3)) != null)
					return;
				comp = GetComplement ();
				if (comp != null && comp.Validate () && is_set && parms.AccessorReturnType == comp.s_ret)
					return;
				if (comp != null && is_set && parms.AccessorReturnType != comp.s_ret)
				{
					is_set = false;
					parms.CreateSignature (false);
					call = "(Handle, " + parms.CallString + ")";
					comp = null;
				}
				/* some setters take more than one arg */
				if (comp != null && !comp.is_set)
					comp = null;
			}
			
			GenerateImport (gen_info.Writer);
			if (comp != null && s_ret == comp.parms.AccessorReturnType)
				comp.GenerateImport (gen_info.Writer);
			
			gen_info.Writer.Write("\t\t");
			if (protection != "")
				gen_info.Writer.Write("{0} ", protection);
			GenerateDeclCommon (gen_info.Writer, implementor);

			if (is_get || is_set)
			{
				gen_info.Writer.Write ("\t\t\t");
				gen_info.Writer.Write ((is_get) ? "get" : "set");
				GenerateBody (gen_info, "\t");
			}
			else
				GenerateBody (gen_info, "");
			
			if (is_get || is_set)
			{
				if (comp != null && s_ret == comp.parms.AccessorReturnType)
				{
					gen_info.Writer.WriteLine ();
					gen_info.Writer.Write ("\t\t\tset");
					comp.GenerateBody (gen_info, "\t");
				}
				gen_info.Writer.WriteLine ();
				gen_info.Writer.WriteLine ("\t\t}");
			}
			else
				gen_info.Writer.WriteLine();
			
			gen_info.Writer.WriteLine();

			Statistics.MethodCount++;
		}

		public void GenerateBody (GenerationInfo gen_info, string indent)
		{
			StreamWriter sw = gen_info.Writer;
			sw.WriteLine(" {");
			if (parms != null)
				parms.Initialize(gen_info, is_get, is_set, indent);

			SymbolTable table = SymbolTable.Table;

			sw.Write(indent + "\t\t\t");
			if (m_ret == "void") {
				sw.WriteLine(cname + call + ";");
			} else {
				if (table.IsObject (rettype) || table.IsOpaque (rettype))
				{
					sw.WriteLine(m_ret + " raw_ret = " + cname + call + ";");
					if (table.IsObject (rettype))
						sw.WriteLine(indent + "\t\t\tbool ref_owned = false;");
					sw.WriteLine(indent +"\t\t\t" + s_ret + " ret = " + table.FromNativeReturn(rettype, "raw_ret") + ";");
					if (table.IsOpaque (rettype))
						sw.WriteLine(indent + "\t\t\tif (ret == null) ret = new " + s_ret + "(raw_ret);");
				}
				else {
					sw.WriteLine(m_ret + " raw_ret = " + cname + call + ";");
					sw.Write(indent + "\t\t\t");
					string raw_parms = "raw_ret";
					if (element_type != null)
						raw_parms += ", typeof (" + element_type + ")";
					if (table.IsInterface (rettype))
						sw.WriteLine(indent + "\t\t\tbool ref_owned = false;");
					sw.WriteLine(s_ret + " ret = " + table.FromNativeReturn(rettype, raw_parms) + ";");
				}
			}
			
			if (parms != null) {
				parms.Finish (sw, indent);
				parms.HandleException (sw, indent);
			}

			if (is_get && parms != null) 
				sw.WriteLine (indent + "\t\t\treturn " + parms.AccessorName + ";");
			else if (m_ret != "void")
				sw.WriteLine (indent + "\t\t\treturn ret;");

			sw.Write(indent + "\t\t}");
		}
	}
}


// GtkSharp.Generation.Method.cs - The Method Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2003 Mike Kestner, (c) 2003 Novell, Inc.

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class Method  {
		
		private string libname;
		private XmlElement elem;
		private Parameters parms;
		private Signature sig;
		private ImportSignature isig;
		private MethodBody body;
		private ClassBase container_type;

		private bool initialized = false;
		private string call;
		private string rettype, m_ret, s_ret;
		private string element_type = null;
		private string name, cname, safety;
		private string protection = "public";
		private bool is_get, is_set;
		private bool needs_ref = false;

		public Method (string libname, XmlElement elem, ClassBase container_type) 
		{
			this.elem = elem;
			if (elem["parameters"] != null) {
				parms = new Parameters (elem["parameters"], container_type.NS);
			}
			this.container_type = container_type;
			this.name = elem.GetAttribute("name");
			if (name == "GetType")
				name = "GetGType";
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

		private bool IsShared {
			get {
				return elem.HasAttribute("shared");
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

		public Signature Signature {
			get {
				return sig;
			}
		}

		public override bool Equals (object o)
		{
			if (!(o is Method))
				return false;
			Method a = this;
			Method b = (Method) o;

			if (a.Name != b.Name)
				return false;

			if (a.Signature == null)
				return b.Signature == null;

			if (b.Signature == null)
				return false;

			return (a.Signature.Types == b.Signature.Types);
		}

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
			
			if (ret_elem.HasAttribute("array")) {
					s_ret += "[]";
					m_ret += "[]";
				}

			is_get = (((parms != null && ((parms.IsAccessor && s_ret == "void") || (parms.Count == 0 && s_ret != "void"))) || (parms == null && s_ret != "void")) && Name.Length > 3 && (Name.StartsWith ("Get") || Name.StartsWith ("Is") || Name.StartsWith ("Has")));
			is_set = ((parms != null && (parms.IsAccessor || (parms.Count == 1 && s_ret == "void"))) && (Name.Length > 3 && Name.Substring(0, 3) == "Set"));
			
			if (parms != null)
				parms.Static = IsShared;

			sig = new Signature (parms);
			isig = new ImportSignature (parms, container_type.NS);
			body = new MethodBody (parms, container_type.NS);
			call = "(" + (IsShared ? "" : container_type.CallByName () + (parms != null ? ", " : "")) + body.GetCallString (is_set) + ")";

			if (body.ThrowsException)
				safety = "unsafe ";
			else
				safety = "";

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
			if (container_type != null)
				dup = container_type.GetMethodRecursively (Name);
			if (implementor != null)
				dup = implementor.GetMethodRecursively (Name);

			if (Name == "ToString" && parms == null)
				sw.Write("override ");
			else if (Name == "GetGType" && container_type is ObjectGen)
				sw.Write("new ");
			else if (elem.HasAttribute("new_flag") || (dup != null && dup.Initialize () && ((dup.Signature != null && sig != null && dup.Signature.ToString() == sig.ToString()) || (dup.Signature == null && sig == null))))
				sw.Write("new ");

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
				sw.Write(s_ret + " " + Name + "(" + (sig != null ? sig.ToString() : "") + ")");
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
			string import_sig = IsShared ? "" : container_type.MarshalType + " raw";
			import_sig += !IsShared && parms != null ? ", " : "";
			import_sig += isig.ToString();
			sw.WriteLine("\t\t[DllImport(\"" + libname + "\")]");
			sw.WriteLine("\t\tstatic extern " + safety + m_ret + " " + cname + "(" + import_sig + ");");
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
					call = "(Handle, " + body.GetCallString (false) + ")";
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
			body.Initialize(gen_info, is_get, is_set, indent);

			SymbolTable table = SymbolTable.Table;

			sw.Write(indent + "\t\t\t");
			if (m_ret == "void") {
				sw.WriteLine(cname + call + ";");
			} else {
				if (table.IsObject (rettype) || table.IsOpaque (rettype))
				{
					sw.WriteLine(m_ret + " raw_ret = " + cname + call + ";");
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
					sw.WriteLine(s_ret + " ret = " + table.FromNativeReturn(rettype, raw_parms) + ";");
				}
			}
			
			body.Finish (sw, indent);
			body.HandleException (sw, indent);

			if (is_get && parms != null) 
				sw.WriteLine (indent + "\t\t\treturn " + parms.AccessorName + ";");
			else if (m_ret != "void")
				sw.WriteLine (indent + "\t\t\treturn ret;");

			sw.Write(indent + "\t\t}");
		}
	}
}


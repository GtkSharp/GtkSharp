// GtkSharp.Generation.Parameters.cs - The Parameters Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class Parameter {

		private XmlElement elem;

		public Parameter (XmlElement e)
		{
			elem = e;
		}

		public string CType {
			get {
				string type = elem.GetAttribute("type");
				if (type == "void*")
					type = "gpointer";
				return type;
			}
		}

		public string CSType {
			get {
				string cstype = SymbolTable.GetCSType( elem.GetAttribute("type"));
				if (cstype == "void")
					cstype = "System.IntPtr";
				if (elem.HasAttribute("array")) {
					cstype += "[]";
					cstype = cstype.Replace ("ref ", "");
				}
				return cstype;
			}
		}

		public bool IsLength {
			get {
				return (CSType == "int" && 
					(Name.EndsWith("len") || Name.EndsWith("length")));
			}
		}

		public bool IsString {
			get {
				return (CSType == "string");
			}
		}

		public string MarshalType {
			get {
				string type = SymbolTable.GetMarshalType( elem.GetAttribute("type"));
				if (type == "void")
					type = "System.IntPtr";
				if (elem.HasAttribute("array")) {
					type += "[]";
					type = type.Replace ("ref ", "");
				}
				return type;
			}
		}

		public string Name {
			get {
				return MangleName (elem.GetAttribute("name"));
			}
		}

		private string MangleName(string name)
		{
			switch (name) {
			case "string":
				return "str1ng";
			case "event":
				return "evnt";
			case "null":
				return "is_null";
			case "object":
				return "objekt";
			case "params":
				return "parms";
			case "ref":
				return "reference";
			case "in":
				return "in_param";
			case "out":
				return "out_param";
			case "fixed":
				return "mfixed";
			default:
				break;
			}

			return name;
		}
		public string StudlyName {
			get {
				string name = elem.GetAttribute("name");
				string[] segs = name.Split('_');
				string studly = "";
				foreach (string s in segs) {
					studly += (s.Substring(0,1).ToUpper() + s.Substring(1));
				}
				return studly;
				
			}
		}

	}

	public class Parameters  {
		
		private XmlElement elem;
		private string import_sig;
		private string call_string;
		private string signature;
		private string signature_types;
		private bool hide_data;

		public Parameters (XmlElement elem) {
			
			this.elem = elem;
		}

		public string CallString {
			get {
				return call_string;
			}
		}

		public Parameter this [int idx] {
			get {
				int count = 0;
				foreach (XmlNode node in elem.ChildNodes) {
					if ((node is XmlElement) && (idx == count++))
						return new Parameter ((XmlElement) node);
				}
				return null;
			}
		}

		public string ImportSig {
			get {
				return import_sig;
			}
		}
		
		public string Signature {
			get {
				return signature;
			}
		}

		public string SignatureTypes {
			get {
				return signature_types;
			}
		}

		public bool HideData {
			get { return hide_data; }
			set { hide_data = value; }
		}

		public bool Validate ()
		{
			foreach (XmlNode parm in elem.ChildNodes) {
				if (!(parm is XmlElement) || parm.Name != "parameter") {
					continue;
				}

				XmlElement p_elem = (XmlElement) parm;

				if (p_elem.HasAttribute("ellipsis")) {
					Console.Write("Ellipsis parameter ");
					return false;
				}

				Parameter p = new Parameter (p_elem);
				
				if ((p.CSType == "") || (p.Name == "") || 
				    (p.MarshalType == "") || (SymbolTable.CallByName(p.CType, p.Name) == "")) {
					Console.Write("Name: " + p.Name + " Type: " + p.CType + " ");
					return false;
				}
			}
			
			return true;
		}

		public void CreateSignature (bool is_set)
		{
			signature_types = signature = import_sig = call_string = "";
			bool need_sep = false;
			bool has_callback = hide_data;
			bool last_was_user_data = false;
			bool has_user_data = false;
			
			int len = 0;
			Parameter last_param = null;
			foreach (XmlNode parm in elem.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}
				XmlElement p_elem = (XmlElement) parm;
				if (p_elem.GetAttribute("type") == "gpointer" && (p_elem.GetAttribute("name").EndsWith ("data") || p_elem.GetAttribute("name").EndsWith ("data_or_owner")))
					has_user_data = true;
				len++;
				last_param = new Parameter ((XmlElement) parm);
			}

			int i = 0;
			Parameter prev = null;

			foreach (XmlNode parm in elem.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement p_elem = (XmlElement) parm;
				Parameter curr = new Parameter (p_elem);

				string type = curr.CType;
				string cs_type = curr.CSType;
				string m_type = curr.MarshalType;
				string name = curr.Name;

				if (prev != null && prev.IsString && curr.IsLength) {
					call_string += ", " + prev.Name + ".Length";
					import_sig += ", " + m_type + " " + name;
					prev = curr;
					i++;
					continue;
				}

				string call_parm_name = name;
				if (is_set && i == 0) 
					call_parm_name = "value";

				string call_parm;
				if (SymbolTable.IsCallback (type)) {
					has_callback = true;
					call_parm = SymbolTable.CallByName (type, call_parm_name + "_wrapper");
				} else
					call_parm = SymbolTable.CallByName(type, call_parm_name);
				
				if (p_elem.HasAttribute ("null_ok") && cs_type != "IntPtr" && cs_type != "System.IntPtr" && !SymbolTable.IsStruct (type))
					call_parm = String.Format ("({0} != null) ? {1} : {2}", call_parm_name, call_parm, SymbolTable.IsCallback (type) ? "null" : "IntPtr.Zero");
				
				if (p_elem.HasAttribute("array"))
					call_parm = call_parm.Replace ("ref ", "");

				if (IsVarArgs && i == (len - 1) && VAType == "length_param") {
					cs_type = "params " + cs_type + "[]";
					m_type += "[]";
				}
				
				if (need_sep) {
					call_string += ", ";
					import_sig += ", ";
					if (!(type == "GError**" || last_was_user_data) && !(IsVarArgs && i == (len - 1) && VAType == "length_param"))
					{
						signature += ", ";
						signature_types += ":";
					}
				} else {
					need_sep = true;
				}

				if (p_elem.HasAttribute("pass_as")) {
					string pass_as = p_elem.GetAttribute("pass_as");
					signature += pass_as + " ";
					// We only need to do this for value types
					if (type != "GError**" && m_type != "IntPtr" && m_type != "System.IntPtr")
					{
						import_sig += pass_as + " ";
						call_string += pass_as + " ";
					}
					
					if (SymbolTable.IsEnum (type))
						call_parm = name + "_as_int";
				}
				else if (type == "GError**")
				{
					call_string += "out ";				
					import_sig += "out ";				
				}
				
				if (IsVarArgs && i == (len - 2) && VAType == "length_param")
				{
					call_string += last_param.Name + ".Length";
					last_was_user_data = false; 
				}
				else
				{
					if (!(type == "GError**" || (has_callback && (type == "gpointer" || type == "void*") && (i == Count - 1) && (name.EndsWith ("data") || name.EndsWith ("data_or_owner"))))) {
						signature += (cs_type + " " + name);
						signature_types += cs_type;
						last_was_user_data = false; 
					} else if (type == "GError**") {
						call_parm = call_parm.Replace (name, "error");
						last_was_user_data = false; 
					} else if ((type == "gpointer" || type == "void*") && (i == Count - 1) && (name.EndsWith ("data") || name.EndsWith ("data_or_owner"))) {
						call_parm = "IntPtr.Zero"; 
						last_was_user_data = true;
					} else
						last_was_user_data = false; 
					
					call_string += call_parm;
				}
				import_sig += (m_type + " " + name);
				prev = curr;
				i++;
			}

			// FIXME: lame
			call_string = call_string.Replace ("out ref", "out");
			import_sig = import_sig.Replace ("out ref", "out");
			call_string = call_string.Replace ("ref ref", "ref");
			import_sig = import_sig.Replace ("ref ref", "ref");

			// FIXME: this is also lame, I need to fix the need_sep algo
			if (signature.EndsWith (", ")) 
				signature = signature.Remove (signature.Length - 2, 2);
		}

		public void Initialize (StreamWriter sw, bool is_get, bool is_set, string indent)
		{
			string name = "";

			foreach (XmlNode parm in elem.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement p_elem = (XmlElement) parm;
				Parameter p = new Parameter (p_elem);

				string c_type = p.CType;
				string type = p.CSType;

				if (is_set) {
					name = "value";
				} else {
					name = p.Name;
				}

				if (is_get) {
					sw.WriteLine (indent + "\t\t\t" + type + " " + name + ";");
				}

				if ((is_get || (p_elem.HasAttribute("pass_as") && p_elem.GetAttribute ("pass_as") == "out")) && (SymbolTable.IsObject (c_type) || SymbolTable.IsOpaque (c_type) || type == "GLib.Value")) {
					sw.WriteLine(indent + "\t\t\t" + name + " = new " + type + "();");
				}

				if (p_elem.HasAttribute("pass_as") && p_elem.GetAttribute ("pass_as") == "out" && SymbolTable.IsEnum (c_type)) {
					sw.WriteLine(indent + "\t\t\tint " + name + "_as_int;");
				}
			}

			if (ThrowsException)
				sw.WriteLine (indent + "\t\t\tIntPtr error = IntPtr.Zero;");

			foreach (XmlNode parm in elem.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement p_elem = (XmlElement) parm;
				Parameter p = new Parameter (p_elem);

				string c_type = p.CType;
				string type = p.CSType;

				if (is_set) {
					name = "value";
				} else {
					name = p.Name;
				}

				if (SymbolTable.IsCallback (c_type)) {
					type = type.Replace(".", "Sharp.") + "Wrapper";

					sw.WriteLine (indent + "\t\t\t{0} {1}_wrapper = null;", type, name);
					sw.Write (indent + "\t\t\t");
					if (p_elem.HasAttribute ("null_ok"))
					{
						sw.Write ("if ({0} != null) ", name);
					}
					sw.WriteLine ("{1}_wrapper = new {0} ({1});", type, name);
				}
			}
		}

		public void Finish (StreamWriter sw, string indent)
		{
			foreach (XmlNode parm in elem.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}
				
				XmlElement p_elem = (XmlElement) parm;
				Parameter p = new Parameter (p_elem);
				string c_type = p.CType;
				string name = p.Name;
				string type = p.CSType;

				if (p_elem.HasAttribute("pass_as") && p_elem.GetAttribute ("pass_as") == "out" && SymbolTable.IsEnum (c_type)) {
					sw.WriteLine(indent + "\t\t\t" + name + " = (" + type + ") " + name + "_as_int;");
				}
			}
		}


		public void HandleException (StreamWriter sw, string indent)
		{
			if (!ThrowsException)
				return;
			sw.WriteLine (indent + "\t\t\tif (error != IntPtr.Zero) throw new GLib.GException (error);");
		}
		
		public int Count {
			get {
				int length = 0;
				foreach (XmlNode parm in elem.ChildNodes) {
					if (parm.Name != "parameter") {
						continue;
					}
	
					length++;
				}
				return length;
			}
		}

		public bool IsAccessor {
			get {
				int length = 0;
				string pass_as = "";
				foreach (XmlNode parm in elem.ChildNodes) {
					if (!(parm is XmlElement) || parm.Name != "parameter") {
						continue;
					}
	
					XmlElement p_elem = (XmlElement) parm;
					length++;
					if (length > 1)
						return false;
					if (p_elem.HasAttribute("pass_as"))
						pass_as = p_elem.GetAttribute("pass_as");
				}

				return (length == 1 && pass_as == "out");
			}
		}

		public bool ThrowsException {
			get {
				if ((elem.ChildNodes == null) || (elem.ChildNodes.Count < 1))
					return false;

				XmlElement p_elem = null;
				foreach (XmlNode parm in elem.ChildNodes) {
					if (!(parm is XmlElement) || parm.Name != "parameter")
						continue;
					p_elem = (XmlElement) parm;
				}
				string type = p_elem.GetAttribute("type");
				return (type == "GError**");
			}
		}

		public bool IsVarArgs {
			get {
				return elem.HasAttribute ("va_type");
			}
		}

		public string VAType {
			get {
				return elem.GetAttribute ("va_type");
			}
		}

		public string AccessorReturnType {
			get {
				foreach (XmlNode parm in elem.ChildNodes) {
					if (parm.Name != "parameter") 
						continue;
					XmlElement p_elem = (XmlElement) parm;
					return SymbolTable.GetCSType(p_elem.GetAttribute ("type"));
				}
				return null;
			}
		}

		public string AccessorName {
			get {
				foreach (XmlNode parm in elem.ChildNodes) {
					if (parm.Name != "parameter") 
						continue;
					XmlElement p_elem = (XmlElement) parm;
					Parameter p = new Parameter (p_elem);
					return p.Name;
				}
				return null;
			}
		}

	}
}


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

		public string CSType {
			get {
				return SymbolTable.GetCSType( elem.GetAttribute("type"));
			}
		}

		public string Name {
			get {
				string name = elem.GetAttribute("name");
				switch (name) {
				case "string":
					return "str1ng";
				case "event":
					return "evnt";
				case "object":
					return "objekt";
				default:
					break;
				}

				return name;
			}
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

		public bool Validate ()
		{
			foreach (XmlNode parm in elem.ChildNodes) {
				if (!(parm is XmlElement) || parm.Name != "parameter") {
					continue;
				}

				XmlElement p_elem = (XmlElement) parm;
				string type = p_elem.GetAttribute("type");
				string cs_type = SymbolTable.GetCSType(type);
				string m_type = SymbolTable.GetMarshalType(type);
				string name = MangleName(p_elem.GetAttribute("name"));
				string call_parm = SymbolTable.CallByName(type, name);
				
				if ((cs_type == "") || (name == "") || 
				    (m_type == "") || (call_parm == "")) {
					Console.Write("Name: " + name + " Type: " + type + " ");
					return false;
				}
			}
			
			return true;
		}

		public void CreateSignature (bool is_set)
		{
			signature_types = signature = import_sig = call_string = "";
			bool need_sep = false;
			
			int len = 0;
			XmlElement last_param = null;
			foreach (XmlNode parm in elem.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}
				len++;
				last_param = (XmlElement) parm;
			}

			int i = 0;
			foreach (XmlNode parm in elem.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement p_elem = (XmlElement) parm;
				string type = p_elem.GetAttribute("type");
				string cs_type = SymbolTable.GetCSType(type);
				string m_type = SymbolTable.GetMarshalType(type);
				string name = MangleName(p_elem.GetAttribute("name"));
				string call_parm, call_parm_name;;
				
				if (is_set && i == 0) 
					call_parm_name = "value";
				else
					call_parm_name = name;

				call_parm = SymbolTable.CallByName(type, call_parm_name);
				
				if (p_elem.HasAttribute ("null_ok") && cs_type != "IntPtr" && cs_type != "System.IntPtr" && !SymbolTable.IsStruct (type))
					call_parm = String.Format ("({0} != null) ? {1} : IntPtr.Zero", call_parm_name, call_parm);
				
				if (p_elem.HasAttribute("array")) {
					cs_type += "[]";
					m_type += "[]";
				}

				if (IsVarArgs && i == (len - 1) && VAType == "length_param") {
					cs_type = "params " + cs_type + "[]";
					m_type += "[]";
				}
				
				if (need_sep) {
					call_string += ", ";
					import_sig += ", ";
					if (type != "GError**" && !(IsVarArgs && i == (len - 1) && VAType == "length_param"))
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
						call_string += "out ";
					}
				}
				else if (type == "GError**")
				{
					call_string += "out ";				
					import_sig += "out ";				
				}
				
				if (IsVarArgs && i == (len - 2) && VAType == "length_param")
				{
					call_string += MangleName(last_param.GetAttribute("name")) + ".Length";
				}
				else
				{
					if (type != "GError**") {
						signature += (cs_type + " " + name);
						signature_types += cs_type;
					}
					call_string += call_parm;
				}
				import_sig += (m_type + " " + name);

				i++;
			}
		}

		public void Initialize (StreamWriter sw, bool is_get, string indent)
		{
			string name = "";
			foreach (XmlNode parm in elem.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement p_elem = (XmlElement) parm;

				string c_type = p_elem.GetAttribute ("type");
				string type = SymbolTable.GetCSType(c_type);
				name = MangleName(p_elem.GetAttribute("name"));
				if (is_get) {
					sw.WriteLine (indent + "\t\t\t" + type + " " + name + ";");
				}

				if ((is_get || (p_elem.HasAttribute("pass_as") && p_elem.GetAttribute ("pass_as") == "out")) && (SymbolTable.IsObject (c_type) || SymbolTable.IsBoxed (c_type))) {
					sw.WriteLine(indent + "\t\t\t" + name + " = new " + type + "();");
				}
			}

			if (ThrowsException)
				sw.WriteLine (indent + "\t\t\tIntPtr error = IntPtr.Zero;");
		}
/*
		public void Finish (StreamWriter sw)
		{
			foreach (XmlNode parm in elem.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}
				
				XmlElement p_elem = (XmlElement) parm;
				string c_type = p_elem.GetAttribute ("type");
				string name = MangleName(p_elem.GetAttribute("name"));

				if ((p_elem.HasAttribute("pass_as") && p_elem.GetAttribute ("pass_as") == "out")) {
					string call_parm = SymbolTable.CallByName(c_type, name);
					string local_parm = GetPossibleLocal (call_parm);
					if (call_parm != local_parm)
						sw.WriteLine ("\t\t\t{0} = {1};", call_parm, local_parm);
				}
			}
		}
*/

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
					return MangleName (p_elem.GetAttribute("name"));
				}
				return null;
			}
		}

		private string MangleName(string name)
		{
			switch (name) {
			case "string":
				return "str1ng";
			case "event":
				return "evnt";
			case "object":
				return "objekt";
			default:
				break;
			}

			return name;
		}
	}
}


// GtkSharp.Generation.Parameters.cs - The Parameters Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

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
			signature_types = signature = import_sig = call_string = "";
			bool need_sep = false;
			
			foreach (XmlNode parm in elem.ChildNodes) {
				if (parm.Name != "parameter") {
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
				
				if (p_elem.HasAttribute("array")) {
					cs_type += "[]";
					m_type += "[]";
				}
				
				if (need_sep) {
					call_string += ", ";
					signature += ", ";
					import_sig += ", ";
					signature_types += ":";
				} else {
					need_sep = true;
				}

				if (p_elem.HasAttribute("pass_as")) {
					signature += p_elem.GetAttribute("pass_as") + " ";
				}

				signature += (cs_type + " " + name);
				signature_types += cs_type;
				call_string += call_parm;
				import_sig += (m_type + " " + name);
			}
			
			return true;
		}

		public void Initialize (StreamWriter sw, bool is_get)
		{
			foreach (XmlNode parm in elem.ChildNodes) {
				if (parm.Name != "parameter") {
					continue;
				}

				XmlElement p_elem = (XmlElement) parm;

				string type = SymbolTable.GetCSType(p_elem.GetAttribute ("type"));
				string name = MangleName(p_elem.GetAttribute("name"));
				if (is_get) {
					sw.WriteLine ("\t\t\t" + type + " " + name + ";");
				}

				if (is_get || (p_elem.HasAttribute("pass_as") && p_elem.GetAttribute ("pass_as") == "out")) {
					sw.WriteLine("\t\t\t" + name + " = new " + type + "();"); 
				}
			}
			
		}

		public bool IsAccessor {
			get {
				int length = 0;
				string pass_as;
				foreach (XmlNode parm in elem.ChildNodes) {
					if (parm.Name != "parameter") {
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


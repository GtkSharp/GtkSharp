// GtkSharp.Generation.Parameters.cs - The Parameters Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
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

				signature += (cs_type + " " + name);
				signature_types += cs_type;
				call_string += call_parm;
				import_sig += (m_type + " " + name);
			}
			
			return true;
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


// GtkSharp.Generation.Parameters.cs - The Parameters Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
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
				string cstype = SymbolTable.Table.GetCSType( elem.GetAttribute("type"));
				if (cstype == "void")
					cstype = "System.IntPtr";
				if (IsArray) {
					cstype += "[]";
					cstype = cstype.Replace ("ref ", "");
				}
				return cstype;
			}
		}

		public IGeneratable Generatable {
			get {
				return SymbolTable.Table[CType];
			}
		}

		public bool IsArray {
			get {
				return elem.HasAttribute("array");
			}
		}

		public bool IsEllipsis {
			get {
				return elem.HasAttribute("ellipsis");
			}
		}

		public bool IsLength {
			get {
				
				if (Name.EndsWith("len") || Name.EndsWith("length"))
					switch (CSType) {
					case "int":
					case "uint":
					case "long":
					case "ulong":
					case "short":
					case "ushort": 
						return true;
					default:
						return false;
					}
				else
					return false;
			}
		}

		public bool IsString {
			get {
				return (CSType == "string");
			}
		}

		public bool IsUserData {
			get {
				return CType == "gpointer" && (Name.EndsWith ("data") || Name.EndsWith ("data_or_owner"));
			}
		}

		public string MarshalType {
			get {
				string type = SymbolTable.Table.GetMarshalType( elem.GetAttribute("type"));
				if (type == "void")
					type = "System.IntPtr";
				if (IsArray) {
					type += "[]";
					type = type.Replace ("ref ", "");
				}
				return type;
			}
		}

		public string Name {
			get {
				return SymbolTable.Table.MangleName (elem.GetAttribute("name"));
			}
		}

		public bool NullOk {
			get {
				return elem.HasAttribute ("null_ok");
			}
		}

		public string PassAs {
			get {
				if (elem.HasAttribute ("pass_as"))
					return elem.GetAttribute ("pass_as");

				if (IsArray)
					return "";

				if (Generatable is SimpleGen && !(Generatable is ConstStringGen) && CType.EndsWith ("*") && !CSType.EndsWith ("IntPtr"))
					return "out";

				return "";
			}
		}

		public string StudlyName {
			get {
				string name = elem.GetAttribute("name");
				string[] segs = name.Split('_');
				string studly = "";
				foreach (string s in segs) {
					if (s.Trim () == "")
						continue;
					studly += (s.Substring(0,1).ToUpper() + s.Substring(1));
				}
				return studly;
				
			}
		}

	}

	public class Parameters  {
		
		private ArrayList param_list;
		private XmlElement elem;
		private string impl_ns;
		private string import_sig = "";
		private string call_string = "";
		private string signature = "";
		private string signature_types = "";
		private bool hide_data;
		private bool is_static;

		public Parameters (XmlElement elem, string impl_ns) {
			
			this.elem = elem;
			this.impl_ns = impl_ns;
			param_list = new ArrayList ();
			foreach (XmlNode node in elem.ChildNodes) {
				XmlElement parm = node as XmlElement;
				if (parm != null && parm.Name == "parameter")
					param_list.Add (new Parameter (parm));
			}
		}

		public string CallString {
			get {
				return call_string;
			}
		}

		public int Count {
			get {
				return param_list.Count;
			}
		}

		public Parameter this [int idx] {
			get {
				return param_list [idx] as Parameter;
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

		public bool Static {
			set { is_static = value; }
		}

		public bool Validate ()
		{
			foreach (Parameter p in param_list) {
				
				if (p.IsEllipsis) {
					Console.Write("Ellipsis parameter ");
					return false;
				}

				if ((p.CSType == "") || (p.Name == "") || 
				    (p.MarshalType == "") || (SymbolTable.Table.CallByName(p.CType, p.Name) == "")) {
					Console.Write("Name: " + p.Name + " Type: " + p.CType + " ");
					return false;
				}
			}
			
			return true;
		}

		public void CreateSignature (bool is_set)
		{
			import_sig = call_string = signature = signature_types = "";	
			bool need_sep = false;
			bool has_callback = hide_data;
			
			SymbolTable table = SymbolTable.Table;

			for (int i = 0; i < Count; i++) {

				string type = this [i].CType;
				string cs_type = this [i].CSType;
				string m_type = this [i].MarshalType;
				string name = this [i].Name;

				if (i > 0 && this [i - 1].IsString && this [i].IsLength) {
					call_string += ", " + this [i - 1].Name + ".Length";
					import_sig += ", " + m_type + " " + name;
					continue;
				}

				string call_parm_name = name;
				if (is_set && i == 0) 
					call_parm_name = "value";

				string call_parm;
				if (table.IsCallback (type)) {
					has_callback = true;
					call_parm = table.CallByName (type, call_parm_name + "_wrapper");
				} else
					call_parm = table.CallByName(type, call_parm_name);
				
				if (this [i].NullOk && !cs_type.EndsWith ("IntPtr") && !table.IsStruct (type))
					call_parm = String.Format ("({0} != null) ? {1} : {2}", call_parm_name, call_parm, table.IsCallback (type) ? "null" : "IntPtr.Zero");
				
				if (this [i].IsArray)
					call_parm = call_parm.Replace ("ref ", "");

				if (IsVarArgs && i == (Count - 1) && VAType == "length_param") {
					cs_type = "params " + cs_type + "[]";
					m_type += "[]";
				}
				
				if (need_sep) {
					call_string += ", ";
					import_sig += ", ";
					if (type != "GError**"  && !(IsVarArgs && i == (Count - 1) && VAType == "length_param"))
					{
						signature += ", ";
						signature_types += ":";
					}
				} else {
					need_sep = true;
				}

				if (type == "GError**") {
					call_string += "out ";				
					import_sig += "out ";				
				} else if (this [i].PassAs != "" && !IsVarArgs) {
					signature += this [i].PassAs + " ";
					// We only need to do this for value types
					if (type != "GError**" && m_type != "IntPtr" && m_type != "System.IntPtr")
					{
						import_sig += this [i].PassAs + " ";
						call_string += this [i].PassAs + " ";
					}
					
					if (table.IsEnum (type))
						call_parm = name + "_as_int";
					else if (table.IsObject (type) || table.IsOpaque (type) || cs_type == "GLib.Value") {
						call_parm = this [i].PassAs + " " + call_parm.Replace (".Handle", "_handle");
						import_sig += this [i].PassAs + " ";
					}
				}

				if (IsVarArgs && i == (Count - 2) && VAType == "length_param") {
					call_string += this [Count - 1].Name + ".Length";
				} else {
					if (!(type == "GError**" || (has_callback && (type == "gpointer" || type == "void*") && (i == Count - 1) && (name.EndsWith ("data") || name.EndsWith ("data_or_owner"))))) {
						signature += (cs_type + " " + name);
						signature_types += cs_type;
					} else if (type == "GError**") {
						call_parm = call_parm.Replace (name, "error");
					} else if ((type == "gpointer" || type == "void*") && (i == Count - 1) && (name.EndsWith ("data") || name.EndsWith ("data_or_owner"))) {
						call_parm = "IntPtr.Zero"; 
					}
					
					call_string += call_parm;
				}
				if (table.IsCallback (type))
					m_type = impl_ns + "Sharp" + m_type.Substring(m_type.IndexOf("."));
				import_sig += (m_type + " " + name);
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

		public void Initialize (GenerationInfo gen_info, bool is_get, bool is_set, string indent)
		{
			StreamWriter sw = gen_info.Writer;
			foreach (Parameter p in param_list) {

				IGeneratable gen = p.Generatable;
				string name = p.Name;
				if (is_set)
					name = "value";

				if (is_get) {
					sw.WriteLine (indent + "\t\t\t" + p.CSType + " " + name + ";");
					if (gen is ObjectGen || gen is OpaqueGen || p.CSType == "GLib.Value")
						sw.WriteLine(indent + "\t\t\t" + name + " = new " + p.CSType + "();");
				}

				if ((is_get || p.PassAs == "out") && (gen is ObjectGen || gen is OpaqueGen || p.CSType == "GLib.Value"))
					sw.WriteLine(indent + "\t\t\tIntPtr " + name + "_handle;");

				if (p.PassAs == "out" && gen is EnumGen)
					sw.WriteLine(indent + "\t\t\tint " + name + "_as_int;");

				if (gen is CallbackGen) {
					CallbackGen cbgen = gen as CallbackGen;
					string wrapper = cbgen.GenWrapper(impl_ns, gen_info);
					sw.WriteLine (indent + "\t\t\t{0} {1}_wrapper = null;", wrapper, name);
					sw.Write (indent + "\t\t\t");
					if (p.NullOk)
						sw.Write ("if ({0} != null) ", name);
					sw.WriteLine ("{1}_wrapper = new {0} ({1}, {2});", wrapper, name, is_static ? "null" : "this");
				}
			}

			if (ThrowsException)
				sw.WriteLine (indent + "\t\t\tIntPtr error = IntPtr.Zero;");
		}

		public void Finish (StreamWriter sw, string indent)
		{
			bool ref_owned_needed = true;
			foreach (Parameter p in param_list) {

				if (p.PassAs == "out" && p.Generatable is EnumGen) {
					sw.WriteLine(indent + "\t\t\t" + p.Name + " = (" + p.CSType + ") " + p.Name + "_as_int;");
				}

				IGeneratable gen = p.Generatable;
				if (ref_owned_needed && gen is ObjectGen && p.PassAs == "out") {
					ref_owned_needed = false;
					sw.WriteLine(indent + "\t\t\tbool ref_owned = false;");
				}

				if (p.PassAs == "out" && (gen is ObjectGen || gen is OpaqueGen || p.CSType == "GLib.Value"))
					sw.WriteLine(indent + "\t\t\t" + p.Name + " = " + gen.FromNativeReturn (p.Name + "_handle") + ";");
			}
		}


		public void HandleException (StreamWriter sw, string indent)
		{
			if (!ThrowsException)
				return;
			sw.WriteLine (indent + "\t\t\tif (error != IntPtr.Zero) throw new GLib.GException (error);");
		}
		
		public bool IsAccessor {
			get {
				return Count == 1 && this [0].PassAs == "out";
			}
		}

		public bool ThrowsException {
			get {
				if (Count < 1)
					return false;

				return this [Count - 1].CType == "GError**";
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
				if (Count > 0)
					return this [0].CSType;
				else
					return null;
			}
		}

		public string AccessorName {
			get {
				if (Count > 0)
					return this [0].Name;
				else
					return null;
			}
		}

	}
}


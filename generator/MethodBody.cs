// GtkSharp.Generation.MethodBody.cs - The MethodBody Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2003 Mike Kestner, (c) 2003 Novell, Inc.

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;

	public class MethodBody  {
		
		Parameters parameters;
		private string impl_ns;

		public MethodBody (Parameters parms, string impl_ns) {
			
			parameters = parms;
			this.impl_ns = impl_ns;
		}

		private bool UsesHandle (IGeneratable igen)
		{
			return igen is ManualGen || igen is ObjectGen || igen is InterfaceGen || igen is OpaqueGen;
		}

		private string CastFromInt (string type)
		{
			return type != "int" ? "(" + type + ") " : "";
		}

		private string CallArrayLength (Parameter array, Parameter length)
		{
			string result = array.NullOk ? array.Name + " != null ? " : "";
			result += CastFromInt (length.CSType) + array.Name + ".Length";
			result += array.NullOk ? ": 0" : "";
			return result;
		}

		public string GetCallString (bool is_set)
		{
			if (parameters == null)
				return "";

			string[] result = new string [parameters.Count];
			for (int i = 0; i < parameters.Count; i++) {
				Parameter p = parameters [i];
				IGeneratable igen = p.Generatable;

				if (p.IsCount) {
					if (i > 0 && parameters [i - 1].IsArray) {
						result[i] = CallArrayLength (parameters[i - 1], p);
						continue;
					} else if (i < parameters.Count - 1 && parameters [i + 1].IsArray) {
						result[i] = CallArrayLength (parameters[i + 1], p);
						continue;
					}
				}

				if (i > 0 && parameters [i - 1].IsString && p.IsLength) {
					result[i] = CastFromInt (p.CSType) + parameters [i - 1].Name + ".Length";
					continue;
				}

				string call_parm = p.CallByName (is_set && i == 0 ? "value" : p.Name);

				if (p.CType == "GError**") {
					result [i] += "out ";				
				} else if (p.PassAs != "") {
					if (!p.MarshalType.EndsWith ("IntPtr")) 
						result [i] += p.PassAs + " ";
					
					if (igen is EnumGen)
						call_parm = p.Name + "_as_int";
					else if (UsesHandle (igen) || p.CSType == "GLib.Value") {
						call_parm = p.PassAs + " " + call_parm.Replace (".Handle", "_handle");
					}
				}

				if (p.CType == "GError**") {
					call_parm = call_parm.Replace (p.Name, "error");
				} else if (p.IsUserData && !parameters.HideData && (i == parameters.Count - 1)) {
					call_parm = "IntPtr.Zero"; 
				}

				result [i] += call_parm;
			}

			string call_string = String.Join (", ", result);
			call_string = call_string.Replace ("out ref", "out");
			call_string = call_string.Replace ("ref ref", "ref");
			return call_string;
		}

		public void Initialize (GenerationInfo gen_info, bool is_get, bool is_set, string indent)
		{
			if (parameters == null)
				return;

			StreamWriter sw = gen_info.Writer;
			for (int i = 0; i < parameters.Count; i++) {
				Parameter p = parameters [i];

				IGeneratable gen = p.Generatable;
				string name = p.Name;
				if (is_set)
					name = "value";

				if (is_get) {
					sw.WriteLine (indent + "\t\t\t" + p.CSType + " " + name + ";");
					if (gen is ObjectGen || gen is OpaqueGen || p.CSType == "GLib.Value")
						sw.WriteLine(indent + "\t\t\t" + name + " = new " + p.CSType + "();");
				}

				if ((is_get || p.PassAs == "out") && (UsesHandle (gen) || p.CSType == "GLib.Value"))
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
					sw.WriteLine ("{1}_wrapper = new {0} ({1}, {2});", wrapper, p.Name, parameters.Static ? "null" : "this");
				}
			}

			if (ThrowsException)
				sw.WriteLine (indent + "\t\t\tIntPtr error = IntPtr.Zero;");
		}

		public void Finish (StreamWriter sw, string indent)
		{
			if (parameters == null)
				return;

			bool ref_owned_needed = true;
			for (int i = 0; i < parameters.Count; i++) {
				Parameter p = parameters [i];

				if (p.PassAs == "out" && p.Generatable is EnumGen) {
					sw.WriteLine(indent + "\t\t\t" + p.Name + " = (" + p.CSType + ") " + p.Name + "_as_int;");
				}

				IGeneratable gen = p.Generatable;
				if (ref_owned_needed && (gen is ObjectGen || gen is InterfaceGen) && p.PassAs == "out") {
					ref_owned_needed = false;
					sw.WriteLine(indent + "\t\t\tbool ref_owned = false;");
				}

				if (p.PassAs == "out" && (UsesHandle (gen) || p.CSType == "GLib.Value"))
					sw.WriteLine(indent + "\t\t\t" + p.Name + " = " + gen.FromNativeReturn (p.Name + "_handle") + ";");
			}
		}

		public void HandleException (StreamWriter sw, string indent)
		{
			if (!ThrowsException)
				return;
			sw.WriteLine (indent + "\t\t\tif (error != IntPtr.Zero) throw new GLib.GException (error);");
		}
		
		public bool ThrowsException {
			get {
				if (parameters == null || parameters.Count < 1)
					return false;

				return parameters [parameters.Count - 1].CType == "GError**";
			}
		}
	}
}


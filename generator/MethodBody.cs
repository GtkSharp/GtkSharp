// GtkSharp.Generation.MethodBody.cs - The MethodBody Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
// Copyright (c) 2003-2004 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;

	public class MethodBody  {
		
		Parameters parameters;

		public MethodBody (Parameters parms) 
		{
			parameters = parms;
		}

		private string CastFromInt (string type)
		{
			return type != "int" ? "(" + type + ") " : "";
		}

		private string CallArrayLength (string array_name, Parameter length)
		{
			string result = array_name + " != null ? ";
			result += CastFromInt (length.CSType) + array_name + ".Length";
			result += ": 0";
			return length.Generatable.CallByName (result);
		}

		public string GetCallString (bool is_set)
		{
			if (parameters.Count == 0)
				return "";

			string[] result = new string [parameters.Count];
			for (int i = 0; i < parameters.Count; i++) {
				Parameter p = parameters [i];
				IGeneratable igen = p.Generatable;
				string name = (i == 0 && is_set) ? "value" : p.Name;

				if (p.IsCount) {
					if (i > 0 && parameters [i - 1].IsArray) {
						string array_name = (i == 1 && is_set) ? "value" : parameters [i - 1].Name;
						result[i] = CallArrayLength (array_name, p);
						continue;
					} else if (i < parameters.Count - 1 && parameters [i + 1].IsArray) {
						string array_name = (i == 0 && is_set) ? "value" : parameters [i + 1].Name;
						result[i] = CallArrayLength (array_name, p);
						continue;
					}
				} else if (i > 0 && parameters [i - 1].IsString && p.IsLength) {
					string string_name = (i == 1 && is_set) ? "value" : parameters [i - 1].Name;
					result[i] = igen.CallByName (CastFromInt (p.CSType) + "System.Text.Encoding.UTF8.GetByteCount (" +  string_name + ")");
					continue;
				} else if (p.IsArray && p.MarshalType != p.CSType) {
					result[i] = "native_" + p.Name;
					continue;
				}

				string call_parm = p.CallByName (name);

				if (p.CType == "GError**") {
					result [i] += "out ";				
				} else if (p.PassAs != "") {
					result [i] += p.PassAs + " ";

 					if (p.CSType != p.MarshalType && !(igen is StructBase || igen is ByRefGen))
						call_parm = p.Name + "_as_native";
				} else if (igen is IManualMarshaler)
					call_parm = p.Name + "_as_native";

				if (p.CType == "GError**") {
					call_parm = call_parm.Replace (p.Name, "error");
				} else if (p.IsUserData && parameters.IsHidden (p) && !parameters.HideData &&
					   (i == 0 || parameters [i - 1].Scope != "notified")) {
					call_parm = "IntPtr.Zero"; 
				}

				result [i] += call_parm;
			}

			string call_string = String.Join (", ", result);
			call_string = call_string.Replace ("out ref", "out");
			call_string = call_string.Replace ("out out ", "out ");
			call_string = call_string.Replace ("ref ref", "ref");
			return call_string;
		}

		public void Initialize (GenerationInfo gen_info, bool is_get, bool is_set, string indent)
		{
			if (parameters.Count == 0)
				return;

			StreamWriter sw = gen_info.Writer;
			for (int i = 0; i < parameters.Count; i++) {
				Parameter p = parameters [i];

				IGeneratable gen = p.Generatable;
				string name = p.Name;
				if (is_set)
					name = "value";

				if ((is_get || p.PassAs == "out") && p.CSType != p.MarshalType && !(gen is StructBase || gen is ByRefGen))
					sw.WriteLine(indent + "\t\t\t" + gen.MarshalType + " " + name + "_as_native;");
				else if (p.IsArray && p.MarshalType != p.CSType) {
					sw.WriteLine(indent + "\t\t\tint cnt_" + p.Name + " = {0} == null ? 0 : {0}.Length;", name);
					sw.WriteLine(indent + "\t\t\t{0}[] native_" + p.Name + " = new {0} [cnt_{1}];", p.MarshalType.TrimEnd('[', ']'), p.Name);
					sw.WriteLine(indent + "\t\t\tfor (int i = 0; i < cnt_{0}; i++)", p.Name);
					if (gen is IManualMarshaler)
						sw.WriteLine(indent + "\t\t\t\tnative_{0} [i] = {1};", p.Name, (gen as IManualMarshaler).AllocNative (name + "[i]"));
					else
						sw.WriteLine(indent + "\t\t\t\tnative_{0} [i] = {1};", p.Name, p.CallByName (name + "[i]"));
				} else if (gen is IManualMarshaler)
					sw.WriteLine(indent + "\t\t\t" + gen.MarshalType + " " + p.Name + "_as_native = " + (gen as IManualMarshaler).AllocNative (name) + ";");


				if (gen is CallbackGen) {
					CallbackGen cbgen = gen as CallbackGen;
					string wrapper = cbgen.GenWrapper(gen_info);
					switch (p.Scope) {
					case "notified":
						sw.WriteLine (indent + "\t\t\t{0} {1}_wrapper;", wrapper, name);
						sw.WriteLine (indent + "\t\t\tIntPtr {0};", parameters [i + 1].Name);
						sw.WriteLine (indent + "\t\t\t{0} {1};", parameters [i + 2].CSType, parameters [i + 2].Name);
						sw.WriteLine (indent + "\t\t\tif ({0} == null) {{", name);
						sw.WriteLine (indent + "\t\t\t\t{0}_wrapper = null;", name);
						sw.WriteLine (indent + "\t\t\t\t{0} = IntPtr.Zero;", parameters [i + 1].Name);
						sw.WriteLine (indent + "\t\t\t\t{0} = null;", parameters [i + 2].Name);
						sw.WriteLine (indent + "\t\t\t} else {");

						sw.WriteLine (indent + "\t\t\t\t{0}_wrapper = new {1} ({0});", name, wrapper);
						sw.WriteLine (indent + "\t\t\t\t{0} = (IntPtr) GCHandle.Alloc ({1}_wrapper);", parameters [i + 1].Name, name);
						sw.WriteLine (indent + "\t\t\t\t{0} = GLib.DestroyHelper.NotifyHandler;", parameters [i + 2].Name, parameters [i + 2].CSType);
						sw.WriteLine (indent + "\t\t\t}");
						break;

					case "call":
					default:
						if (p.Scope == String.Empty)
							Console.WriteLine ("Defaulting " + gen.Name + " param to 'call' scope in method " + gen_info.CurrentMember);
						sw.WriteLine (indent + "\t\t\t{0} {1}_wrapper = new {0} ({1});", wrapper, name);
						break;
					}
						
				}
			}

			if (ThrowsException)
				sw.WriteLine (indent + "\t\t\tIntPtr error = IntPtr.Zero;");
		}

		public void InitAccessor (StreamWriter sw, Signature sig, string indent)
		{
			sw.WriteLine (indent + "\t\t\t" + sig.AccessorType + " " + sig.AccessorName + ";");
		}

		public void Finish (StreamWriter sw, string indent)
		{
			for (int i = 0; i < parameters.Count; i++) {
				Parameter p = parameters [i];

				IGeneratable gen = p.Generatable;

				if (p.PassAs == "out" && p.CSType != p.MarshalType && !(gen is StructBase || gen is ByRefGen))
					sw.WriteLine(indent + "\t\t\t" + p.Name + " = " + p.FromNative (p.Name + "_as_native") + ";");
				else if (p.IsArray && gen is IManualMarshaler) {
					sw.WriteLine(indent + "\t\t\tfor (int i = 0; i < native_" + p.Name + ".Length; i++)");
					sw.WriteLine(indent + "\t\t\t\t" + (gen as IManualMarshaler).ReleaseNative ("native_" + p.Name + "[i]") + ";");
				} else if (gen is IManualMarshaler)
					sw.WriteLine(indent + "\t\t\t" + (gen as IManualMarshaler).ReleaseNative (p.Name + "_as_native") + ";");
			}
		}

		public void FinishAccessor (StreamWriter sw, Signature sig, string indent)
		{
			sw.WriteLine (indent + "\t\t\treturn " + sig.AccessorName + ";");
		}

		public void HandleException (StreamWriter sw, string indent)
		{
			if (!ThrowsException)
				return;
			sw.WriteLine (indent + "\t\t\tif (error != IntPtr.Zero) throw new GLib.GException (error);");
		}
		
		public bool ThrowsException {
			get {
				if (parameters.Count < 1)
					return false;

				return parameters [parameters.Count - 1].CType == "GError**";
			}
		}
	}
}


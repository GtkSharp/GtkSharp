// GtkSharp.Generation.Signal.cs - The Signal Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner 
// Copyright (c) 2003-2005 Novell, Inc.
// Copyright (c) 2007 Novell, Inc.
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
	using System.Xml;

	public class Signal {

		bool marshaled;
		string name;
		XmlElement elem;
		ReturnValue retval;
		Parameters parms;
		ClassBase container_type;

		public Signal (XmlElement elem, ClassBase container_type)
		{
			this.elem = elem;
			name = elem.GetAttribute ("name");
			marshaled = elem.GetAttribute ("manual") == "true";
			retval = new ReturnValue (elem ["return-type"]);
			parms = new Parameters (elem["parameters"]);
			this.container_type = container_type;
		}

		bool Marshaled {
			get { return marshaled; }
		}

		public string Name {
			get {
				return name; 
			}
			set {
				name = value;
			}
		}

		public bool Validate ()
		{
			if (Name == "") {
				Console.Write ("Nameless signal ");
				Statistics.ThrottledCount++;
				return false;
			}
			
			if (!parms.Validate () || !retval.Validate ()) {
				Console.Write (" in signal " + Name + " ");
				Statistics.ThrottledCount++;
				return false;
			}

			return true;
		}

 		public void GenerateDecl (StreamWriter sw)
 		{
			if (elem.HasAttribute("new_flag") || (container_type != null && container_type.GetSignalRecursively (Name) != null))
				sw.Write("new ");

 			sw.WriteLine ("\t\tevent " + EventHandlerQualifiedName + " " + Name + ";");
		}

		public string CName {
			get {
				return "\"" + elem.GetAttribute("cname") + "\"";
			}
		}

		string CallbackSig {
			get {
				string result = "";
				for (int i = 0; i < parms.Count; i++) {
					if (i > 0)
						result += ", ";

					Parameter p = parms [i];
					if (p.PassAs != "" && !(p.Generatable is StructBase))
						result += p.PassAs + " ";
					result += (p.MarshalType + " arg" + i);
				}

				return result;
			}
		}

		string CallbackName {
			get { return Name + "SignalCallback"; }
		}

		string DelegateName {
			get { return Name + "SignalDelegate"; }
		}

                private string EventArgsName {
                        get {
                                if (IsEventHandler)
                                        return "EventArgs";
                                else
                                        return Name + "Args";
                        }
                }
                                                                                                                        
                private string EventArgsQualifiedName {
                        get {
                                if (IsEventHandler)
                                        return "System.EventArgs";
                                else
                                        return container_type.NS + "." + Name + "Args";
                        }
                }
                                                                                                                        
                private string EventHandlerName {
                        get {
                                if (IsEventHandler)
                                        return "EventHandler";
                                else if (SymbolTable.Table [container_type.NS + Name + "Handler"] != null)
                                        return Name + "EventHandler";
				else
                                        return Name + "Handler";
                        }
                }
                                                                                                                        
                private string EventHandlerQualifiedName {
                        get {
                                if (IsEventHandler)
                                        return "System.EventHandler";
                                else
                                        return container_type.NS + "." + EventHandlerName;
                        }
                }

		string ClassFieldName {
			get {
				return elem.HasAttribute ("field_name") ? elem.GetAttribute("field_name") : String.Empty;
			}
		}

		private bool HasOutParams {
			get {
				foreach (Parameter p in parms) {
					if (p.PassAs == "out")
						return true;
				}
				return false;
			}
		}

		private bool IsEventHandler {
			get {
				return retval.CSType == "void" && parms.Count == 1 && (parms [0].Generatable is ObjectGen || parms [0].Generatable is InterfaceGen);
			}
		}

		private bool IsVoid {
			get {
				return retval.CSType == "void";
			}
		}

		private string ReturnGType {
			get {
				IGeneratable igen = SymbolTable.Table [retval.CType];

				if (igen is ObjectGen)
					return "GLib.GType.Object";
				if (igen is BoxedGen)
					return retval.CSType + ".GType";
				if (igen is EnumGen)
					return retval.CSType + "GType.GType";

				switch (retval.CSType) {
				case "bool":
					return "GLib.GType.Boolean";
				case "string":
					return "GLib.GType.String";
				case "int":
					return "GLib.GType.Int";
				default:
					throw new Exception (retval.CSType);
				}
			}
		}

		public string GenArgsInitialization (StreamWriter sw)
		{
			if (parms.Count > 1)
				sw.WriteLine("\t\t\t\targs.Args = new object[" + (parms.Count - 1) + "];");
			string finish = "";
			for (int idx = 1; idx < parms.Count; idx++) {
				Parameter p = parms [idx];
				IGeneratable igen = p.Generatable;
				if (p.PassAs != "out") {
					if (igen is ManualGen) {
						sw.WriteLine("\t\t\t\tif (arg{0} == IntPtr.Zero)", idx);
						sw.WriteLine("\t\t\t\t\targs.Args[{0}] = null;", idx - 1);
						sw.WriteLine("\t\t\t\telse {");
						sw.WriteLine("\t\t\t\t\targs.Args[" + (idx - 1) + "] = " + p.FromNative ("arg" + idx)  + ";");
						sw.WriteLine("\t\t\t\t}");
					} else
						sw.WriteLine("\t\t\t\targs.Args[" + (idx - 1) + "] = " + p.FromNative ("arg" + idx)  + ";");
				}
				if (igen is StructBase && p.PassAs == "ref")
					finish += "\t\t\t\tif (arg" + idx + " != IntPtr.Zero) System.Runtime.InteropServices.Marshal.StructureToPtr (args.Args[" + (idx-1) + "], arg" + idx + ", false);\n";
				else if (p.PassAs != "")
					finish += "\t\t\t\targ" + idx + " = " + igen.ToNativeReturn ("((" + p.CSType + ")args.Args[" + (idx - 1) + "])") + ";\n";
			}
			return finish;
		}

		public void GenArgsCleanup (StreamWriter sw, string finish)
		{
			if (IsVoid && finish.Length == 0)
				return;

			sw.WriteLine("\n\t\t\ttry {");
			sw.Write (finish);
			if (!IsVoid) {
				if (retval.CSType == "bool") {
					sw.WriteLine ("\t\t\t\tif (args.RetVal == null)");
					sw.WriteLine ("\t\t\t\t\treturn false;");
				}
				sw.WriteLine("\t\t\t\treturn " + SymbolTable.Table.ToNativeReturn (retval.CType, "((" + retval.CSType + ")args.RetVal)") + ";");
			}
			sw.WriteLine("\t\t\t} catch (Exception) {");
			sw.WriteLine ("\t\t\t\tException ex = new Exception (\"args.RetVal or 'out' property unset or set to incorrect type in " + EventHandlerQualifiedName + " callback\");");
			sw.WriteLine("\t\t\t\tGLib.ExceptionManager.RaiseUnhandledException (ex, true);");
			
			sw.WriteLine ("\t\t\t\t// NOTREACHED: above call doesn't return.");
			sw.WriteLine ("\t\t\t\tthrow ex;");
			sw.WriteLine("\t\t\t}");
		}

		public void GenCallback (StreamWriter sw)
		{
			if (IsEventHandler)
				return;

			sw.WriteLine ("\t\t[GLib.CDeclCallback]");
			sw.WriteLine ("\t\tdelegate " + retval.ToNativeType + " " + DelegateName + " (" + CallbackSig + ", IntPtr gch);");
			sw.WriteLine ();
			sw.WriteLine ("\t\tstatic " + retval.ToNativeType + " " + CallbackName + " (" + CallbackSig + ", IntPtr gch)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\t{0} args = new {0} ();", EventArgsQualifiedName);
			sw.WriteLine("\t\t\ttry {");
			sw.WriteLine("\t\t\t\tGLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;");
			sw.WriteLine("\t\t\t\tif (sig == null)");
			sw.WriteLine("\t\t\t\t\tthrow new Exception(\"Unknown signal GC handle received \" + gch);");
			sw.WriteLine();
			string finish = GenArgsInitialization (sw);
			sw.WriteLine("\t\t\t\t{0} handler = ({0}) sig.Handler;", EventHandlerQualifiedName);
			sw.WriteLine("\t\t\t\thandler (GLib.Object.GetObject (arg0), args);");
			sw.WriteLine("\t\t\t} catch (Exception e) {");
			sw.WriteLine("\t\t\t\tGLib.ExceptionManager.RaiseUnhandledException (e, false);");
			sw.WriteLine("\t\t\t}");
			GenArgsCleanup (sw, finish);
			sw.WriteLine("\t\t}");
			sw.WriteLine();
		}

		private bool NeedNew (ClassBase implementor)
		{
			return elem.HasAttribute ("new_flag") ||
				(container_type != null && container_type.GetSignalRecursively (Name) != null) ||
				(implementor != null && implementor.GetSignalRecursively (Name) != null);
		}

		public void GenEventHandler (GenerationInfo gen_info)
		{
			if (IsEventHandler)
				return;

			string ns = container_type.NS;

			StreamWriter sw = gen_info.OpenStream (EventHandlerName);
			
			sw.WriteLine ("namespace " + ns + " {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");

			sw.WriteLine ();
			sw.WriteLine ("\tpublic delegate void " + EventHandlerName + "(object o, " + EventArgsName + " args);");
			sw.WriteLine ();
			sw.WriteLine ("\tpublic class " + EventArgsName + " : GLib.SignalArgs {");
			for (int i = 1; i < parms.Count; i++) {
				sw.WriteLine ("\t\tpublic " + parms[i].CSType + " " + parms[i].StudlyName + "{");
				if (parms[i].PassAs != "out") {
					sw.WriteLine ("\t\t\tget {");
					sw.WriteLine ("\t\t\t\treturn (" + parms[i].CSType + ") Args[" + (i - 1) + "];");
					sw.WriteLine ("\t\t\t}");
				}
				if (parms[i].PassAs != "") {
					sw.WriteLine ("\t\t\tset {");
					sw.WriteLine ("\t\t\t\tArgs[" + (i - 1) + "] = (" + parms[i].CSType + ")value;");
					sw.WriteLine ("\t\t\t}");
				}
				sw.WriteLine ("\t\t}");
				sw.WriteLine ();
			}
			sw.WriteLine ("\t}");
			sw.WriteLine ("}");
			sw.Close ();
		}

		private void GenVMDeclaration (StreamWriter sw, ClassBase implementor)
		{
			VMSignature vmsig = new VMSignature (parms);
			sw.WriteLine ("\t\t[GLib.DefaultSignalHandler(Type=typeof(" + (implementor != null ? implementor.QualifiedName : container_type.QualifiedName) + "), ConnectionMethod=\"Override" + Name +"\")]");
			sw.Write ("\t\tprotected ");
			if (NeedNew (implementor))
				sw.Write ("new ");
			sw.WriteLine ("virtual {0} {1} ({2})", retval.CSType, "On" + Name, vmsig.ToString ());
		}

		private string CastFromInt (string type)
		{
			return type != "int" ? "(" + type + ") " : "";
		}

		private string GlueCallString {
			get {
				string result = "Handle";

				for (int i = 1; i < parms.Count; i++) {
					Parameter p = parms [i];
					IGeneratable igen = p.Generatable;

					if (i > 1 && parms [i - 1].IsString && p.IsLength && p.PassAs == String.Empty) {
						string string_name = parms [i - 1].Name;
						result += ", " + igen.CallByName (CastFromInt (p.CSType) + "System.Text.Encoding.UTF8.GetByteCount (" +  string_name + ")");
						continue;
					}

					p.CallName = p.Name;
					string call_parm = p.CallString;

					if (p.IsUserData && parms.IsHidden (p) && !parms.HideData && (i == 1 || parms [i - 1].Scope != "notified")) {
						call_parm = "IntPtr.Zero"; 
					}

					result += ", " + call_parm;
				}
				return result;
			}
		}

		private string GlueSignature {
			get {
				string result = String.Empty;
				for (int i = 0; i < parms.Count; i++)
					result += parms[i].CType.Replace ("const-", "const ") + " " + parms[i].Name + (i == parms.Count-1 ? "" : ", ");
				return result;
			}
		}

		private string DefaultGlueValue {
			get {
				string val = retval.DefaultValue;
				switch (val) {
				case "null":
					return "NULL";
				case "false":
					return "FALSE";
				case "true":
					return "TRUE";
				default:
					return val;
				}
			}
		}

		private void GenGlueVirtualMethod (GenerationInfo gen_info)
		{
			StreamWriter glue = gen_info.GlueWriter;
			string glue_name = String.Format ("{0}sharp_{1}_base_{2}", container_type.NS.ToLower ().Replace (".", "_"), container_type.Name.ToLower (), ClassFieldName);
			glue.WriteLine ("{0} {1} ({2});\n", retval.CType, glue_name, GlueSignature);
			glue.WriteLine ("{0}\n{1} ({2})", retval.CType, glue_name, GlueSignature);
			glue.WriteLine ("{");
			glue.WriteLine ("\t{0}Class *klass = ({0}Class *) get_threshold_class (G_OBJECT ({1}));", container_type.CName, parms[0].Name);
			glue.Write ("\tif (klass->{0})\n\t\t", ClassFieldName);
			if (!IsVoid)
				glue.Write ("return ");
			glue.Write ("(* klass->{0}) (", ClassFieldName);
			for (int i = 0; i < parms.Count; i++)
				glue.Write (parms[i].Name + (i == parms.Count - 1 ? "" : ", "));
			glue.WriteLine (");");
			if (!IsVoid)
				glue.WriteLine ("\treturn " + DefaultGlueValue + ";");
			glue.WriteLine ("}");

			StreamWriter sw = gen_info.Writer;
			sw.WriteLine ("\t\t[DllImport (\"{0}\")]", gen_info.GluelibName);
			sw.WriteLine ("\t\tstatic extern {0} {1} ({2});\n", retval.MarshalType, glue_name, parms.ImportSignature);
			GenVMDeclaration (sw, null);
			sw.WriteLine ("\t\t{");
			MethodBody body = new MethodBody (parms);
			body.Initialize (gen_info, false, false, String.Empty);
			sw.WriteLine ("\t\t\t{0}{1} ({2});", IsVoid ? "" : retval.MarshalType + " __ret = ", glue_name, GlueCallString);
			body.Finish (sw, "");
			if (!IsVoid)
				sw.WriteLine ("\t\t\treturn {0};", retval.FromNative ("__ret"));
			sw.WriteLine ("\t\t}\n");
		}

		private void GenChainVirtualMethod (StreamWriter sw, ClassBase implementor)
		{
			GenVMDeclaration (sw, implementor);
			sw.WriteLine ("\t\t{");
			if (IsVoid)
				sw.WriteLine ("\t\t\tGLib.Value ret = GLib.Value.Empty;");
			else
				sw.WriteLine ("\t\t\tGLib.Value ret = new GLib.Value (" + ReturnGType + ");");

			sw.WriteLine ("\t\t\tGLib.ValueArray inst_and_params = new GLib.ValueArray (" + parms.Count + ");");
			sw.WriteLine ("\t\t\tGLib.Value[] vals = new GLib.Value [" + parms.Count + "];");
			sw.WriteLine ("\t\t\tvals [0] = new GLib.Value (this);");
			sw.WriteLine ("\t\t\tinst_and_params.Append (vals [0]);");
			string cleanup = "";
			for (int i = 1; i < parms.Count; i++) {
				Parameter p = parms [i];
				if (p.PassAs != "") {
					if (SymbolTable.Table.IsBoxed (p.CType)) {
						if (p.PassAs == "ref")
							sw.WriteLine ("\t\t\tvals [" + i + "] = new GLib.Value (" + p.Name + ");");
						else
							sw.WriteLine ("\t\t\tvals [" + i + "] = new GLib.Value ((GLib.GType)typeof (" + p.CSType + "));");
						cleanup += "\t\t\t" + p.Name + " = (" + p.CSType + ") vals [" + i + "];\n";
					} else {
						if (p.PassAs == "ref")
							sw.WriteLine ("\t\t\tIntPtr " + p.Name + "_ptr = GLib.Marshaller.StructureToPtrAlloc (" + p.Generatable.CallByName (p.Name) + ");");
						else
							sw.WriteLine ("\t\t\tIntPtr " + p.Name + "_ptr = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (" + p.MarshalType + ")));");

						sw.WriteLine ("\t\t\tvals [" + i + "] = new GLib.Value (" + p.Name + "_ptr);");
						cleanup += "\t\t\t" + p.Name + " = " + p.FromNative ("(" + p.MarshalType + ") Marshal.PtrToStructure (" + p.Name + "_ptr, typeof (" + p.MarshalType + "))") + ";\n";
						cleanup += "\t\t\tMarshal.FreeHGlobal (" + p.Name + "_ptr);\n";
					}
				} else if (p.IsLength && parms [i - 1].IsString)
					sw.WriteLine ("\t\t\tvals [" + i + "] = new GLib.Value (System.Text.Encoding.UTF8.GetByteCount (" + parms [i-1].Name + "));");
				else
					sw.WriteLine ("\t\t\tvals [" + i + "] = new GLib.Value (" + p.Name + ");");

				sw.WriteLine ("\t\t\tinst_and_params.Append (vals [" + i + "]);");
			}

			sw.WriteLine ("\t\t\tg_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);");
			if (cleanup != "")
				sw.WriteLine (cleanup);
			sw.WriteLine ("\t\t\tforeach (GLib.Value v in vals)");
			sw.WriteLine ("\t\t\t\tv.Dispose ();");
			if (!IsVoid) {
				IGeneratable igen = SymbolTable.Table [retval.CType];
				sw.WriteLine ("\t\t\t" + retval.CSType + " result = (" + (igen is EnumGen ? retval.CSType + ") (Enum" : retval.CSType) + ") ret;");
				sw.WriteLine ("\t\t\tret.Dispose ();");
				sw.WriteLine ("\t\t\treturn result;");
			}
			sw.WriteLine ("\t\t}\n");
		}

		private void GenDefaultHandlerDelegate (GenerationInfo gen_info, ClassBase implementor)
		{
			StreamWriter sw = gen_info.Writer;
			StreamWriter glue;
			bool use_glue = gen_info.GlueEnabled && implementor == null && ClassFieldName.Length > 0;
			string glue_name = String.Empty;
			ManagedCallString call = new ManagedCallString (parms, true);
			sw.WriteLine ("\t\t[GLib.CDeclCallback]");
			sw.WriteLine ("\t\tdelegate " + retval.ToNativeType + " " + Name + "VMDelegate (" + parms.ImportSignature + ");\n");

			if (use_glue) {
				glue = gen_info.GlueWriter;
				glue_name = String.Format ("{0}sharp_{1}_override_{2}", container_type.NS.ToLower ().Replace (".", "_"), container_type.Name.ToLower (), ClassFieldName);
				sw.WriteLine ("\t\t[DllImport (\"{0}\")]", gen_info.GluelibName);
				sw.WriteLine ("\t\tstatic extern void {0} (IntPtr gtype, {1}VMDelegate cb);\n", glue_name, Name);
				glue.WriteLine ("void {0} (GType gtype, gpointer cb);\n", glue_name);
				glue.WriteLine ("void\n{0} (GType gtype, gpointer cb)", glue_name);
				glue.WriteLine ("{");
				glue.WriteLine ("\tGObjectClass *klass = g_type_class_peek (gtype);");
				glue.WriteLine ("\tif (klass == NULL)");
				glue.WriteLine ("\t\tklass = g_type_class_ref (gtype);");
				glue.WriteLine ("\t(({0} *)klass)->{1} = cb;", container_type.CName + "Class", ClassFieldName);
				glue.WriteLine ("}\n");
			}

			sw.WriteLine ("\t\tstatic {0} {1};\n", Name + "VMDelegate", Name + "VMCallback");
			sw.WriteLine ("\t\tstatic " + retval.ToNativeType + " " + Name.ToLower() + "_cb (" + parms.ImportSignature + ")");
			sw.WriteLine ("\t\t{");
			string unconditional = call.Unconditional ("\t\t\t");
			if (unconditional.Length > 0)
				sw.WriteLine (unconditional);
			sw.WriteLine ("\t\t\ttry {");
			sw.WriteLine ("\t\t\t\t{0} {1}_managed = GLib.Object.GetObject ({1}, false) as {0};", implementor != null ? implementor.Name : container_type.Name, parms[0].Name);
			sw.Write (call.Setup ("\t\t\t\t"));
			sw.Write ("\t\t\t\t{0}", IsVoid ? "" : retval.CSType == retval.ToNativeType ? "return " : retval.CSType + " raw_ret = ");
			sw.WriteLine ("{2}_managed.{0} ({1});", "On" + Name, call.ToString (), parms[0].Name);
			sw.Write (call.Finish ("\t\t\t\t"));
			if (!IsVoid && retval.CSType != retval.ToNativeType)
				sw.WriteLine ("\t\t\t\treturn {0};", SymbolTable.Table.ToNativeReturn (retval.CType, "raw_ret"));
			sw.WriteLine ("\t\t\t} catch (Exception e) {");
			bool fatal = HasOutParams || !IsVoid;
			sw.WriteLine ("\t\t\t\tGLib.ExceptionManager.RaiseUnhandledException (e, " + (fatal ? "true" : "false") + ");");
			if (fatal) {
				sw.WriteLine ("\t\t\t\t// NOTREACHED: above call doesn't return");
				sw.WriteLine ("\t\t\t\tthrow e;");
			}
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t}\n");
			sw.WriteLine ("\t\tprivate static void Override" + Name + " (GLib.GType gtype)");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tif (" + Name + "VMCallback == null)");
			sw.WriteLine ("\t\t\t\t" + Name + "VMCallback = new " + Name + "VMDelegate (" + Name.ToLower() + "_cb);");
			if (use_glue)
				sw.WriteLine ("\t\t\t{0} (gtype.Val, {1}VMCallback);", glue_name, Name);
			else
				sw.WriteLine ("\t\t\tOverrideVirtualMethod (gtype, " + CName + ", " + Name + "VMCallback);");
			sw.WriteLine ("\t\t}\n");
		}

		public void GenEvent (StreamWriter sw, ClassBase implementor, string target)
		{
			string args_type = IsEventHandler ? "" : ", typeof (" + EventArgsQualifiedName + ")";
			
			if (Marshaled) {
				GenCallback (sw);
				args_type = ", new " + DelegateName + "(" + CallbackName + ")";
			}

			sw.WriteLine("\t\t[GLib.Signal("+ CName + ")]");
			sw.Write("\t\tpublic ");
			if (NeedNew (implementor))
				sw.Write("new ");
			sw.WriteLine("event " + EventHandlerQualifiedName + " " + Name + " {");
			sw.WriteLine("\t\t\tadd {");
			sw.WriteLine("\t\t\t\tGLib.Signal sig = GLib.Signal.Lookup (" + target + ", " + CName + args_type + ");");
			sw.WriteLine("\t\t\t\tsig.AddDelegate (value);");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t\tremove {");
			sw.WriteLine("\t\t\t\tGLib.Signal sig = GLib.Signal.Lookup (" + target + ", " + CName + args_type + ");");
			sw.WriteLine("\t\t\t\tsig.RemoveDelegate (value);");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t}");
			sw.WriteLine();
		}

		public void Generate (GenerationInfo gen_info, ClassBase implementor)
		{
			StreamWriter sw = gen_info.Writer;

			if (implementor == null)
				GenEventHandler (gen_info);

			GenDefaultHandlerDelegate (gen_info, implementor);
			if (gen_info.GlueEnabled && implementor == null && ClassFieldName.Length > 0)
				GenGlueVirtualMethod (gen_info);
			else
				GenChainVirtualMethod (sw, implementor);
			GenEvent (sw, implementor, "this");
			
			Statistics.SignalCount++;
		}
	}
}


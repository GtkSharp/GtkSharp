// GtkSharp.Generation.Signal.cs - The Signal Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner 
// Copyright (c) 2003-2005 Novell, Inc.
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

		private string name;
		private XmlElement elem;
		private ReturnValue retval;
		private Parameters parms;
		private ClassBase container_type;

		public Signal (XmlElement elem, ClassBase container_type)
		{
			this.elem = elem;
			name = elem.GetAttribute ("name");
			retval = new ReturnValue (elem ["return-type"]);
			parms = new Parameters (elem["parameters"]);
			this.container_type = container_type;
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
				Console.Write ("bad signal " + Name);
				Statistics.ThrottledCount++;
				return false;
			}
			
			if (!parms.Validate ())
				return false;

			if (!retval.Validate ())
				return false;

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

		public string CallbackName {
			get {
				return Name + "SignalCallback";
			}
		}

		private string CallbackSig {
			get {
				string result = "";
				for (int i = 0; i < parms.Count; i++) {
					if (i > 0)
						result += ", ";

					if (parms[i].PassAs != "")
						result += parms[i].PassAs + " ";
					result += (parms[i].MarshalType + " arg" + i);
				}
				result += ", IntPtr gch";

				result = result.Replace ("out ref", "out");
				result = result.Replace ("ref ref", "ref");

				return result;
			}
		}
				
		public string DelegateName {
			get {
				return Name + "SignalDelegate";
			}
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

		public bool IsEventHandler {
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
				ClassBase igen = SymbolTable.Table.GetClassGen (retval.CType);

				if (igen is ObjectGen)
					return "GLib.GType.Object";
				if (igen is BoxedGen)
					return retval.CSType + ".GType";

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

		public void GenCallback (StreamWriter sw)
		{
			SymbolTable table = SymbolTable.Table;

			sw.WriteLine ("\t\t[GLib.CDeclCallback]");
			sw.WriteLine ("\t\tdelegate " + retval.ToNativeType + " " + DelegateName + " (" + CallbackSig + ");");
			sw.WriteLine ();
			sw.WriteLine ("\t\tstatic " + retval.ToNativeType + " " + CallbackName + " (" + CallbackSig + ")");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\tGLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;");
			sw.WriteLine("\t\t\tif (sig == null)");
			sw.WriteLine("\t\t\t\tthrow new Exception(\"Unknown signal GC handle received \" + gch);");
			sw.WriteLine();
			sw.WriteLine("\t\t\t{0} args = new {0} ();", EventArgsQualifiedName);
			if (parms.Count > 1)
				sw.WriteLine("\t\t\targs.Args = new object[" + (parms.Count - 1) + "];");
			string finish = "";
			for (int idx = 1; idx < parms.Count; idx++) {
				Parameter p = parms [idx];
				IGeneratable igen = p.Generatable;
				if (p.PassAs == "out")
					finish += "\t\t\targ" + idx + " = " + igen.ToNativeReturn ("((" + p.CSType + ")args.Args[" + (idx - 1) + "])") + ";\n";
				else if (igen is ManualGen) {
					sw.WriteLine("\t\t\tif (arg{0} == IntPtr.Zero)", idx);
					sw.WriteLine("\t\t\t\targs.Args[{0}] = null;", idx - 1);
					sw.WriteLine("\t\t\telse {");
					sw.WriteLine("\t\t\t\targs.Args[" + (idx - 1) + "] = " + igen.FromNative ("arg" + idx)  + ";");
					sw.WriteLine("\t\t\t}");
				} else
					sw.WriteLine("\t\t\targs.Args[" + (idx - 1) + "] = " + igen.FromNative ("arg" + idx)  + ";");
			}
			sw.WriteLine("\t\t\t{0} handler = ({0}) sig.Handler;", EventHandlerQualifiedName);
			sw.WriteLine("\t\t\thandler (GLib.Object.GetObject (arg0), args);");
			sw.WriteLine (finish);
			if (!IsVoid) {
				sw.WriteLine ("\t\t\tif (args.RetVal == null)");
				if (retval.CSType == "bool")
					sw.WriteLine ("\t\t\t\treturn false;");
				else
					sw.WriteLine ("\t\t\t\tthrow new Exception(\"args.RetVal unset in callback\");");

				sw.WriteLine("\t\t\treturn " + table.ToNativeReturn (retval.CType, "((" + retval.CSType + ")args.RetVal)") + ";");
			}
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

		private void GenVirtualMethod (StreamWriter sw, ClassBase implementor)
		{
			VMSignature vmsig = new VMSignature (parms);
			sw.WriteLine ("\t\t[GLib.DefaultSignalHandler(Type=typeof(" + (implementor != null ? implementor.QualifiedName : container_type.QualifiedName) + "), ConnectionMethod=\"Override" + Name +"\")]");
			sw.Write ("\t\tprotected ");
			if (NeedNew (implementor))
				sw.Write ("new ");
			sw.WriteLine ("virtual {0} {1} ({2})", retval.CSType, "On" + Name, vmsig.ToString ());
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
				if (parms [i].PassAs == "out") {
					sw.WriteLine ("\t\t\tvals [" + i + "] = GLib.Value.Empty;");
					cleanup += "\t\t\t" + parms [i].Name + " = (" + parms [i].CSType + ") vals [" + i + "];\n";
				} else if (parms [i].IsLength && parms [i - 1].IsString)
					sw.WriteLine ("\t\t\tvals [" + i + "] = new GLib.Value (" + parms [i-1].Name + ".Length);");
				else
					sw.WriteLine ("\t\t\tvals [" + i + "] = new GLib.Value (" + parms [i].Name + ");");

				sw.WriteLine ("\t\t\tinst_and_params.Append (vals [" + i + "]);");
			}

			sw.WriteLine ("\t\t\tg_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);");
			if (cleanup != "")
				sw.WriteLine (cleanup);
			sw.WriteLine ("\t\t\tforeach (GLib.Value v in vals)");
			sw.WriteLine ("\t\t\t\tv.Dispose ();");
			if (!IsVoid)
				sw.WriteLine ("\t\t\treturn (" + retval.CSType + ") ret;");
			sw.WriteLine ("\t\t}\n");
		}

		private void GenDefaultHandlerDelegate (StreamWriter sw, ClassBase implementor)
		{
			ImportSignature isig = new ImportSignature (parms, container_type.NS);
			ManagedCallString call = new ManagedCallString (parms);
			sw.WriteLine ("\t\t[GLib.CDeclCallback]");
			sw.WriteLine ("\t\tdelegate " + retval.ToNativeType + " " + Name + "VMDelegate (" + isig.ToString () + ");\n");
			sw.WriteLine ("\t\tstatic {0} {1};\n", Name + "VMDelegate", Name + "VMCallback");
			sw.WriteLine ("\t\tstatic " + retval.ToNativeType + " " + Name.ToLower() + "_cb (" + isig.ToString () + ")");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\t{0} obj = GLib.Object.GetObject ({1}, false) as {0};", implementor != null ? implementor.Name : container_type.Name, parms[0].Name);
			sw.Write (call.Setup ("\t\t\t"));
			sw.Write ("\t\t\t{0}", IsVoid ? "" : retval.CSType == retval.ToNativeType ? "return " : retval.CSType + " raw_ret = ");
			sw.WriteLine ("obj.{0} ({1});", "On" + Name, call.ToString ());
			sw.Write (call.Finish ("\t\t\t"));
			if (!IsVoid && retval.CSType != retval.ToNativeType)
				sw.WriteLine ("\t\t\treturn {0};", SymbolTable.Table.ToNativeReturn (retval.CType, "raw_ret"));
			sw.WriteLine ("\t\t}\n");
			sw.WriteLine ("\t\tprivate static void Override" + Name + " (GLib.GType gtype)");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tif (" + Name + "VMCallback == null)");
			sw.WriteLine ("\t\t\t\t" + Name + "VMCallback = new " + Name + "VMDelegate (" + Name.ToLower() + "_cb);");
			sw.WriteLine ("\t\t\tOverrideVirtualMethod (gtype, " + CName + ", " + Name + "VMCallback);");
			sw.WriteLine ("\t\t}\n");
		}

		public void Generate (GenerationInfo gen_info, ClassBase implementor)
		{
			StreamWriter sw = gen_info.Writer;

			if (implementor == null)
				GenEventHandler (gen_info);

			if (!IsEventHandler)
				GenCallback (sw);
			GenDefaultHandlerDelegate (sw, implementor);
			GenVirtualMethod (sw, implementor);
			string marsh = IsEventHandler ? "" : ", new " + DelegateName + "(" + CallbackName + ")";

			sw.WriteLine("\t\t[GLib.Signal("+ CName + ")]");
			sw.Write("\t\tpublic ");
			if (NeedNew (implementor))
				sw.Write("new ");
			sw.WriteLine("event " + EventHandlerQualifiedName + " " + Name + " {");
			sw.WriteLine("\t\t\tadd {");
			sw.WriteLine("\t\t\t\tGLib.Signal sig = GLib.Signal.Lookup (this, " + CName + marsh + ");");
			sw.WriteLine("\t\t\t\tsig.AddDelegate (value);");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t\tremove {");
			sw.WriteLine("\t\t\t\tGLib.Signal sig = GLib.Signal.Lookup (this, " + CName + marsh + ");");
			sw.WriteLine("\t\t\t\tsig.RemoveDelegate (value);");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			Statistics.SignalCount++;
		}
	}
}


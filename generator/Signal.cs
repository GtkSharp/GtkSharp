// GtkSharp.Generation.Signal.cs - The Signal Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2003 Mike Kestner, (c) 2003 Novell, Inc.

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class Signal {

		private string name;
		private XmlElement elem;
		private Parameters parms;
		private ClassBase container_type;
		SignalHandler sig_handler;

		public Signal (XmlElement elem, ClassBase container_type)
		{
			this.elem = elem;
			this.name = elem.GetAttribute ("name");
			if (elem["parameters"] != null)
				parms = new Parameters (elem["parameters"], container_type.NS);
			this.container_type = container_type;
			sig_handler = new SignalHandler (elem, container_type.NS);
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
			if (Name == "" || !sig_handler.Validate ()) {
				Console.Write ("bad signal " + Name);
				Statistics.ThrottledCount++;
				return false;
			}
			
			if (parms != null && !parms.Validate ())
				return false;

			return true;
		}

 		public void GenerateDecl (StreamWriter sw)
 		{
			if (elem.HasAttribute("new_flag") || (container_type != null && container_type.GetSignalRecursively (Name) != null))
				sw.Write("new ");

 			sw.WriteLine ("\t\tevent " + EventHandlerQualifiedName + " " + Name + ";");
		}

                private string EventArgsName {
                        get {
                                if (sig_handler.Name == "voidObjectSignal")
                                        return "EventArgs";
                                else
                                        return Name + "Args";
                        }
                }
                                                                                                                        
                private string EventArgsQualifiedName {
                        get {
                                if (sig_handler.Name == "voidObjectSignal")
                                        return "System.EventArgs";
                                else
                                        return container_type.NS + "Sharp." + Name + "Args";
                        }
                }
                                                                                                                        
                private string EventHandlerName {
                        get {
                                if (sig_handler.Name == "voidObjectSignal")
                                        return "EventHandler";
                                else
                                        return Name + "Handler";
                        }
                }
                                                                                                                        
                private string EventHandlerQualifiedName {
                        get {
                                if (sig_handler.Name == "voidObjectSignal")
                                        return "System.EventHandler";
                                else
                                        return container_type.NS + "Sharp." + Name + "Handler";
                        }
                }

		private bool IsVoid {
			get {
				return ReturnType == "void";
			}
		}

		private string MarshalReturnType {
			get {
				string ctype = elem ["return-type"].GetAttribute("type");
				return SymbolTable.Table.GetMarshalType (ctype);
			}
		}

		private string ReturnType {
			get {
				string ctype = elem ["return-type"].GetAttribute("type");
				return SymbolTable.Table.GetCSType (ctype);
			}
		}

		public void GenEventHandler (GenerationInfo gen_info)
		{
			if (EventHandlerName == "EventHandler")
				return;

			string ns = container_type.NS;

			StreamWriter sw = gen_info.OpenStream (ns + "Sharp." + EventHandlerName);
			
			sw.WriteLine ("namespace " + ns + "Sharp {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");

			sw.WriteLine ();
			sw.WriteLine ("\tpublic delegate void " + EventHandlerName + "(object o, " + EventArgsName + " args);");
			sw.WriteLine ();
			sw.WriteLine ("\tpublic class " + EventArgsName + " : GtkSharp.SignalArgs {");
			if (parms != null) {
				for (int i = 1; i < parms.Count; i++) {
					sw.WriteLine ("\t\tpublic " + parms[i].CSType + " " + parms[i].StudlyName + "{");
					sw.WriteLine ("\t\t\tget {");
					sw.WriteLine ("\t\t\t\treturn (" + parms[i].CSType + ") Args[" + (i - 1) + "];");
					sw.WriteLine ("\t\t\t}");
					sw.WriteLine ("\t\t}");
					sw.WriteLine ();
				}
			}
			sw.WriteLine ("\t}");
			sw.WriteLine ("}");
			sw.Close ();
		}

		private void GenVirtualMethod (StreamWriter sw)
		{
			VMSignature vmsig = new VMSignature (parms);
			sw.WriteLine ("\t\tprotected virtual {0} {1} ({2})", ReturnType, "On" + Name, vmsig.ToString ());
			sw.WriteLine ("\t\t{");
			if (!IsVoid)
				sw.WriteLine ("\t\t\tGLib.Value ret = new GLib.Value ();");

			sw.WriteLine ("\t\t\tIntPtr[] args = new IntPtr [" + parms.Count + "];");
			sw.WriteLine ("\t\t\targs [0] = Handle;");
			sw.WriteLine ("\t\t\tGLib.Value[] vals = new GLib.Value [" + (parms.Count - 1) + "];");
			string cleanup = "";
			for (int i = 1; i < parms.Count; i++) {
				if (parms [i].PassAs == "out") {
					sw.WriteLine ("\t\t\tvals [" + (i - 1) + "] = new GLib.Value ();");
					cleanup += "\t\t\t" + parms [i].Name + " = (" + parms [i].CSType + ") vals [" + (i - 1) + "];\n";
				} else if (parms [i].IsLength && parms [i - 1].IsString)
					sw.WriteLine ("\t\t\tvals [" + (i - 1) + "] = new GLib.Value (" + parms [i-1].Name + ".Length);");
				else
					sw.WriteLine ("\t\t\tvals [" + (i - 1) + "] = new GLib.Value (" + parms [i].Name + ");");

				sw.WriteLine ("\t\t\targs [" + i + "] = vals [" + (i - 1) + "].Handle;");
			}

			sw.WriteLine ("\t\t\tg_signal_chain_from_overridden (args, " + (IsVoid ? "IntPtr.Zero " : "ret.Handle ") + ");");
			if (cleanup != "")
				sw.WriteLine (cleanup);
			if (!IsVoid)
				sw.WriteLine ("\t\t\treturn (" + ReturnType + ") ret;");
			sw.WriteLine ("\t\t}\n");
		}

		public void Generate (GenerationInfo gen_info, ClassBase implementor)
		{
			StreamWriter sw = gen_info.Writer;
			string cname = "\"" + elem.GetAttribute("cname") + "\"";
			string ns;
			if (implementor == null) {
				ns = container_type.NS;
				GenEventHandler (gen_info);
			} else
				ns = implementor.NS;

			sig_handler.Generate (ns, gen_info);
			GenVirtualMethod (sw);
			string qual_marsh = ns + "Sharp." + sig_handler.Name;

			sw.WriteLine("\t\t[GLib.Signal("+ cname + ")]");
			sw.Write("\t\tpublic ");
			if (elem.HasAttribute("new_flag") || (container_type != null && container_type.GetSignalRecursively (Name) != null) || (implementor != null && implementor.GetSignalRecursively (Name) != null))
				sw.Write("new ");
			sw.WriteLine("event " + EventHandlerQualifiedName + " " + Name + " {");
			sw.WriteLine("\t\t\tadd {");
			sw.WriteLine("\t\t\t\tif (EventList[" + cname + "] == null)");
			sw.Write("\t\t\t\t\tSignals[" + cname + "] = new " + qual_marsh);
			sw.Write("(this, Handle, " + cname + ", value, System.Type.GetType(\"" + EventArgsQualifiedName);
			if (EventArgsQualifiedName != "System.EventArgs")
				sw.Write("," + gen_info.AssemblyName);
			sw.WriteLine("\"));\n\t\t\t\telse");
			sw.WriteLine("\t\t\t\t\t((GtkSharp.SignalCallback) Signals [{0}]).AddDelegate (value);", cname);
			sw.WriteLine("\t\t\t\tEventList.AddHandler(" + cname + ", value);");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t\tremove {");
			sw.WriteLine("\t\t\t\tEventList.RemoveHandler(" + cname + ", value);");
			sw.WriteLine("\t\t\t\tGtkSharp.SignalCallback cb = Signals [{0}] as GtkSharp.SignalCallback;", cname);
			sw.WriteLine("\t\t\t\tif (cb == null)");
			sw.WriteLine("\t\t\t\t\treturn;");
			sw.WriteLine();
			sw.WriteLine("\t\t\t\tcb.RemoveDelegate (value);");
			sw.WriteLine();
			sw.WriteLine("\t\t\t\tif (EventList[" + cname + "] == null) {");
			sw.WriteLine("\t\t\t\t\tSignals.Remove(" + cname + ");");
			sw.WriteLine("\t\t\t\t\tcb.Dispose ();");
			sw.WriteLine("\t\t\t\t}");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			Statistics.SignalCount++;
		}
	}
}


// GtkSharp.Generation.Signal.cs - The Signal Generatable.
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
	using System.Xml;

	public class Signal {

		private string name;
		private XmlElement elem;
		private ReturnValue retval;
		private Parameters parms;
		private ClassBase container_type;
		SignalHandler sig_handler;

		public Signal (XmlElement elem, ClassBase container_type)
		{
			this.elem = elem;
			name = elem.GetAttribute ("name");
			retval = new ReturnValue (elem ["return-type"]);
			parms = new Parameters (elem["parameters"]);
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
                                        return container_type.NS + "." + Name + "Args";
                        }
                }
                                                                                                                        
                private string EventHandlerName {
                        get {
                                if (sig_handler.Name == "voidObjectSignal")
                                        return "EventHandler";
                                else if (SymbolTable.Table [container_type.NS + Name + "Handler"] != null)
                                        return Name + "EventHandler";
				else
                                        return Name + "Handler";
                        }
                }
                                                                                                                        
                private string EventHandlerQualifiedName {
                        get {
                                if (sig_handler.Name == "voidObjectSignal")
                                        return "System.EventHandler";
                                else
                                        return container_type.NS + "." + EventHandlerName;
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

		private bool NeedNew (ClassBase implementor)
		{
			return elem.HasAttribute ("new_flag") ||
				(container_type != null && container_type.GetSignalRecursively (Name) != null) ||
				(implementor != null && implementor.GetSignalRecursively (Name) != null);
		}

		public void GenEventHandler (GenerationInfo gen_info)
		{
			if (EventHandlerName == "EventHandler")
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
			if (!IsVoid)
				sw.WriteLine ("\t\t\treturn (" + retval.CSType + ") ret;");
			sw.WriteLine ("\t\t}\n");
		}

		private void GenDefaultHandlerDelegate (StreamWriter sw, ClassBase implementor)
		{
			ImportSignature isig = new ImportSignature (parms, container_type.NS);
			ManagedCallString call = new ManagedCallString (parms);
			sw.WriteLine ("\t\tdelegate " + retval.ToNativeType + " " + Name + "Delegate (" + isig.ToString () + ");\n");
			sw.WriteLine ("\t\tstatic {0} {1};\n", Name + "Delegate", Name + "Callback");
			sw.WriteLine ("\t\tstatic " + retval.ToNativeType + " " + Name.ToLower() + "_cb (" + isig.ToString () + ")");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\t{0} obj = GLib.Object.GetObject ({1}, false) as {0};", implementor != null ? implementor.Name : container_type.Name, parms[0].Name);
			sw.Write (call.Setup ("\t\t\t"));
			sw.Write ("\t\t\t{0}", IsVoid ? "" : "return ");
			sw.WriteLine ("obj.{0} ({1});", "On" + Name, call.ToString ());
			sw.Write (call.Finish ("\t\t\t"));
			sw.WriteLine ("\t\t}\n");
			string cname = "\"" + elem.GetAttribute("cname") + "\"";
			sw.WriteLine ("\t\tprivate static void Override" + Name + " (GLib.GType gtype)");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tif (" + Name + "Callback == null)");
			sw.WriteLine ("\t\t\t\t" + Name + "Callback = new " + Name + "Delegate (" + Name.ToLower() + "_cb);");
			sw.WriteLine ("\t\t\tOverrideVirtualMethod (gtype, " + cname + ", " + Name + "Callback);");
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
			GenDefaultHandlerDelegate (sw, implementor);
			GenVirtualMethod (sw, implementor);
			string qual_marsh = ns + "Sharp." + sig_handler.Name;

			sw.WriteLine("\t\t[GLib.Signal("+ cname + ")]");
			sw.Write("\t\tpublic ");
			if (NeedNew (implementor))
				sw.Write("new ");
			sw.WriteLine("event " + EventHandlerQualifiedName + " " + Name + " {");
			sw.WriteLine("\t\t\tadd {");
			sw.WriteLine("\t\t\t\tif (value.Method.GetCustomAttributes(typeof(GLib.ConnectBeforeAttribute), false).Length > 0) {");
			sw.WriteLine("\t\t\t\t\tif (BeforeHandlers[" + cname + "] == null)");
			sw.Write("\t\t\t\t\t\tBeforeSignals[" + cname + "] = new " + qual_marsh);
			sw.WriteLine("(this, " + cname + ", value, typeof (" + EventArgsQualifiedName + "), 0);");
			sw.WriteLine("\t\t\t\t\telse");
			sw.WriteLine("\t\t\t\t\t\t((GLib.SignalCallback) BeforeSignals [{0}]).AddDelegate (value);", cname);
			sw.WriteLine("\t\t\t\t\tBeforeHandlers.AddHandler(" + cname + ", value);");
			sw.WriteLine("\t\t\t\t} else {");
			sw.WriteLine("\t\t\t\t\tif (AfterHandlers[" + cname + "] == null)");
			sw.Write("\t\t\t\t\t\tAfterSignals[" + cname + "] = new " + qual_marsh);
			sw.WriteLine("(this, " + cname + ", value, typeof (" + EventArgsQualifiedName + "), 1);");
			sw.WriteLine("\t\t\t\t\telse");
			sw.WriteLine("\t\t\t\t\t\t((GLib.SignalCallback) AfterSignals [{0}]).AddDelegate (value);", cname);
			sw.WriteLine("\t\t\t\t\tAfterHandlers.AddHandler(" + cname + ", value);");
			sw.WriteLine("\t\t\t\t}");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t\tremove {");
			sw.WriteLine("\t\t\t\tSystem.ComponentModel.EventHandlerList event_list = AfterHandlers;");
			sw.WriteLine("\t\t\t\tHashtable signals = AfterSignals;");
			sw.WriteLine("\t\t\t\tif (value.Method.GetCustomAttributes(typeof(GLib.ConnectBeforeAttribute), false).Length > 0) {");
			sw.WriteLine("\t\t\t\t\tevent_list = BeforeHandlers;");
			sw.WriteLine("\t\t\t\t\tsignals = BeforeSignals;");
			sw.WriteLine("\t\t\t\t}");
			sw.WriteLine("\t\t\t\tGLib.SignalCallback cb = signals [{0}] as GLib.SignalCallback;", cname);
			sw.WriteLine("\t\t\t\tevent_list.RemoveHandler(" + cname + ", value);");
			sw.WriteLine("\t\t\t\tif (cb == null)");
			sw.WriteLine("\t\t\t\t\treturn;");
			sw.WriteLine();
			sw.WriteLine("\t\t\t\tcb.RemoveDelegate (value);");
			sw.WriteLine();
			sw.WriteLine("\t\t\t\tif (event_list[" + cname + "] == null) {");
			sw.WriteLine("\t\t\t\t\tsignals.Remove(" + cname + ");");
			sw.WriteLine("\t\t\t\t\tcb.Dispose ();");
			sw.WriteLine("\t\t\t\t}");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			Statistics.SignalCount++;
		}
	}
}


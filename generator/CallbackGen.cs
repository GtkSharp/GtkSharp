// GtkSharp.Generation.CallbackGen.cs - The Callback Generatable.
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2002-2003 Mike Kestner
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
	using System.IO;
	using System.Xml;

	public class CallbackGen : GenBase, IAccessor {

		private Parameters parms;
		private Signature sig = null;
		private ReturnValue retval;
		private bool valid = true;

		public CallbackGen (XmlElement ns, XmlElement elem) : base (ns, elem) 
		{
			retval = new ReturnValue (elem ["return-type"]);
			parms = new Parameters (elem ["parameters"]);
			parms.HideData = true;
		}

		public override bool Validate ()
		{
			if (!retval.Validate ()) {
				Console.WriteLine ("rettype: " + retval.CType + " in callback " + CName);
				Statistics.ThrottledCount++;
				valid = false;
				return false;
			}

			if (!parms.Validate ()) {
				Console.WriteLine (" in callback " + CName);
				Statistics.ThrottledCount++;
				valid = false;
				return false;
			}

			valid = true;
			return true;
		}

		public string InvokerName {
			get {
				if (!valid)
					return String.Empty;
				return NS + "Sharp." + Name + "Invoker";
			}
		}

		public override string MarshalType {
			get {
				if (valid)
					return NS + "Sharp." + Name + "Native";
				else
					return "";
			}
		}

		public override string CallByName (string var_name)
		{
			return var_name + ".NativeDelegate";
		}

		public override string FromNative (string var)
		{
			return NS + "Sharp." + Name + "Wrapper.GetManagedDelegate (" + var + ")";
		}

		public void WriteAccessors (StreamWriter sw, string indent, string var)
		{
			sw.WriteLine (indent + "get {");
			sw.WriteLine (indent + "\treturn " + FromNative (var) + ";");
			sw.WriteLine (indent + "}");
		}

		string CastFromInt (string type)
		{
			return type != "int" ? "(" + type + ") " : "";
		}

		string InvokeString {
			get {
				if (parms.Count == 0)
					return String.Empty;

				string[] result = new string [parms.Count];
				for (int i = 0; i < parms.Count; i++) {
					Parameter p = parms [i];
					IGeneratable igen = p.Generatable;

					if (i > 0 && parms [i - 1].IsString && p.IsLength) {
						string string_name = parms [i - 1].Name;
						result[i] = igen.CallByName (CastFromInt (p.CSType) + "System.Text.Encoding.UTF8.GetByteCount (" +  string_name + ")");
						continue;
					}

					p.CallName = p.Name;
					result [i] = p.CallString;
					if (p.IsUserData)
						result [i] = "IntPtr.Zero"; 
				}

				return String.Join (", ", result);
			}
		}

		MethodBody body;

		void GenInvoker (GenerationInfo gen_info, StreamWriter sw)
		{
			if (sig == null)
				sig = new Signature (parms);

			sw.WriteLine ("\tinternal class " + Name + "Invoker {");
			sw.WriteLine ();
			sw.WriteLine ("\t\t" + Name + "Native native_cb;");
			sw.WriteLine ();
			sw.WriteLine ("\t\tinternal " + Name + "Invoker (" + Name + "Native native_cb)");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tthis.native_cb = native_cb;");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\tinternal " + QualifiedName + " Handler {");
			sw.WriteLine ("\t\t\tget {");
			sw.WriteLine ("\t\t\t\treturn new " + QualifiedName + "(InvokeNative);");
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\t" + retval.CSType + " InvokeNative (" + sig + ")");
			sw.WriteLine ("\t\t{");
			body.Initialize (gen_info);
			string call = "native_cb (" + InvokeString + ")";
			if (retval.IsVoid)
				sw.WriteLine ("\t\t\t" + call + ";");
			else
				sw.WriteLine ("\t\t\t" + retval.CSType + " result = " + retval.FromNative (call) + ";");
			body.Finish (sw, String.Empty);
			if (!retval.IsVoid)
				sw.WriteLine ("\t\t\treturn result;");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ("\t}");
			sw.WriteLine ();
		}

		public string GenWrapper (GenerationInfo gen_info)
		{
			string wrapper = Name + "Native";
			string qualname = MarshalType;

			if (!Validate ())
				return String.Empty;

			body = new MethodBody (parms);

			StreamWriter save_sw = gen_info.Writer;
			StreamWriter sw = gen_info.Writer = gen_info.OpenStream (qualname);

			sw.WriteLine ("namespace " + NS + "Sharp {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();
			sw.WriteLine ("#region Autogenerated code");
			sw.WriteLine ("\t[GLib.CDeclCallback]");
			sw.WriteLine ("\tinternal delegate " + retval.MarshalType + " " + wrapper + "(" + parms.ImportSignature + ");");
			sw.WriteLine ();
			GenInvoker (gen_info, sw);
			sw.WriteLine ("\tinternal class " + Name + "Wrapper {");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic " + retval.MarshalType + " NativeCallback (" + parms.ImportSignature + ")");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\ttry {");

			bool need_sep = false;
			bool throws_error = false;
			string call_str = "";
			string cleanup_str = "";
			for (int i = 0, idx = 0; i < parms.Count; i++)
			{
				Parameter p = parms [i];

				if (p.CType == "GError**") {
					sw.WriteLine ("\t\t\t\t" + p.Name + " = IntPtr.Zero;");
					throws_error = true;
					continue;
				} else if (parms.IsHidden (p))
					continue;

				IGeneratable gen = p.Generatable;

				sw.Write("\t\t\t\t" + p.CSType + " _arg" + idx);
				if (p.PassAs == "out") {
					sw.WriteLine(";");
					cleanup_str += "\t\t\t\t" + p.Name + " = " + gen.CallByName ("_arg" + idx) + ";\n";
				} else
					sw.WriteLine(" = " + gen.FromNative (p.Name) + ";");

				if (need_sep)
					call_str += ", ";
				else
					need_sep = true;
				call_str += String.Format ("{0} _arg{1}", p.PassAs, idx);
				idx++;
			}

			bool has_out_params = cleanup_str.Length > 0;
			cleanup_str += "\t\t\t\tif (release_on_call)\n\t\t\t\t\tgch.Free ();\n";

			sw.Write ("\t\t\t\t");
			string invoke = "managed (" + call_str + ")";
			if (retval.MarshalType != "void") {
				sw.Write (retval.MarshalType + " ret = ");
				cleanup_str += "\t\t\t\treturn ret;\n";

				SymbolTable table = SymbolTable.Table;
				ClassBase ret_wrapper = table.GetClassGen (retval.CType);
				if (ret_wrapper != null && ret_wrapper is HandleBase)
					sw.WriteLine ("(({0}) {1}).Handle;", retval.CSType, invoke);
				else if (table.IsStruct (retval.CType) || table.IsBoxed (retval.CType)) {
					// Shoot. I have no idea what to do here.
					Console.WriteLine ("Struct return type {0} in callback {1}", retval.CType, CName);
					sw.WriteLine ("IntPtr.Zero;"); 
				} else if (table.IsEnum (retval.CType))
					sw.WriteLine ("(int) {0};", invoke);
				else
					sw.WriteLine ("({0}) ({1});", retval.MarshalType, table.ToNativeReturn (retval.CType, invoke));
			} else
				sw.WriteLine (invoke + ";");

			sw.Write (cleanup_str);
			bool fatal = (retval.MarshalType != "void" && retval.MarshalType != "bool") || has_out_params || throws_error;
			sw.WriteLine ("\t\t\t} catch (Exception e) {");
			sw.WriteLine ("\t\t\t\tGLib.ExceptionManager.RaiseUnhandledException (e, " + (fatal ? "true" : "false") + ");");
			if (fatal) {
				sw.WriteLine ("\t\t\t\t// NOTREACHED: Above call does not return.");
				sw.WriteLine ("\t\t\t\tthrow e;");
			} else if (retval.MarshalType == "bool") {
				if (throws_error)
					sw.WriteLine ("\t\t\t\terror = IntPtr.Zero;");
				sw.WriteLine ("\t\t\t\treturn false;");
			}
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\tbool release_on_call = false;");
			sw.WriteLine ("\t\tGCHandle gch;");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic void PersistUntilCalled ()");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\trelease_on_call = true;");
			sw.WriteLine ("\t\t\tgch = GCHandle.Alloc (this);");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\tinternal " + wrapper + " NativeDelegate;");
			sw.WriteLine ("\t\t" + NS + "." + Name + " managed;");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic " + Name + "Wrapper (" + NS + "." + Name + " managed)");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tthis.managed = managed;");
			sw.WriteLine ("\t\t\tif (managed != null)");
			sw.WriteLine ("\t\t\t\tNativeDelegate = new " + wrapper + " (NativeCallback);");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic static " + NS + "." + Name + " GetManagedDelegate (" + wrapper + " native)");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tif (native == null)");
			sw.WriteLine ("\t\t\t\treturn null;");
			sw.WriteLine ("\t\t\t" + Name + "Wrapper wrapper = (" + Name + "Wrapper) native.Target;");
			sw.WriteLine ("\t\t\tif (wrapper == null)");
			sw.WriteLine ("\t\t\t\treturn null;");
			sw.WriteLine ("\t\t\treturn wrapper.managed;");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ("\t}");
			sw.WriteLine ("#endregion");
			sw.WriteLine ("}");
			sw.Close ();
			gen_info.Writer = save_sw;
			return NS + "Sharp." + Name + "Wrapper";
		}
		
		public override void Generate (GenerationInfo gen_info)
		{
			gen_info.CurrentType = Name;

			sig = new Signature (parms);

			StreamWriter sw = gen_info.OpenStream (Name);

			sw.WriteLine ("namespace " + NS + " {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ();
			sw.WriteLine ("\tpublic delegate " + retval.CSType + " " + Name + "(" + sig.ToString() + ");");
			sw.WriteLine ();
			sw.WriteLine ("}");

			sw.Close ();
			
			GenWrapper (gen_info);

			Statistics.CBCount++;
		}
	}
}


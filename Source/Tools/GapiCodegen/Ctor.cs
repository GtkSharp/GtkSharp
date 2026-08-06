// GtkSharp.Generation.Ctor.cs - The Constructor Generation Class.
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2001-2003 Mike Kestner
// Copyright (c) 2004-2005 Novell, Inc.
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
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;

	public class Ctor : MethodBase  {

		private bool preferred;
		private bool deprecated;
		private string name;
		private bool needs_chaining;
		private bool creates_native_object;
		private bool fundamental;

		public Ctor (XmlElement elem, ClassBase implementor) : base (elem, implementor)
		{
			preferred = elem.GetAttributeAsBoolean ("preferred");
			deprecated = elem.GetAttributeAsBoolean("deprecated");

			ObjectGen obj = implementor as ObjectGen;
			if (obj != null) {
				needs_chaining = true;

				// Chaining serves two purposes, and a fundamental type wants
				// only the first: reach the base IntPtr ctor, and -- when the
				// instance is really a managed subclass -- build the native
				// object through g_object_new_with_properties instead. A
				// fundamental type is not a GObject and cannot be subclassed
				// from managed code, so that second branch has nothing to call.
				creates_native_object = !obj.IsFundamental;
				fundamental = obj.IsFundamental;
			}

			name = implementor.Name;
		}

		public bool Preferred {
			get { return preferred; }
			set { preferred = value; }
		}

		public bool IsDeprecated {
			get {
				return deprecated;
			}
		}

		public string StaticName {
			get {
				if (!IsStatic)
					return String.Empty;

				if (Name != null && Name != String.Empty)
					return Name;

				string[] toks = CName.Substring(CName.IndexOf("new")).Split ('_');
				string result = String.Empty;

				foreach (string tok in toks)
					result += tok.Substring(0,1).ToUpper() + tok.Substring(1);
				return result;
			}
		}

		void GenerateImport (StreamWriter sw)
		{
            sw.WriteLine("\t\t[UnmanagedFunctionPointer (CallingConvention.Cdecl)]");
            sw.WriteLine("\t\tdelegate IntPtr d_{0}({1});", CName, Parameters.ImportSignature);
            sw.WriteLine("\t\tstatic d_{0} {0} = FuncLoader.LoadFunction<d_{0}>(FuncLoader.GetProcAddress(GLibrary.Load({1}), \"{0}\"));", CName, LibraryName);
            sw.WriteLine();
		}

		void GenerateStatic (GenerationInfo gen_info)
		{
			StreamWriter sw = gen_info.Writer;
			sw.WriteLine("\t\t" + Protection + " static " + Safety + Modifiers +  name + " " + StaticName + "(" + Signature + ")");
			sw.WriteLine("\t\t{");

			Body.Initialize(gen_info, false, false, "");

			sw.Write("\t\t\t" + name + " result = ");
			if (container_type is StructBase)
				sw.Write ("{0}.New (", name);
			else
				sw.Write ("new {0} (", name);
			sw.WriteLine (CName + "(" + Body.GetCallString (false) + "));");
			Body.Finish (sw, ""); 
			Body.HandleException (sw, "");
			sw.WriteLine ("\t\t\treturn result;");
		}

		public void Generate (GenerationInfo gen_info)
		{
			StreamWriter sw = gen_info.Writer;
			gen_info.CurrentMember = CName;

			GenerateImport (sw);
			
			if (IsStatic)
				GenerateStatic (gen_info);
			else {

				if (IsDeprecated)
					sw.WriteLine("\t\t[Obsolete]");

				sw.WriteLine("\t\t{0} {1}{2} ({3}) {4}", Protection, Safety, name, Signature.ToString(), needs_chaining ? ": base (IntPtr.Zero)" : "");
				sw.WriteLine("\t\t{");

				if (needs_chaining && creates_native_object) {
					sw.WriteLine ("\t\t\tif (GetType () != typeof (" + name + ")) {");
					
					if (Parameters.Count == 0) {
						sw.WriteLine ("\t\t\t\tCreateNativeObject (Array.Empty<string> (), Array.Empty<GLib.Value> ());");
						sw.WriteLine ("\t\t\t\treturn;");
					} else {
						var names = new List<string> ();
						var values = new List<string> ();
						// Parallel to names/values: the loop below used to index
						// Parameters by the names index, which only lined up while
						// every parameter contributed a name. Hidden ones never do.
						var props = new List<Parameter> ();
						for (int i = 0; i < Parameters.Count; i++) {
							Parameter p = Parameters[i];
							// user_data and destroy-notify parameters are generated
							// as locals further down, so naming them here refers to
							// something not yet declared.
							if (Parameters.IsHidden (p))
								continue;
							if (container_type.GetPropertyRecursively (p.StudlyName) != null) {
								names.Add (p.Name);
								values.Add (p.Name);
								props.Add (p);
							} else if (p.PropertyName != String.Empty) {
								names.Add (p.PropertyName);
								values.Add (p.Name);
								props.Add (p);
							}
						}

						//if (names.Count == Parameters.Count) {
							sw.WriteLine ("\t\t\t\tvar vals = new List<GLib.Value> ();");
							sw.WriteLine ("\t\t\t\tvar names = new List<string> ();");
							for (int i = 0; i < names.Count; i++) {
								Parameter p = props [i];
								string indent = "\t\t\t\t";
								if (p.Generatable is ClassBase && !(p.Generatable is StructBase)) {
									sw.WriteLine (indent + "if (" + p.Name + " != null) {");
									indent += "\t";
								}
								sw.WriteLine (indent + "names.Add (\"" + names [i] + "\");");
								sw.WriteLine (indent + "vals.Add (new GLib.Value (" + values[i] + "));");

								if (p.Generatable is ClassBase && !(p.Generatable is StructBase))
									sw.WriteLine ("\t\t\t\t}");
							}

							sw.WriteLine ("\t\t\t\tCreateNativeObject (names.ToArray (), vals.ToArray ());");
							sw.WriteLine ("\t\t\t\treturn;");
						//} else
						//	sw.WriteLine ("\t\t\t\tthrow new InvalidOperationException (\"Can't override this constructor.\");");
					}
					
					sw.WriteLine ("\t\t\t}");
				}
	
				// GLib.Opaque's Raw setter takes a reference on assignment, via
				// the Ref hook, for the common case of wrapping a borrowed
				// pointer. A constructor result is transfer-full, so that
				// reference would be one too many and the type would never
				// reach a refcount of zero. Claiming ownership first makes the
				// hook -- which is guarded on !Owned -- correctly do nothing.
				if (fundamental)
					sw.WriteLine ("\t\t\tOwned = true;");

				Body.Initialize(gen_info, false, false, "");
				sw.WriteLine("\t\t\t{0} = {1}({2});", container_type.AssignToName, CName, Body.GetCallString (false));
				Body.Finish (sw, "");
				Body.HandleException (sw, "");
			}
			
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			Statistics.CtorCount++;
		}
	}
}


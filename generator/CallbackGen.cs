// GtkSharp.Generation.CallbackGen.cs - The Callback Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class CallbackGen : GenBase, IGeneratable  {

		private Parameters parms;

		public CallbackGen (XmlElement ns, XmlElement elem) : base (ns, elem) 
		{
			if (elem ["parameters"] != null)
				parms = new Parameters (elem ["parameters"]);
		}

		public String MarshalType {
			get
			{
				return QualifiedName;
			}
		}

		public String MarshalReturnType {
			get
			{
				return MarshalType;
			}
		}

		public String CallByName (String var_name)
		{
			return var_name;
		}

		public String FromNative(String var)
		{
			return var;
		}

		public String FromNativeReturn(String var)
		{
			return FromNative (var);
		}

		public void Generate ()
		{
			if (!DoGenerate)
				return;

			XmlElement ret_elem = Elem["return-type"];
			if (ret_elem == null) {
				Console.WriteLine("No return type in callback " + CName);
				Statistics.ThrottledCount++;
				return;
			}

			string rettype = ret_elem.GetAttribute("type");
			string s_ret = SymbolTable.GetCSType(rettype);
			if (s_ret == "") {
				Console.WriteLine("rettype: " + rettype + " in callback " + CName);
				Statistics.ThrottledCount++;
				return;
			}

			if ((parms != null) && !parms.Validate ()) {
				Console.WriteLine(" in callback " + CName + " **** Stubbing it out ****");
				Statistics.ThrottledCount++;
				parms = null;
			}

			StreamWriter sw = CreateWriter ();

			string sig = "";
			if (parms != null) 
				sig = parms.Signature;

			sw.WriteLine ("\tpublic delegate " + s_ret + " " + Name + "(" + sig + ");");

			CloseWriter (sw);
			Statistics.CBCount++;
		}
	}
}


// GtkSharp.Generation.ManagedCallString.cs - The ManagedCallString Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2003 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;

	public class ManagedCallString {
		
		ArrayList parms = new ArrayList ();

		public ManagedCallString (Parameters parms)
		{
			for (int i = 1; i < parms.Count; i ++) {
				Parameter p = parms [i];
				if (p.IsLength && parms [i-1].IsString)
					continue;
				this.parms.Add (p);
			}
		}

		public override string ToString ()
		{
			if (parms.Count < 1)
				return "";

			string[] result = new string [parms.Count];

			for (int i = 0; i < parms.Count; i ++) {
				Parameter p = parms [i] as Parameter;
				IGeneratable igen = p.Generatable;
				result [i] = igen is StructGen ? "ref " : (p.PassAs == "" ? "" : p.PassAs + " ");
				result [i] += igen.FromNative (p.Name);
			}

			return String.Join (", ", result);
		}
	}
}


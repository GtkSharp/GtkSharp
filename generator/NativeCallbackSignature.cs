// GtkSharp.Generation.NativeCallbackSignature.cs - The NativeCallbackSignature Generation Class.
//
// Author: Mike Kestner <mkestner@novell.com>
//
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

	public class NativeCallbackSignature {

		Parameters parameters;

		public NativeCallbackSignature (Parameters parms) 
		{
			parameters = parms;
		}

		public override string ToString ()
		{
			if (parameters.Count == 0)
				return "";

			string[] parms = new string [parameters.Count];
			for (int i = 0; i < parameters.Count; i++) {
				Parameter p = parameters [i];

				parms [i] = "";
				if (p.CType == "GError**")
					parms [i] += "out ";
				else if (p.PassAs != "" && !(p.Generatable is StructBase))
					parms [i] += p.PassAs + " ";
				parms [i] += p.NativeCallbackType + " " + p.Name;
			}

			string import_sig = String.Join (", ", parms);
			return import_sig;
		}
	}
}


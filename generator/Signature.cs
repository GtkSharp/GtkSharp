// GtkSharp.Generation.Signature.cs - The Signature Generation Class.
//
// Author: Mike Kestner <mkestner@ximian.com>
//
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
	using System.Collections.Generic;
	using System.Xml;

	public class Signature  {
		
		private IList<Parameter> parms = new List<Parameter> ();

		public Signature (Parameters parms)
		{
			foreach (Parameter p in parms) {
				if (!parms.IsHidden (p))
					this.parms.Add (p);
			}
		}

		public override string ToString ()
		{
			if (parms.Count == 0)
				return "";

			string[] result = new string [parms.Count];
			int i = 0;

			foreach (Parameter p in parms) {
				result [i] = p.PassAs != "" ? p.PassAs + " " : "";
				result [i++] += p.CSType + " " + p.Name;
			}

			return String.Join (", ", result);
		}

		public string Types {
			get {
				if (parms.Count == 0)
					return "";

				string[] result = new string [parms.Count];
				int i = 0;

				foreach (Parameter p in parms)
					result [i++] = p.CSType;

				return String.Join (":", result);
			}
		}

		public bool IsAccessor {
			get {
				int count = 0;
				foreach (Parameter p in parms) {
					if (p.PassAs == "out")
						count++;
					
					if (count > 1)
						return false;
				}
				return count == 1;
			}
		}

		public string AccessorType {
			get {
				foreach (Parameter p in parms)
					if (p.PassAs == "out")
						return p.CSType;
				
				return null;
			}
		}

		public string AccessorName {
			get {
				foreach (Parameter p in parms)
					if (p.PassAs == "out")
						return p.Name;
				
				return null;
			}
		}

		public string AsAccessor {
			get {
				string[] result = new string [parms.Count - 1];
				int i = 0;

				foreach (Parameter p in parms) {
					if (p.PassAs == "out")
						continue;

					result [i] = p.PassAs != "" ? p.PassAs + " " : "";
					result [i++] += p.CSType + " " + p.Name;
				}
				
				return String.Join (", ", result);
			}
		}

		public string WithoutOptional ()
		{
			if (parms.Count == 0)
				return String.Empty;

			var result = new string [parms.Count];
			int i = 0;

			foreach (Parameter p in parms) {
				if (p.IsOptional && p.PassAs == String.Empty)
					continue;
				result [i] = p.PassAs != String.Empty ? p.PassAs + " " : String.Empty;
				result [i++] += p.CSType + " " + p.Name;
			}

			return String.Join (", ", result, 0, i);
		}

		public string CallWithoutOptionals ()
		{
			if (parms.Count == 0)
				return String.Empty;

			var result = new string [parms.Count];
			int i = 0;

			foreach (Parameter p in parms) {

				result [i] = p.PassAs != "" ? p.PassAs + " " : "";
				if (p.IsOptional && p.PassAs == String.Empty) {
					if (p.IsArray)
						result [i++] += "null";
					else
						result [i++] += p.Generatable.DefaultValue;
				}
				else
					result [i++] += p.Name;
			}

			return String.Join (", ", result);
		}
	}
}


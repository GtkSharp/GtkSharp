// GtkSharp.Generation.ByRefGen.cs - The ByRef type Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2003 Mike Kestner
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

	public class ByRefGen : IGeneratable  {
		
		string type;
		string ctype;
		string ns = "";

		public ByRefGen (string ctype, string type)
		{
			string[] toks = type.Split('.');
			this.ctype = ctype;
			this.type = toks[toks.Length - 1];
			if (toks.Length > 2)
				this.ns = String.Join (".", toks, 0, toks.Length - 2);
			else if (toks.Length == 2)
				this.ns = toks[0];
		}
		
		public string CName {
			get
			{
				return ctype;
			}
		}

		public string Name {
			get
			{
				return type;
			}
		}

		public string QualifiedName {
			get
			{
				return ns + "." + type;
			}
		}

		public string MarshalType {
			get
			{
				return "ref " + QualifiedName;
			}
		}
		public virtual string MarshalReturnType {
			get
			{
				return QualifiedName;
			}
		}

		public string CallByName (string var_name)
		{
			return "ref " + var_name;
		}
		
		public string FromNative(string var)
		{
			return var;
		}
		
		public virtual string FromNativeReturn(string var)
		{
			return var;
		}

		public virtual string ToNativeReturn(string var)
		{
			return var;
		}

		public void Generate ()
		{
		}
		
		public void Generate (GenerationInfo gen_info)
		{
		}
	}
}


// GtkSharp.Generation.AliasGen.cs - The Alias type Generatable.
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

	public class AliasGen : IGeneratable  {
		
		string type;
		string ctype;

		public AliasGen (string ctype, string type)
		{
			this.ctype = ctype;
			this.type = type;
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
				return type;
			}
		}

		public string MarshalType {
			get
			{
				return type;
			}
		}
		public virtual string MarshalReturnType {
			get
			{
				return type;
			}
		}

		public string CallByName (string var_name)
		{
			return var_name;
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


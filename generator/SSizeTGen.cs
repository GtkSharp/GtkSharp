// GtkSharp.Generation.SizeTGen.cs - The size_t Generatable.
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2004 Novell, Inc.
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

	public class SSizeTGen : IGeneratable  {
		
		string ctype;
		string type;

		public string CName {
			get {
				return "ssize_t";
			}
		}

		public string Name {
			get {
				return "long";
			}
		}

		public string QualifiedName {
			get {
				return "long";
			}
		}

		public string MarshalType {
			get {
				return "IntPtr";
			}
		}

		public string MarshalReturnType {
			get
			{
				return MarshalType;
			}
		}

		public string ToNativeReturnType {
			get
			{
				return MarshalType;
			}
		}

		public string CallByName (string var_name)
		{
			return "new IntPtr (" + var_name + ")";
		}
		
		public virtual string FromNative(string var)
		{
			return "(long) " + var;
		}
		
		public virtual string FromNativeReturn(string var)
		{
			return FromNative (var);
		}

		public virtual string ToNativeReturn(string var)
		{
			return CallByName (var);
		}
		
		public void Generate ()
		{
		}
		
		public void Generate (GenerationInfo gen_info)
		{
		}
	}
}


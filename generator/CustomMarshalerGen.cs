// GtkSharp.Generation.CustomMarshalerGen.cs - The CustomMarshaler type Generatable.
//
// Author: Mike Kestner <mkestner@ximian.com>
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

	public class CustomMarshalerGen : IGeneratable  {
		
		string type;
		string ctype;
		string marshaler;

		public CustomMarshalerGen (string ctype, string type, string marshaler)
		{
			this.ctype = ctype;
			this.type = type;
			this.marshaler = marshaler;
		}
		
		public string CName {
			get {
				return ctype;
			}
		}

		public string Name {
			get {
				return type;
			}
		}

		public string QualifiedName {
			get {
				return type;
			}
		}

		public string MarshalType {
			get {
				return "[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(" + marshaler + "))] " + type;
			}
		}

		public virtual string MarshalReturnType {
			get {
				return "[return:MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(" + marshaler + "))]";
			}
		}

		public virtual string ToNativeReturnType {
			get {
				return "[return:MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(" + marshaler + "))]";
			}
		}

		public string CallByName (string var_name)
		{
			return var_name;
		}
		
		public string FromNative(string var)
		{
			return String.Empty;
		}
		
		public virtual string FromNativeReturn(string var)
		{	
			return String.Empty;
		}

		public virtual string ToNativeReturn(string var)
		{
			return String.Empty;
		}

		public void Generate ()
		{
		}
		
		public void Generate (GenerationInfo gen_info)
		{
		}
	}
}


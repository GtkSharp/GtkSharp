// GtkSharp.Generation.CustomMarshalerGen.cs - The CustomMarshaler type Generatable.
//
// Author: Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.

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
				return String.Empty;
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


// GtkSharp.Generation.TimeTGen.cs - The time_t Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2003 Mike Kestner
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

	public class TimeTGen : IGeneratable  {
		
		string ctype;
		string type;
		string ns = "";

		public string CName {
			get
			{
				return "time_t";
			}
		}

		public string Name {
			get
			{
				return "DateTime";
			}
		}

		public string QualifiedName {
			get
			{
				return "System.DateTime";
			}
		}

		public string MarshalType {
			get
			{
				return "IntPtr";
			}
		}

		public string MarshalReturnType {
			get
			{
				return "IntPtr";
			}
		}

		public string ToNativeReturnType {
			get
			{
				return "IntPtr";
			}
		}

		public string CallByName (string var_name)
		{
			return "GLib.Marshaller.DateTimeTotime_t (" + var_name + ")";
		}
		
		public virtual string FromNative(string var)
		{
			return "GLib.Marshaller.time_tToDateTime (" + var + ")";
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


// GtkSharp.Generation.GStringGen.cs - The GString type Generatable.
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

	public class GStringGen : IGeneratable {

		public string CName {
			get {
				return "GString";
			}
		}

		public string Name {
			get {
				return "string";
			}
		}

		public string QualifiedName {
			get {
				return "string";
			}
		}

		public string MarshalType {
			get {
				return "IntPtr";
			}
		}

		public string CallByName (string var_name)
		{
			return "(new GLib.GString (" + var_name + ")).Handle";
		}
		
		public string FromNative (string var)
		{
			return "GLib.GString.PtrToString (" + var + ")";
		}
		
		public string FromNativeReturn (string var)
		{
			return FromNative (var);
		}

		public string ToNativeReturn (string var)
		{
			return CallByName (var);
		}

		public string MarshalReturnType {
			get {
				return "IntPtr";
			}
		}
		
		public string ToNativeReturnType {
			get {
				return "IntPtr";
			}
		}
		
		public void Generate ()
		{
		}
		
		public void Generate (GenerationInfo gen_info)
		{
		}
	}

}


// GtkSharp.Generation.IGeneratable.cs - Interface to generate code for a type.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001 Mike Kestner
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

	public interface IGeneratable  {

		string CName {get;}

		string MarshalType {get;}

		string MarshalReturnType {get;}

		string ToNativeReturnType {get;}

		string Name {get;}

		string QualifiedName {get;}

		string CallByName (string var_name);
		
		string FromNative (string var);

		string FromNativeReturn (string var);

		string ToNativeReturn (string var);

		void Generate ();

		void Generate (GenerationInfo gen_info);
	}
}

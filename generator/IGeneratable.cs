// GtkSharp.IGeneratable.cs - Interface to generate code for a type.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp {

	using System;

	public interface IGeneratable  {

		String CName {get;}

		String MarshalType {get;}

		String Name {get;}

		String CallByName (String var_name);

		void Generate ();
	}
}

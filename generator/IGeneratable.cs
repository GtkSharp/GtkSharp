// GtkSharp.Generation.IGeneratable.cs - Interface to generate code for a type.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	public interface IGeneratable  {

		string CName {get;}

		string MarshalType {get;}

		string MarshalReturnType {get;}

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

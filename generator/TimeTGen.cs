// GtkSharp.Generation.TimeTGen.cs - The time_t Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2003 Mike Kestner

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


// GtkSharp.Generation.StructGen.cs - The Structure Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class StructGen : StructBase, IGeneratable  {
		
		public StructGen (XmlElement ns, XmlElement elem) : base (ns, elem) {}
		
		public override void Generate ()
		{
			base.Generate ();
			Statistics.StructCount++;
		}		
	}
}


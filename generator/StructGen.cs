// GtkSharp.Generation.StructGen.cs - The Structure Generatable.
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

	using System;
	using System.IO;
	using System.Xml;

	public class StructGen : StructBase, IGeneratable  {
		
		public StructGen (XmlElement ns, XmlElement elem) : base (ns, elem) {}
		
		public void Generate ()
		{
			GenerationInfo gen_info = new GenerationInfo (NSElem);
			Generate (gen_info);
		}

		public override void Generate (GenerationInfo gen_info)
		{
			base.Generate (gen_info);
			Statistics.StructCount++;
		}		
	}
}


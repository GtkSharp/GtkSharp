// GtkSharp.Generation.GenBase.cs - The Generatable base class.
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2001-2002 Mike Kestner
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
	using System.IO;
	using System.Xml;

	public abstract class GenBase : IGeneratable {
		
		private XmlElement ns;
		private XmlElement elem;

		protected GenBase (XmlElement ns, XmlElement elem)
		{
			this.ns = ns;
			this.elem = elem;
		}

		public string CName {
			get {
				return elem.GetAttribute ("cname");
			}
		}

		public XmlElement Elem {
			get {
				return elem;
			}
		}

		public int ParserVersion {
			get {
				XmlElement root = elem.OwnerDocument.DocumentElement;
				return root.HasAttribute ("parser_version") ? int.Parse (root.GetAttribute ("parser_version")) : 1;
			}
		}

		public bool IsInternal {
			get {
				return elem.GetAttributeAsBoolean ("internal");
			}
		}

		public string LibraryName {
			get {
				return ns.GetAttribute ("library");
			}
		}

		public abstract string MarshalType { get; }

		public virtual string Name {
			get {
				return elem.GetAttribute ("name");
			}
		}

		public string NS {
			get {
				return ns.GetAttribute ("name");
			}
		}

		public abstract string DefaultValue { get; }

		public string QualifiedName {
			get {
				return NS + "." + Name;
			}
		}

		public abstract string CallByName (string var);

		public abstract string FromNative (string var);

		public abstract bool Validate ();

		public virtual string GenerateGetSizeOf () {
			return null;
		}

		/// <summary>
		/// The alignment this type imposes on a field of it, when the api.xml
		/// says so explicitly.
		/// </summary>
		/// <remarks>
		/// Normally the alignment is measured, by asking the runtime where a
		/// managed replica of the field lands after a leading sbyte. That
		/// cannot see an alignment C asks for and the members do not imply:
		/// graphene_simd4f_t is declared GRAPHENE_ALIGN16 and *is* __m128 on
		/// every SIMD build, so it aligns to 16, while its managed replica is
		/// four floats and aligns to 4. Everything embedding it was measured
		/// short as a result - graphene_plane_t, graphene_euler_t and
		/// graphene_sphere_t are each { 16-byte vector; float } and came out
		/// 20 bytes against C's 32, so every caller-allocates out parameter of
		/// those types under-allocated by twelve and let graphene write past
		/// the end of the block.
		/// </remarks>
		public virtual string GenerateAlign () {
			string align = Elem.GetAttribute ("align");
			return align == String.Empty ? null : align;
		}

		public void Generate ()
		{
			GenerationInfo geninfo = new GenerationInfo (ns);
			Generate (geninfo);
		}

		public abstract void Generate (GenerationInfo geninfo);
	}
}


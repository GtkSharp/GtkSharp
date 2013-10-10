// Authors:
//   Stephan Sundermann <stephansundermann@gmail.com>
//
// Copyright (c) 2013 Stephan Sundermann
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

using System;
using System.Xml;
using System.IO;

namespace GtkSharp.Generation
{
	public class Constant
	{
		private readonly string name;
		private readonly string value;
		private readonly string ctype;

		public Constant (XmlElement elem)
		{
			this.name = elem.GetAttribute ("name");
			this.value = elem.GetAttribute ("value");
			this.ctype = elem.GetAttribute ("ctype");
		}

		public string Name {
			get {
				return this.name;
			}
		}
		public string ConstType {
			get {
				if (IsString)
					return "string";
				// gir registers all integer values as gint even for numbers which do not fit into a gint
				// if the number is too big for an int, try to fit it into a long
				if (SymbolTable.Table.GetMarshalType (ctype) == "int" && value.Length < 20 && long.Parse (value) > Int32.MaxValue)
					return "long";
				return SymbolTable.Table.GetMarshalType (ctype);
			}
		}

		public bool IsString {
			get {
				return (SymbolTable.Table.GetCSType (ctype) == "string");
			}
		}

		public virtual bool Validate (LogWriter log)
		{
			if (ConstType == String.Empty) {
				log.Warn ("{0} type is missing or wrong", Name);
				return false;
			}
			if (SymbolTable.Table.GetMarshalType (ctype) == "int" && value.Length >= 20) {
				return false;
			}
			return true;
		}

		public virtual void Generate (GenerationInfo gen_info, string indent)
		{
			StreamWriter sw = gen_info.Writer;

			sw.WriteLine ("{0}public const {1} {2} = {3}{4}{5};",
			              indent,
			              ConstType,
			              Name,
			              IsString ? "@\"": String.Empty,
			              value,
			              IsString ? "\"": String.Empty);
		}
	}
}

// AbiField.cs - Utility to handle complex C structure ABI.
//
// Authors: Thibault Saunier <thibault.saunier@osg.samsung.com>
//
// Copyright (c) 2017 Thibault Saunier
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

// Helper utility to build C structure ABI even when using complex
// packing with union and bit fields. It basically uses the same
// logic as what is done inside csharp implementation (marshal.c in
// the mono codebase) but handling more things.
namespace GLib {

	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Linq;
	using System.Collections.Specialized;
	using System.CodeDom.Compiler;

	public class AbiField {
		public string Prev_name;
		public string Next_name;
		public string Name;
		public long Offset;
		public long Align;
		public uint Natural_size;
		public AbiStruct container;
		public OrderedDictionary Parent_fields; // field_name<string> -> AbiField<> dictionary.
		List<List<string>> Union_fields;

		long End;
		long Size;
		public uint Bits;

		public AbiField(string name,
				long offset,
				uint natural_size,
				string prev_fieldname,
				string next_fieldname,
				long align,
				uint bits)
		{
			Name = name;
			Offset = offset;
			Natural_size = natural_size;
			Prev_name = prev_fieldname;
			Next_name = next_fieldname;
			End = -1;
			Size = -1;
			Union_fields = null;
			Align = (long) align;
			Bits = bits;

			if (Bits > 0) {
				var nbytes = (uint) (Math.Ceiling((double) Bits / (double) 8));

				if (nbytes < GetSize())
					Align = 1;
				else // Like if no bitfields were used.
					Bits = 0;
			}
		}

		public AbiField(string name,
				OrderedDictionary parent_fields,
				uint natural_size,
				string prev_fieldname,
				string next_fieldname,
				long align,
				uint bits): this (name, -1,
					natural_size, prev_fieldname, next_fieldname, align, bits)
		{
			Parent_fields = parent_fields;
		}


		public AbiField(string name,
				long offset,
				List<List<string>> fields_lists,
				string prev_fieldname,
				string next_fieldname, uint bits): this(name,
					offset, (uint) 0, prev_fieldname, next_fieldname,
					-1, bits)
		{
			Union_fields = fields_lists;
		}

		public uint GetEnd() {
			if (End == -1)
				End = (long) GetOffset() + GetSize();

			return (uint) End;
		}

		public uint GetOffset() {
			return (uint) Offset;
		}

		public bool InUnion () {
			return Name.Contains(".");
		}

		public uint GetAlign () {
			if (Union_fields != null) {
				uint align = 1;

				foreach (var fieldnames in Union_fields) {
					foreach (var fieldname in fieldnames) {
						var field = (AbiField) container.Fields[fieldname];
						align = Math.Max(align, field.GetAlign());
					}
				}

				return align;

			}

			return (uint) Align;
		}

		public uint GetUnionSize() {
			uint size = 0;

			foreach (var fieldnames in Union_fields) {

				foreach (var fieldname in fieldnames) {
					var field = ((AbiField) container.Fields[fieldname]);
					var align = field.GetAlign();

					if (field.Prev_name != null) {
						var prev = ((AbiField)container.Fields[field.Prev_name]);

						if (!prev.InUnion())
							field.Offset = Offset;
						else
							field.Offset = prev.GetEnd();
					}

					field.Offset += align - 1;
					field.Offset &= ~(align - 1);

					size = Math.Max(size, field.GetEnd() - (uint) Offset);
				}
			}

			return size;
		}

		public uint GetSize() {
			if (Size != -1)
				return (uint) Size;

			if (Union_fields != null) {
				Size = GetUnionSize();
			} else {
				Size = Natural_size;
			}

			return (uint) Size;
		}
	}
}

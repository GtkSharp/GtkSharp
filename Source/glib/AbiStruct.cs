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

	public class AbiStruct {
		public OrderedDictionary Fields = null;

		public AbiStruct(OrderedDictionary fields) {
			Fields = fields;
		}
		public AbiStruct(List<GLib.AbiField> fields) {
			Fields = new OrderedDictionary();

			foreach (var field in fields)
				Fields[field.Name] = field;

			Load();
		}

		static uint GetMinAlign(OrderedDictionary fields) {
			uint align = 1;

			for (var i = 0; i < fields.Count; i++) {
				var field = ((GLib.AbiField) fields[i]);

				if (field.Parent_fields != null) {
					align = Math.Max(align, GetMinAlign(field.Parent_fields));
				}
				align = Math.Max(align, field.GetAlign());
			}

			return align;
		}

		public uint Align {
			get {
				return GetMinAlign(Fields);
			}
		}

		void Load () {
			long bitfields_offset = -1;
			long bitfields_size = -1;

			for (var i = 0; i < Fields.Count; i++ ) {
				var field = (AbiField) Fields[i];
				field.container = this;
				var align = field.GetAlign();

				if (field.InUnion())
					continue;

				if (field.Prev_name != null) {
					field.Offset = ((AbiField)Fields[field.Prev_name]).GetEnd();
				} else if (field.Parent_fields != null) {
					field.Offset = GetStructureSize(field.Parent_fields);
				}

				field.Offset += align - 1;
				field.Offset &= ~(align - 1);

				if (field.Bits > 0) {
					// Pack all following bitfields into the same area.
					if (bitfields_offset == -1) {
						uint nbits = 0;

						bitfields_offset = field.Offset;
						for (var j = i + 1; j < Fields.Count; j++ ) {
							var nfield = (AbiField) Fields[j];
							if (nfield.Bits > 0)
								nbits += nfield.Bits;
						}

						bitfields_size = (long) (Math.Ceiling((double) field.Bits / (double) 8));
					}

					field.Offset = (uint) bitfields_offset;
					field.Natural_size = (uint) bitfields_size;
				} else {
					bitfields_offset = bitfields_size = -1;
				}

			}
		}

		public uint GetFieldOffset(string field) {
			return ((AbiField) Fields[field]).GetOffset();
		}

		static uint GetStructureSize(OrderedDictionary fields) {
			uint size = 0;
			uint min_align = GetMinAlign(fields);

			for (var i = 0; i < fields.Count; i++) {
				var field = ((GLib.AbiField) fields[i]);

				if (field.InUnion())
					continue;

				var tsize = (uint) (Math.Ceiling((double) (field.GetEnd() / (double) min_align)) * (double) min_align);

				size = Math.Max(tsize, size);
			}

			return size;
		}

		public uint Size {
			get {

				return GetStructureSize(Fields);
			}
		}
	}
}

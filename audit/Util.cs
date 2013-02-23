//
// Util.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// Copyright (C) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

using GuiCompare;

namespace Mono.AssemblyCompare {

	static class TypeHelper {

		public static AssemblyResolver Resolver = new AssemblyResolver ();

		internal static bool IsPublic (TypeReference typeref)
		{
			if (typeref == null)
				throw new ArgumentNullException ("typeref");

			TypeDefinition td = typeref.Resolve ();
			if (td == null)
				return false;

			return td.IsPublic;
		}

		internal static bool IsDelegate (TypeReference typeref)
		{
			return IsDerivedFrom (typeref, "System.MulticastDelegate");
		}

		internal static bool IsDerivedFrom (TypeReference type, string derivedFrom)
		{
			bool first = true;
			foreach (var def in WalkHierarchy (type)) {
				if (first) {
					first = false;
					continue;
				}
				
				if (def.FullName == derivedFrom)
					return true;
			}
			
			return false;
		}

		internal static IEnumerable<TypeDefinition> WalkHierarchy (TypeReference type)
		{
			for (var def = type.Resolve (); def != null; def = GetBaseType (def))
				yield return def;
		}

		internal static IEnumerable<TypeReference> GetInterfaces (TypeReference type)
		{
			var ifaces = new Dictionary<string, TypeReference> ();

			foreach (var def in WalkHierarchy (type))
				foreach (TypeReference iface in def.Interfaces)
					ifaces [iface.FullName] = iface;

			return ifaces.Values;
		}

		internal static TypeDefinition GetBaseType (TypeDefinition child)
		{
			if (child.BaseType == null)
				return null;

			return child.BaseType.Resolve ();
		}

		internal static bool IsPublic (CustomAttribute att)
		{
			return IsPublic (att.AttributeType);
		}

		internal static string GetFullName (CustomAttribute att)
		{
			return att.AttributeType.FullName;
		}

		internal static TypeDefinition GetTypeDefinition (CustomAttribute att)
		{
			return att.AttributeType.Resolve ();
		}
	}
}

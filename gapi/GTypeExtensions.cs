// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2009 Novell, Inc.
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


using System;
using System.Runtime.InteropServices;

namespace Gapi {

	public static class GTypeExtensions {

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_class_peek (IntPtr gtype);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_class_ref (IntPtr gtype);

		public static IntPtr GetClassPtr (this GLib.GType gtype)
		{
			IntPtr klass = g_type_class_peek (gtype.Val);
			if (klass == IntPtr.Zero)
				klass = g_type_class_ref (gtype.Val);
			return klass;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_parent (IntPtr type);

		public static GLib.GType GetBaseType (this GLib.GType gtype)
		{
			IntPtr parent = g_type_parent (gtype.Val);
			return parent == IntPtr.Zero ? GLib.GType.None : new GLib.GType (parent);
		}

		public static GLib.GType GetThresholdType (this GLib.GType gtype)
		{
			GLib.GType curr = gtype;
			while (curr.ToString ().StartsWith ("__gtksharp_"))
				curr = GetBaseType (curr);
			return curr;
		}

		struct GTypeQuery {
			public IntPtr type;
			public IntPtr type_name;
			public uint class_size;
			public uint instance_size;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_type_query (IntPtr type, out GTypeQuery query);

		public static uint GetClassSize (this GLib.GType gtype)
		{
			GTypeQuery query;
			g_type_query (gtype.Val, out query);
			return query.class_size;
		}

		static IntPtr ValFromInstancePtr (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return IntPtr.Zero;

			// First field of instance is a GTypeClass*.  
			IntPtr klass = Marshal.ReadIntPtr (handle);
			// First field of GTypeClass is a GType.
			return Marshal.ReadIntPtr (klass);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern bool g_type_is_a (IntPtr type, IntPtr is_a_type);

		public static bool IsInstance (this GLib.GType gtype, IntPtr raw)
		{
			return g_type_is_a (ValFromInstancePtr (raw), gtype.Val);
		}
	}
}

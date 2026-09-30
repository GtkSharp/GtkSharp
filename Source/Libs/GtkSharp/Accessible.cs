// Gtk.Accessible.cs - the halves of GtkAccessible the api.xml cannot describe
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

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	/// <summary>
	/// The parts of <c>GtkAccessible</c> that neither the api.xml nor the gir
	/// can express, so that a state, a property or a relation can be set at
	/// all.
	/// </summary>
	/// <remarks>
	/// Gtk offers each of the three in two spellings. The varargs one
	/// (<c>gtk_accessible_update_state</c>) cannot be bound, and codegen drops
	/// it. The other (<c>gtk_accessible_update_state_value</c>) takes two
	/// parallel arrays behind a single count, which the api.xml has no way to
	/// say either - so codegen bound the array of states as an <c>out</c>
	/// return value and the array of GValues as one struct. That left the whole
	/// update API unusable on every widget in the tree, since these are
	/// interface methods on GtkAccessible.
	///
	/// The <c>InitValue</c> helpers exist in Gtk for exactly this purpose - the
	/// documentation says they are "mostly meant for language bindings" - but
	/// the gir attaches them to the enum rather than to a class, and gapi
	/// enums carry no methods, so they never reached the api.xml.
	/// </remarks>
	public static class AccessibleExtensions {

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_accessible_update_state_value (IntPtr raw, int n_states, int[] states, IntPtr values);
		static d_gtk_accessible_update_state_value gtk_accessible_update_state_value = FuncLoader.LoadFunction<d_gtk_accessible_update_state_value> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gtk), "gtk_accessible_update_state_value"));

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_accessible_update_property_value (IntPtr raw, int n_properties, int[] properties, IntPtr values);
		static d_gtk_accessible_update_property_value gtk_accessible_update_property_value = FuncLoader.LoadFunction<d_gtk_accessible_update_property_value> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gtk), "gtk_accessible_update_property_value"));

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_accessible_update_relation_value (IntPtr raw, int n_relations, int[] relations, IntPtr values);
		static d_gtk_accessible_update_relation_value gtk_accessible_update_relation_value = FuncLoader.LoadFunction<d_gtk_accessible_update_relation_value> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gtk), "gtk_accessible_update_relation_value"));

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_accessible_state_init_value (int state, ref GLib.Value value);
		static d_gtk_accessible_state_init_value gtk_accessible_state_init_value = FuncLoader.LoadFunction<d_gtk_accessible_state_init_value> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gtk), "gtk_accessible_state_init_value"));

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_accessible_property_init_value (int property, ref GLib.Value value);
		static d_gtk_accessible_property_init_value gtk_accessible_property_init_value = FuncLoader.LoadFunction<d_gtk_accessible_property_init_value> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gtk), "gtk_accessible_property_init_value"));

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_gtk_accessible_relation_init_value (int relation, ref GLib.Value value);
		static d_gtk_accessible_relation_init_value gtk_accessible_relation_init_value = FuncLoader.LoadFunction<d_gtk_accessible_relation_init_value> (FuncLoader.GetProcAddress (GLibrary.Load (Library.Gtk), "gtk_accessible_relation_init_value"));

		/// <summary>A GValue initialised to the type <paramref name="state"/> expects.</summary>
		/// <remarks>
		/// Gtk 4.22 has no case for <c>GTK_ACCESSIBLE_STATE_VISITED</c>, so that
		/// one comes back untyped (<c>GType.Invalid</c>) and has to be built as
		/// a plain boolean instead. Check <c>ValueType</c> rather than assuming.
		/// </remarks>
		public static GLib.Value InitValue (this Gtk.AccessibleState state)
		{
			GLib.Value value = default (GLib.Value);
			gtk_accessible_state_init_value ((int) state, ref value);
			return value;
		}

		/// <summary>A GValue initialised to the type <paramref name="property"/> expects.</summary>
		public static GLib.Value InitValue (this Gtk.AccessibleProperty property)
		{
			GLib.Value value = default (GLib.Value);
			gtk_accessible_property_init_value ((int) property, ref value);
			return value;
		}

		/// <summary>A GValue initialised to the type <paramref name="relation"/> expects.</summary>
		public static GLib.Value InitValue (this Gtk.AccessibleRelation relation)
		{
			GLib.Value value = default (GLib.Value);
			gtk_accessible_relation_init_value ((int) relation, ref value);
			return value;
		}

		public static void UpdateState (this Gtk.IAccessible accessible, Gtk.AccessibleState state, GLib.Value value)
		{
			accessible.UpdateState (new Gtk.AccessibleState [] { state }, new GLib.Value [] { value });
		}

		public static void UpdateState (this Gtk.IAccessible accessible, Gtk.AccessibleState[] states, GLib.Value[] values)
		{
			int count = Check (accessible, states, values, "states");
			if (count == 0)
				return;

			int[] raw_states = new int [count];
			for (int i = 0; i < count; i++)
				raw_states [i] = (int) states [i];

			IntPtr raw_values = ValuesToPtr (values, count);
			try {
				gtk_accessible_update_state_value (accessible.Handle, count, raw_states, raw_values);
			} finally {
				Marshal.FreeHGlobal (raw_values);
			}
		}

		public static void UpdateProperty (this Gtk.IAccessible accessible, Gtk.AccessibleProperty property, GLib.Value value)
		{
			accessible.UpdateProperty (new Gtk.AccessibleProperty [] { property }, new GLib.Value [] { value });
		}

		public static void UpdateProperty (this Gtk.IAccessible accessible, Gtk.AccessibleProperty[] properties, GLib.Value[] values)
		{
			int count = Check (accessible, properties, values, "properties");
			if (count == 0)
				return;

			int[] raw_properties = new int [count];
			for (int i = 0; i < count; i++)
				raw_properties [i] = (int) properties [i];

			IntPtr raw_values = ValuesToPtr (values, count);
			try {
				gtk_accessible_update_property_value (accessible.Handle, count, raw_properties, raw_values);
			} finally {
				Marshal.FreeHGlobal (raw_values);
			}
		}

		public static void UpdateRelation (this Gtk.IAccessible accessible, Gtk.AccessibleRelation relation, GLib.Value value)
		{
			accessible.UpdateRelation (new Gtk.AccessibleRelation [] { relation }, new GLib.Value [] { value });
		}

		/// <summary>Sets a relation that points at other accessibles - labelled-by, described-by, controls, flow-to, owns, active-descendant and their inverses.</summary>
		/// <remarks>
		/// The two shapes Gtk uses here are both awkward to build by hand, and
		/// which one a relation wants is not evident from its name:
		/// active-descendant is a single GtkAccessible, and every other
		/// reference relation - error-message included - is a bare
		/// G_TYPE_POINTER holding a GList of GtkAccessible*. For the list ones
		/// Gtk also accepts a GtkAccessibleList in the GValue, which is what
		/// this sends, since a GList is not a shape a caller should have to
		/// assemble.
		///
		/// The shape is taken from gtk_accessible_relation_init_value rather
		/// than from a table here, so a relation added to a later Gtk is
		/// classified by Gtk itself.
		/// </remarks>
		public static void UpdateRelation (this Gtk.IAccessible accessible, Gtk.AccessibleRelation relation, params Gtk.IAccessible[] targets)
		{
			if (targets == null)
				throw new ArgumentNullException ("targets");

			GLib.Value shape = relation.InitValue ();
			bool takes_one = shape.ValueType.Val == Gtk.AccessibleAdapter.GType.Val;
			// Gtk leaves a relation it has no case for untyped, and g_value_unset
			// on one of those is an assertion failure of its own.
			if (shape.ValueType.Val != GLib.GType.Invalid.Val)
				shape.Dispose ();

			if (takes_one) {
				if (targets.Length != 1)
					throw new ArgumentException (relation + " points at exactly one accessible", "targets");

				GLib.Object target = targets [0] as GLib.Object;
				if (target == null)
					throw new ArgumentException ("target must be a GLib.Object", "targets");

				GLib.Value one = new GLib.Value (target);
				try {
					accessible.UpdateRelation (relation, one);
				} finally {
					one.Dispose ();
				}
				return;
			}

			Gtk.AccessibleList list = new Gtk.AccessibleList (targets);
			GLib.Value value = new GLib.Value (Gtk.AccessibleList.GType);
			try {
				value.Val = list;
				accessible.UpdateRelation (relation, value);
			} finally {
				value.Dispose ();
				list.Dispose ();
			}
		}

		public static void UpdateRelation (this Gtk.IAccessible accessible, Gtk.AccessibleRelation[] relations, GLib.Value[] values)
		{
			int count = Check (accessible, relations, values, "relations");
			if (count == 0)
				return;

			int[] raw_relations = new int [count];
			for (int i = 0; i < count; i++)
				raw_relations [i] = (int) relations [i];

			IntPtr raw_values = ValuesToPtr (values, count);
			try {
				gtk_accessible_update_relation_value (accessible.Handle, count, raw_relations, raw_values);
			} finally {
				Marshal.FreeHGlobal (raw_values);
			}
		}

		static int Check<T> (Gtk.IAccessible accessible, T[] attributes, GLib.Value[] values, string name)
		{
			if (accessible == null)
				throw new ArgumentNullException ("accessible");
			if (attributes == null)
				throw new ArgumentNullException (name);
			if (values == null)
				throw new ArgumentNullException ("values");

			// Gtk reads values[i] for every attributes[i] it was told about, so a
			// shorter values array is a read past the end rather than an error.
			if (attributes.Length != values.Length)
				throw new ArgumentException ("values must have one entry per " + name, "values");

			return attributes.Length;
		}

		// A block of n GValues laid end to end. Gtk only reads them, so the
		// caller keeps ownership of the originals and only the block is freed.
		static IntPtr ValuesToPtr (GLib.Value[] values, int count)
		{
			int size = Marshal.SizeOf (typeof (GLib.Value));
			IntPtr block = Marshal.AllocHGlobal (size * count);
			for (int i = 0; i < count; i++)
				Marshal.StructureToPtr (values [i], new IntPtr (block.ToInt64 () + i * size), false);
			return block;
		}
	}

	public partial class AccessibleList {

		/// <summary>Builds a list of accessibles - the value a reference-list relation takes.</summary>
		/// <remarks>
		/// gtk_accessible_list_new_from_array takes "GtkAccessible **" plus a
		/// count, which codegen bound as a single GtkAccessible* - so the
		/// generated constructor took one accessible and a number, and Gtk read
		/// that many pointers out of a block holding one object.
		///
		/// It is not simply rebound over a real array because the array entry
		/// point cannot be used at all: Gtk 4.22 guards it with
		/// g_return_val_if_fail (accessibles == NULL || n_accessibles == 0),
		/// an inverted assertion that rejects every non-empty array and hands
		/// back NULL. The GList entry point beside it has no such guard, so
		/// this goes through that instead.
		/// </remarks>
		public AccessibleList (Gtk.IAccessible[] accessibles)
		{
			if (accessibles == null)
				throw new ArgumentNullException ("accessibles");

			// owned: the GList cells are ours to free once Gtk has copied them.
			// elements_owned stays false - the accessibles belong to the caller.
			GLib.List list = new GLib.List (IntPtr.Zero, typeof (Gtk.IAccessible), true, false);
			try {
				foreach (Gtk.IAccessible accessible in accessibles)
					list.Append (accessible == null ? IntPtr.Zero : accessible.Handle);

				Raw = gtk_accessible_list_new_from_list (list.Handle);
			} finally {
				list.Dispose ();
			}
		}
	}
}

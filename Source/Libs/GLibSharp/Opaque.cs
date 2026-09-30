// Opaque .cs - Opaque struct wrapper implementation
//
// Authors: Bob Smith <bob@thestuff.net>
//	    Mike Kestner <mkestner@speakeasy.net>
//	    Rachel Hestilow <hestilow@ximian.com>
//
// Copyright (c) 2001 Bob Smith 
// Copyright (c) 2001 Mike Kestner
// Copyright (c) 2002 Rachel Hestilow
// Copyright (c) 2004 Novell, Inc.
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


namespace GLib {

	using System;
	using System.Reflection;

	public class Opaque : IWrapper, IDisposable {

		IntPtr _obj;
		bool owned;

		static readonly System.Collections.Generic.Dictionary<Type, bool> takes_a_reference =
			new System.Collections.Generic.Dictionary<Type, bool> ();

		// Whether wrapping a raw pointer in this type gives the wrapper a claim
		// of its own, i.e. whether the type overrides the Ref hook the Raw
		// setter calls. For a reference-counted opaque the answer is yes and the
		// wrapper outlives whatever handed the pointer over; for a plain boxed
		// one -- Gtk.TreePath, Pango.FontDescription -- the wrapper is a bare
		// alias, and anyone who needs it to survive has to copy.
		//
		// The lookup is reflective, so it is cached: this is on the path every
		// boxed signal argument and every boxed property getter takes.
		internal static bool WrappingTakesAReference (Type type)
		{
			if (type == null)
				return false;

			lock (takes_a_reference) {
				bool cached;
				if (takes_a_reference.TryGetValue (type, out cached))
					return cached;

				MethodInfo mi = type.GetMethod ("Ref",
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy,
					null, new Type[] { typeof (IntPtr) }, null);

				bool result = mi != null && mi.DeclaringType != typeof (Opaque);
				takes_a_reference [type] = result;
				return result;
			}
		}

		public static Opaque GetOpaque (IntPtr o, Type type, bool owned)
		{
			if (o == IntPtr.Zero)
				return null;

			Opaque opaque = (Opaque)Activator.CreateInstance (type, new object[] { o });
			if (owned) {
				if (opaque.owned) {
					// The constructor took a Ref it shouldn't have, so undo it
					opaque.Unref (o);
				}
				opaque.owned = true;
			} else 
				opaque = opaque.Copy (o);

			return opaque;
  		}
  
		/// <summary>
		/// A block for a C function to fill in, allocated by whatever the type's
		/// own free function is going to release.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A caller-allocates out parameter is storage this side provides and
		/// the wrapper then owns, so it is eventually handed to the type's
		/// <c>Free</c> -- <c>graphene_matrix_free</c>,
		/// <c>pango_glyph_string_free</c>. <b>Those do not all free what
		/// g_malloc allocates.</b> Every graphene type carrying a SIMD vector is
		/// allocated with <c>graphene_aligned_alloc</c>, which is
		/// <c>_aligned_malloc</c> where the compiler has it, and freeing such a
		/// pointer with the wrong deallocator is heap corruption on Windows --
		/// exit code 0xC0000374, minutes later, in unrelated work.
		/// </para>
		/// <para>
		/// So the block comes from the type's own allocator when it has one: a
		/// parameterless constructor (<c>new Graphene.Vec3 ()</c> is
		/// <c>graphene_vec3_alloc</c>) or a static <c>Alloc</c>
		/// (<c>Graphene.Rect.Alloc</c>). The wrapper that made it gives up
		/// ownership immediately, so the pointer belongs to whoever receives it.
		/// Where a type has neither -- <c>Gtk.BitsetIter</c>, <c>Gsk.PathPoint</c>
		/// -- this falls back to a zeroed g_malloc, which is what every one of
		/// these used to get.
		/// </para>
		/// <para>
		/// The factory is looked up once per type and cached, because these are
		/// not cold paths: this is what every matrix multiply allocates.
		/// </para>
		/// </remarks>
		public static IntPtr AllocateNative (Type type, ulong fallbackSize)
		{
			Func<Opaque> make = AllocatorFor (type);

			if (make != null) {
				Opaque allocated = null;
				try {
					allocated = make ();
				} catch (Exception) {
					allocated = null;
				}

				if (allocated != null && allocated.Handle != IntPtr.Zero) {
					allocated.Owned = false;
					return allocated.Handle;
				}
			}

			return Marshaller.Malloc0 (fallbackSize);
		}

		static readonly System.Collections.Generic.Dictionary<Type, Func<Opaque>> allocators =
			new System.Collections.Generic.Dictionary<Type, Func<Opaque>> ();

		static Func<Opaque> AllocatorFor (Type type)
		{
			lock (allocators) {
				Func<Opaque> cached;
				if (allocators.TryGetValue (type, out cached))
					return cached;

				Func<Opaque> make = null;

				var ctor = type.GetConstructor (Type.EmptyTypes);
				if (ctor != null)
					make = () => (Opaque) ctor.Invoke (null);
				else {
					var alloc = type.GetMethod ("Alloc",
						System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
						null, Type.EmptyTypes, null);
					if (alloc != null && typeof (Opaque).IsAssignableFrom (alloc.ReturnType))
						make = () => (Opaque) alloc.Invoke (null, null);
				}

				allocators [type] = make;
				return make;
			}
		}

		public Opaque ()
		{
			owned = true;
		}

		public Opaque (IntPtr raw)
		{
			owned = false;
			Raw = raw;
		}

		// Since .NET 7, IntPtr is nint and int converts to it implicitly. Where a
		// type has both a raw-pointer constructor and a numeric one -- Date takes
		// a Julian day, DateTime a Unix time, ValueArray a preallocation count --
		// "new Date (2)" reads as the numeric overload and binds to the pointer
		// one, then dereferences address 2. The wrapper libraries are
		// LangVersion 9, where the conversion does not exist, so this reaches
		// only consumers of the package.
		//
		// No address in the first page is mappable on any platform this runs on:
		// the null page is reserved precisely so a small integer faults. Such an
		// argument is a mistake, and saying so beats the fault -- which was an
		// access violation, and worse still one raised by the finalizer later,
		// landing on an unrelated piece of work.
		//
		// Returns the pointer so it can be used in a base-call argument, which is
		// the only place it runs *before* the handle is stored.
		internal static IntPtr CheckRaw (IntPtr raw, string name)
		{
			if (raw != IntPtr.Zero && (ulong) (long) raw < 0x10000)
				throw new ArgumentException (
					"An address in the first page cannot be a valid pointer. This is almost " +
					"certainly a number that bound to the raw-pointer constructor, because int " +
					"converts to IntPtr implicitly: add the suffix the numeric overload needs " +
					"(2u, 2L) to reach it.", name);

			return raw;
		}

		protected IntPtr Raw {
			get {
				return _obj;
			}
			set {
				if (_obj == value) {
					return;
				}

				if (_obj != IntPtr.Zero) {
					Unref (_obj);
					if (owned)
						Free (_obj);
				}
				_obj = value;
				if (_obj != IntPtr.Zero) {
					Ref (_obj);
				}
			}
		}       

		public virtual void Dispose ()
		{
			Raw = IntPtr.Zero;
			GC.SuppressFinalize (this);
		}

		// These take an IntPtr arg so we don't get conflicts if we need
		// to have an "[Obsolete] public void Ref ()"

		protected virtual void Ref (IntPtr raw) {}
		protected virtual void Unref (IntPtr raw) {}
		protected virtual void Free (IntPtr raw) {}
		protected virtual Opaque Copy (IntPtr raw) 
		{
			return this;
		}

		public IntPtr Handle {
			get {
				return _obj;
			}
		}

		public IntPtr OwnedCopy {
			get {
				Opaque result = Copy (Handle);
				result.Owned = false;
				return result.Handle;
			}
		}

		public bool Owned {
			get {
				return owned;
			}
			set {
				owned = value;
			}
		}

		public override bool Equals (object o)
		{
			if (!(o is Opaque))
				return false;

			return (Handle == ((Opaque) o).Handle);
		}

		public override int GetHashCode ()
		{
			return Handle.GetHashCode ();
		}
	}
}

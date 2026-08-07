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

	public class Opaque : IWrapper, IDisposable {

		IntPtr _obj;
		bool owned;

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

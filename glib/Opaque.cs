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
	using System.Collections;
	using System.ComponentModel;
	using System.Runtime.InteropServices;

	public class Opaque : IWrapper, IDisposable {

		IntPtr _obj;
		bool owned;

		// We don't have to do as much work here as GLib.Object.GetObject
		// does; users can't subclass opaque types, so nothing bad will happen
		// if we accidentally end up creating two wrappers for the same object.

		static Hashtable Opaques = new Hashtable();

		public static Opaque GetOpaque (IntPtr o)
		{
			WeakReference reference = (WeakReference) Opaques[o];
			if (reference == null)
				return null;
			if (!reference.IsAlive) {
				Opaques.Remove (o);
				return null;
			}

			return (Opaque) reference.Target;
  		}
  
		public static Opaque GetOpaque (IntPtr o, Type type, bool owned)
		{
			Opaque opaque = GetOpaque (o);
			if (opaque != null) {
				if (owned)
					opaque.owned = true;
				return opaque;
			}

			opaque = (Opaque)Activator.CreateInstance (type, new object[] { o });
			opaque.owned = owned;
			return opaque;
  		}
  
		public Opaque ()
		{
			owned = true;
		}

		public Opaque (IntPtr raw)
		{
			Raw = raw;
			owned = false;
		}

		protected IntPtr Raw {
			get {
				return _obj;
			}
			set {
				if (_obj != IntPtr.Zero) {
					Opaques.Remove (_obj);
					Unref (_obj);
					if (owned)
						Free (_obj);
				}
				_obj = value;
				if (_obj != IntPtr.Zero) {
					Ref (_obj);
					Opaques [_obj] = new WeakReference (this);
				}
			}
		}       

		~Opaque ()
		{
			Dispose ();
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

		public IntPtr Handle {
			get {
				return _obj;
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

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

	public class Opaque : IWrapper {

		IntPtr _obj;
		static Hashtable Opaques = new Hashtable();

		public static Opaque GetOpaque(IntPtr o)
		{
			WeakReference reference = (WeakReference) Opaques[(int)o];
			if (reference == null || !reference.IsAlive)
				return null;

			return (Opaque) reference.Target;
  		}
  
		public Opaque () {}

		public Opaque (IntPtr raw)
		{
			Raw = raw;
		}

		protected IntPtr Raw {
			get {
				return _obj;
			}
			set {
				Opaques [value] = new WeakReference (this);
				_obj = value;
			}
		}       

		public IntPtr Handle {
			get {
				return _obj;
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

// BindingAttribute.cs - Attribute to specify key bindings
//
// Author: Mike Kestner  <mkestner@ximian.com>
//
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


namespace Gtk {

	using System;

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class BindingAttribute : Attribute {
		Gdk.Key key;
		Gdk.ModifierType mod;
		string handler;
		object[] parms;

		public BindingAttribute (Gdk.Key key, string handler, params object[] parms) : this (key, 0, handler, parms) {}

		public BindingAttribute (Gdk.Key key, Gdk.ModifierType mod, string handler, params object[] parms)
		{
			this.key = key;
			this.mod = mod;
			this.handler = handler;
			this.parms = parms;
		}

		public Gdk.Key Key {
			get {
				return key;
			}
		}

		public Gdk.ModifierType Mod {
			get {
				return mod;
			}
		}

		public string Handler {
			get {
				return handler;
			}
		}

		public object[] Parms {
			get {
				return parms;
			}
		}
	}
}


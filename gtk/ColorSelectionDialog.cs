// Gtk.ColorSelectionDialog.cs - Gtk ColorSelectionDialog class customizations
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002 Ximian, Inc. 
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

	public partial class ColorSelectionDialog {

		[Obsolete ("Do not use this class. It will cause your app to crash in mysterious ways.")]
		public class ColorSelectionButton : Gtk.Button {
			private ColorSelectionDialog color_sel;

			public ColorSelectionDialog ColorSelectionDialog {
				get { return color_sel; }
			}

			public ColorSelectionButton (ColorSelectionDialog cs, IntPtr raw) : base (raw) {
				color_sel = cs;
			}
		}
	}
}

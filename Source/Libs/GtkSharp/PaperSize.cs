// Gtk.PaperSize.cs - Allow customization of values in the GtkPaperSize
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
//

namespace Gtk {

	using System;

	public partial class PaperSize {

		// Each of these used to hand back a lazily created *singleton*, cached in a
		// static field. PaperSize is IDisposable, so
		//
		//     using (var paper = Gtk.PaperSize.A4) { ... }
		//
		// -- the obvious thing to write -- freed the shared GtkPaperSize and left
		// the static field pointing at it. Every later read of Gtk.PaperSize.A4 in
		// the process then returned a dangling handle, and the next call through it
		// was an access violation rather than an exception.
		//
		// A caller cannot be expected to know that a property is secretly shared,
		// and there is nothing to gain by sharing it: gtk_paper_size_new is cheap
		// and the result is small. So each read now returns a paper size of its
		// own, which the caller owns and may dispose.
		public static PaperSize Letter {
			get { return new PaperSize ("na_letter"); }
		}

		public static PaperSize Executive {
			get { return new PaperSize ("na_executive"); }
		}

		public static PaperSize Legal {
			get { return new PaperSize ("na_legal"); }
		}

		public static PaperSize A3 {
			get { return new PaperSize ("iso_a3"); }
		}

		public static PaperSize A4 {
			get { return new PaperSize ("iso_a4"); }
		}

		public static PaperSize A5 {
			get { return new PaperSize ("iso_a5"); }
		}

		public static PaperSize B5 {
			get { return new PaperSize ("iso_b5"); }
		}
	}
}

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

		static PaperSize letter;
		public static PaperSize Letter {
			get {
				if (letter == null)
					letter = new PaperSize ("na_letter");
				return letter;
			}
		}

		static PaperSize executive;
		public static PaperSize Executive {
			get {
				if (executive == null)
					executive = new PaperSize ("na_executive");
				return executive;
			}
		}

		static PaperSize legal;
		public static PaperSize Legal {
			get {
				if (legal == null)
					legal = new PaperSize ("na_legal");
				return legal;
			}
		}

		static PaperSize a3;
		public static PaperSize A3 {
			get {
				if (a3 == null)
					a3 = new PaperSize ("iso_a3");
				return a3;
			}
		}

		static PaperSize a4;
		public static PaperSize A4 {
			get {
				if (a4 == null)
					a4 = new PaperSize ("iso_a4");
				return a4;
			}
		}

		static PaperSize a5;
		public static PaperSize A5 {
			get {
				if (a5 == null)
					a5 = new PaperSize ("iso_a5");
				return a5;
			}
		}

		static PaperSize b5;
		public static PaperSize B5 {
			get {
				if (b5 == null)
					b5 = new PaperSize ("iso_b5");
				return b5;
			}
		}
	}
}

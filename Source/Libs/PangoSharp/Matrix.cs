// Pango.Matrix.cs - Pango Matrix class customizations
//
// Authors: John Luke <john.luke@gmail.com>
//
// Copyright (c) 2005 John Luke.
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

namespace Pango {

	using System;

	public partial struct Matrix {

		// Every operation on a PangoMatrix mutates it in place, and this used to
		// be a static *field*: Pango.Matrix.Identity.Rotate (90) compiles, reads
		// like arithmetic on a constant, and leaves the identity permanently
		// rotated for every other caller in the process. A get-only property
		// hands out a fresh copy, so the mutation lands on the temporary.
		public static Matrix Identity {
			get {
				return new Matrix { Xx = 1.0, Xy = 0.0, Yx = 0.0, Yy = 1.0, X0 = 0.0, Y0 = 0.0 };
			}
		}
	}
}

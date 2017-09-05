// Scale.cs
//
// Author: Daniel Morgan <danmorg@sc.rr.com>
//
// Copyright (C) 2002 Daniel Morgan
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

using System;

namespace Pango {
	
	public sealed class Scale {
		public static readonly double PangoScale = 1024.0;

		public static readonly double XXSmall = 0.5787037037037;
		public static readonly double XSmall = 0.6444444444444;
		public static readonly double Small = 0.8333333333333;
		public static readonly double Medium = 1.0;
		public static readonly double Large = 1.2;
		public static readonly double XLarge = 1.4399999999999;
		public static readonly double XXLarge = 1.728;

		[Obsolete ("Replaced by XXSmall")]
		public static readonly double XX_Small = XXSmall;
		[Obsolete ("Replaced by XSmall")]
		public static readonly double X_Small = XSmall;
		[Obsolete ("Replaced by XLarge")]
		public static readonly double X_Large = XLarge;
		[Obsolete ("Replaced by XXLarge")]
		public static readonly double XX_Large = XXLarge;
	}
}

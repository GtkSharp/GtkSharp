// WidgetAttribute.cs 
//
// Author: Rachel Hestilow <hestilow@ximian.com> 
//
// Copyright (c) 2003 Rachel Hestilow
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

namespace Glade {
	using System;
	
	[AttributeUsage (AttributeTargets.Field)]
	public class WidgetAttribute : Attribute
	{
		private string name;
		private bool specified;
		
		public WidgetAttribute (string name)
		{
			specified = true;
			this.name = name;
		}

		public WidgetAttribute ()
		{
			specified = false;
		}

		public string Name
		{
			get { return name; }
		}

		public bool Specified
		{
			get { return specified; }
		}
	}
}


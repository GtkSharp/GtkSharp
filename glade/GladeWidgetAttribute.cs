// GladeWidgetAttribute.cs 
//
// Author: Rachel Hestilow <hestilow@ximian.com> 
//
// (c) 2003 Rachel Hestilow

namespace Glade {
	using System;
	
	[AttributeUsage (AttributeTargets.Field)]
	public class GladeWidgetAttribute : Attribute
	{
		private string name;
		
		public GladeWidgetAttribute (string name)
		{
			this.name = name;
		}

		public string Name
		{
			get { return name; }
		}
	}
}


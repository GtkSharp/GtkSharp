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
		private bool specified;
		
		public GladeWidgetAttribute (string name)
		{
			specified = true;
			this.name = name;
		}

		public GladeWidgetAttribute ()
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


// WidgetAttribute.cs 
//
// Author: Rachel Hestilow <hestilow@ximian.com> 
//
// (c) 2003 Rachel Hestilow

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


using System;

namespace GtkDemo
{
	[AttributeUsage (AttributeTargets.Class)]
	public class DemoAttribute : Attribute
	{
		string label, filename, parent;

		public DemoAttribute (string label, string filename) : this (label, filename, null)
		{
		}

		public DemoAttribute (string label, string filename, string parent)
		{
			this.label = label;
			this.filename = filename;
			this.parent = parent;
		}

		public string Filename {
			get { return filename; }
		}

		public string Label {
			get { return label; }
		}

		public string Parent {
			get { return parent; }
		}
	}
}


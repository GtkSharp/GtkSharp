namespace Gtk {

	using System;

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class CssNameAttribute : Attribute {

		public CssNameAttribute (string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
	}
}

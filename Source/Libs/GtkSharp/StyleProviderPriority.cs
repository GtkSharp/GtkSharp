namespace Gtk {

        using System;

        public static class StyleProviderPriority 
	{
		public const uint Fallback = 1;
		public const uint Theme = 200;
		public const uint Settings = 400;
		public const uint Application = 600;
		public const uint User = 800;
	}
}

namespace Gtk
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Runtime.CompilerServices;

	public partial class CssProvider
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		public bool LoadFromResource(string resource) => LoadFromResource(Assembly.GetCallingAssembly(), resource);

		[MethodImpl(MethodImplOptions.NoInlining)]
		public bool LoadFromResource(Assembly assembly, string resource)
		{
			if (assembly == null)
				assembly = Assembly.GetCallingAssembly();

			Stream stream = assembly.GetManifestResourceStream(resource);
			if (stream == null)
				throw new ArgumentException("'" + resource + "' is not a valid resource name of assembly '" + assembly + "'.", nameof(resource));

			using (var reader = new StreamReader(stream))
			{
				string data = reader.ReadToEnd();
				return LoadFromData(data);
			}
		}
	}
}

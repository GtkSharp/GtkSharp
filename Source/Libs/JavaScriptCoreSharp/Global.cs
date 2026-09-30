namespace JavaScriptCore
{

	public partial class Global
	{

		/// <summary>
		/// Whether the JavaScriptCore shared library could be loaded.
		/// </summary>
		/// <remarks>
		/// It ships separately from WebKit, and not at all in some
		/// distributions -- the gvsbuild bundle GtkSharp.targets installs on
		/// Windows contains neither. Callers that reach for javascript results
		/// should check this first, exactly as WebKit.Global.IsSupported exists
		/// for the same reason.
		/// </remarks>
		public static bool IsSupported => GLibrary.IsSupported(Library.JavaScriptCore);

	}

}

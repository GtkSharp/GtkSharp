namespace JSC
{

	// https://webkitgtk.org/reference/jsc-glib/2.32.2/

	public partial class Global
	{

		public static bool IsSupported => GLibrary.IsSupported(Library.JavaScriptCore);

	}

}
namespace WebKit
{

	// https://webkitgtk.org/reference/webkit2gtk/stable/index.html

	public partial class Global
	{

		public static bool IsSupported => GLibrary.IsSupported(Library.Webkit);

	}

}
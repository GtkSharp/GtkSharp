namespace JSC 
{

	public partial class Global
	{

		public static bool IsSupported => GLibrary.IsSupported(Library.JavaScriptCore);

	}

}
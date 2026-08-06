namespace GtkSharp.WebkitGtkSharp
{

	public partial class ObjectManager
	{

		// Call this method from the appropriate module init function.
		static partial void InitializeExtras()
		{

			// WebKitJavascriptResult was removed in WebKitGTK 6.0; script results
			// come back as a JSCValue.
			//
			// This used to register a hand-written JavaScript.Value against that
			// GType. JavaScriptCoreSharp now binds JSCValue properly and
			// registers JavaScriptCore.Value for it, so registering a second
			// managed type for the same GType only decided which of the two a
			// signal argument would arrive as -- and the signal marshaller then
			// handed back the wrong one.

		}

	}

}

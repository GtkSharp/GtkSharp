using JavaScript;

namespace GtkSharp.WebkitGtkSharp
{

	public partial class ObjectManager
	{

		// Call this method from the appropriate module init function.
		static partial void InitializeExtras()
		{

			// WebKitJavascriptResult was removed in WebKitGTK 6.0; script results
			// come back as a JSCValue now.

			GLib.GType.Register(Value.GType, typeof(Value));

		}

	}

}
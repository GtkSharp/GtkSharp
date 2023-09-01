using JavaScript;

namespace GtkSharp.WebkitGtkSharp
{

	public partial class ObjectManager
	{

		// Call this method from the appropriate module init function.
		static partial void InitializeExtras()
		{

			GLib.GType.Register(WebKit.JavascriptResult.GType, typeof(WebKit.JavascriptResult));

			GLib.GType.Register(Value.GType, typeof(Value));

		}

	}

}
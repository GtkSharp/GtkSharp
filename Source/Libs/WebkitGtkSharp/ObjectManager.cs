namespace GtkSharp.WebkitGtkSharp
{

	public partial class ObjectManager
	{

		// Call this method from the appropriate module init function.
		static partial void InitializeExtras()
		{

			// WebKit hands out JSCValues through signals -- script-message-received
			// carries one -- and a signal argument is resolved by looking its GType
			// up in the registry, not by the static type in the handler's signature.
			// Nothing in a WebKit-only program touches a JavaScriptCore type first,
			// so that assembly's ObjectManager would never have run and the lookup
			// would miss.
			//
			// The name-based fallback in GType.LookupType cannot cover for it
			// either: it splits a C name at the second capital, which turns
			// "JSCValue" into "J.SCValue". So the registration has to be explicit.

			global::GtkSharp.JavaScriptCoreSharp.ObjectManager.Initialize();

		}

	}

}

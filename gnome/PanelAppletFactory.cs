using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Gnome
{
	public class PanelAppletFactory
	{
		private PanelAppletFactory () {}

		private static string _IID;
		private static string _factoryIID;
		private static GnomeSharp.PanelAppletFactoryCallbackWrapper cb_wrapper;

		public static void Register (Type applet_type)
		{
			PanelApplet applet = (PanelApplet) Activator.CreateInstance (applet_type, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] {IntPtr.Zero}, null);

			cb_wrapper = new GnomeSharp.PanelAppletFactoryCallbackWrapper (new PanelAppletFactoryCallback (Creation), null);
			_IID = applet.IID;
			_factoryIID = applet.FactoryIID;
			panel_applet_factory_main(_factoryIID, GLib.Object.LookupGType (applet_type).Val, cb_wrapper.NativeDelegate, IntPtr.Zero);
		}

		private static bool Creation (PanelApplet applet, string iid)
		{
			if (_IID != iid)
				return false;
			applet.Creation ();
			return true;
		}

		[DllImport("panel-applet-2")]
		static extern int panel_applet_factory_main(string iid, IntPtr applet_type, GnomeSharp.PanelAppletFactoryCallbackNative cb, IntPtr data);
	}
}

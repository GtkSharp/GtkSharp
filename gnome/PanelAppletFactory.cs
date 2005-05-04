// Gnome.PanelAppletFactory.cs - PanelAppletFactory class impl
//
// Copyright (c) 2004-2005 Novell, Inc.
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.



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

			cb_wrapper = new GnomeSharp.PanelAppletFactoryCallbackWrapper (new PanelAppletFactoryCallback (Creation));
			_IID = applet.IID;
			_factoryIID = applet.FactoryIID;
			IntPtr native_iid = GLib.Marshaller.StringToPtrGStrdup (_factoryIID);
			panel_applet_factory_main (native_iid, ((GLib.GType) applet_type).Val, cb_wrapper.NativeDelegate, IntPtr.Zero);
			GLib.Marshaller.Free (native_iid);
		}

		private static bool Creation (PanelApplet applet, string iid)
		{
			if (_IID != iid)
				return false;
			applet.Creation ();
			return true;
		}

		[DllImport("panel-applet-2")]
		static extern int panel_applet_factory_main(IntPtr iid, IntPtr applet_type, GnomeSharp.PanelAppletFactoryCallbackNative cb, IntPtr data);
	}
}

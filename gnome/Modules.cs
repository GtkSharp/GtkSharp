namespace Gnome
{
	using System;
	using System.Runtime.InteropServices;

	public class Modules
	{
		[DllImport("gnome-2")]
		static extern System.IntPtr libgnome_module_info_get ();
		[DllImport("gnomeui-2")]
		static extern System.IntPtr libgnomeui_module_info_get ();

		public static ModuleInfo LibGnome {
			get { 
				return ModuleInfo.New (libgnome_module_info_get ());
			}
		}

		public static ModuleInfo UI {
			get { 
				return ModuleInfo.New (libgnomeui_module_info_get ());
			}
		}
	}
}


using GConf;
using GConf.PropertyEditors;
using Gtk;
using GtkSharp;
using Gnome;
using System;

namespace Sample
{
	public enum Names
	{
		Roy,
		George,
		Bob,
		Sally
	}

	public enum Direction
	{
		Up,
		Down,
		Left,
		Right
	}
	
	class X
	{
		void DeleteEvent (object obj, DeleteEventArgs args)
		{
			Application.Quit ();
		}

		void Close (object obj, EventArgs args)
		{
			Application.Quit ();
		}

		static void Changed (object obj, NotifyEventArgs args)
		{
			Console.WriteLine ("Something changed:");
			Console.WriteLine ("\tkey: {0}", args.Key);
			Console.WriteLine ("\tvalue: {0}", args.Value);
		}
		
		static void ColorChanged (object obj, NotifyEventArgs args)
		{
			Console.WriteLine ("The color changed!");
		}

		public static void Main (string[] argv)
		{
			Program app = new Program ("sampleapp", "0.0.1", Modules.UI, argv);

			Glade.XML gxml = new Glade.XML (null, "sample.glade", "preferences_dialog", null);
			gxml.Autoconnect (new X ());

			Settings.Changed += new NotifyEventHandler (Changed);
			Settings.TheColorChanged += new NotifyEventHandler (ColorChanged);

			EditorShell shell = new EditorShell (gxml);
			shell.Add (SettingKeys.Enable, "enable");
			shell.Add (SettingKeys.TheColor, "colorpicker");
			shell.Add (SettingKeys.TheFilename, "fileentry");
			shell.Add (SettingKeys.TheInteger, "spinbutton_int");
			shell.Add (SettingKeys.TheFloat, "spinbutton_float");
			shell.Add (SettingKeys.TheFirstEnum, "optionmenu", typeof (Names));
			shell.Add (SettingKeys.TheSecondEnum, "radiobutton", typeof (Direction));
			shell.Add (SettingKeys.TheText, "entry");

			shell.AddGuard (SettingKeys.Enable, "table1");
			
			app.Run ();
		}
	}
}

using Gnome.Vfs;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TestGnomeVfs {
	public class TestMime {
		static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: TestSync <uri>");
				return;
			}
		
			Gnome.Vfs.Vfs.Initialize ();
			
			Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (args[0]);

			MimeType mimetype = uri.MimeType;
			Console.WriteLine ("Uri `{0}' looks like ", uri, mimetype.Name);
			
			Gnome.Vfs.Vfs.Shutdown ();
		}
	}
}

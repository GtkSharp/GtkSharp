using Gnome.Vfs;
using System;

namespace Test.Gnome.Vfs {
	public class TestUnlink {
		static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: TestUnlink <uri>");
				return;
			}
		
			Gnome.Vfs.Vfs.Initialize ();

			Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (args[0]);
			Result result = uri.Unlink ();
			
			Console.WriteLine ("result unlink ('{0}') = {1}", uri, result);
			
			Gnome.Vfs.Vfs.Shutdown ();
		}
	}
}

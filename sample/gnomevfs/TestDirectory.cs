using Gnome.Vfs;
using System;
using System.Text;

namespace Test.Gnome.Vfs {
	public class TestInfo {
		static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: TestDirectory <uri>");
				return;
			}
		
			Gnome.Vfs.Vfs.Initialize ();

			FileInfo[] entries = Gnome.Vfs.Directory.GetEntries (args[0]);

			foreach (FileInfo info in entries)			
				Console.WriteLine (info.ToString ());
			
			Gnome.Vfs.Vfs.Shutdown ();
		}
	}
}

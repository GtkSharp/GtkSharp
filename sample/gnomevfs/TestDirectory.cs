using GLib;
using Gnome.Vfs;
using System;
using System.Text;

namespace TestGnomeVfs {
	public class TestInfo {
		private static MainLoop loop;
	
		static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: TestDirectory <uri>");
				return;
			}
		
			Gnome.Vfs.Vfs.Initialize ();

			FileInfo[] entries = Gnome.Vfs.Directory.GetEntries (args[0]);

			Console.WriteLine ("Directory {0} contains {1} entries:", args[0], entries.Length);
			foreach (FileInfo info in entries) {
				//Console.WriteLine (info.Name);
			}
			
			Gnome.Vfs.Directory.GetEntries (args[0], FileInfoOptions.Default,
							20, (int)Gnome.Vfs.Async.Priority.Default,
							new AsyncDirectoryLoadCallback (OnDirectoryLoad));
			
			loop = new MainLoop ();
			loop.Run ();
			
			Gnome.Vfs.Vfs.Shutdown ();
		}
		
		private static void OnDirectoryLoad (Result result, FileInfo[] entries, uint entries_read)
		{
			Console.WriteLine ("DirectoryLoad: {0}", result);
			if (result != Result.Ok && result != Result.ErrorEof) {
				loop.Quit ();
				return;
			}
			
			Console.WriteLine ("read {0} entries", entries_read);
			foreach (FileInfo info in entries) {
				//Console.WriteLine (info.Name);
			}
			
			if (result == Result.ErrorEof)
				loop.Quit ();
		}
	}
}

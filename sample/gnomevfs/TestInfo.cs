using Gnome.Vfs;
using System;
using System.Text;

namespace Test.Gnome.Vfs {
	public class TestInfo {
		static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: TestInfo <uri>");
				return;
			}
		
			Gnome.Vfs.Vfs.Initialize ();

			Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (args[0]);
			
			FileInfoOptions options = FileInfoOptions.GetMimeType |
						  FileInfoOptions.FollowLinks |
						  FileInfoOptions.GetAccessRights;
			FileInfo info = uri.GetFileInfo (options);
			Console.WriteLine (info.ToString ());
			
			Gnome.Vfs.Vfs.Shutdown ();
		}
	}
}

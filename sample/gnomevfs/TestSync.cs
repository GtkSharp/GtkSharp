using Gnome.Vfs;
using System;
using System.Text;

namespace TestGnomeVfs {
	public class TestSync {
		static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: TestSync <uri>");
				return;
			}
		
			Gnome.Vfs.Vfs.Initialize ();

			Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (args[0]);
			Handle handle = Sync.Open (uri, OpenMode.Read);

			UTF8Encoding utf8 = new UTF8Encoding ();
			byte[] buffer = new byte[1024];
			Result result = Result.Ok;			
			while (result == Result.Ok) {
				ulong bytesRead;
				result = Sync.Read (handle, out buffer[0],
						    (ulong)buffer.Length, out bytesRead);
				Console.WriteLine ("result read '{0}' = {1}", uri, result);
				if (bytesRead == 0)
					break;
				Console.WriteLine ("read ({0} bytes) : '{1}'",
						   bytesRead, utf8.GetString (buffer, 0, (int)bytesRead));
			}
			
			string test;
			result = Sync.FileControl (handle, "file:test", out test);
			Console.WriteLine ("result filecontrol '{0}' = {1}", uri, result);
			Console.WriteLine ("result file:test = {0}", test);
			
			result = Sync.Close (handle);
			Console.WriteLine ("result close '{0}' = {1}", uri, result);
			
			Gnome.Vfs.Vfs.Shutdown ();
		}
	}
}

using GLib;
using Gnome.Vfs;
using System;
using System.IO;
using System.Text;

namespace TestGnomeVfs {
	public class TestSyncStream {
		private static MainLoop loop;
	
		static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: TestSyncStream <uri>");
				return;
			}
		
			Gnome.Vfs.Vfs.Initialize ();

			VfsStream stream = new VfsStream (args[0], FileMode.Open);
			
			UTF8Encoding utf8 = new UTF8Encoding ();
			byte[] buffer = new byte[1024];
			int read;
			while ((read = stream.Read (buffer, 0, buffer.Length)) != 0) {
				Console.WriteLine ("read ({0} bytes) : '{1}'",
						   read, utf8.GetString (buffer, 0, read));
			}

			long offset = stream.Seek (0, SeekOrigin.Begin);
			Console.WriteLine ("Offset after seek is {0}", offset);
			
			Gnome.Vfs.Vfs.Shutdown ();
		}
	}
}

using GLib;
using Gnome.Vfs;
using System;
using System.IO;
using System.Text;

namespace TestGnomeVfs {
	public class TestAsyncStream {
		private static MainLoop loop;
		private static byte[] buffer;
	
		static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: TestAsyncStream <uri>");
				return;
			}
		
			Gnome.Vfs.Vfs.Initialize ();

			VfsStream stream = new VfsStream (args[0], FileMode.Open, true);
			
			UTF8Encoding utf8 = new UTF8Encoding ();
			buffer = new byte[1024];
			int read;
			while ((read = stream.Read (buffer, 0, buffer.Length)) != 0) {
				Console.WriteLine ("read ({0} bytes) : '{1}'",
						   read, utf8.GetString (buffer, 0, read));
			}

			long offset = stream.Seek (0, SeekOrigin.Begin);
			Console.WriteLine ("Offset after seek is {0}", offset);
			
			buffer = new byte[1024];
			IAsyncResult result = stream.BeginRead (buffer, 0, buffer.Length,
								new System.AsyncCallback (OnReadComplete),
								stream);

			loop = new MainLoop ();
			loop.Run ();

			Gnome.Vfs.Vfs.Shutdown ();
		}
		
		private static void OnReadComplete (IAsyncResult result)
		{
			VfsStreamAsyncResult asyncResult = result as VfsStreamAsyncResult;
			Console.WriteLine ("Read completed: {0}, {1}", asyncResult.IsCompleted, asyncResult.NBytes);
			UTF8Encoding utf8 = new UTF8Encoding ();
			Console.WriteLine (utf8.GetString (buffer, 0, buffer.Length));
			loop.Quit ();
		}
	}
}

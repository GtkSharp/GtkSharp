//
// TestVfs.cs: Test gnome-vfs bindings.
//
// Author:
//   Jeroen Zwartepoorte <jeroen@xs4all.nl>
//
// (C) Copyright Jeroen Zwartepoorte 2004
//

using System;
using System.Text;
using GLib;
using Gtk;
using Gnome.Vfs;

public class TestVfs {
	static void Main (string[] args)
	{
		Application.Init ();
	
		FileChooserDialog fcd = new FileChooserDialog ("Open File",
							       new Window ("test"),
							       FileChooserAction.Open,
							       "gnome-vfs");
		fcd.LocalOnly = false;
		fcd.AddButton (Stock.Cancel, ResponseType.Cancel);
		fcd.AddButton (Stock.Open, ResponseType.Ok);
		fcd.DefaultResponse = ResponseType.Ok;
		int resp = fcd.Run ();
		fcd.Hide ();
		
		if (resp != (int)ResponseType.Ok)
			return;

		Console.WriteLine ("Selected uri      = {0}", fcd.Uri);
		string mimetype = Mime.GetMimeType (fcd.Uri);
		Console.WriteLine ("Mimetype          = {0}", mimetype);

		FileInfoOptions options = FileInfoOptions.Default;
		Gnome.Vfs.FileInfo info = new Gnome.Vfs.FileInfo (fcd.Uri, options);
		Console.WriteLine ("isLocal = " + info.IsLocal);

#if false
		Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (fcd.Uri);
		Console.WriteLine (uri.ToString ());
		
		VfsStream vs = new VfsStream (fcd.Uri, FileMode.OpenOrCreate);
		UTF8Encoding utf8 = new UTF8Encoding ();
		byte[] buf = utf8.GetBytes ("Testing 1 2 3, asdjfaskjdhfkajshdf");
		vs.Write (buf, 0, buf.Length);
		/*for (int i = 0; i < 200; i++) {
			int c = vs.ReadByte ();
			if (c == -1) {
				Console.WriteLine ("EOF");
				break;
			}
			Console.Write ((char)c);
		}*/
		vs.Close ();
		
		//Handle handle = Vfs.Open (fcd.Uri, OpenMode.Read);
		
		//Vfs.OpenAsync (fcd.Uri, OpenMode.Read, 0, new Gnome.Vfs.AsyncCallback (open_cb));
		
		/*FilePermission perms = FilePermission.UserRead |
				       FilePermission.UserWrite |
				       FilePermission.GroupRead |
				       FilePermission.OtherRead;
		Vfs.CreateAsync ("file:///home/jeroen/test.txt", OpenMode.Write,
				 false, perms, 0, new Gnome.Vfs.AsyncCallback (create_cb));*/
#endif
		Application.Run ();
	}
	
	static void open_cb (Handle handle, Result result)
	{
		Console.WriteLine ("OpenAsync result = {0} ({1})", Vfs.ResultToString (result), result);

		byte[] buffer = new byte[128];
		Vfs.ReadAsync (handle, out buffer[0], 128, new AsyncReadCallback (read_cb));		
	}
	
	static void read_cb (Handle handle, Result result, byte[] buffer, ulong bytes_requested, ulong bytes_read)
	{
		Console.WriteLine ("ReadAsync result = {0} ({1})", Vfs.ResultToString (result), result);

		if (result == Result.Ok) {
			Console.WriteLine ("bytes_requested   = {0}", bytes_requested);
			Console.WriteLine ("bytes_read        = {0}", bytes_read);
			/*Console.WriteLine ("bytes             = ");
			for (int i = 0; i < (int)bytes_read; i++)
				Console.Write ("" + (char)buffer[i]);*/
			
			byte[] buf = new byte[128];
			Vfs.ReadAsync (handle, out buf[0], 128, new AsyncReadCallback (read_cb));
		}
	}
	
	static void create_cb (Handle handle, Result result)
	{
		Console.WriteLine ("CreateAsync result = {0} ({1})", Vfs.ResultToString (result), result);
		
		if (result == Result.Ok) {
			UTF8Encoding utf8 = new UTF8Encoding ();
			byte[] buffer = utf8.GetBytes ("Testing 1 2 3 asdlfjalsjdfksjdf \nGustavo Giráldez\n");
			Vfs.WriteAsync (handle, out buffer[0], (uint)buffer.Length, new AsyncWriteCallback (write_cb));
		}
	}
	
	static void write_cb (Handle handle, Result result, byte[] buffer, ulong bytes_requested, ulong bytes_written)
	{
		Console.WriteLine ("WriteAsync result = {0} ({1})", Vfs.ResultToString (result), result);
	}
}

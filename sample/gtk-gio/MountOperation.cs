/*
 * MountOperation.cs: code sample using Gtk.MountOperation
 *
 * Author(s):
 *	Stephane Delcroix  (stephane@delcroix.org)
 *
 * Copyright (c) 2009 Novell, Inc.
 *
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using Gtk;
using GLib;

public class TestMount
{

	static GLib.File file;
	static Gtk.MountOperation operation;

	static void Main ()
	{
		Gtk.Application.Init ();
		file = FileFactory.NewForUri (new Uri ("smb://admin@192.168.42.3/myshare/test"));

		Window w = new Window ("test");
		operation = new Gtk.MountOperation (w);
		Button b = new Button ("Mount");
		b.Clicked += new EventHandler (HandleButtonClicked);
		b.Show ();
		w.Add (b);
		w.Show ();
		Gtk.Application.Run ();
	}

	static void HandleButtonClicked (object sender, EventArgs args)
	{
		System.Console.WriteLine ("clicked");
		file.MountEnclosingVolume (0, operation, null, new GLib.AsyncReadyCallback (HandleMountFinished));
	}

	static void HandleMountFinished (GLib.Object sender, GLib.AsyncResult result)
	{
		System.Console.WriteLine ("handle mount finished");
		if (file.MountEnclosingVolumeFinish (result))
			System.Console.WriteLine ("successfull");
		else
			System.Console.WriteLine ("Failed");
	}
}

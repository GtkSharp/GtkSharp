// Redirector.cs - launches a program and sends its output to a file.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2009 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


namespace GtkSharpInstaller {

	using System;
	using System.IO;
	using System.Diagnostics;

	public class Redirector  {

		public static int Main (string[] args)
		{
			if (args.Length != 2) {
				Console.Error.WriteLine ("Usage: redirector.exe <program> <file>");
				return 1;
			}

			var outfile = new FileInfo (args [1]);
			outfile.Directory.Create ();

			ProcessStartInfo info = new ProcessStartInfo (args [0]);
			info.RedirectStandardOutput = true;
			info.UseShellExecute = false;
			Process proc = Process.Start (info);
			StreamWriter sw = new StreamWriter (outfile.Create ());
			sw.WriteLine (proc.StandardOutput.ReadToEnd ());
			sw.Close ();
			return 0;
		}
	}
}


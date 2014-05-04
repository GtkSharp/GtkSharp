// Copyright (c) 2010 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class BundleScanner  {

	public static int Main (string[] args)
	{
		Dictionary<string, bool> known_files = null;
		string bundle_path = null;

		foreach (string arg in args) {

			if (arg.StartsWith("--wix=")) {
				string wix_filename = arg.Substring (6);
				try {
					XmlDocument wix_doc = new XmlDocument ();
					wix_doc.Load (wix_filename);
					known_files = GetSourcesFromDoc (wix_doc);
				} catch (XmlException e) {
					Console.WriteLine ("Invalid wix file.");
					Console.WriteLine (e);
					return 1;
				}
			} else if (arg.StartsWith ("--bundle=")) {
				bundle_path = arg.Substring (9);
				if (!Directory.Exists (bundle_path)) {
					Console.WriteLine ("Invalid bundle directory.");
					return 1;
				}
			} else {
				Console.WriteLine ("Usage: bundle-scanner --wix=<filename> --bundle=<dir>");
				return 1;
			}
		}

		if (bundle_path == null || known_files == null) {
			Console.WriteLine ("Usage: bundle-scanner --wix=<filename> --bundle=<dir>");
			return 1;
		}

		Dictionary<string, bool> ignores = new Dictionary<string, bool> ();
		if (File.Exists ("ignores")) {
			using (StreamReader rdr = new StreamReader ("ignores")) {
				while (rdr.Peek () >= 0)
					ignores [rdr.ReadLine ()] = true;
			}
		}

		BundleScanner scanner = new BundleScanner (bundle_path, known_files, ignores);
		scanner.Scan ();

		List<string> missing = scanner.ExpectedFiles;
		if (missing.Count > 0) {
			Console.WriteLine ();
			Console.WriteLine ("Expected files missing in bundle:");
			Console.WriteLine ("---------------------------");
			foreach (string file in missing)
				Console.WriteLine ("   " + file);
		}

		List<string> unexpected = scanner.UnexpectedFiles;
		if (unexpected.Count > 0) {
			Console.WriteLine ();
			Console.WriteLine ("Unexpected files in bundle:");
			Console.WriteLine ("---------------------------");
			foreach (string file in unexpected)
				Console.WriteLine ("   " + file);
		}

		return 0;
	}

	static Dictionary<string, bool> GetSourcesFromDoc (XmlDocument doc)
	{
		Dictionary<string, bool> result = new Dictionary<string, bool> ();
		foreach (XmlNode node in doc.DocumentElement.ChildNodes)
			FindFileNodes (node as XmlElement, result);
		return result;
	}

	static void FindFileNodes (XmlElement elem, Dictionary<string, bool> sources)
	{
		if (elem == null)
			return;
		if (elem.Name == "File") {
			string source = elem.GetAttribute ("Source");
			if (!source.StartsWith ("custom\\"))
				sources [source] = true;
		} else {
			foreach (XmlNode node in elem.ChildNodes)
				FindFileNodes (node as XmlElement, sources);
		}
	}

	Dictionary<string, bool> ignores = null;
	Dictionary<string, bool> known_files = null;
	DirectoryInfo bundle_dir = null;
	List<string> unexpected_files = new List<string> ();
	int relative_prefix_length;

	BundleScanner (string path, Dictionary<string, bool> known_files, Dictionary<string, bool> ignores)
	{
		bundle_dir = new DirectoryInfo (path);
		this.known_files = known_files;
		this.ignores = ignores;
		relative_prefix_length = bundle_dir.FullName.Length - bundle_dir.Name.Length;
	}

	public List<string> ExpectedFiles {
		get {
			List<string> result = new List<string> ();
			foreach (string s in known_files.Keys)
				result.Add (s);
			result.Sort ();
			return result;
		}
	}

	public List<string> UnexpectedFiles {
		get {
			unexpected_files.Sort ();
			return unexpected_files;
		}
	}

	public void Scan ()
	{
		Scan (bundle_dir);
	}

	string GetRelativeFileName (string filename)
	{
		return filename.Substring (relative_prefix_length);
	}

	void Scan (DirectoryInfo dir)
	{
		foreach (FileInfo file in dir.GetFiles ()) {
			string relative = GetRelativeFileName (file.FullName);
			relative = relative.Replace ('/', '\\');
			if (ignores.ContainsKey (relative))
				continue;
			if (known_files.ContainsKey (relative))
				known_files.Remove (relative);
			else
				unexpected_files.Add (relative);
		}

		foreach (DirectoryInfo sub in dir.GetDirectories ()) {
			string relative = GetRelativeFileName (sub.FullName);
			relative = relative.Replace ('/', '\\');
			if (ignores.ContainsKey (relative))
				continue;
			Scan (sub);
		}
	}

}

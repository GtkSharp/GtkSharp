using Gnome.Vfs;
using Mono.GetOptions;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("TestXfer")]
[assembly: AssemblyDescription("Test case for the Gnome.Vfs.Xfer class")]
[assembly: AssemblyCopyright("(C) 2004 Jeroen Zwartepoorte")]
[assembly: Mono.About("Distributed under the GPL")]
[assembly: Mono.UsageComplement("<src> <target>")]
[assembly: Mono.Author("Jeroen Zwartepoorte")]
[assembly: AssemblyVersion("1.0.*")]

namespace Test.Gnome.Vfs {
	class TestXferOptions : Options {
		[Option("Copy directories recursively", 'r')]
		public bool recursive = false;
		
		[Option("Follow symlinks", 'L', "follow-symlinks")]
		public bool followSymlinks = false;
		
		[Option("Follow symlinks recursively", 'Z', "recursive-symlinks")]
		public bool recursiveSymlinks = false;

		[Option("Replace files automatically", 'R')]
		public bool replace = false;

		[Option("Delete source files", 'd', "delete-source")]
		public bool deleteSource = false;
		
		public TestXferOptions ()
		{
			ParsingMode = OptionsParsingMode.Both;
		}

		public override WhatToDoNext DoHelp ()
		{
			base.DoHelp();
			return WhatToDoNext.AbandonProgram; 
		}

		[Option("Show usage syntax", 'u', "usage")]
		public override WhatToDoNext DoUsage()
		{
			base.DoUsage();
			return WhatToDoNext.AbandonProgram; 
		}
	}

	public class TestXfer {
		static void Main (string[] args)
		{
			TestXferOptions opt = new TestXferOptions ();
			opt.ProcessArgs (args);
			
			if (opt.RemainingArguments.Length < 2) {
				opt.DoUsage ();
				return;
			}
			
			XferOptions options = XferOptions.Default;
			XferOverwriteMode overwriteMode = XferOverwriteMode.Query;
			if (opt.recursive) {
				Console.WriteLine ("Warning: Recursive xfer of directories.");
				options |= XferOptions.Recursive;
			}
			if (opt.followSymlinks) {
				Console.WriteLine ("Warning: Following symlinks.");
				options |= XferOptions.FollowLinks;
			}
			if (opt.recursiveSymlinks) {
				Console.WriteLine ("Warning: Following symlinks recursively.");
				options |= XferOptions.FollowLinksRecursive;
			}
			if (opt.replace) {
				Console.WriteLine ("Warning: Using replace overwrite mode.");
				overwriteMode = XferOverwriteMode.Replace;
			}
			if (opt.deleteSource) {
				Console.WriteLine ("Warning: Removing source files.");
				options |= XferOptions.Removesource;
			}
			
			Gnome.Vfs.Vfs.Initialize ();
			
			Gnome.Vfs.Uri source = new Gnome.Vfs.Uri (opt.RemainingArguments[0]);
			Console.WriteLine ("Source: `{0}'", source);
			Gnome.Vfs.Uri target = new Gnome.Vfs.Uri (opt.RemainingArguments[1]);
			Console.WriteLine ("Target: `{0}'", target);

			Result result = Xfer.XferUri (source, target, options,
						      XferErrorMode.Query,
						      overwriteMode,
						      new XferProgressCallback (OnProgress));
			Console.WriteLine ("Result: {0}", Gnome.Vfs.Vfs.ResultToString (result));
			
			Gnome.Vfs.Vfs.Shutdown ();
		}
		
		public static int OnProgress (XferProgressInfo info)
		{
			switch (info.Status) {
			case XferProgressStatus.Vfserror:
				Console.WriteLine ("Vfs error: {0}",
					Gnome.Vfs.Vfs.ResultToString (info.VfsStatus));
				break;
			case XferProgressStatus.Overwrite:
				Console.WriteLine ("Overwriting `{0}' with `{1}'",
						   info.TargetName, info.SourceName);
				break;
			case XferProgressStatus.Ok:
				Console.WriteLine ("Status: Ok");
				switch (info.Phase) {
				case XferPhase.PhaseInitial:
					Console.WriteLine ("Initial phase");
					return 1;
				case XferPhase.PhaseCollecting:
					Console.WriteLine ("Collecting file list");
					return 1;
				case XferPhase.PhaseReadytogo:
					Console.WriteLine ("Ready to go!");
					return 1;
				case XferPhase.PhaseOpensource:
					Console.WriteLine ("Opening source");
					return 1;
				case XferPhase.PhaseOpentarget:
					Console.WriteLine ("Opening target");
					return 1;
				case XferPhase.PhaseCopying:
					Console.WriteLine ("Transferring `{0}' to `{1}' " + 
							   "(file {2}/{3}, byte {4}/{5} in file, " +
							   "{6}/{7} total", info.SourceName,
							   info.TargetName, info.FileIndex,
							   info.FilesTotal, (long)info.BytesCopied,
							   (long)info.FileSize, info.TotalBytesCopied,
							   info.BytesTotal);
					break;
				case XferPhase.PhaseClosesource:
					Console.WriteLine ("Closing source");
					return 1;
				case XferPhase.PhaseClosetarget:
					Console.WriteLine ("Closing target");
					return 1;
				case XferPhase.PhaseFilecompleted:
					Console.WriteLine ("Done with `{0}' -> `{1}', going next",
							   info.SourceName, info.TargetName);
					return 1;
				case XferPhase.PhaseCompleted:
					Console.WriteLine ("All done.");
					return 1;
				default:
					Console.WriteLine ("Unexpected phase: {0}", info.Phase);
					return 1; // Keep going anyway.
				}
				break;
			case XferProgressStatus.Duplicate:
				break;
			}

			return 0;
		}
	}
}

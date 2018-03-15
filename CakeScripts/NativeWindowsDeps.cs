using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using P = System.IO.Path;
using F = System.IO.File;
using D = System.IO.Directory;

// "Compile" native libraries for Gtk... we're spaniards after all!!!
// see "DEFCON 20: Javascript Botnets" for the joke reference
public class NativeWindowsDeps
{
    private const string Dumpbin = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Tools\MSVC\14.13.26128\bin\Hostx64\x86\dumpbin.exe";

    private static string _libdir, _rid;
    private static List<string> _libraries;

    public static void Fetch32Bit()
    {
        _libdir = @"C:\msys64\mingw32\bin";
        _rid = "windows-x86";

        Fetch();
    }

    public static void Fetch64Bit()
    {
        _libdir = @"C:\msys64\mingw64\bin";
        _rid = "windows-x64";

        Fetch();
    }

    public static void Fetch()
    {
        var passed = new List<string>();
        _libraries = new List<string>();

        int i = 0;
        while (passed.Count != Settings.AssemblyList.Count)
        {
            var a = Settings.AssemblyList[i];
            var pass = true;

            foreach (var dep in a.Deps)
                pass &= passed.Contains(dep);

            if (pass && !passed.Contains(a.Name))
            {
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine(a.Name);
                foreach (var ndep in a.NativeDeps)
                    ProcessDir(a.Name, "", ndep);
                passed.Add(a.Name);
            }

            i = (i + 1) % Settings.AssemblyList.Count;
        }
    }

    private static void ProcessDir(string aname, string spacing, string lib)
    {
        if (_libraries.Contains(lib))
            return;

        _libraries.Add(lib);

        var libpath = P.Combine(_libdir, lib);

        if (F.Exists(libpath))
        {
            D.CreateDirectory(P.Combine(@"Source/Libs", aname, _rid));
            F.Copy(libpath, P.Combine(@"Source/Libs", aname, _rid, lib), true);
            
            Console.WriteLine(spacing + " - " + lib);

            var proc = new Process();
            proc.StartInfo.FileName = Dumpbin;
            proc.StartInfo.Arguments = "/Imports \"" + libpath + "\"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                var line = proc.StandardOutput.ReadLine();
                
                if (line.Length > 5 && line.StartsWith("    ") && line[5] != ' ')
                {
                    var nextlib = line.Trim();

                    if (!_libraries.Contains(nextlib))
                        ProcessDir(aname, spacing + "  ", nextlib);
                }
            }

        }
    }
}
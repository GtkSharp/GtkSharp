using System;
using P = System.IO.Path;

public class GAssembly
{
    private ICakeContext Cake;

    public bool Init { get; private set; }
    public string Name { get; private set; }
    public string Dir { get; private set; }
    public string GDir { get; private set; }
    public string Csproj { get; private set; }
    public string RawApi { get; private set; }
    public string Metadata { get; private set; }

    public string[] Deps { get; set; }
    public string[] NativeDeps { get; set; }
    public string ExtraArgs { get; set; }

    public GAssembly(string name)
    {
        Cake = Settings.Cake;
        Deps = new string[0];

        Name = name;
        Dir = P.Combine("Source", "Libs", name);
        GDir = P.Combine(Dir, "Generated");

        var temppath = P.Combine(Dir, name);
        Csproj = temppath + ".csproj";
        RawApi = temppath + "-api.raw";
        Metadata = temppath + ".metadata";
    }

    public void Prepare()
    {
        Cake.CreateDirectory(GDir);

        // Raw API file found, time to generate some stuff!!!
        if (Cake.FileExists(RawApi))
        {
            // Fixup API file
            var tempapi = P.Combine(GDir, Name + "-api.xml");
            var symfile = P.Combine(Dir, Name + "-symbols.xml");
            Cake.CopyFile(RawApi, tempapi);
            Cake.DotNetCoreExecute("BuildOutput/Tools/GapiFixup.dll", 
                "--metadata=" + Metadata + " " + "--api=" + tempapi + 
                (Cake.FileExists(symfile) ? " --symbols=" + symfile : string.Empty)
            );

            var extraargs = ExtraArgs + " ";

            // Locate APIs to include
            foreach(var dep in Deps)
            {
                var ipath = P.Combine("Source", "Libs", dep, dep + "-api.xml");

                if (!Cake.FileExists(ipath))
                    ipath = P.Combine("Source", "Libs", dep, "Generated", dep + "-api.xml");

                if (Cake.FileExists(ipath))
                    extraargs += " --include=" + ipath + " ";
            }

            // Generate code
            Cake.DotNetCoreExecute("BuildOutput/Tools/GapiCodegen.dll", 
                "--outdir=" + GDir + " " +
                "--schema=Source/Libs/Gapi.xsd " +
                extraargs + " " +
                "--assembly-name=" + Name + " " +
                "--generate=" + tempapi
            );
        }

        Init = true;
    }

    public void Clean()
    {
        if (Cake.DirectoryExists(GDir))
            Cake.DeleteDirectory(GDir, new DeleteDirectorySettings { Recursive = true, Force = true });
    }

    public void GenerateLinuxStubs()
    {
        for (int i = 0; i < NativeDeps.Length; i += 2)
        {
            Cake.CreateDirectory(P.Combine(Dir, "linux-x64"));
            Cake.CreateDirectory(P.Combine(Dir, "linux-x86"));

            // Generate x64 stub
            Cake.StartProcess("gcc", "-shared -o BuildOutput/LinuxStubs/" + NativeDeps[i] + " BuildOutput/LinuxStubs/empty.c");
            Cake.StartProcess("gcc", "-Wl,--no-as-needed -shared -o " + P.Combine(Dir, "linux-x64", NativeDeps[i + 1] + ".so") + " -fPIC -L. -l:BuildOutput/LinuxStubs/" + NativeDeps[i] + "");

            // GEnerate x86 stub
            Cake.StartProcess("gcc", "-m32 -shared -o BuildOutput/LinuxStubs/" + NativeDeps[i] + " BuildOutput/LinuxStubs/empty.c");
            Cake.StartProcess("gcc", "-m32 -Wl,--no-as-needed -shared -o " + P.Combine(Dir, "linux-x86", NativeDeps[i + 1] + ".so") + " -fPIC -L. -l:BuildOutput/LinuxStubs/" + NativeDeps[i] + "");
        }
    }
}

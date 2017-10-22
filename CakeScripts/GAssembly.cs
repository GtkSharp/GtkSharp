using System;
using P = System.IO.Path;

public class GAssembly
{
    public static ICakeContext Cake;

    public bool Init { get; private set; }
    public string Name { get; private set; }
    public string Dir { get; private set; }
    public string GDir { get; private set; }
    public string Csproj { get; private set; }
    public string RawApi { get; private set; }
    public string Metadata { get; private set; }

    public string[] Includes { get; set; }
    public string ExtraArgs { get; set; }

    public GAssembly(string name)
    {
        Includes = new string[0];

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
        // Raw API file found, time to generate some stuff!!!
        if (Cake.FileExists(RawApi))
        {
            Cake.DeleteDirectory(GDir, true);
            Cake.CreateDirectory(GDir);

            // Fixup API file
            var tempapi = P.Combine(GDir, Name + "-api.xml");
            var symfile = P.Combine(Dir, Name + "-symbols.xml");
            Cake.CopyFile(RawApi, tempapi);
            GapiFixup.Run(tempapi, Metadata, Cake.FileExists(symfile) ? symfile : string.Empty);

            // Locate APIs to include
            foreach(var dep in Includes)
            {
                var ipath = P.Combine("Source", "Libs", dep, dep + "-api.xml");

                if (!Cake.FileExists(ipath))
                    ipath = P.Combine("Source", "Libs", dep, "Generated", dep + "-api.xml");

                if (Cake.FileExists(ipath))
                {
                    ExtraArgs += "--include=" + ipath + " ";
                    ExtraArgs += "--include=" + ipath + " ";
                }
            }

            // Generate code
            GAssembly.Cake.DotNetCoreExecute(P.Combine("BuildOutput", "Generator", "GapiCodegen.dll"), 
                "--outdir=" + GDir + " " +
                "--schema=" + P.Combine("Source", "Libs", "Gapi.xsd") + " " +
                ExtraArgs + " " +
                "--assembly-name=" + Name + " " +
                "--generate=" + tempapi
            );
        }

        Init = true;
    }

    public void Clean()
    {
        Cake.DeleteDirectory(GDir, true);
    }
}

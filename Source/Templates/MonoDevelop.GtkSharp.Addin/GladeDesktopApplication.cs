using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using MonoDevelop.Ide.Desktop;

namespace MonoDevelop.GtkSharp.Addin
{
    public class GladeDesktopApplication : DesktopApplication
    {
        private static readonly string s_unixgladeapp;

        static GladeDesktopApplication()
        {
            try
            {
                var assembly = typeof(GladeDesktopApplication).Assembly.Location;
                var gladesh = Path.Combine(Path.GetDirectoryName(assembly), "glade.sh");

                s_unixgladeapp = "-c '" + File.ReadAllText(gladesh) + "'";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private readonly string _filename;

        public GladeDesktopApplication(string filename) : base("GladeApp", "Glade", true)
        {
            _filename = filename;
        }

        public override void Launch(params string[] files)
        {
            try
            {
                var process = new Process();

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    var location = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Uninstall\GNOME Foundation Glade Interface Designer", "InstallLocation", "");
                    if (location != null)
                    {
                        process.StartInfo.FileName = Path.Combine(location.ToString(), "bin", "glade.exe");
                        process.StartInfo.Arguments = _filename;
                    }
                }
                else
                {
                    process.StartInfo.FileName = "bash";
                    process.StartInfo.Arguments = s_unixgladeapp.Replace("$@", _filename);
                }

                if (!string.IsNullOrEmpty(process.StartInfo.FileName))
                    process.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

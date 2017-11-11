using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DBus;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Desktop;

namespace MonoDevelop.GtkSharp.Addin
{
    public class GladeDesktopApplication : DesktopApplication
    {
        private readonly string _filename;

        public GladeDesktopApplication(string filename) : base("GladeApp", "Glade", true)
        {
            _filename = filename;
        }

        public override void Launch(params string[] files)
        {
            try
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    var bus = Bus.Session.GetObject<IFlatpak>("org.freedesktop.Flatpak", new ObjectPath("/org/freedesktop/Flatpak/Development"));

                    if (bus != null)
                    {
                        var pid = bus.HostCommand(
                            new byte[0],
                            new byte[][] {
                                Encoding.ASCII.GetBytes ("xdg-open\0"),
                                Encoding.ASCII.GetBytes (_filename + "\0")
                            },
                            new Dictionary<UInt32, UnixFD> { },
                            new Dictionary<string, string> { },
                            0
                        );

                        return;
                    }
                }

                Process.Start(_filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

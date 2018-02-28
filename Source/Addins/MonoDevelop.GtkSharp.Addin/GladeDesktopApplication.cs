using System.Diagnostics;
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
            Process.Start("glade", _filename);
        }
    }
}
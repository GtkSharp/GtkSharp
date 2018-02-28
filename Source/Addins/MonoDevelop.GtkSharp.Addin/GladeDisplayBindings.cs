using MonoDevelop.Core;
using MonoDevelop.Ide.Desktop;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

namespace MonoDevelop.GtkSharp.Addin
{
    public class GladeDisplayBinding : IExternalDisplayBinding
    {
        public bool CanUseAsDefault => true;

        public bool CanHandle(FilePath fileName, string mimeType, Project ownerProject)
        {
            return fileName.Extension == ".glade";
        }

        public DesktopApplication GetApplication(FilePath fileName, string mimeType, Project ownerProject)
        {
            return new GladeDesktopApplication(fileName.FullPath);
        }
    }
}
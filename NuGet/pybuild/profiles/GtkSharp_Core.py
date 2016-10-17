#!/usr/bin/python3
import os, shutil
from pybuild.ProfileBase import ProfileBase
from pybuild.vsgenext.CoreVSProject import CoreVSProject
from os.path import abspath, join
from pybuild.Helper import Helper
from glob import glob
import vsgen

# Note at this stage we can't complile GtkSharp using the .Net Core platform libraries, such as netstandard1.6
# https://docs.microsoft.com/en-us/dotnet/articles/standard/library
# This is due to some small api changes in the platform that the Gtksharp code would need to be adjusted to

# We can however use the newer dotnet build system specifying the net461 platform 
# This is the same set of platform libs under the surface (using mono) but a step in the right direction
# with modernising the build system. One advantage to this newer build system is that we don't need to list all the .cs files
# Within the project files (see generated .xproj file and project.json)

# TODO look into package for symbols, via NuGet -symbols

class GtkSharp_Core(ProfileBase):

    def __init__(self):
        """Class Init"""
        super().__init__()
        self._NuGet_PackageName = 'GtkSharp.Core'
        self._Version = Helper.get_gtksharp_version(self.SrcDir)
        self.Solution = None
        self.BuildConfig = 'Release'

    @property
    def Build_CoreDir(self):
        return abspath(join(self._BuildDir, 'core'))

    @property
    def Dotnet_BuildExe(self):
        return 'dotnet.exe'


    def Copy_CS_Files(self, csfiles):
        srclist = glob(join(self.SrcDir, csfiles[0]))
        destdir = join(self.Build_CoreDir, csfiles[1])
        os.makedirs(destdir, exist_ok=True)
        for fname in srclist:
            shutil.copy(fname, destdir)

    def SetupProject(self, projname):
        proj = CoreVSProject()
        proj.Name = projname
        proj.RootNamespace=projname
        proj.FileName = join(self.Build_CoreDir, projname, projname + '.xproj')
        proj.Frameworks = {'net461': {}}
        proj.Depends = {}
        proj.BuildOptions = { "allowUnsafe": True , "outputName": projname + "-sharp"}
        proj.Version = self._Version
        self.Solution.Projects.append(proj)
        self.Solution.write()
        return proj

    def Build_Project(self, proj):
        projdir = join(self.Build_CoreDir, proj.Name)
        Helper.run_cmd([self.Dotnet_BuildExe, 'restore'], projdir)
        Helper.run_cmd([self.Dotnet_BuildExe, 'build',
                        '--configuration', self.BuildConfig,
                        '--framework', 'net461',
                        '--output', join(self.Build_CoreDir, 'build')]
                        , projdir)


    def build(self):
        """Build the gtksharp binaries for .Net 4.5"""
        os.makedirs(self.Build_CoreDir, exist_ok=True)
        self.Solution = vsgen.solution.VSGSolution()
        self.Solution.FileName = join(self.Build_CoreDir, 'GtkSharp.sln')

        # Build Glib
        self.Copy_CS_Files(['glib/*.cs', 'glib/'])
        proj = self.SetupProject('glib')
        proj.write()
        self.Build_Project(proj)

        # Build Gio
        self.Copy_CS_Files(['gio/*.cs', 'gio/'])
        self.Copy_CS_Files(['gio/generated/GLib/*.cs', 'gio/generated/GLib/'])
        proj = self.SetupProject('gio')
        proj.Depends = {'glib': self._Version}
        proj.write()
        self.Build_Project(proj)

        # Build Cairo
        self.Copy_CS_Files(['cairo/*.cs', 'cairo/'])
        proj = self.SetupProject('cairo')
        proj.write()
        self.Build_Project(proj)

        # Build Pango
        self.Copy_CS_Files(['pango/*.cs', 'pango/'])
        self.Copy_CS_Files(['pango/generated/Pango/*.cs', 'pango/generated/Pango/'])
        proj = self.SetupProject('pango')
        proj.Depends = {'glib': self._Version,
                        'cairo': self._Version}
        proj.write()
        self.Build_Project(proj)

        # Build Atk
        self.Copy_CS_Files(['atk/*.cs', 'atk/'])
        self.Copy_CS_Files(['atk/generated/Atk/*.cs', 'atk/generated/Atk/'])
        proj = self.SetupProject('atk')
        proj.Depends = {'glib': self._Version}
        proj.write()
        self.Build_Project(proj)

        # Build Gdk
        self.Copy_CS_Files(['gdk/*.cs', 'gdk/'])
        self.Copy_CS_Files(['gdk/generated/Gdk/*.cs', 'gdk/generated/Gdk/'])
        self.Copy_CS_Files(['gdk/generated/GLib/*.cs', 'gdk/generated/GLib/'])
        proj = self.SetupProject('gdk')
        proj.Depends = {'atk': self._Version,
                        'cairo': self._Version,
                        'gio': self._Version,
                        'glib': self._Version,
                        'pango': self._Version}
        proj.write()
        self.Build_Project(proj)

        # Build Gtk
        self.Copy_CS_Files(['gtk/*.cs', 'gtk/'])
        self.Copy_CS_Files(['gtk/generated/GLib/*.cs', 'gtk/generated/GLib/'])
        self.Copy_CS_Files(['gtk/generated/Gtk/*.cs', 'gtk/generated/Gtk/'])
        proj = self.SetupProject('gtk')
        proj.Depends = {'gdk': self._Version,
                        'glib': self._Version}
        proj.write()
        self.Build_Project(proj)


    def copy_dll(self):
        """Copy the .Net 4.5 dll's to the build dir"""

        net45_build_dir = join(self.Build_NugetDir, 'build', 'net45')
        net45_lib_dir = join(self.Build_NugetDir, 'lib', 'net45')

        os.makedirs(net45_build_dir, exist_ok=True)
        os.makedirs(net45_lib_dir, exist_ok=True)
        shutil.copy('./misc/GtkSharp.targets', net45_build_dir)

        srclist = glob(join(self.Build_CoreDir, 'build', '*.dll'))
        for item in srclist:
            shutil.copy(item, net45_lib_dir)

        # Get the Config files
        dll_list = ['atk', 'cairo', 'gdk', 'gio', 'glib', 'gtk', 'pango']
        for item in dll_list:
            if item != 'cairo':
                shutil.copy(join(self.SrcDir, item, item + '-sharp.dll.config'), net45_build_dir)
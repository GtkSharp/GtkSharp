#! python3
import os, shutil
from pybuild.Settings import Settings
from pybuild.Helper import Helper as helpers
from os.path import join
from xml.etree import ElementTree as et

# This script assumes the gtk libraries have already been installed via MSYS2 / MinGW32 / MinGW64

class Gtk_Builder(object):

    def __init__(self):
        """Class Init"""
        self.setts = Settings()

    def clean(self):
        """Clean the build dir"""
        helpers.emptydir('./build')
        print ("Clean finished")

    def build_nuget_win32(self):
        self.build_nuget('Win32')

    def build_nuget_win64(self):
        self.build_nuget('Win64')

    def build_nuget(self, arch):
        """Package up a nuget file based on the default build"""

        if os.name != 'nt':
           print("Skipping Native Nuget package build, as this needs to be run on Windows")
           return

        self.clean()
        net45_build_dir = join(self.setts.BuildDir, 'build', 'net45')
        if arch == 'Win32':
            mingwdir = self.setts.mingwin32path
        else:
            mingwdir = self.setts.mingwin64path

        os.makedirs(self.setts.BuildDir, exist_ok=True)
        os.makedirs(net45_build_dir, exist_ok=True)

        print ('Copying Files')
        shutil.copy('./misc/GtkSharp.nuspec', self.setts.BuildDir)
        shutil.copy('./misc/GtkSharp.Native.targets', join(net45_build_dir, 'GtkSharp.' + arch + '.targets'))

        # Copy dlls
        dll_list = []
        dll_list += self.setts.get_native_win_dlls()
        dll_list += self.setts.get_native_win_deps()

        for item in dll_list:
            src = join(mingwdir, item)
            helpers.copy_files(src, net45_build_dir)

        # Get version
        GtkVersion = helpers.get_gtk_version(self.setts.msys2path)

        # Edit the XML version / package name in the .nuspec file
        nuspecfile = join(self.setts.BuildDir, 'GtkSharp.nuspec')
        et.register_namespace('', 'http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd')
        tree = et.parse(nuspecfile)
        xmlns = {'nuspec': '{http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd}'}
        tree.find('.//{nuspec}version'.format(**xmlns)).text = GtkVersion
        tree.find('.//{nuspec}id'.format(**xmlns)).text = self.setts.Gtksharp_PackageName + '.' + arch
        tree.write(nuspecfile)

        # Run Nuget
        helpers.run_cmd([self.setts.NuGetExe, 'pack', 'GtkSharp.nuspec'], self.setts.BuildDir)

        nugetfile = join(self.setts.BuildDir, self.setts.Gtksharp_PackageName + '.' + arch + '.' + GtkVersion + '.nupkg')
        os.makedirs(self.setts.NugetPkgs, exist_ok=True)
        shutil.copy(nugetfile, self.setts.NugetPkgs)

        print ('Generation of Nuget package complete')


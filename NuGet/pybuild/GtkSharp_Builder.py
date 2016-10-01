#! python3
import os, shutil
from pybuild.Settings import Settings
from pybuild.Helper import Helper as helpers
from os.path import join
from xml.etree import ElementTree as et

class GtkSharp_Builder(object):

    def __init__(self):
        """Class Init"""
        self.setts = Settings()

    def clean(self):
        """Clean the build dir"""
        helpers.emptydir('./build')
        print ("Clean finished")

    def build_net45(self):
        """Build the gtksharp binaries for .Net 4.5"""
        self.clean()
        os.makedirs(self.setts.BuildDir, exist_ok=True)
        buildfile = join(self.setts.BuildDir, 'net45_build.sh')

        # run script via MSYS for windows, or shell for linux
        if os.name == 'nt':
            print("Building .Net GtkSharp using MSYS2")
            with open(buildfile, 'w') as f:
                f.write('PATH=$PATH:/c/Program\ Files\ \(x86\)/Microsoft\ SDKs/Windows/v10.0A/bin/NETFX\ 4.6\ Tools/\n')
                f.write('PATH=$PATH:/c/Windows/Microsoft.NET/Framework/v4.0.30319/\n')
                f.write('cd ' + helpers.winpath_to_msyspath(self.setts.SrcDir + '\n'))
                f.write('./autogen.sh --prefix=/tmp/install\n')
                f.write('make clean\n')
                f.write('make\n')
            cmds = [join(self.setts.msys2path, 'usr\\bin\\bash.exe'), '--login', buildfile]
            cmd = helpers.run_cmd(cmds, self.setts.SrcDir)

        else:
            print("Building using Linux shell")
            with open(buildfile, 'w') as f:
                f.write('cd ' + self.setts.SrcDir + '\n')
                f.write('./autogen.sh --prefix=/tmp/install\n')
                f.write('make clean\n')
                f.write('make\n')
            cmds = [self.setts.bashpath, buildfile]
            cmd = helpers.run_cmd(cmds, self.setts.SrcDir)

    def build_nuget_net45(self):
        """Package up a nuget file based on the default build"""
        self.clean()
        net45_build_dir = join(self.setts.BuildDir, 'build', 'net45')
        net45_lib_dir = join(self.setts.BuildDir, 'lib', 'net45')
        GtkVersion = helpers.get_gtksharp_version(self.setts.SrcDir)

        os.makedirs(self.setts.BuildDir, exist_ok=True)
        os.makedirs(net45_build_dir, exist_ok=True)
        os.makedirs(net45_lib_dir, exist_ok=True)

        print ('Copying Files')
        shutil.copy('./misc/GtkSharp.nuspec', self.setts.BuildDir)
        shutil.copy('./misc/GtkSharp.targets', net45_build_dir)

        dll_list = ['atk', 'cairo', 'gdk', 'gio', 'glib', 'gtk', 'pango']
        for item in dll_list:
            shutil.copy(join(self.setts.SrcDir, item, item + "-sharp.dll"), net45_lib_dir)
            if item != 'cairo':
                shutil.copy(join(self.setts.SrcDir, item, item + '-sharp.dll.config'), net45_build_dir)
  
        # Edit the XML version / package name in the .nuspec file
        nuspecfile = join(self.setts.BuildDir, 'GtkSharp.nuspec')
        et.register_namespace('', 'http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd')
        tree = et.parse(nuspecfile)
        xmlns = {'nuspec': '{http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd}'}
        tree.find('.//{nuspec}version'.format(**xmlns)).text = GtkVersion
        tree.find('.//{nuspec}id'.format(**xmlns)).text = self.setts.Gtksharp_PackageName
        tree.write(nuspecfile)

        # Run Nuget
        helpers.run_cmd([self.setts.NuGetExe, 'pack', 'GtkSharp.nuspec'], self.setts.BuildDir)

        nugetfile = join(self.setts.BuildDir, self.setts.Gtksharp_PackageName + '.' + GtkVersion + '.nupkg')
        os.makedirs(self.setts.NugetPkgs, exist_ok=True)
        shutil.copy(nugetfile, self.setts.NugetPkgs)

        print ('Generation of Nuget package complete')

#!/usr/bin/python3
import os, shutil
from pybuild.ProfileBase import ProfileBase
from os.path import abspath, join
from pybuild.Helper import Helper

# TODO Add .Net Core generation

class GtkSharp(ProfileBase):

    def __init__(self):
        """Class Init"""
        super().__init__()
        self._NuGet_PackageName = 'GtkSharp'
        self._Version = Helper.get_gtksharp_version(self.SrcDir)
        self.MSYSTEM = 'MINGW64'

    @property
    def Build_ScriptDir(self):
        return abspath(join(self._BuildDir, 'scripts'))

    def build_net45(self):
        """Build the gtksharp binaries for .Net 4.5"""
        os.makedirs(self.Build_ScriptDir, exist_ok=True)
        buildfile = join(self.Build_ScriptDir, 'net45_build.sh')
        buildbatch = join(self.Build_ScriptDir, 'net45_build.bat')

        # run script via MSYS for windows, or shell for linux
        if os.name == 'nt':
            print("Building .Net GtkSharp using MSYS2")
            with open(buildfile, 'w') as f:
                f.write('PATH=$PATH:/c/Program\ Files\ \(x86\)/Microsoft\ SDKs/Windows/v10.0A/bin/NETFX\ 4.6\ Tools/\n')
                f.write('PATH=$PATH:/c/Windows/Microsoft.NET/Framework/v4.0.30319/\n')
                f.write('cd ' + Helper.winpath_to_msyspath(self.SrcDir + '\n'))
                f.write('./autogen.sh --prefix=/tmp/install\n')
                f.write('make clean\n')
                f.write('make\n')

            bashexe = join(self.MsysPath, 'usr\\bin\\bash.exe')

            with open(buildbatch, 'w') as f:
                f.write('set MSYSTEM=' + self.MSYSTEM + '\n')
                f.write(bashexe + ' --login ' + buildfile)

            cmds = ['C:\Windows\System32\cmd.exe', '/C', buildbatch]
            cmd = Helper.run_cmd(cmds, self.SrcDir)

        else:
            print("Building using Linux shell")
            with open(buildfile, 'w') as f:
                f.write('cd ' + self.SrcDir + '\n')
                f.write('./autogen.sh --prefix=/tmp/install\n')
                f.write('make clean\n')
                f.write('make\n')
            cmds = [self.LinuxBashPath, buildfile]
            cmd = Helper.run_cmd(cmds, self.SrcDir)


    def copy_net45(self):
        """Copy the .Net 4.5 dll's to the build dir"""

        net45_build_dir = join(self.Build_NugetDir, 'build', 'net45')
        net45_lib_dir = join(self.Build_NugetDir, 'lib', 'net45')

        os.makedirs(net45_build_dir, exist_ok=True)
        os.makedirs(net45_lib_dir, exist_ok=True)
        shutil.copy('./misc/GtkSharp.targets', net45_build_dir)

        dll_list = ['atk', 'cairo', 'gdk', 'gio', 'glib', 'gtk', 'pango']
        for item in dll_list:
            shutil.copy(join(self.SrcDir, item, item + "-sharp.dll"), net45_lib_dir)
            if item != 'cairo':
                shutil.copy(join(self.SrcDir, item, item + '-sharp.dll.config'), net45_build_dir)

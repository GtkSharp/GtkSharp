#!/usr/bin/python3

import ntpath
import os
import shutil
from glob import glob
from os.path import abspath, join

from pybuild.Helper import Helper
from pybuild.ProfileBase import ProfileBase
from pybuild.profiles.GtkSharp import GtkSharp


class Glue_Win32(ProfileBase):
    def __init__(self):
        """Class Init"""
        super().__init__()
        self._NuGet_PackageName = 'GtkSharp.Win32.Glue'
        self._Version = Helper.get_gtksharp_version(self.SrcDir)
        self.MSYSTEM = 'MINGW32'

    def Get_Dlls_GtkSharp_Glue(self):
        ret = []

        # Gtksharp Glue libs
        ret.append(['atk/glue/.libs/*atksharpglue-3.dll', 'atksharpglue-3.dl_'])
        ret.append(['pango/glue/.libs/*pangosharpglue-3.dll', 'pangosharpglue-3.dl_'])
        ret.append(['gio/glue/.libs/*giosharpglue-3.dll', 'giosharpglue-3.dl_'])
        ret.append(['gtk/glue/.libs/*gtksharpglue-3.dll', 'gtksharpglue-3.dl_'])
        return ret

    def build(self):
        """Package up a nuget file based on the default build"""

        if os.name != 'nt':
            print("Skipping Native Nuget package build, as this needs to be run on Windows")
            return

        # Trigger build of gtksharp with specific bash for Mingw32
        builder = GtkSharp()
        builder.MSYSTEM = self.MSYSTEM
        builder.build()

        net45_build_dir = join(self.Build_NugetDir, 'build', 'net45')
        os.makedirs(net45_build_dir, exist_ok=True)

        print('Copying Files')
        dll_list = self.Get_Dlls_GtkSharp_Glue()

        for item in dll_list:
            src = glob(abspath(join(self.SrcDir, item[0])))[0]
            f_basename, f_extension = os.path.splitext(ntpath.basename(src))
            dest = join(net45_build_dir, item[1])
            shutil.copy(src, dest)

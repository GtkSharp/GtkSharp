#!/usr/bin/python3
"""Build of GTK3 into a NuGet package - Windows 32bit"""

import ntpath
import os
import shutil
from glob import iglob
from os.path import abspath, join

from pybuild.Helper import Helper
from pybuild.ProfileBase import ProfileBase


class Gtk_Win32(ProfileBase):
    def __init__(self):
        """Class Init"""
        super().__init__()
        self._NuGet_PackageName = 'GtkSharp.Win32'
        self._MingwBinPath = join(self.MsysPath + '\\mingw32\\bin')
        self.arch = 'Win32'
        self._Version = Helper.get_gtk_version_msys(self.MsysPath)

    @property
    def MingwBinPath(self):
        return abspath(self._MingwBinPath)

    def Get_Dlls_Native_GTK(self):
        ret = []

        # Gtk
        ret.append('libgtk-3-0.dll')
        ret.append('libgdk-3-0.dll')

        # atk
        ret.append('libatk-1.0-0.dll')

        # cairo
        ret.append('libcairo-2.dll')
        ret.append('libcairo-gobject-2.dll')

        # gdk-pixbuf
        ret.append('libgdk_pixbuf-2.0-0.dll')

        # glib2
        ret.append('libgio-2.0-0.dll')
        ret.append('libglib-2.0-0.dll')
        ret.append('libgmodule-2.0-0.dll')
        ret.append('libgobject-2.0-0.dll')

        # pango
        ret.append('libpango-1.0-0.dll')
        ret.append('libpangocairo-1.0-0.dll')
        ret.append('libpangoft2-1.0-0.dll')
        ret.append('libpangowin32-1.0-0.dll')
        return ret

    def Get_Dlls_Native_GTK_Deps(self):
        ret = []
        # Determined by using PE Explorer
        ret.append('libgcc_*.dll')
        ret.append('libepoxy-0.dll')
        ret.append('libintl-8.dll')
        ret.append('libwinpthread-1.dll')
        ret.append('libiconv-2.dll')
        ret.append('libfontconfig-1.dll')
        ret.append('libexpat-1.dll')
        ret.append('libfreetype-6.dll')
        ret.append('libpixman-1-0.dll')
        ret.append('libpng16-16.dll')
        ret.append('zlib1.dll')
        ret.append('libpcre-1.dll')
        ret.append('libffi-6.dll')
        ret.append('libharfbuzz-0.dll')
        ret.append('libgraphite2.dll')
        ret.append('libstdc++-6.dll')
        ret.append('libbz2-1.dll')
        return ret

    def build(self):
        """Package up a nuget file based on the default build"""

        if os.name != 'nt':
            print("Skipping Native Nuget package build, as this needs to be run on Windows")
            return

        net45_build_dir = join(self.Build_NugetDir, 'build', 'net45')
        os.makedirs(net45_build_dir, exist_ok=True)

        print('Copying Files')
        shutil.copy('./misc/GtkSharp.Native.targets', join(net45_build_dir, 'GtkSharp.' + self.arch + '.targets'))

        # Copy dlls
        dll_list = []
        dll_list += self.Get_Dlls_Native_GTK()
        dll_list += self.Get_Dlls_Native_GTK_Deps()

        for item in dll_list:
            src = join(self.MingwBinPath, item)

            srclist = iglob(src)
            for fname in srclist:
                f_basename, f_extension = os.path.splitext(ntpath.basename(fname))
                shutil.copy(fname, join(net45_build_dir, f_basename + '.dl_'))

#!/usr/bin/python3
"""Build of GTK3 into a NuGet package - Windows 64bit"""

from os.path import join

from pybuild.Helper import Helper
from pybuild.profiles.Gtk_Win32 import Gtk_Win32


class Gtk_Win64(Gtk_Win32):
    def __init__(self):
        """Class Init"""
        super().__init__()
        self._NuGet_PackageName = 'GtkSharp.Win64'
        self._MingwBinPath = join(self.MsysPath + '\\mingw64\\bin')
        self.arch = 'Win64'
        self._Version = Helper.get_gtk_version_msys(self.MsysPath)

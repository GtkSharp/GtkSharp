#!/usr/bin/python3

from pybuild.ProfileBase import ProfileBase
from pybuild.Helper import Helper
from pybuild.profiles.Glue_Win32 import Glue_Win32

class Glue_Win64(Glue_Win32):

    def __init__(self):
        """Class Init"""
        super().__init__()
        self._NuGet_PackageName = 'GtkSharp.Win64.Glue'
        self._Version = Helper.get_gtksharp_version(self.SrcDir)
        self.MSYSTEM = 'MINGW64'

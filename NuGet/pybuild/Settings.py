#! python3
"""Settings for scripts"""

import os

class Settings(object):

    def __init__(self):
        """Class Init"""

        self.NuGetExe = 'nuget.exe'
        self.Gtksharp_PackageName = 'GBD.GtkSharp'

        self.SrcDir = '../'
        self.BuildDir = './build'
        self.NugetPkgs = './nupkg'
        self.SrcDir = os.path.abspath(self.SrcDir)
        self.BuildDir = os.path.abspath(self.BuildDir)
        self.NugetPkgs = os.path.abspath(self.NugetPkgs)

        self.msys2path = 'C:\\msys64'
        self.msys2path = os.path.abspath(self.msys2path)
        self.bashpath = '/bin/bash'

        self.mingwin32path = 'C:\\msys64\\mingw32\\bin'
        self.mingwin32path = os.path.abspath(self.mingwin32path)
        self.mingwin64path = 'C:\\msys64\\mingw64\\bin'
        self.mingwin64path = os.path.abspath(self.mingwin64path)

    def get_native_win_dlls(self):
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

    def get_native_win_deps(self):
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
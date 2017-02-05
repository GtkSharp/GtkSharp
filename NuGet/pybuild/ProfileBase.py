#!/usr/bin/python3
"""Base class for Settings profiles"""

import os
import shutil
from os.path import abspath, join
from xml.etree import ElementTree as et

from pybuild.Helper import Helper


class ProfileBase(object):
    def __init__(self):
        """Class Init"""
        self._NuGetPath = 'nuget.exe'
        self._SrcDir = '../'
        self._BuildDir = './build'
        self._PackageDestination = './nupkg'
        self._NuGet_PackageName = ''
        self._MsysPath = 'C:\\msys64'
        self._LinuxBashPath = '/bin/bash'
        self._Version = '0.0.0'

    @property
    def NuGetPath(self):
        return self._NuGetPath

    @property
    def SrcDir(self):
        return abspath(self._SrcDir)

    @property
    def BuildDir(self):
        return abspath(self._BuildDir)

    @property
    def Build_NugetDir(self):
        return abspath(join(self._BuildDir, 'nuget'))

    @property
    def PackageDestination(self):
        return abspath(self._PackageDestination)

    @property
    def NuGet_PackageName(self):
        return self._NuGet_PackageName

    @property
    def MsysPath(self):
        return abspath(self._MsysPath)

    @property
    def LinuxBashPath(self):
        return abspath(self._LinuxBashPath)

    @property
    def Version(self):
        return self._Version

    @staticmethod
    def clean():
        """Clean the build dir"""
        Helper.emptydir('./build')
        print("Clean finished")

    def build_nuget(self):
        """Package up a nuget file based on the default build"""

        # Copy Nuget Spec file
        shutil.copy('./misc/GtkSharp.nuspec', self.Build_NugetDir)

        # Edit the XML version / package name in the .nuspec file
        nuspecfile = join(self.Build_NugetDir, 'GtkSharp.nuspec')
        et.register_namespace('', 'http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd')
        tree = et.parse(nuspecfile)
        xmlns = {'nuspec': '{http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd}'}
        tree.find('.//{nuspec}version'.format(**xmlns)).text = self.Version
        tree.find('.//{nuspec}id'.format(**xmlns)).text = self.NuGet_PackageName
        tree.write(nuspecfile)

        # Run Nuget
        Helper.run_cmd([self.NuGetPath, 'pack', 'GtkSharp.nuspec'], self.Build_NugetDir)

        # Copy Nuget files out of build directory
        nugetfile = join(self.Build_NugetDir, self.NuGet_PackageName + '.' + self.Version + '.nupkg')
        os.makedirs(self.PackageDestination, exist_ok=True)
        shutil.copy(nugetfile, self.PackageDestination)
        print('Generation of Nuget package complete - ' + self.NuGet_PackageName)

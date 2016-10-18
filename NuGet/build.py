#!/usr/bin/python3
"""Script to build out the .Net dll's and package them into a Nuget Package for gtksharp3"""
import os, sys
from pybuild.profiles.GtkSharp import GtkSharp
from pybuild.profiles.GtkSharp_Core import GtkSharp_Core
from pybuild.profiles.Glue_Win32 import Glue_Win32
from pybuild.profiles.Glue_Win64 import Glue_Win64
from pybuild.profiles.Gtk_Win32 import Gtk_Win32
from pybuild.profiles.Gtk_Win64 import Gtk_Win64

# Ideally I'd like to see the GtkSharp Build system redone via the build system of .Net core (dotnet cli tool)
# and using Scons / Cuppa for the glue libraries
# For now though we rely on the use of make to build the .Net dll's
# under linux we run this natively, under windows we can use MSYS2

class Build(object):

    # Clean the Build directory
    def clean(self):
        """Clean the build dir"""
        helpers.emptydir('./build')
        print ("Clean finished")

    # Print Usage
    def usage(self):
        print ("Please use GtkSharp3_Build.py <target> where <target> is one of")
        print ("  clean                 to clean the output directory: ./build")

        print ("  gtksharp              to build .Net libs for GtkSharp, via .Net 4.5")
        print ("  gtksharp_core         to build .Net libs for GtkSharp, via .Net 4.5 using the dotnet cli tool")

        print ("  gtk_win32             to build the Nuget package for GtkSharp.Win32")
        print ("  gtk_win64             to build the Nuget package for GtkSharp.Win64")
        print ("  all                   to make all")

    def main(self):
        if len(sys.argv) != 2:
            self.usage()
            return

        if sys.argv[1] == 'all':
            self.runbuild('gtksharp')
            self.runbuild('gtk_win32')
            self.runbuild('gtk_win64')
            return

        self.runbuild(sys.argv[1])


    def runbuild(self, build_type):
        
        if build_type == 'clean':
            self.clean()

        elif build_type == 'gtksharp':
            profile = GtkSharp()
            profile.clean()
            profile.build()
            profile.copy()
            profile.build_nuget()

        elif build_type == 'gtksharp_core':
            profile = GtkSharp_Core()
            profile.clean()
            profile.build()
            profile.copy_dll()
            profile.build_nuget()

        elif build_type == 'gtk_win32':
            profile_glue = Glue_Win32()
            profile_glue.clean()
            profile_glue.build()
            profile_gtk = Gtk_Win32()
            profile_gtk.build()
            profile_gtk.build_nuget()

        elif build_type == 'gtk_win64':
            profile_glue = Glue_Win64()
            profile_glue.clean()
            profile_glue.build()
            profile = Gtk_Win64()
            profile.build()
            profile.build_nuget()

if __name__ == "__main__":
    Build().main()
